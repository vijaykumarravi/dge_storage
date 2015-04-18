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
        Thread t;
        public List<String> ips = new List<String>();
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
                MessageBox.Show("COOOO");
                MessageBox.Show(client_socket.RemoteEndPoint.ToString());

//                conn_client.ips.Add(client_sock.RemoteEndPoint.ToString());
                byte[] clientData = new byte[1024 * 5000];
                int recv_len = client_socket.Receive(clientData);

                MessageBox.Show(recv_len.ToString());
                forward(clientData);
                //    client_sock.Send(clientData);
            }

            catch (Exception e)
            {

            }

        }
        public void forward(byte[] buffer)
        {
            try
            {
                int fileNameLen;
                String fileName;
                String connetionString = null;
                OleDbConnection cnn = new OleDbConnection();
                IPEndPoint myip = client_socket.RemoteEndPoint as IPEndPoint;
                IPAddress IpAddress = IPAddress.Parse("75.102.77.180");
                store(buffer, buffer.Length);
                /*Socket connectedlist;
                if (mapclass.map.ContainsKey(IpAddress))
                     MessageBox.Show("Object present");
                connectedlist = mapclass.map[IpAddress];
                connectedlist.Send(buffer);*/
                fileNameLen = BitConverter.ToInt32(buffer, 0);
                fileName = Encoding.UTF8.GetString(buffer, 4, fileNameLen);
                cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay\Documents\GitHub\dge_storage\Server Application\Server_Database.mdb";

                // DATABASE//
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT  INTO Server_Table ([ClientId],[File Name],[Time Stamp],[EndClient]) VALUES (?,?,?,?) ;";
                cmd.Parameters.AddWithValue("@ClientId", myip.ToString());
                cmd.Parameters.AddWithValue("@File Name", fileName);
                cmd.Parameters.AddWithValue("@Time Stamp", System.DateTime.Now.TimeOfDay.ToString());
                cmd.Parameters.AddWithValue("@EndClient", IpAddress.ToString());

                cmd.Connection = cnn;
                cnn.Open();
                cmd.ExecuteNonQuery();

                MessageBox.Show("Insert Successful");
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
            //  StateObject state = (StateObject)ar.AsyncState;
            //StateObject state = ob;
            //Socket handler = state.workSocket;
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
