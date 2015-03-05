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

namespace WindowsFormsApplication1
{
    public partial class Client : Form
    {
        
        public Client()
        {
            InitializeComponent();
            OpenFileDialog ofd = new OpenFileDialog();
            StatusStrip statusStrip1 = new StatusStrip();
            statusStrip1.Text="Select a File";
            statusStrip1.Refresh();
            IPAddress[] ipaddress = Dns.GetHostAddresses("localhost");
            UdpClient udp_client = new UdpClient("localhost", 2055);
             
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(ofd.ShowDialog() == DialogResult.OK)
            {
                //textBox1.Text = ofd.FileName;
                textBox1.Text = ofd.SafeFileName;
            }
            
   
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            statusStrip1.Text = "File Selected";
            statusStrip1.Refresh();
        }

        
    }
}
