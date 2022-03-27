using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace psi_2
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server("127.0.0.1", 5000);
            Thread t = new Thread(() => server.Run());
            t.Start();

            Console.ReadLine();
        }
    }

    class Server
    {
        private TcpListener _tcpListener = null;
        private string _ip;
        private int _port;

        public Server(string ip, int port)
        {
            _ip = ip;
            _port = port;
        }

        public void Run()
        {
            if (_ip == string.Empty)
            {
                return;
            }

            _tcpListener = new TcpListener(IPAddress.Parse(_ip), _port);
            _tcpListener.Start();
            Console.WriteLine("> Server started");

            try
            {
                while (true)
                {
                    TcpClient client = _tcpListener.AcceptTcpClient();
                    Console.WriteLine("> Client connected");

                    Thread t = new Thread(new ParameterizedThreadStart(ReadCallback));
                    t.Start(client);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"{nameof(SocketException)}: {ex.Message}");
                _tcpListener.Stop();
            }
        }

        public void ReadCallback(object obj)
        {
            TcpClient client = (TcpClient)obj;
            var stream = client.GetStream();

            byte[] bytes = new byte[256];

            try
            {
                // Read data from the client socket.
                int i = stream.Read(bytes, 0, bytes.Length);

                string data = Encoding.ASCII.GetString(bytes, 0, i);
                string responseContent = Regex.Match(data, @"(?<=(GET )).+(?=( HTTP\/1\.1))").Value;
                Console.WriteLine($"#{Thread.CurrentThread.ManagedThreadId} thread > Received: {responseContent}");

                // Echo the data back to the client. 
                string message = $"<p style=\"text-align:center;\">&nbsp;</p>" +
                    $"<p style=\"text-align:center;\">&nbsp;</p>" +
                    $"<p style=\"text-align:center;\">&nbsp;</p>" +
                    $"<p style=\"text-align: center;\"><span style=\"color: #ff0000;\"><strong>Welcome to <span style=\"color: #000000;\">'{responseContent}'</span></strong></span></p>";
                
                Send(stream, message);
                Console.WriteLine($"#{Thread.CurrentThread.ManagedThreadId} thread > Sent: {message}");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.GetType()}: {ex.Message}");
                client.Close();
            }
        }

        private void Send(NetworkStream stream, string message)
        {
            var writer = new StreamWriter(stream);
            writer.Write("HTTP/1.0 200 OK");
            writer.Write(Environment.NewLine);
            writer.Write("Content-Type: text/html; charset=UTF-8");
            writer.Write(Environment.NewLine);
            writer.Write("Content-Length: " + message.Length);
            writer.Write(Environment.NewLine);
            writer.Write(Environment.NewLine);
            writer.Write(message);
            writer.Flush();
        }
    }
}
