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


namespace WindowsFormsApplication1
{
    
       
   
    
    public partial class Client : Form
    {
   
        public String fName;
        private String clientid;
        public String s_fName;
        Socket client_sock; 
        String ip;
        String connetionString = null;
        OleDbConnection cnn;
        int flag =0;
        FileList flform;
        String retFName;
        Boolean send_request;
        Boolean connect;
        peerHandle peer_ob;
        public Client()
        {
            try
            {
                
                send_request = false;
                connect = false;
                clientid = "5";
                retFName = null;
                InitializeComponent();
                peer_ob = new peerHandle();
                peer_ob.start();
                OpenFileDialog ofd = new OpenFileDialog();
                client_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                cnn = new OleDbConnection();
                cnn.ConnectionString = @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\Users\Vijay Kumar Ravi\Documents\GitHub\dge_storage\WindowsFormsApplication1\client_db.mdb";
                textBox1.Text = null;
                textBox2.Text = null;
                ip = null;
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message);
            }
        }
        
        
        
    public void Read(byte[] buff, int recv_len)
        {
            
            int fileNameLen = 1;
            string fileName;
            string receivedPath = null;
            String content = String.Empty;
            int bytesRead = recv_len;
            if (bytesRead > 0)
            {
                if (flag == 0)
                {
                    fileNameLen = BitConverter.ToInt32(buff, 0);
                    fileName = Encoding.UTF8.GetString(buff, 4, fileNameLen);
                    receivedPath = @"E:\DGE\"+fileName;
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
    

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (connect)
                {
                    String req = "StoreRe";
                    byte[] msg = Encoding.UTF8.GetBytes(req);
                    client_sock.Send(msg);
                    int recv_le = client_sock.Receive(msg);
                    String repl = Encoding.UTF8.GetString(msg, 0, recv_le);
                    send_request = true;
                    if (repl.Equals("Send th"))
                    {
                        MessageBox.Show("Select the file and click Send to Sore the File");
                    }
                    else
                    {
                        MessageBox.Show("Server denied the request");
                    }
                }
                else
                {
                    MessageBox.Show("Connect to the Server First");
                }
            }
            catch(Exception ae)
            {
                MessageBox.Show(ae.Message);
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
           
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (send_request & connect)
                {
                    if (fName != null)
                    {
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
                        byte [] repl = new byte[7];
                        client_sock.Receive(repl);
                        if (Encoding.UTF8.GetString(repl).Equals("Success"))
                        {
                            // DATABASE//
                            OleDbCommand cmd = new OleDbCommand();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = "INSERT  INTO client_table ([File Name]) VALUES (?) ;";
                            cmd.Parameters.AddWithValue("@File Name", s_fName);
                            cmd.Connection = cnn;
                            cnn.Open();
                            cmd.ExecuteNonQuery();
                            //                   MessageBox.Show("Insert Successful");
                            MessageBox.Show("Successfully Stored");
                        }
                        else
                        {
                            MessageBox.Show("Unsuccessful");
                        }

                    }
                    else
                        MessageBox.Show("Select a File");

                }
                else
                {
                    if (connect)
                        MessageBox.Show("Click Request to Store First");
                    else
                        MessageBox.Show("Connect to the Server First");
                }

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
                if (String.IsNullOrEmpty(textBox2.Text) || String.IsNullOrWhiteSpace(textBox2.Text))
                {
                    MessageBox.Show("Enter Server's IP");
                }
                else
                {
                    client_sock.Connect(ip, 8080);
                    client_sock.Send(Encoding.UTF8.GetBytes(clientid));
                    byte[] b = new byte[10];
                    client_sock.Receive(b);
                    MessageBox.Show(client_sock.Connected ? "Connected to the Server" : "Server Not Listening");
                    connect = client_sock.Connected;
                }
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
            retFName = textBox1.Text;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            String req = "Retrive";
            if (retFName != null & connect)
            {

                byte[] req_msg = Encoding.UTF8.GetBytes(req);
                byte[] fileName = Encoding.UTF8.GetBytes(textBox1.Text);
                byte[] msg = new byte[7 + fileName.Length];
                req_msg.CopyTo(msg, 0);
                fileName.CopyTo(msg, 7);
                client_sock.Send(msg);
                int recv_le = client_sock.Receive(msg);
                String repl = Encoding.UTF8.GetString(msg, 0, recv_le);
                byte[] data = new byte[500 * 1000];
                if (repl.Equals("Waitttt"))
                {
                    MessageBox.Show("File being retrived");
                    client_sock.Receive(data);
                    MessageBox.Show("Read Successful");
                    Read(data, data.Length);
                }
                else
                {
                    MessageBox.Show("File not found");
                }
                retFName = null;
            }
            else
            {
                if (connect)
                    MessageBox.Show("Select a file name from list of files");
                else
                    MessageBox.Show("Connect to the Server First");
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            String req = "Deletee";
            if (retFName != null && connect)
            {

                byte[] req_msg = Encoding.UTF8.GetBytes(req);
                byte[] fileName = Encoding.UTF8.GetBytes(textBox1.Text);
                byte[] msg = new byte[7 + fileName.Length];
                req_msg.CopyTo(msg, 0);
                fileName.CopyTo(msg, 7);
                client_sock.Send(msg);
                int recv_le = client_sock.Receive(msg);
                String repl = Encoding.UTF8.GetString(msg, 0, recv_le);
                byte[] data = new byte[500 * 1000];
                if (repl.Equals("Waitttt"))
                {
                }

                else
                {
                    MessageBox.Show("File Deleted");
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "DELETE * FROM client_table  WHERE [File Name] = ? ;";
                    cmd.Parameters.AddWithValue("@File Name", textBox1.Text);
                    cmd.Connection = cnn;
                    cnn.Open();
                    cmd.ExecuteNonQuery();
                        
                }
            }
            else
            {
                if (connect)
                    MessageBox.Show("Select a file name from list of files");
                else
                    MessageBox.Show("Connect to the Server first");
            }
        }
    }
   }
