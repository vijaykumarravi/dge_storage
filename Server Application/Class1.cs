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
                cnn.Close();
                //MesssageBox.Show("Client ID FROM SERVER:" + clientid);
                while(true)
                { 
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

                    if (msg.Equals("Retrive"))
                    {
                        cnn = new OleDbConnection();
                        cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                        byte[] b = new byte[10];
                        cmd = new OleDbCommand();
                        cmd.CommandText = "SELECT * FROM Server_Table  WHERE [File Name] = ?;";
                        cmd.Parameters.AddWithValue("@File Name", fName);
                        cmd.Connection = cnn;
                        cnn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show(fName);
                        OleDbDataReader read = cmd.ExecuteReader();
                        read.Read();
                        String peer_id = read.GetString(4);
                        read.Close();
                        cnn.Close();
                        OleDbConnection conn = new OleDbConnection();
                        conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                        cmd = new OleDbCommand();
                        cmd.CommandText = "SELECT * FROM Status  WHERE [Client Id] = ?;";
                        cmd.Parameters.AddWithValue("@Client Id", peer_id);
                        cmd.Connection = conn;
                        conn.Open();
                        OleDbDataReader rd = cmd.ExecuteReader();
                        rd.Read();
                        String ip_peer = rd.GetString(3);
                        rd.Close();
                        conn.Close();
                        retriveFile(client_socket, fName, ip_peer);

                    }
                    else
                    {
                        cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                        cmd = new OleDbCommand();
                        cmd = new OleDbCommand();
                        cmd.CommandText = "SELECT * FROM Server_Table  WHERE [File Name] = ?;";
                        cmd.Parameters.AddWithValue("@File Name", fName);
                        cnn.Open();
                        cmd.Connection = cnn;
                        cmd.ExecuteNonQuery();
                        OleDbDataReader read = cmd.ExecuteReader();
                        read.Read();
                        String peer_id = read.GetString(4);
                        read.Close();
                        cnn.Close();
                        OleDbConnection conn = new OleDbConnection();
                        conn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                        cmd = new OleDbCommand();
                        cmd.CommandText = "SELECT * FROM Status  WHERE [Client Id] = ?;";
                        cmd.Parameters.AddWithValue("@Client Id", peer_id);
                        cmd.Connection = conn;
                        conn.Open();
                        OleDbDataReader rd = cmd.ExecuteReader();
                        rd.Read();
                        String ip_peer = rd.GetString(3);
                        //rd.Close();
                        deleteFile(client_socket, fName, ip_peer);
                        cmd = new OleDbCommand();
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE * FROM Server_Table  WHERE [File Name] = ? ;";
                        cmd.Parameters.AddWithValue("@File Name", fName);
                        cmd.ExecuteNonQuery();
                        conn.Close();
                    }     
                     }
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
                MessageBox.Show(e.ToString());
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
                byte[] b = new byte[100];
                byte[] msg = Encoding.UTF8.GetBytes(fName);
                int fn_len = msg.Length;
                peer.Send(Encoding.UTF8.GetBytes(fn_len.ToString()));
                peer.Send(msg);
                peer.Receive(b); // Receive the Length of the Data
                byte[] clientData = new byte[int.Parse(Encoding.UTF8.GetString(b))];
                int totalbytesread = 0;
                byte[] temp = new byte[8192];
                while (totalbytesread < int.Parse(Encoding.UTF8.GetString(b)))
                {
                    int recv_len = client_socket.Receive(temp);
                    Buffer.BlockCopy(temp, 0, clientData, totalbytesread, recv_len);
                    totalbytesread += recv_len;

                }
                client_socket.Send(b);
                client_socket.Send(clientData);
            }
        }
        public void receiveFile(Socket client_socket)
        {
            try
            {
                byte [] b = new byte[1000];
                String reply = "Send the file";
                byte[] msg = Encoding.UTF8.GetBytes(reply);
                client_socket.Send(msg);
                client_socket.Receive(b);
                int len = int.Parse(Encoding.UTF8.GetString(b));
                MessageBox.Show("FILE LENGTH"+len.ToString());
                Byte [] clientData = new byte[len];
                byte [] temp = new byte[8192];
                int totalbytesread = 0;
                while (totalbytesread < len)
                {
                    int recv_len = client_socket.Receive(temp);
                    Buffer.BlockCopy(temp, 0, clientData, totalbytesread, recv_len);
                    totalbytesread += recv_len;
                    
                }
                cnn = new OleDbConnection();
                cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT * FROM Status ORDER BY ([Free Space]) DESC;";
                // Load Balancing
                cmd.Connection = cnn;
                cnn.Open();
                OleDbDataReader reader = cmd.ExecuteReader();
                String peer_id = null;
                String free_sapce = null;
                String ip_peer = null;
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
                MessageBox.Show(ip_peer);
               forward(clientData, ip_peer, free_sapce, peer_id,len);
                store(clientData, totalbytesread);
                cnn.Close();
            }
            catch (Exception ae)
            {
                MessageBox.Show(ae.Message + " DATA ");
            }

        }
        public void forward(byte[] buffer, String ip_peer, String free_space, String peer_id, int len)
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
                    to_peer.Send(Encoding.UTF8.GetBytes(len.ToString()));
                    to_peer.Send(buffer);
                    to_peer.Receive(b);
                    fileNameLen = BitConverter.ToInt32(buffer, 0);
                    fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                    Double n_free = Double.Parse(free_space);
                    OleDbCommand cmd = new OleDbCommand();
                    cnn = new OleDbConnection();
                    cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";
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
            int fileNameLen = 0;
            int flag = 0;
            string fileName;
            string receivedPath = null;
            String content = String.Empty;
            int bytesRead = buffer.Length;
            if (bytesRead > 0)
            {
                if (flag == 0)
                {
                    
                    MessageBox.Show("f");
                    fileNameLen = BitConverter.ToInt32(buffer, 0);
                    fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                    receivedPath = @"D:\" + fileName;
                    flag++;
                }

                if (flag >= 1)
                {
                    MessageBox.Show("SAS"+buffer.Length.ToString());
                    byte[] data = new byte[buffer.Length - 4 - fileNameLen];
                    MessageBox.Show("LENGTH:" + length.ToString());
                    Array.Copy(buffer, ( 4 + fileNameLen), data, 0, (length - 4 - fileNameLen));
                    MessageBox.Show("DATA LENGTH:"+data.Length.ToString());
                    BinaryWriter WR = new BinaryWriter(File.OpenWrite(receivedPath));
                    WR.Write(data);
                    WR.Flush();
                    WR.Close();

                }
            }
        }

    }
}
