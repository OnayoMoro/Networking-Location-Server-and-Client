
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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


namespace locationserver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool serverStart = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Startbtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (serverStart)
            {
                return;
            }
            new Thread(Program.runServer).Start();
         
            CommandLine.Text = "Server is running";
            serverStart = true;
        }

        private void Stopbtn_Click_1(object sender, RoutedEventArgs e)
        {
            if (!serverStart)
            {
                return;
            }
            Program .listener.Stop();
     
            CommandLine.Text = "Server is not running";
            serverStart = false;
        }

        private void CommandLine_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (serverStart)
            {
                Program.listener.Stop();
            }
        }

    }
}
