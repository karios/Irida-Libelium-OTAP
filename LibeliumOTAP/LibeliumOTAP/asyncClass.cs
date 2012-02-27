using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace LibeliumOTAP
{
    public class SynchronousSocketListener
    {

        // Incoming data from the client.
        public string data = null;
        static public bool keepGoing = true;
        UdpClient udpClient;

        public void initPort(int portnumber)
        {
            udpClient = new UdpClient(portnumber, AddressFamily.InterNetwork);
        }

        private byte[] stringToBytes(string sourceString)
        {

            BinaryFormatter bf = new BinaryFormatter();
            byte[] bytes;
            MemoryStream ms = new MemoryStream();

            string orig = sourceString;
            bf.Serialize(ms, orig);
            ms.Seek(0, 0);
            bytes = ms.ToArray();

            return bytes;

        }

        internal object StartListening(List<string> commandsBuffer)
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];
            IPEndPoint riep = new IPEndPoint(IPAddress.Any, 0);

            while (keepGoing)
            {
                udpClient.Client.ReceiveTimeout = 1000; //times out after 1 second to kill thread if needed
                try
                {
                    Byte[] receiveBytes = udpClient.Receive(ref riep);
                    string receiveString = Encoding.ASCII.GetString(receiveBytes);

                    commandsBuffer.Add(receiveString.TrimEnd('\r', '\n'));
                    //form1.processCommands();
                    //IPEndPoint ipEndPoint = riep;
                    //byte[] dgram = Encoding.ASCII.GetBytes("Received:" + receiveString);
                    //form1.setStatus("New command:" + receiveString.TrimEnd('\r', '\n'));

                    //udpClient.Send(dgram, dgram.Count(), ipEndPoint);
                }
                catch
                {
                }

            }

            return null;
        }

        

        
    }
}
