using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Data.OleDb;
using Demiguise;
using System.Threading;

namespace Demiguise
{
    public class peerHandle
    {
        Thread peer;
        String myHost = System.Net.Dns.GetHostName();
        Socket peer_req;
        String myIp;
        Socket peer_handle;
        int flag;
        public peerHandle()
        {
        
        }
        public void start()
        {
            peer = new Thread(new ThreadStart(listen));
            peer.Start();
        
        }
        public void listen()
        {
            try
            {
                for (int i = 0; i <= System.Net.Dns.GetHostEntry(myHost).AddressList.Length - 1; i++)
                {
                    if (System.Net.Dns.GetHostEntry(myHost).AddressList[i].IsIPv6LinkLocal == false)
                    {
                        myIp = System.Net.Dns.GetHostEntry(myHost).AddressList[i].ToString();
                    }

                }

                IPEndPoint ip_end = new IPEndPoint(IPAddress.Parse(myIp), 5050);
                peer_req = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                peer_req.Bind(ip_end);

                while (true)
                {
                    
                    byte[] data = new byte[1000 * 500];
                    peer_req.Listen(100);
                    peer_handle = peer_req.Accept();
                    byte[] msg = new byte[100];
                    int recv = peer_handle.Receive(msg);
                    if (Encoding.UTF8.GetString(msg).Equals("StoreRe"))
                    {
                        MessageBox.Show("peer_running");
                        peer_req.Send(Encoding.UTF8.GetBytes("OK To Send"));
                        recv = peer_req.Receive(data);
                        Read(data, recv);
                        String rep = "OK";
                        byte[] rep_msg = Encoding.UTF8.GetBytes(rep);
                        peer_handle.Send(rep_msg);

                    }
                    else if (Encoding.UTF8.GetString(msg).Equals("Retrive"))
                    {
                        peer_req.Send(Encoding.UTF8.GetBytes("File Name"));
                        recv = peer_req.Receive(msg);
                        String fName = Encoding.UTF8.GetString(msg);
                        byte[] filename = Encoding.UTF8.GetBytes(fName);
                        String fPath = @"E:\" + fName;
                        byte[] fileData = File.ReadAllBytes(fPath);
                        byte[] send_data = new byte[4 + fName.Length + fileData.Length];
                        byte[] fileNameLen = BitConverter.GetBytes(fName.Length);
                        fileNameLen.CopyTo(send_data, 0);
                        filename.CopyTo(send_data, 4);
                        fileData.CopyTo(send_data, 4 + fName.Length);
                        peer_req.Send(send_data);

                    }
                }
            }
            catch(Exception ae)
            {
                MessageBox.Show("Peer Handler: " + ae.Message);
            }
        }
        public void Read(byte[] buff, int recv_len)
        {

            int fileNameLen = 1;
            string fileName;
            string receivedPath = null;
            String content = String.Empty;
            MessageBox.Show("Reading");
            int bytesRead = recv_len;
            if (bytesRead > 0)
            {
                if (flag == 0)
                {
                    fileNameLen = BitConverter.ToInt32(buff, 0);
                    fileName = Encoding.UTF8.GetString(buff, 4, fileNameLen);
                    receivedPath = @"E:\" + fileName;
                    flag++;
                }

                if (flag >= 1)
                {
                    BinaryWriter writer = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                    if (flag == 1)
                    {
                        writer.Write(buff, 4 + fileNameLen, bytesRead - (4 + fileNameLen));
                        flag++;
                    }
                    else
                        writer.Write(buff, 0, bytesRead);
                    writer.Close();
                }
            }
        }
    
    }
}
