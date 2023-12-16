using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Client1
{
    internal class Program
    {
        static Socket clientSocket;
        static int PORT = 1235;
        static string entry = "has joined the chat";
        static string username;

        static void Main(string[] args)
        {
            Console.WriteLine("enter username:");
            username = Console.ReadLine();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint LocalEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), PORT);

            try
            {
                clientSocket.Connect(LocalEndPoint);
                Console.WriteLine("Connected to server");
            }
            catch
            {
                Console.WriteLine("Unable to connect");
                return;
            }
            getOldMsg();
            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();

            while (clientSocket.Connected)
            {
                string msg = Console.ReadLine();
                SendMessages(msg);
            }
        }
        
        static void SendMessages(string msg)
        {
            string umsg = username+": "+ msg;

            byte[] data = Encoding.UTF8.GetBytes(umsg);
            clientSocket.Send(data);

            if (msg=="exit//")
            {
                Console.WriteLine("You Have left the chat");
                clientSocket.Close();
            }
            
        }

        static void getOldMsg()
        {
            while (true)
            {
                try
                {
                    
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    if (message == "endofold")
                    {
                        return;
                    }
                    Console.WriteLine(message);

                }
                catch
                {
                    Console.WriteLine("Failed to get old messages from server");
                    return;
                }
            }
            
        }
        static void ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    
                    byte[] buffer = new byte[1024];
                    int bytesRead = clientSocket.Receive(buffer);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);
                    if (message == "kill//")
                    {
                        clientClose();
                    }
                }
                catch
                {
                    Console.WriteLine("Disconnected from server");
                    return;
                }
            }
            Console.WriteLine("server closed app closing");
            Environment.Exit(0);
        }
            
        static void clientClose()
        {
            Console.WriteLine("Server is Getting Shutdown");
            Thread.Sleep(2000);

            clientSocket.Close();  
            Environment.Exit(0);
        }

    }
}