using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using Exception = System.Exception;


namespace Server
{
    internal class server
    {
        static List<Socket> clients = new List<Socket>();
        static int PORT = 1235;
        static Socket serverSocket;

        static void Main(string[] args)
        {
            Thread servermsgThread = new Thread(servermsg);
            servermsgThread.Start();
            ServerStart();
        }

        static void servermsg()
        {
            string msg;
            while (true)
            {
                msg = Console.ReadLine();
                Broadcast(msg,null);
                Log(msg);
                if (msg=="kill//")
                {
                    Console.WriteLine("Server Closing...");
                    ServerClose();
                }
                

            }
        }
        
        static void ServerStart()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(100);
            Console.WriteLine("Server started. Waiting for connections...");

            while (true)
            {
                Socket clientSocket = serverSocket.Accept();
                clients.Add(clientSocket);
                Broadcast("new client",clientSocket);

                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(clientSocket);
            }
        }

        static void ServerClose()
        {
            Thread.Sleep(1000);
            serverSocket.Close();
            Environment.Exit(0);
        }
        
        static void HandleClient(object clientObj)
        {
            Socket clientSocket = (Socket)clientObj;
            bool run = true;
            sendoldmsg(clientSocket);

            while (run)
            {
                try
                {
                    while (clientSocket.Connected)
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead = clientSocket.Receive(buffer);
                        if (bytesRead == 0)
                            break; 

                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine(message);
                        Log(message);

                        Broadcast(message, clientSocket);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                clients.Remove(clientSocket);
                Broadcast("A client has disconnected.",null);
                clientSocket.Close();
                run = false;
                return;
            }
        }
        
        static void Broadcast(string message, Socket sender)
        {
            foreach (Socket client in clients)
            {
                if (message!="")
                {
                    try
                    {
                        if (client != sender || sender==null)
                        {
                            byte[] data = Encoding.UTF8.GetBytes(message);
                            client.Send(data);
                        }
                    }
                    catch
                    {
                        Console.WriteLine("An Error Has Accured");   
                    }
                    
                }
            }
        }
        
        static void sendsinglemsg(Socket newclient,string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            newclient.Send(data);
        }
        
        static void sendoldmsg(Socket nclient)
        {
            using(StreamReader file = new StreamReader("../../oldmsg/Oldmessages.txt")) {
                int counter = 0;
                string oldmsg;

                while ((oldmsg = file.ReadLine()) != null) 
                {
                    sendsinglemsg(nclient,oldmsg);
                    counter++;
                    Thread.Sleep(100);
                }
                file.Close();
                sendsinglemsg(nclient,"endofold");
            }
        }
        
        static void Log(string msg)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter("../../oldmsg/Oldmessages.txt", true))
                {
                    writer.WriteLine(msg);
                    writer.Flush(); 
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error with the log: " + e.Message);
            }
        }

    }
}
