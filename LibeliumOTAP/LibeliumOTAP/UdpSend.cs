using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace LibeliumOTAP
{
    /// <summary>
    /// (Singleton), send messages over the network.
    /// </summary>
    public sealed class UdpSend
    {

        private static readonly UdpSend instance = new UdpSend();
        private UdpClient udpClient;

        public static UdpSend Instance
        {
            get
            {
                return instance;
            }
        }

        public UdpSend()
        {
            Console.WriteLine("UDPSend Initialized");
            this.udpClient = new UdpClient();
        }


        /// <summary>
        /// Callback after sending bytes.
        /// </summary>
        /// <param name="ar"></param>
        public void SendCallback(IAsyncResult ar)
        {
            Console.WriteLine("number of bytes sent: {0}", udpClient.EndSend(ar));

        }

        /// <summary>
        /// Send a UDP message to a specific port and address
        /// </summary>
        /// <param name="server"></param>
        /// <param name="port"></param>
        /// <param name="message"></param>
        /// 
        public void SendMessage(string ip, int port, string message)
        {
            // create the udp socket

            Byte[] sendBytes = Encoding.ASCII.GetBytes(message);

            // send the message
            // the destination is defined by the IPEndPoint

            udpClient.BeginSend(sendBytes, sendBytes.Length, ip, port, new AsyncCallback(SendCallback), null);


        }



    }
}
