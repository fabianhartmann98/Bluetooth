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
        BT_connection bt;
        public MainWindow()
        {
            InitializeComponent();

             bt= new BT_connection(); 
            bt.DeviceConnected+=bt_DeviceConnected;
            
            Devices.ItemsSource = bt.GetAvailableDevices();
            Devices.SelectedIndex = 0;
        }


        void bt_DeviceConnected(object sender, EventArgs e)
        {
            TextBoxUpdate(Receive_tb, "connected to Device");
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
            bt.ConnectToDevice(Devices.SelectedItem.ToString());
        }

        private void Send_b_Click(object sender, RoutedEventArgs e)
        {

            bt.SendStayingAlive();
            bt.SendPositionRequest();
            //bt.SendInit();
            //bt.SendMotorAdjusting(300);
        }


    }
}
