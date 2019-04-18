using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;


namespace locationserver
{
    class Program
    {

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            static extern bool FreeConsole();
            public static TcpListener listener;
            [STAThread]

            public static int Main(string[] args)
            {
                if (!args.Contains("-w"))
                {
                    runServer();
                    return 0;
                }
                else
                {
                    FreeConsole();
                    var app = new App();
                    return app.Run();
                
                }
               
            }
            static Dictionary<string, string> storeUser = new Dictionary<string, string>();

            public static void runServer()
            {

                
                Socket connection;
                ThreadingClass threader;

                try
                {
                                                                                                //wating for someone to connect and accept the connection
                    int port = 43;
                    listener = new TcpListener(IPAddress.Any, port);                            //initialising the listener to listen for any IP adress coming through port 43, pointing the listener in the right direction 
                    listener.Start(); //starts the listening
                    Console.WriteLine("Server started listening");
                    while (true)
                    {
                                                                                                //this establishes the connection from the client to the server 
                                                                                                //wating for someone to connect and accept the connection
                        connection = listener.AcceptSocket();                                   //socket interface accepts the incoming data

                        threader = new ThreadingClass();
                        Thread thread = new Thread(() => threader.doRequest(connection));
                        thread.Start();


                                                                                                //allows for the stream of data for the network access
                                                                                                //socket interface closes the connection 
                        Console.WriteLine("Connection Established\r\n");
                    }
                }
                catch (Exception e)
                {
                    // class for a login function
                    Console.WriteLine("Exception: " + e);
                }
            }


            class ThreadingClass
            {

                public void doRequest(Socket connection)
                {

                    NetworkStream socketStream;
                    socketStream = new NetworkStream(connection);                  //opens connection
                    Console.WriteLine("Connection Closed\r\n");

                    bool pass = false;  
                    int q = 0;
                    string name;
                    string location;
                    string HTTP = "";
                    try
                    {
                        StreamWriter sw = new StreamWriter(socketStream);          //streamwriter will write back to the client 
                        StreamReader sr = new StreamReader(socketStream);          //streamreader wil read from the client  
                        List<string> output = new List<string>();                  //this list will store the incomming stream from the client

                        sw.AutoFlush = true;                                       //autoflush will streamwrite back to the client after every streamwriter

                        socketStream.ReadTimeout = 1000;                           //if the client takes too long to respond the server will timout after 1000 millisecionds
                        socketStream.WriteTimeout = 1000;                          //the server will time out if it takes too long to  write back to the client after 1000 millisecionds

                        while (sr.Peek() >= 0)                                                                                   //sr.Peek() will read the incomming stream as long as there is data to read
                        {
                            output.AddRange(sr.ReadLine().Split(new string[] { "\r\n", "&", "=" }, StringSplitOptions.None));    //this line takes the incomming stream from the ReadLine() line by line and split it by \r\n, & and = 
                                                                                                                                 //the split line is then added to a new line in the output list
                        }
                        if (output[0].StartsWith("PUT /") && output.Count >= 2)                                                  //if the first line starts with any of these conditions and the list count is greater or equal to 2...
                        {                                                                                                        //... then the current client request is a protocol request and not a whois
                            pass = true;                                                                                         
                        }
                        if (output[0].StartsWith("POST /") && output.Count >= 2)
                        {
                            pass = true;
                        }

                    if (!output[0].StartsWith("GET /") && ! output[0].EndsWith("\r\n") && pass == false)    //this condition checks for a whois protocol
                        {
                            HTTP = "WHOIS";                                                                                                    //sets HTTP to whois
                        }
                        else if ((output[0].StartsWith("GET /") || output[0].StartsWith("GET /?") || output[0].StartsWith("GET /?name")))      //this condition checks for a GET protocol
                        {   
                            HTTP = "GET";                                                                                                      //sets HTTP to GET
                        }
                        else if (output[0].StartsWith("POST / HTTP1.1") || output[0].StartsWith("POST /"))                                     //this condition checks for a POST protocol
                        {
                            HTTP = "POST";                                                                                                     //sets HTTP to POST
                        }       
                        else                                                                                                                   //elses its a PUT protocol
                        {
                            HTTP = "PUT";                                                                                                      //sets HTTP to PUT
                        }



                        switch (HTTP)                                                                                                               //this statement checks the HTTP variable to see what protocol it was set to
                        {
                            case "GET":                                                                                                             //if HTTP is GET then it will excetute a lookup protocol 
                                //HTTP 0.9
                                if (!output[0].StartsWith("GET /?") && !output[0].EndsWith("HTTP/1.0") && !output[0].EndsWith("HTTP/1.1"))          //this identifies if the protocol is for HTTP 0.9
                                {
                                    name = output[0].Remove(0, 5);                                                                                
                                    if (storeUser.ContainsKey(name))
                                    {
                                        sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n\r\n" + storeUser[name] + "\r\n");             
                                    }
                                    else if (!storeUser.ContainsKey(name))
                                    {
                                        sw.WriteLine("HTTP/0.9 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                    }
                                }
                                //HTTP 1.0
                                else if (output[0].EndsWith("HTTP/1.0") && !output[0].EndsWith("HTTP/1.1") && output[0].StartsWith("GET /?"))       //this identifies if the protocol is for HTTP 1.0     
                            {
                                    name = output[0].Remove(0, 6);                                                                                  
                                    name = name.Substring(0, name.Length - 9);
                                    if (storeUser.ContainsKey(name))
                                    {
                                        sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n" + storeUser[name] + "\r\n");
                                    }

                                    else if (!storeUser.ContainsKey(name))
                                    {
                                        sw.WriteLine("HTTP/1.0 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                    }

                                }
                                //HTTP 1.1
                                else                                                                                                                //else it is for 1.1
                                {
                                    name = output[1].Substring(0, output[1].Length - 9);
                                    if (storeUser.ContainsKey(name))
                                    {
                                        sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n" + storeUser[name] + "\r\n");
                                    }

                                    else if (!storeUser.ContainsKey(name))
                                    {
                                        sw.WriteLine("HTTP/1.1 404 Not Found\r\nContent-Type: text/plain\r\n\r\n");
                                    }

                                }
                                break;
                            case "PUT":                                                                                                              //if HTTP is PUT then it will excetute a lookup or update protocol which is only for 0.9
                                //HTTP 0.9
                                name = output[0].Remove(0, 5);
                                if (output.Count == 2)
                                {
                                    location = output[1];
                                }
                                else
                                {
                                    location = output[2];
                                }

                                if (storeUser.ContainsKey(name))
                                {
                                    storeUser[name] = location;
                                }
                                else if (!storeUser.ContainsKey(name))
                                {
                                    storeUser.Add(name, location);
                                }
                                sw.WriteLine("HTTP/0.9 200 OK\r\nContent-Type: text/plain\r\n");

                                break;
                            case "POST":                                                                                                                //if HTTP is POST then it will excetute a lookup or update protocol
                                //HTTP 1.0
                                if (output[0].EndsWith("HTTP/1.0") && (output[0] != ("POST / HTTP/1.1")) && output[0].StartsWith("POST /"))             //this identifies if the protocol is for HTTP 1.0
                            {
                                    name = output[0].Remove(0, 6);
                                    name = name.Substring(0, name.Length - 9);
                                    location = output[3];

                                    if (storeUser.ContainsKey(name))
                                    {
                                        storeUser[name] = location;
                                    }
                                    else
                                    {
                                        storeUser.Add(name, location);
                                    }
                                    sw.WriteLine("HTTP/1.0 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                }
                                //HTTP 1.1
                                else if ((output[0] == ("POST / HTTP/1.1")) && !output[0].EndsWith("HTTP/1.0") && output[0].StartsWith("POST /"))        //this identifies if the protocol is for HTTP 1.1
                            {
                                    name = output[5];
                                    location = output[7];
                                    if (storeUser.ContainsKey(name))
                                    {
                                        storeUser[name] = location;
                                    }
                                    else if (!storeUser.ContainsKey(name))
                                    {
                                        storeUser.Add(name, location);
                                    }

                                    sw.WriteLine("HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                                }
                                break;
                            case "WHOIS":                                                                                                                 //if HTTP is WHOIS then it will excetute a lookup or update protocol
                                //WHOIS
                                var whois = output[0].Split(new[] { ' ' }, 2);
                                if (whois.Length == 1)
                                {
                                    if (storeUser.ContainsKey(whois[0]))
                                    {
                                        sw.WriteLine(storeUser[whois[0]]);
                                        Console.WriteLine("Location: " + storeUser[whois[0]]);
                                    }
                                    else
                                    {
                                        sw.WriteLine("ERROR: no entries found\r\n");
                                    }
                                }

                                else if (whois.Length == 2)
                                {
                                    if (storeUser.ContainsKey(whois[0]))
                                    {
                                        storeUser[whois[0]] = whois[1];
                                        sw.WriteLine("OK\r\n");
                                    }
                                    else
                                    {
                                        storeUser.Add(whois[0], whois[1]);
                                        sw.WriteLine("OK\r\n");
                                    }
                                }
                                break;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Somthing Went Wrong");
                    }
                    finally
                    {
                        socketStream.Close();
                        connection.Close();
                    }
                }
            }
        }
    }

