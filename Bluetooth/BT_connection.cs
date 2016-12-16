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
        BluetoothListener bl;
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
            //s.BeginRead(RX_buf, rx_tail, buf_len - rx_tail, beginRead_cal, s);

        }

        private byte AccessRXBuf(int i)
        { 
            return RX_buf[i%256];
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
                            if (ComputeChecksum(crcpacket) != 0)
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
                                    Logger("received StayingAlive");

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

        private void CreateTable()
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

        private static byte ComputeChecksum(params byte[] bytes)
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

        public void SendStayingAlive()
        { 
            byte[] b= new byte[BT_Protocoll.StayingAliveLength+BT_Protocoll.FrameLengthOverhead];
            SettingPräamble(ref b);
            b[2] = (byte)BT_Protocoll.StayingAliveLength;
            b[3] = BT_Protocoll.StayingAliveCommand;
            b[4] = ComputeChecksum(b);
            b[5] = BT_Protocoll.CarriageReturn;
            Send(b);
            Logger("sending StayingAlive\n");
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
