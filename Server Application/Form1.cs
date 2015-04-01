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
        Thread t1;
        int flag = 0;
        string receivedPath = "nil";
        string myHost = System.Net.Dns.GetHostName();
        string myIP = null;
        public delegate void MyDelegate();
        private string fileName;
        public Server()
        {
            t1 = new Thread(new ThreadStart(StartListening));
            t1.Start();
            InitializeComponent();

            for (int i = 0; i <= System.Net.Dns.GetHostEntry(myHost).AddressList.Length - 1; i++)
            {
                if (System.Net.Dns.GetHostEntry(myHost).AddressList[i].IsIPv6LinkLocal == false)
                {
                    myIP = System.Net.Dns.GetHostEntry(myHost).AddressList[i].ToString();
                }
            }
          }


        public class StateObject
        {
            // Client socket.
            public Socket workSocket = null;

            public const int BufferSize = 8096;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
        }

        public static ManualResetEvent allDone = new ManualResetEvent(true);

        public void StartListening()
        {
            try
            {
                byte[] bytes = new Byte[8096];

                // WebClient webClient = new WebClient();
                //string IP = webClient.DownloadString("http://myip.ozymo.com/");
               // MessageBox.Show(myIP);
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 8080);
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                try
                {
                    listener.Bind(ipEnd);
                    listener.Listen(100);
                    //SetText("Listening For Connection");//.net framework 4.5
                    while (true)
                    {
                        allDone.Reset();
                        listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                        allDone.WaitOne();

                    }
                }
                catch (Exception ex)
                {
                }
            }
            catch (Exception e) 
            {
                Console.WriteLine("Exception");
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();

            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
            flag = 0;

        }

        public void ReadCallback(IAsyncResult ar)
        {
            int fileNameLen = 1;
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;
            int bytesRead = handler.EndReceive(ar);
            if (bytesRead > 0)
            {
                if (flag == 0)
                {
                    fileNameLen = BitConverter.ToInt32(state.buffer, 0);
                    fileName = Encoding.UTF8.GetString(state.buffer, 4, fileNameLen);
                    receivedPath = @"C:\Users\pragathi\" + fileName;
                    flag++;
                }

                if (flag >= 1)
                {
                    BinaryWriter writer = new BinaryWriter(File.Open(receivedPath, FileMode.Append));
                    if (flag == 1)
                    {
                        writer.Write(state.buffer, 4 + fileNameLen, bytesRead - (4 + fileNameLen));
                        flag++;
                    }
                    else
                        writer.Write(state.buffer, 0, bytesRead);
                    writer.Close();

                   // InsertNewFile(state.buffer);

                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                }
            }
            else
            {
                Invoke(new MyDelegate(LabelWriter));
            }

        }

        public void LabelWriter()
        {
            label1.Text = "Data has been received " + fileName;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            t1.Abort();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private Int32 InsertNewFile(byte[] fileBytes)
        {
            string connetionString = null;
            OleDbConnection cnn;
            connetionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\DEMIGUISE\DEMIGUISE.mdb";
            cnn = new OleDbConnection(connetionString);
            Int32 fileID = 1;
            try
            {
                // open the data connection
                cnn.Open();

                //Create a command
                OleDbCommand nonqueryCommand = cnn.CreateCommand();

                nonqueryCommand.CommandText = "INSERT  INTO tblFile ([File_Template], [Entered_Date], [Entered_By]) VALUES (?,?,?)";

                nonqueryCommand.Parameters.AddWithValue("FileTemplate", fileBytes);
                nonqueryCommand.Parameters.AddWithValue("EnteredDate", DateTime.Now);
                nonqueryCommand.Parameters.AddWithValue("EnteredBy", "Server");

                nonqueryCommand.ExecuteNonQuery();

                cnn.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while saving the file to the database. Error Message:" + ex.Message);
            }

            return fileID;
        }
    }
}

