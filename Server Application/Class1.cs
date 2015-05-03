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
        private Dictionary<String, IPAddress> client_IP_map = new Dictionary<String, IPAddress>();
        private ReaderWriterLockSlim thislock = new ReaderWriterLockSlim();
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
                MessageBox.Show("Inside Listen");
                 byte[] clientid_msg = new byte[10];
                client_socket.Receive(clientid_msg);
                IPEndPoint r = client_socket.RemoteEndPoint as IPEndPoint;
                clientid = Encoding.UTF8.GetString(clientid_msg);
                MessageBox.Show(clientid);
                cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                client_socket.Send(Encoding.UTF8.GetBytes("CONNECTED"));
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM Status WHERE ([Client Id]) = (?);";
                cmd.Parameters.AddWithValue("@Client Id", clientid);
                cmd.Connection = cnn;
                cnn.Open();
                OleDbDataReader reader = cmd.ExecuteReader();
                if(!reader.Read())
                {
                    reader.Close();
                    cmd = new OleDbCommand();
                    MessageBox.Show("as");
                    OleDbCommand cmd1 = new OleDbCommand();
                    cmd.CommandText = "INSERT INTO Status ([Client Id],[Free Space],[IP]) VALUES (?,?,?) ;";
                    cmd.Parameters.AddWithValue("@Client Id", clientid);
                    int val = 300;
                    cmd.Parameters.AddWithValue("@Free Space", val.ToString());
                    cmd.Parameters.AddWithValue("@IP", r.Address.ToString());
                    
                    cmd.Connection = cnn;
                    cmd.ExecuteNonQuery();
                    MessageBox.Show("e");
                 }
                else
                {
                    reader.Close();
                    cmd = new OleDbCommand();
                    cmd.Parameters.AddWithValue("@IP", r.Address.ToString());
                    cmd.Parameters.AddWithValue("@Client Id", clientid);
                    
                    cmd.CommandText = "UPDATE Status SET [IP] = ? WHERE [Client Id] = ?;";
                    cmd.Connection = cnn;
                    cmd.ExecuteNonQuery();
                     
                    
                }
                //MesssageBox.Show("Client ID FROM SERVER:" + clientid);
                //Server.client_IP_map.Add(clientid, r.Address);
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
                            retriveFile(client_socket,fName,"104.39.4.52");
                            
                        }
                     else
                     {
                         deleteFile(client_socket, fName, "104.39.4.52");
                     }
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
            finally
            {
                cnn.Close();

            }
        }
        public void deleteFile(Socket client_socket,String fName,String ip)
        {
            Socket peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peer.Connect(ip, 5050);
            String req = "Deletee";
            byte[] req_msg = Encoding.UTF8.GetBytes(req);
            peer.Send(req_msg);
            peer.Receive(req_msg);
            if (Encoding.UTF8.GetString(req_msg).Equals("File Na"))
            {
                byte[] msg = Encoding.UTF8.GetBytes(fName);
                int fn_len = msg.Length;
                peer.Send(Encoding.UTF8.GetBytes(fn_len.ToString()));
                peer.Send(msg);
                byte[] b = new byte[2];
                int recv = peer.Receive(b);
                if(Encoding.UTF8.GetString(b).Equals("OK"))
                    client_socket.Send(b);
                else
                    b = Encoding.UTF8.GetBytes("NO");
            }
           
        }
        public void retriveFile(Socket client_socket,String fName,String ip)
        {
            client_socket.Send(Encoding.UTF8.GetBytes("Waitttt"));
            Socket peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            peer.Connect(ip, 5050);
            String req = "Retrive";
            byte[] req_msg = Encoding.UTF8.GetBytes(req);
            peer.Send(req_msg);
            peer.Receive(req_msg);
            if (Encoding.UTF8.GetString(req_msg).Equals("File Na"))
            {
                byte[] msg = Encoding.UTF8.GetBytes(fName);
                int fn_len = msg.Length;
                peer.Send(Encoding.UTF8.GetBytes(fn_len.ToString()));
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
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM Status WHERE NOT ([Client Id]) = (?) ORDER BY ([Free Space]) DESC;";
                cmd.Parameters.AddWithValue("@Client Id", clientid);
                cmd.Connection = cnn;
                cnn.Open();
                OleDbDataReader reader = cmd.ExecuteReader();
                String peer_id=null;
                String free_sapce=null;
                String ip_peer= null;
                IPAddress IpAddress = null;
               while (reader.Read())
                {
                    peer_id = reader.GetString(1);
                    free_sapce = reader.GetString(2);
                    ip_peer = reader.GetString(3);
                   if (!peer_id.Equals(clientid))
                    {
                        
                        break;

                    }
                }
   //             IpAddress = mapclass.client_IP_map[peer_id];
            
                forward(clientData, ip_peer, free_sapce, peer_id);
            }
            catch (Exception ae)
            {
                MessageBox.Show(ae.Message + " DATA ");
            }

        }
        public void forward(byte[] buffer, String ip_peer, String free_space, String peer_id)
        {
            try
            {
                
                int fileNameLen;
                String fileName;
                byte[] b = new byte[7];
                IPEndPoint myip = client_socket.RemoteEndPoint as IPEndPoint;
                Socket to_peer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress IpAddress = IPAddress.Parse(ip_peer);
                to_peer.Connect(IpAddress, 5050);
                MessageBox.Show(to_peer.Connected ? "Connected to peer" : "peer Not Listening");
                to_peer.Send(Encoding.UTF8.GetBytes("StoreRe"));
                to_peer.Receive(b);
                String t = Encoding.UTF8.GetString(b);
                if (t.Equals("OK To S"))
                {
                    MessageBox.Show("SERVER MESSAGE: Peer Running");
                    to_peer.Send(buffer);
                    to_peer.Receive(b);
                    fileNameLen = BitConverter.ToInt32(buffer, 0);
                    fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                    Double n_free = Double.Parse(free_space);
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
                    cmd.CommandText = "UPDATE Status SET [Free Space]= ? WHERE [Client Id] = ?;";
                    n_free = n_free-buffer.Length;
                    cmd.Parameters.AddWithValue("@Free Space", n_free.ToString());
                    cmd.Parameters.AddWithValue("@Client Id", peer_id);
                    
                }
                else
                {
                    MessageBox.Show("Denied by peer");
                }
            }
            catch(Exception ae)
            {
                MessageBox.Show("Server:" + ae.Message);
            }
            finally
            {
                cnn.Close();
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
