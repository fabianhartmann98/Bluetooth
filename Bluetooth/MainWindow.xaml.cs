using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Bluetooth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        BluetoothClient bc;
        BluetoothDeviceInfo[] infos;
        SerialPort sp = new SerialPort();
        string[] ports;

        private int baudrate = 38400;
        private StopBits stopbits = StopBits.One;
        private int databits = 8;
        private Parity parity = Parity.None; 

        public MainWindow()
        {
            InitializeComponent();

            ports = SerialPort.GetPortNames(); 

            bc = new BluetoothClient();

            infos = bc.DiscoverDevices(255,false,true,true);
            string[] names= new string [infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                names[i] = infos[i].DeviceName; 
            }
            Devices.ItemsSource = names;
            Devices.SelectedIndex = 0;
        }
        

        private void Connect_ac(IAsyncResult ar)
        {
            if (ar.IsCompleted)
                if (bc.Connected)
                {
                    MessageBox.Show("Connected");
                    bc.Close();
                    string[] newports = SerialPort.GetPortNames();
                    string addedport = "";
                    foreach (var item in newports)
                    {
                        bool IsAnOldOne = false;
                        foreach (var item2 in ports)
                        {
                            if(item==item2)
                            {
                                IsAnOldOne=true;
                                break;
                            }
                        }
                        if (!IsAnOldOne)
                        {
                            addedport = item;
                            break;
                        }
                    }
                    settingUpSerialPort("COM6");
                    sp.Open(); 
                }

        }

        private void settingUpSerialPort(string Port)
        {
            if (sp.IsOpen)
                sp.Close();
            sp = new SerialPort();
            sp.BaudRate = baudrate;
            sp.StopBits = stopbits;
            sp.DataBits = databits;
            sp.Parity = parity;
            sp.PortName = Port;            
            sp.DataReceived += sp_DataReceived;
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = Devices.SelectedIndex;
                Guid serviceClass = Guid.NewGuid();
                BluetoothDeviceInfo device = infos[i];

                device.SetServiceState(BluetoothService.SerialPort, true);

                BluetoothSecurity.PairRequest(device.DeviceAddress, "0000");
                
                if (device.Authenticated)
                {
                    bc.BeginConnect(device.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect_ac), device);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void Send_b_Click(object sender, RoutedEventArgs e)
        {
            sp.Write("done");
        }


    }
}
