using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Database;

namespace Server
{
    // State object for reading client data asynchronously 
    public class Client
    { 
        // Client  socket.  
        public Socket workSocket = null;        
        public int ID { get; set; }
        // Size of receive buffer.
        public const int BufferSize = 60000;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
    // Received data string.
        public StringBuilder sb = new StringBuilder();  
    }
    
    public class Connector 
    { 
        public static List<Client> clients = new List<Client>();
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public Connector() 
        {        
        }

        public static void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[60000];

            // IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            // IPAddress ipAddress = ipHostInfo.AddressList[0];
            // IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            // Establish the endpoint for the socket.
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1121);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try 
            {
                listener.Bind(endPoint);
                listener.Listen(100);

                DbConnect.Connect();

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept( 
                        new AsyncCallback(AcceptCallback),
                        listener );

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            } 
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        public static void AcceptCallback(IAsyncResult ar) 
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            
            // Create the client object.
            Client client = new Client();
            client.workSocket = handler;
            client.ID = clients.Count;
            clients.Add(client);

            Console.WriteLine("Clients count is: " + clients.Count);
            handler.BeginReceive(client.buffer, 0, Client.BufferSize, 0,
                new AsyncCallback(ReadCallback), client);
        }

        public static void ReadCallback(IAsyncResult ar) 
        {
            String content = String.Empty;
            // Retrieve the client object and the handler socket
            // from the asynchronous client object.
            Client client = (Client) ar.AsyncState;
            Socket handler = client.workSocket;
            // Read data from the client socket. 
            
            SocketError se;
            int bytesRead = handler.EndReceive(ar, out se);
            Console.WriteLine(se);

            if(se != SocketError.Success)
            {
                bytesRead = 0;
            }

            if (bytesRead > 0) 
            {
                // There  might be more data, so store the data received so far.
                client.sb.Append(Encoding.UTF8.GetString(
                    client.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = client.sb.ToString();

                if (content.IndexOf('±') > -1) 
                {
                    string[] commands = content.Remove(content.Length - 1).Split('§');
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        content.Length, content );
                    //для дебага
                    for(int i = 0; i < commands.Length; i++)
                    {
                        Console.WriteLine(commands[i]);
                    }

                    if(commands[1] == "1")
                    {
                        //передача сокета клиента и команда запроса
                        CommandProcessor.CommandHandler(commands[0], commands[3], handler);
                        // Send(handler, "Broadcast" + '§' + "Server is going down...!" + '☭');
                    }

                    client.sb.Clear();
                    Array.Clear(commands, 0, commands.Length); //очищаем команды
                }
                // TODO сделать систему authToken
                // else if (content.IndexOf('$') > -1) 
                // {
                //     //Disconnect clients
                //    SocketClose(handler, client);
                // }
                else
                {                
                    // Not all data received. Get more.
                    Console.WriteLine(content);
                }      
                handler.BeginReceive(client.buffer, 0, Client.BufferSize, 0,
                        ReadCallback, client);  
                Console.WriteLine("Begin new receive");  
            }
            else
            {
                //Disconnect clients
                SocketClose(handler, client);
                return;
            }
        }

        private static void SocketClose(Socket socket, Client client)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Disconnect(false);
            socket.Close();
            clients.Remove(client);
            Console.WriteLine($"User with ID: {client.ID} is disconnected");
        }

        public static void Send(Socket handler, String data)
        {
            // data = cmd + '§' + json
            // Convert the string data to byte data using UTF8 encoding.
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar) 
        {
            try 
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                // handler.Shutdown(SocketShutdown.Both);
                // handler.Close();
            } 
            catch (Exception e) 
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int Main(String[] args) 
        {
            StartListening();
            return 0;
        }
    }
}