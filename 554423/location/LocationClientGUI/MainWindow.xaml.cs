using NetworkClient;
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

namespace LocationClientGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            whois.IsChecked = true;
        }
        public void print(string msg)
        {
            CommandLine.Text = msg + "\r\n" + CommandLine.Text; 
        }
        private void Launchbtn_Click_1(object sender, RoutedEventArgs e)
        {
            string proto = "";
            if (whois.IsChecked == true)
            {
                proto = "whois";
            }
            else if (HTTP_1_0.IsChecked == true)
            {
                proto = "-h0";
            }
            else if (HTTP_1_1.IsChecked == true)
            {
                proto = "-h1";
            }
            else if (HTTP_0_9.IsChecked == true)
            {
                proto = "-h9";
            }

            try
            {
                Program.ClientRespond(Hosttxt.Text, int.Parse( Timeouttxt.Text), int.Parse( Porttxt.Text), proto, Nametxt.Text, Locationtxt.Text.Length == 0 ? null: Locationtxt.Text, Debugbox.IsChecked.Value, this);
            }
            catch
            {
                MessageBox.Show("Protocal not selected, please select a protocol or finish filling in request");
            }
        }

        private void CommandLine_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
