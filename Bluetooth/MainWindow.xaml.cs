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
        public MainWindow()
        {
            InitializeComponent();
            string [] typ = {"SERVER","CLIENT"};
            Type.ItemsSource = typ;
            Type.SelectedIndex = 0; 

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
        }

        private void Listener_ac(IAsyncResult ar)
        {
            if (ar.IsCompleted)
                MessageBox.Show("Connected");

            bc = bl.EndAcceptBluetoothClient(ar);
            s = bc.GetStream(); 
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
                    bc.SetPin("0000");
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

        }

        private void Type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Type.SelectedValue== "CLIENT")
            {
                Connect.IsEnabled = false;
                bl= new BluetoothListener(new Guid());
                bl.BeginAcceptBluetoothClient(new AsyncCallback(Connect_ac), bl); 
            }
            else
            {
                if (bl != null)
                {
                    bl.Stop(); 
                }
            }
        }


    }
}
