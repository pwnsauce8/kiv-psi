using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace psi_1_server
{

    class Program
    {
        static IPAddress mcastAddress;
        static int mcastPort;
        static Socket mcastSocket;

        static void JoinMulticastGroup()
        {
            MulticastOption mcastOption;
            try
            {
                // Create a multicast socket.
                mcastSocket = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);

                // Get the local IP address
                IPAddress localIPAddr = Dns.GetHostAddresses(Environment.GetEnvironmentVariable("COMPUTERNAME")).Where(ia => (ia.AddressFamily == AddressFamily.InterNetwork)).First();

                // Create an IPEndPoint object.
                IPEndPoint IPlocal = new IPEndPoint(localIPAddr, 0);

                // Bind this endpoint to the multicast socket.
                mcastSocket.Bind(IPlocal);
                mcastOption = new MulticastOption(mcastAddress, localIPAddr);

                Console.WriteLine("> Multicast group is: " + mcastOption.Group);
                Console.WriteLine("> Local address is: " + mcastOption.LocalAddress);

                mcastSocket.SetSocketOption(SocketOptionLevel.IP,
                                            SocketOptionName.AddMembership,
                                            mcastOption);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.ToString());
            }
        }

        static void BroadcastMessage(string message)
        {
            IPEndPoint endPoint;

            try
            {
                //Send multicast packets to the listener.
                endPoint = new IPEndPoint(mcastAddress, mcastPort);
                mcastSocket.SendTo(ASCIIEncoding.ASCII.GetBytes(message), endPoint);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.ToString());
            }

            
        }

        static void Main(string[] args)
        {
            Console.WriteLine("> use \'exit\' to stop a server");
            // Initialize the multicast address group and multicast port.
            mcastAddress = IPAddress.Parse("230.0.0.0");
            mcastPort = 5000;

            // Join the listener multicast group.
            JoinMulticastGroup();

            // Broadcast the message to the listener.
            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (input.Equals("exit"))
                {
                    mcastSocket.Close();
                    break;
                }
                
                BroadcastMessage(input);
            }
            
        }
    }
}
