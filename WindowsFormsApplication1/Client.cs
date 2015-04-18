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

namespace WindowsFormsApplication1
{
    
       
    //sdasdasd
    
    public partial class Client : Form
    {
        public String fName;
        public String s_fName;
        Socket client_sock; 
        String ip;
        String connetionString = null;
        OleDbConnection cnn;
        int flag =0;
        FileList flform;
        public Client()
        {
            InitializeComponent();
            OpenFileDialog ofd = new OpenFileDialog();
            StatusStrip statusStrip1 = new StatusStrip();
            statusStrip1.Text = "Select a File";
            statusStrip1.Refresh();
            client_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            cnn = new OleDbConnection();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Socket serv_socket = client_sock;
                //serv_socket.Listen(100);
                //serv_socket.Accept();
                byte[] servData = new byte[1024 * 5000];
                int recv_len = serv_socket.Receive(servData);
                Read(servData, recv_len);

                /*try
                {
                    SocketFlags flag;
                    client_sock.Connect("104.39.6.191", 8080);
                    //byte[] message = System.Text.Encoding.UTF8.GetBytes("Request");
                    //client_sock.Send(message);
                   // StateObject state = new StateObject();
                    //state.workSocket = client_sock;
                    //client_sock.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    int BufferSize = 8096;
                    byte [] buff = new byte[BufferSize];
                    Console.WriteLine("BEFORE RECEIVE IN PEER");
                    client_sock.Receive(buff);
                    MessageBox.Show("received in client");
                    String res = null;
                    Read(buff);
                }
                catch (Exception ae)
                {
                    Console.WriteLine(ae.Message.ToString());
                }*/
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message);
            }
            }

        public void Read(byte [] buff,int recv_len)
        {
            
            int fileNameLen = 1;
            string fileName;
            string receivedPath = null;
            String content = String.Empty;
          //StateObject state = (StateObject)ar.AsyncState;
            //StateObject state = ob;
            //Socket handler = state.workSocket;
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

        private void button2_Click(object sender, EventArgs e)
        {
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                fName = ofd.FileName;
                textBox1.Text = ofd.SafeFileName;
                s_fName = textBox1.Text;

            }


        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            statusStrip1.Text = "File Selected";
            statusStrip1.Refresh();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //client_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //client_sock.Connect(ip, 8080);
                byte[] filename = Encoding.UTF8.GetBytes(s_fName);
                byte[] fileData = File.ReadAllBytes(fName);
                byte[] data = new byte[4 + fName.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(s_fName.Length);
                fileNameLen.CopyTo(data, 0);
                filename.CopyTo(data, 4);
                fileData.CopyTo(data, 4 + s_fName.Length);
                client_sock.Send(data);
                String current_dir = System.Environment.CurrentDirectory;
                Console.WriteLine(current_dir);
                cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay\Documents\GitHub\dge_storage\WindowsFormsApplication1\client_db.mdb";
                
                // DATABASE//
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "INSERT  INTO client_table ([File Name]) VALUES (?) ;";
                cmd.Parameters.AddWithValue("@File Name", s_fName);
                cmd.Connection = cnn;
                cnn.Open();
                cmd.ExecuteNonQuery();
               
                MessageBox.Show("Insert Successful");
              
                
            }
                
            catch (Exception s)
            {
                MessageBox.Show(s.Message);
            }

        finally
            {
                cnn.Close();
                
            }
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            try
            {


                ip = textBox2.Text;
                client_sock.Connect(ip, 8080);
                MessageBox.Show(client_sock.Connected? "Connected to the Server" : "Servedr Not Listening" );
               
            }
            catch(SocketException ae)
            {
                MessageBox.Show("Server not listening");
            }
                
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            client_sock.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            flform = new FileList();
            flform.ShowDialog();
            textBox1.Text=flform.result;
        }
    }
     /*public class StateObject
            {
                // Client socket.
                public Socket workSocket = null;

                public const int BufferSize = 8096;
                // Receive buffer.
                public byte[] buffer = new byte[BufferSize];
            }*/
}
