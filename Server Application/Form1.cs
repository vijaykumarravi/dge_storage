using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Data.OleDb;
using System.Configuration;

//server
namespace Server_Application
{
    public partial class Server : Form
    {
        Thread t;
        String myIp=null;
        Socket server;
        Socket client_sock;
        string myHost = System.Net.Dns.GetHostName();
        Connected conn_client;
        //TcpListener server = new TcpListener(8080);
        //TcpClient client = default(TcpClient);
        public Server()
        {
            InitializeComponent();
        
            // GET IP OF SERVER //
            for (int i = 0; i <= System.Net.Dns.GetHostEntry(myHost).AddressList.Length - 1; i++)
            {
                if (System.Net.Dns.GetHostEntry(myHost).AddressList[i].IsIPv6LinkLocal == false)
                {
                    myIp = System.Net.Dns.GetHostEntry(myHost).AddressList[i].ToString();
                }

            }
            MessageBox.Show(myIp);
            IPEndPoint ip_end = new IPEndPoint(IPAddress.Parse(myIp), 8080);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            server.Bind(ip_end);
                    
            while (true)
            {
                    server.Listen(100);
                    client_sock = server.Accept();
                    t = new Thread(new ThreadStart(listen));
                    t.Start();
        
            }
            conn_client = new Connected();
            //    server.Start();
          //  listen();
           }

        public void listen()
        {
                try
                {
                    MessageBox.Show("COOOO");
                    MessageBox.Show(client_sock.RemoteEndPoint.ToString());

                    conn_client.ips.Add(client_sock.RemoteEndPoint.ToString());
                    byte[] clientData = new byte[1024 * 5000];
                    int recv_len = client_sock.Receive(clientData);

                    //data ob = new data();
                    //ob.length = client_sock.Receive(ob.buffer);
                    MessageBox.Show(recv_len.ToString());
                    store(clientData,recv_len);
                    //forward(clientData);
                    //    client_sock.Send(clientData);
                }

                catch (Exception e)
                {

                }

            }
        
        public void forward(byte [] buffer)
        {
            Socket peer= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);


        }
        public void store(byte [] buffer,int length)
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
        private void button1_Click(object sender, EventArgs e)
        {
           MessageBox.Show("Listening");
        }
    }
}

