using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Windows.Controls;
using LocationClientGUI;
using System.Runtime.InteropServices;

namespace NetworkClient
{
    public class Program
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
        [STAThread]
        public static int Main(string[] args)
        {

            if (args != null && args.Length > 0)
            {
                String host = "whois.net.dcs.hull.ac.uk";
                int port = 43;
                string protocol = "whois";

                string name = null;
                string location = null;
                int timeout = 1000;

                bool debug = false;


                try
                {


                                                                                                        
                    for (int i = 0; i < args.Length; ++i)                                 //this switcg statement looks through the argument
                    {                                                                     
                        switch (args[i])
                        {
                            case "-h": host = args[++i]; break;                         //this looks to see fot the host...
                            case "-p": port = int.Parse(args[++i]); break;              //the port...
                            case "-h9":                                                 //the HTTP 0.9 protocol
                            case "-h0":                                                 //the HTTP 1.0 protocol
                            case "-h1": protocol = args[i]; break;                      //and the HTTP 1.1 protocol 
                            default:
                                {
                                    if (name == null)                                   //if no protocol is detected or there was no protocol to begin with...
                                    {                                                   //the default will activate and retrieve the name and location
                                        name = args[i];                                 
                                    }
                                    else if (location == null)
                                    {
                                        location = args[i];
                                    }
                                    else
                                    {
                                        Console.WriteLine("Too many arguments");        //if there are too many arguments then this error will print
                                    }

                                    break;
                                }
                        }
                    }
                    if (name == null)                                                   //if a name cannont be identified then an error will display... 
                    {                                                                   //saying that there are too few arguments
                        Console.WriteLine("Too few arguments");

                    }

                }
                catch
                {
                    Console.WriteLine("Something went wrong");                          //if there is any sort of invalid input then this error message will show
                }

                ClientRespond(host, timeout, port, protocol, name, location, debug, null);
                return 0;
            }
            else
            {
                FreeConsole();
                var app = new App();
                return app.Run();
            }
        }

        internal static void ClientRespond(string host, int timeout, int port, string protocol, string name, string location, bool debug, MainWindow form)
        {

            ConsoleOut.form = form;
            
            try
            {

                TcpClient client = new TcpClient();
                client.Connect(host, port);
                StreamWriter sw = new StreamWriter(client.GetStream());                     //this enables the writing to the server
                StreamReader sr = new StreamReader(client.GetStream());                     //this enables the reading from the server
                sw.AutoFlush = true;
                client.ReceiveTimeout = timeout;                                            //if the server takes too long to reply then the client will timeout
                client.SendTimeout = timeout;                                               //if the client takes too long to respond then the client will timeout 
                
                switch (protocol)                                                           //this switch statement identifies which HTTP protocol is needed 
                {
                    case "whois":                                                           //this case is for the whois protocol
                        if (location == null)
                        {
                            sw.WriteLine(name);

                            ConsoleOut.messageout(name + " is " + sr.ReadToEnd(), true);

                        }
                        else
                        {
                            sw.WriteLine(name + " " + location);


                            String reply = sr.ReadLine();
                            if (reply == "OK")
                            {
                                ConsoleOut.messageout(name + " location changed to be " + location + "\r\n", true);
                            }
                            else
                            {
                                ConsoleOut.messageout("ERROR: Unexpected response: " + reply, true);
                            }
                        }
                        break;

                    case "-h9":                                                             //this case is for the HTTP 0.9 protocol
                        if (location == null)
                        {
                            sw.WriteLine("GET /" + name + "\r\n");

                            string line = sr.ReadLine();
                            line = sr.ReadLine();
                            line = sr.ReadLine();
                            ConsoleOut.messageout(name + " is " + sr.ReadLine(), true);
                        }
                        else
                        {
                            sw.WriteLine("PUT /" + name + "\r\n" + location);

                            String reply = sr.ReadLine();
                            if (reply.EndsWith("OK"))
                            {
                                ConsoleOut.messageout(name + " location changed to be " + location + "\r\n", true);
                            }
                            else
                            {
                                ConsoleOut.messageout("ERROR: Unexpected response: " + reply, true);
                            }
                        }
                        break;

                    case "-h0":                                                             //this case is for the HTTP 1.0 protocol
                        if (location == null)
                        {
                            sw.WriteLine("GET /?" + name + " HTTP/1.0" + "\r\n" + "\r\n");
                            string line = sr.ReadLine();
                            line = sr.ReadLine();
                            line = sr.ReadLine();
                            ConsoleOut.messageout(name + " is " + sr.ReadLine(), true);
                        }
                        else
                        {
                            int length = location.Length;

                            sw.WriteLine("POST /" + name + " HTTP/1.0");
                            sw.WriteLine("Content-Length: " + length);
                            sw.WriteLine();
                            sw.WriteLine(location);

                            string reply = sr.ReadLine();
                            if (reply.EndsWith("OK"))
                            {
                                ConsoleOut.messageout(name + " location changed to be " + location + "\r\n", true);
                            }
                            else
                            {
                                ConsoleOut.messageout("ERROR: Unexpected response: " + reply, true);
                            }

                        }
                        break;

                    case "-h1":                                                               //thos case is for the HTTP 1.1 protocol
                        if (location == null)
                        {
                            sw.WriteLine("GET /?name=" + name + " HTTP/1.1");
                            sw.WriteLine("Host: " + host);
                            sw.WriteLine();

                            if (port == 80)                                                   //if the port number is 80 then this functions will run in order to read back the webpage data
                            {
                                List<string> list = new List<string>();
                                string j = "";
                                while (sr.Peek() > 0)
                                {
                                    j = sr.ReadLine().ToString();
                                    list.Add(j);
                                }
                                j = "";
                                int mid = list.IndexOf("");
                                for (int i = mid + 1; i < list.Count; i++)
                                {
                                    j = j + list[i];
                                    j = j + "\r\n";
                                }
                                ConsoleOut.messageout(name + " is " + j, true);

                            }
                            else
                            {
                                string lineOne = sr.ReadLine();
                                lineOne = sr.ReadLine();
                                lineOne = sr.ReadLine();
                                ConsoleOut.messageout(name + " is " + sr.ReadLine(), true);
                            }



                        }
                        else
                        {
                            int x = 15;
                            int y = name.Length;
                            int z = location.Length;

                            int length = x + y + z;

                            sw.WriteLine("POST / HTTP/1.1");
                            sw.WriteLine("Host: " + host);
                            sw.WriteLine("Content-Length: " + length);
                            sw.WriteLine();
                            sw.WriteLine("name=" + name + "&location=" + location);

                            String reply = sr.ReadLine();
                            if (reply.EndsWith("OK"))
                            {
                                ConsoleOut.messageout(name + " location changed to be " + location + "\r\n", true);
                            }
                            else
                            {
                                ConsoleOut.messageout("ERROR: Unexpected response: " + reply, true);
                            }

                        }
                        break;
                }
            }

            catch (Exception e)
            {
                ConsoleOut.messageout(e + "\r\n" + "An error has occurred", true);

            }
            
        }

    }
}












