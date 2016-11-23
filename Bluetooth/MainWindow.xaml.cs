using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.IO;
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
        Stream s;
        BluetoothListener bl;
        const int buf_len = 256;
        byte[] RX_buf = new byte[buf_len]; 
        byte[] TX_buf = new byte[buf_len];
        int rx_head = 0;
        int tx_head = 0;
        int rx_tail = 0;
        int tx_tail = 0; 
        public MainWindow()
        {
            InitializeComponent();
            

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


        private async void Connect_ac(IAsyncResult ar)
        {
            if (ar.IsCompleted)
                MessageBox.Show("Connected");

            s = bc.GetStream();
            await receifingData();

        }

        private async Task receifingData()
        {
            while (true)
            {
                rx_tail += await s.ReadAsync(RX_buf, rx_tail, buf_len - rx_tail);
                for (int i = rx_head; i < rx_tail; i++)
                {
                    Receive_tb.Text += RX_buf[i];
                }
                rx_tail = 0;
                rx_head = 0;
            }
        }

        private void beginRead_cal(IAsyncResult ar)
        {
            
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = Devices.SelectedIndex;
                Guid serviceClass = Guid.NewGuid();
                BluetoothDeviceInfo device = infos[i];
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
            foreach (char item in Send_tb.Text)
            {
                TX_buf[tx_tail++] = Convert.ToByte(item);                
            }
            Send_tb.Text = "";
            s.Write(TX_buf, tx_head, tx_tail - tx_head);
            tx_head = 0;
            tx_tail = 0; 
        }


    }
}
