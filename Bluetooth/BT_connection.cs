using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System.IO;

namespace Bluetooth
{
    public class BT_connection
    {
        BluetoothClient bc;
        BluetoothDeviceInfo[] infos;
        Stream s;
        const int buf_len = 256;
        byte[] RX_buf = new byte[buf_len];
        int rx_head = 0;
        int rx_tail = 0;

        private void Logger(String lines)
        {
         // Write the string to a file.append mode is enabled so that the log
         // lines get appended to  test.txt than wiping content and writing the log
          System.IO.StreamWriter file = new System.IO.StreamWriter("Log.txt",true);
          file.Write(DateTime.Now.ToString()+": ");
          file.WriteLine(lines);
          file.Close();
        }

        private void DeletLogger()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter("Log.txt",false);
            file.Write("");
            file.Close();

        }
        

        static byte[] crc_table = new byte[256];
         // x8 + x7 + x6 + x4 + x2 + 1
        const byte crc_poly = 0xd5;

        public string DeviceName { get; private set; }
        private BluetoothDeviceInfo deviceinfo; 
        
        private string pin = "2017";

        public string Pin
        {
            get { return pin; }
            set { pin = value; }
        }

        public BT_connection()
        {
            DeletLogger();
            crc_CreateTable();              
        }        

        

        public string[] GetAvailableDevices()
        {
            bc = new BluetoothClient();

            infos = bc.DiscoverDevices(255, false, true, true);
            string[] names = new string[infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                names[i] = infos[i].DeviceName;
            }
            return names;
        }

        private void beginRead_cal(IAsyncResult ar)
        {
            rx_tail += s.EndRead(ar);
            DataManager();

            s.BeginRead(RX_buf, rx_tail, buf_len-rx_tail, beginRead_cal, s);            
            //s.BeginRead(RX_buf, rx_tail, buf_len, beginRead_cal, s);

        }

        private byte AccessRXBuf(int i)
        { 
            return RX_buf[i%buf_len];
        }

        private void shiftingRXBuf(int length)
        {
            Array.Copy(RX_buf, length, RX_buf, 0, buf_len - length);
            rx_tail -= length;
        }

        private void DataManager()
        {
            try
            {
                while (true)
                {
                    if (AccessRXBuf(rx_head) == BT_Protocoll.PräambleBytes[0]
                            && AccessRXBuf(rx_head + 1) == BT_Protocoll.PräambleBytes[1])
                    {
                        int framelength = AccessRXBuf(rx_head + 2);
                        if (AccessRXBuf(rx_head + BT_Protocoll.FrameLengthOverhead + framelength - 1) == BT_Protocoll.CarriageReturn)
                        {
                            byte[] crcpacket = new byte[framelength];
                            Array.Copy(RX_buf, rx_head + 2, crcpacket, 0, crcpacket.Length);
                            if (crc_CheckWithCRC(crcpacket) != 0)
                            {
                                Logger("didn't pass Checksum");
                                return;
                            }
                            switch (AccessRXBuf(rx_head + 3))
                            {
                                case (BT_Protocoll.StayingAliveAnswer):
                                    Logger("received StayingAliveAnswer");
                                    break;
                                case (BT_Protocoll.StayingAliveCommand):
                                    Logger("received StayingAliveCommand");
                                    break;
                                case (BT_Protocoll.InitCommand):
                                    Logger("received InitCommand");
                                    break;
                                case (BT_Protocoll.InitAnswer):
                                    Logger("received InitAnswer");
                                    break;
                                case (BT_Protocoll.MeasuredDataCommand):
                                    Logger("received MeasuredDataCommand");
                                    break;
                                case (BT_Protocoll.MeasuredDataAnswer):
                                    Logger("received MeasuredDataAnswer");
                                    break;
                                case (BT_Protocoll.MotorAdjustingCommand):
                                    Logger("received MotorAdjustingCommand");
                                    break;
                                case (BT_Protocoll.MotorAdjustingAnswer):
                                    Logger("received MotorAdjustingAnswer");
                                    break;
                                case (BT_Protocoll.StatusRequestCommand):
                                    Logger("received StatusRequestCommand");
                                    break;
                                case (BT_Protocoll.StatusRequestAnswer):
                                    Logger("received StatusRequestAnswer");
                                    break;
                                case (BT_Protocoll.PositionRequestCommand):
                                    Logger("received PositionRequestCommand");
                                    break;
                                case (BT_Protocoll.PositionRequestAnswer):
                                    Logger("received PositionRequestAnswer");
                                    break;
                                case (BT_Protocoll.MaxGapRequestCommand):
                                    Logger("received MaxGapRequestCommand");
                                    break;
                                case (BT_Protocoll.MaxGapRequestAnswer):
                                    Logger("received MaxGapRequestAnswer");
                                    break;
                                default:
                                    break;
                            }

                            shiftingRXBuf(framelength + BT_Protocoll.FrameLengthOverhead);

                        }
                        else
                            break;
                    }
                    else 
                        break;
                }

	        }
	        catch 
	        {
		
	        }

            //correcting 
            while (AccessRXBuf(0) != BT_Protocoll.PräambleBytes[0] && AccessRXBuf(0)!=0)
                shiftingRXBuf(1);
            
        }
        
        private void Connect_ac(IAsyncResult ar)
        {
            if (ar.IsCompleted)
            {
                OnDeviceConnected();
                Logger("connected to Device: " + deviceinfo.DeviceName + " with Address " + deviceinfo.DeviceAddress);
                s = bc.GetStream();
                s.BeginRead(RX_buf, rx_tail, buf_len - rx_tail, beginRead_cal, s);
            }
        }

        public void ConnectToDevice(string DevName)
        {
            DeviceName = DevName;
            foreach (var item in infos)
            {
                if (item.DeviceName == DeviceName)
                {
                    deviceinfo = item;
                    BluetoothSecurity.PairRequest(deviceinfo.DeviceAddress, pin);

                    if (deviceinfo.Authenticated)
                    {
                        bc.BeginConnect(deviceinfo.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect_ac),deviceinfo);
                    }
                    break;
                }
            }
        }

        private void crc_CreateTable()
        {
            for (int i = 0; i < 256; ++i)
            {
                int temp = i;
                for (int j = 0; j < 8; ++j)
                {
                    if ((temp & 0x80) != 0)
                    {
                        temp = (temp << 1) ^ crc_poly;
                    }
                    else
                    {
                        temp <<= 1;
                    }
                }
                crc_table[i] = (byte)temp;
            }
        }

        private static byte crc_ComputeChecksum(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {        
                //starting at 2 because the Präamble is not included int crc
                for (int i = 2; i < bytes.Length-2; i++) //-2 because the last 0 (CRC and CR) shouldn't be used to calculate crc
                    //this works only, when the first one is only done whith the values 
                {
                    crc = crc_table[crc ^ bytes[i]];
                }
            }
            return crc;
        }

        private static byte crc_CheckWithCRC(params byte[] bytes)
        {
            byte crc = 0;
            if (bytes != null && bytes.Length > 0)
            {
                foreach (byte b in bytes)
                {
                    crc = crc_table[crc ^ b];                    
                }
            }
            return crc;
        } 
        
  


        private void SettingPräamble(ref byte[] b)
        {
            b[0] = BT_Protocoll.PräambleBytes[0];
            b[1] = BT_Protocoll.PräambleBytes[1]; 
        }

        public void SendInit()
        {
            int packetlength = BT_Protocoll.InitLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.InitLength;
            b[3] = BT_Protocoll.InitCommand;

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending Init");
        }

        public void SendStayingAlive()
        {
            int packetlength = BT_Protocoll.StayingAliveLength + BT_Protocoll.FrameLengthOverhead;
            byte[] b= new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.StayingAliveLength;
            b[3] = BT_Protocoll.StayingAliveCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending StayingAlive");
        }

        public void SendMeasuredDataAnswer(byte notlastData)
        {
            int packetlength = BT_Protocoll.MeasuredDataAnswerLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.MeasuredDataAnswerLength;
            b[3] = BT_Protocoll.MeasuredDataAnswer;

            b[4] = notlastData;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending MeasuredDataAnswer");
        }

        public void SendMotorAdjusting(int gap)
        {
            int packetlength = BT_Protocoll.MotorAdjustingLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.MotorAdjustingLength;
            b[3] = BT_Protocoll.MotorAdjustingCommand;

            b[4] = (byte)(gap >> 8);
            b[5] = (byte)(gap&0xFF);

            b[packetlength - 2] = crc_ComputeChecksum(b);
            b[packetlength - 1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending MotorAdjusting");
        }

        public void SendStatusRequest()
        {
            int packetlength = BT_Protocoll.StatusRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.StatusRequestLength;
            b[3] = BT_Protocoll.StatusRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending StatusRequest");
        }

        public void SendPositionRequest()
        {
            int packetlength = BT_Protocoll.PositionRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.PositionRequestLength;
            b[3] = BT_Protocoll.PositionRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending PositionRequest");
        }

        public void SendMaxGapRequest()
        {
            int packetlength = BT_Protocoll.MaxGapRequestLength + BT_Protocoll.FrameLengthOverhead;

            byte[] b = new byte[packetlength];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.MaxGapRequestLength;

            b[3] = BT_Protocoll.MaxGapRequestCommand;

            b[packetlength-2] = crc_ComputeChecksum(b);
            b[packetlength-1] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending MaxGapRequest");
        }

        public void Send(byte[] b)
        {
            s.Write(b, 0, b.Length);
            //s.Write(new byte[] {0x01}, 0, 1);
        }        

        
        public event EventHandler DeviceConnected;

        protected virtual void OnDeviceConnected()
        {
                  
            if (DeviceConnected != null)
                DeviceConnected(this, new EventArgs());
        }




    }
}
