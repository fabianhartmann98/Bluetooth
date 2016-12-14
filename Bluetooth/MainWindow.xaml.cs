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
        string pin = "2017";
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
        

        private void Connect_ac(IAsyncResult ar)
        {
            if (ar.IsCompleted)
                MessageBox.Show("Connected");
            
            s = bc.GetStream();
            s.BeginRead(RX_buf, rx_tail, buf_len - rx_tail, beginRead_cal, s);
        }

        private void beginRead_cal(IAsyncResult ar)
        {
            rx_tail += s.EndRead(ar);
            
            //TextBoxUpdate(Receive_tb,Convert.ToString(ASCIIEncoding.ASCII.GetChars( RX_buf,rx_head,rx_tail-rx_head))); 
            for (int i = rx_head; i < rx_tail; i++)
            {
                char temp = (char)RX_buf[i];
                TextBoxUpdate(Receive_tb, temp.ToString());
            }
            //TextBoxUpdate(Receive_tb, "this is just shit");

            rx_tail = 0;
            rx_head = 0;

            s.BeginRead(RX_buf, rx_tail, buf_len - rx_tail, beginRead_cal, s);
        }

        private void TextBoxUpdate(TextBox textBox, string v)
        {
            if (!textBox.Dispatcher.CheckAccess())
            {
                textBox.Dispatcher.Invoke(
                     (Action<TextBox, string>)TextBoxUpdate, textBox, v);
            }
            else
            {
                textBox.Text += v;
            }
        }


        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = Devices.SelectedIndex;
                Guid serviceClass = Guid.NewGuid();
                BluetoothDeviceInfo device = infos[i];
                BluetoothSecurity.PairRequest(device.DeviceAddress, pin);
                
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
