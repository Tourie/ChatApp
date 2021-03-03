using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.IO;

namespace ChatApp
{
    internal class Program
    {
        public static string IpAddress { get; } = "127.0.0.1";
        public static Person CurPerson { get; set; }
        public static Socket ListeningSocket { get; set; }
        public static List<string> SessionMessages { get; set; }
        public static State State { get; set; } = State.pending;

        public static void Main(string[] args)
        {
            SessionMessages = new();

            try
            {
                Console.WriteLine("Input your local port: ");
                int localPort = int.Parse(Console.ReadLine() ?? throw new ArgumentException());
                Console.WriteLine("Input remote port: ");
                int remotePort = int.Parse(Console.ReadLine() ?? throw new ArgumentException());

                IPEndPoint localIpEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), localPort);
                IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Parse(IpAddress), remotePort);

                string uid = Guid.NewGuid().ToString();
                CurPerson = new Person(uid, localIpEndPoint, remoteIpEndPoint);

                ListeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                ListeningSocket.Bind(CurPerson.LocalIp);

                Console.WriteLine($"Hi! Your unique identifier: {CurPerson.Id}, and endpoint: {CurPerson.LocalIp}. RemoteEndPoint: {CurPerson.RemoteIp}");
            
                Task listening = new Task(TryConnect);
                listening.Start();
                Task checker = new Task(Check);
                checker.Start();

                while (true)
                {
                    string message = Console.ReadLine();
                    Console.WriteLine();
                    if (message == "exit") break;
                    message = CurPerson.Id + " - " + message+"\n";
                    SessionMessages.Add(message);
                    Send(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
            finally
            {
                Close();
            }
        }

        static void Send(string msg)
        {

            byte[] data = Encoding.Unicode.GetBytes(msg);
            var endPoint = CurPerson.RemoteIp as EndPoint;
            ListeningSocket?.SendTo(data, endPoint);
        }

        static void Listening()
        {
            try
            {
                string msg = CurPerson.Id + " joined!\n";
                Send(msg);
                RecreateHistory();

                while (true)
                {
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    byte[] data = new byte[256];

                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, CurPerson.RemoteIp.Port);
                    do
                    {
                        bytes = ListeningSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                        
                    } while (ListeningSocket.Available > 0);

                    if(State == State.pending)
                    {
                        State = State.active;
                        RecreateHistory();
                    }

                    IPEndPoint fullEndPoint = remoteIp as IPEndPoint;
                    if (fullEndPoint?.Port == CurPerson.RemoteIp.Port && builder.ToString() != String.Empty)
                    {
                        SessionMessages.Add(builder.ToString());
                        Console.WriteLine(builder.ToString());
                    }
                }
            }
            catch (Exception)
            {
                State = State.pending;
                //Console.WriteLine("Client is off line, trying to connect...");
            }
        }

        private static void Close()
        {
            if (ListeningSocket != null)
            {
                ListeningSocket.Shutdown(SocketShutdown.Both);
                ListeningSocket.Close();
                ListeningSocket = null;
            }
        }

        public static void TryConnect()
        {
            while (true)
            {
                Listening();
            }
        }

        public static void RecreateHistory()
        {
            foreach (var msg in SessionMessages)
            {
                Send(msg);
            }
        }

        public static void Check()
        {
            while (true)
            {
                Send("");
            }
        }
    }
}
