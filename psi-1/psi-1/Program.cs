using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace psi_1
{
    class Program
    {
        private static IPAddress mcastAddress;
        private static int mcastPort;
        private static Socket mcastSocket;
        private static MulticastOption mcastOption;


        private static void GetInfo()
        {
            Console.WriteLine("> Multicast group is: " + mcastOption.Group);
            Console.WriteLine("> Local address is: " + mcastOption.LocalAddress);
        }


        private static void StartMulticast()
        {

            try
            {
                mcastSocket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);

                // Get local adress
                IPAddress localIPAddr = Dns.GetHostAddresses(Environment.GetEnvironmentVariable("COMPUTERNAME")).Where(ia => (ia.AddressFamily == AddressFamily.InterNetwork)).First();

                EndPoint localEP = (EndPoint)new IPEndPoint(localIPAddr, mcastPort);

                mcastSocket.Bind(localEP);

                mcastOption = new MulticastOption(mcastAddress, localIPAddr);

                mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                                            SocketOptionName.AddMembership,
                                            mcastOption);
            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReceiveBroadcastMessages()
        {
            byte[] bytes = new Byte[100];
            IPEndPoint groupEP = new IPEndPoint(mcastAddress, mcastPort);
            EndPoint remoteEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            try
            {
                while (true)
                {
                    bytes = new Byte[100];
                    mcastSocket.ReceiveFrom(bytes, ref remoteEP);
                    string input = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                    Console.Write("{0} > {1}\n",
                      groupEP.ToString(),
                      input);
                }
                mcastSocket.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Main(String[] args)
        {
            Console.WriteLine("> use \'exit\' to stop a client");

            // Initialize the multicast address group and multicast port.
            mcastAddress = IPAddress.Parse("230.0.0.0");
            mcastPort = 5000;

            // Start a multicast group.
            StartMulticast();

            GetInfo();

            // Receive broadcast messages.
            ReceiveBroadcastMessages();
        }
    }
}
