using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
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
        public MainWindow()
        {
            InitializeComponent();
            bc = new BluetoothClient();

            infos = bc.DiscoverDevices();
            string[] names= new string [infos.Length];
            for (int i = 0; i < infos.Length; i++)
            {
                names[i] = infos[i].DeviceName; 
            }
            comobox.ItemsSource = names;
            comobox.SelectedIndex = 0;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                 

                int i = comobox.SelectedIndex;
                Guid serviceClass = Guid.NewGuid();
                BluetoothSecurity.PairRequest(infos[i].DeviceAddress, "0000"); 
                bc.Connect(infos[i].DeviceAddress, serviceClass);
                textbox.Text += ("connectet to " + infos[i].DeviceAddress.ToString() + " - " + BluetoothService.SerialPort.ToString()); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); 
                throw;
            }
            
        }
    }
}
