using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.OleDb;

namespace Server_Application
{
    
    class HandleClient : Server
    {
        
        Socket client_socket;
        String clientid;
        Thread t;
        public List<String> ips = new List<String>();
        OleDbConnection cnn;
                
        public void startClient(Socket s)
        {
            this.client_socket = s;
            t = new Thread(new ThreadStart(listen));
            t.Start();
          

        }
        public void listen()
        {
            try
            {
                cnn = new OleDbConnection();
                cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                IPEndPoint r = client_socket.RemoteEndPoint as IPEndPoint;
                byte [] clientid_msg = new byte[10];
                client_socket.Receive(clientid_msg);
                clientid = Encoding.UTF8.GetString(clientid_msg);
                MessageBox.Show("Client ID FROM SERVER:" + clientid);
                mapclass.client_IP_map.Add(clientid, r.Address);

                if (mapclass.client_IP_map.ContainsKey(clientid))
                  MessageBox.Show("HAS OB IN HASH");
                MessageBox.Show(client_socket.RemoteEndPoint.ToString());
                client_socket.Send(Encoding.UTF8.GetBytes("CONNECTED"));
                byte[] message = new byte[100];
                int recv_len = client_socket.Receive(message);
                if (recv_len == 7)
                {
                    String msg = Encoding.UTF8.GetString(message, 0, recv_len);
                    if (msg.Equals("StoreRe"))
                    {
                        receiveFile(client_socket);
                    }
                }
                //                conn_client.ips.Add(client_sock.RemoteEndPoint.ToString());
                else if (recv_len >7)
                {
                    String msg = Encoding.UTF8.GetString(message, 0, 7);
                    String fName = Encoding.UTF8.GetString(message,7,recv_len-7);
                     if(msg.Equals("Retrive"))
                    {
                      byte[] b = new byte[10];
                        //retriveFile(client_socket,fName,ip);
                        int rec = client_socket.Receive(b);
                    }
                }
            }

            catch (Exception e)
            {

            }

        }
        public void retriveFile(Socket client_socket,String fName,String ip)
        {
            Socket peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peer.Connect(ip, 5050);
            String req = "Retrive";
            byte[] req_msg = Encoding.UTF8.GetBytes(req);
            peer.Send(req_msg);
            peer.Receive(req_msg);
            if (Encoding.UTF8.GetString(req_msg).Equals("File Name"))
            {
                byte[] msg = Encoding.UTF8.GetBytes(fName);
                peer.Send(msg);
                byte[] clientData = new byte[1024 * 5000];
                int recv = peer.Receive(clientData);
                client_socket.Send(clientData);
            }
        }
        public void receiveFile(Socket client_socket)
        {
            try
            {
                String reply = "Send the file";
                byte[] msg = Encoding.UTF8.GetBytes(reply);
                client_socket.Send(msg);
                byte[] clientData = new byte[1024 * 5000];
                int recv_len = client_socket.Receive(clientData);
                MessageBox.Show(recv_len.ToString());
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM Status WHERE NOT ([Client Id]) = (?) ORDER BY ([Free Space]) DESC;";
                cmd.Parameters.AddWithValue("@Client Id", clientid);
                cmd.Connection = cnn;
                cnn.Open();
                OleDbDataReader reader = cmd.ExecuteReader();
                reader.Read();
                String peer_id = reader.GetString(1);
                MessageBox.Show(peer_id);
                forward(clientData, peer_id);
            }
            catch (Exception ae)
            {
                MessageBox.Show(ae.Message + " DATA ");
            }

        }
        public void forward(byte[] buffer, String peer_id)
        {
            try
            {
                int fileNameLen;
                String fileName;
                byte[] b = new byte[20];
                IPEndPoint myip = client_socket.RemoteEndPoint as IPEndPoint;
                IPAddress IpAddress;
                Socket to_peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //if (mapclass..ContainsKey(IpAddress))
                  //   MessageBox.Show("Object present");
                IpAddress = mapclass.client_IP_map[peer_id];
                to_peer.Connect(IpAddress, 5050);
                to_peer.Send(Encoding.UTF8.GetBytes("SotreRe"));
                to_peer.Receive(b);
                if (Encoding.UTF8.GetString(b).Equals("OK To Send"))
                {
                    to_peer.Send(buffer);
                    fileNameLen = BitConverter.ToInt32(buffer, 0);
                    fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                    // DATABASE//
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT  INTO Server_Table ([ClientId],[File Name],[Time Stamp],[EndClient]) VALUES (?,?,?,?) ;";
                    cmd.Parameters.AddWithValue("@ClientId", clientid);
                    cmd.Parameters.AddWithValue("@File Name", fileName);
                    cmd.Parameters.AddWithValue("@Time Stamp", System.DateTime.Now.TimeOfDay.ToString());
                    cmd.Parameters.AddWithValue("@EndClient", peer_id);

                    cmd.Connection = cnn;
                    cnn.Open();
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Server :Insert Successful");
                    to_peer.Receive(b);
                }

            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message);
            }
        }
        public void store(byte[] buffer, int length)
        {
            int fileNameLen = 1;
            int flag = 0;
            string fileName;
            string receivedPath = null;
            String content = String.Empty;
            int bytesRead = length;
            if (bytesRead > 0)
            {
                if (flag == 0)
                {
                    fileNameLen = BitConverter.ToInt32(buffer, 0);
                    fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                    receivedPath = @"F:\" + fileName;
                    flag++;
                }

                if (flag >= 1)
                {
                    BinaryWriter writer = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                    if (flag == 1)
                    {
                        writer.Write(buffer, 4 + fileNameLen, bytesRead - (4 + fileNameLen));
                        flag++;
                    }
                    else
                        writer.Write(buffer, 0, bytesRead);
                    writer.Close();
                }
            }
        }

    }
}
