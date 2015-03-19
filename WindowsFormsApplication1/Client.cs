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

namespace WindowsFormsApplication1
{
    public partial class Client : Form
    {
        public String fName;
        public String s_fName;
        Socket client_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public Client()
        {
            InitializeComponent();
            OpenFileDialog ofd = new OpenFileDialog();
            StatusStrip statusStrip1 = new StatusStrip();
            statusStrip1.Text = "Select a File";
            statusStrip1.Refresh();

        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

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
                byte[] filename = Encoding.UTF8.GetBytes(s_fName);
                byte[] fileData = File.ReadAllBytes(fName);
                byte[] data = new byte[4 + fName.Length + fileData.Length];
                byte[] fileNameLen = BitConverter.GetBytes(s_fName.Length);
                fileNameLen.CopyTo(data, 0);
                filename.CopyTo(data, 4);
                fileData.CopyTo(data, 4 + s_fName.Length);
                client_sock.Connect("74.102.95.48", 5050);
                client_sock.Send(data);
                client_sock.Close();
            }



            catch (Exception s)
            {
                MessageBox.Show("Exception caught Unable to connect to Server");
            }

        }
    }
}
