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
        public Server()
        {
            
            try
            {
                // GET IP OF SERVER //
                for (int i = 0; i <= System.Net.Dns.GetHostEntry(myHost).AddressList.Length - 1; i++)
                {
                    if (System.Net.Dns.GetHostEntry(myHost).AddressList[i].IsIPv6LinkLocal == false)
                    {
                        myIp = System.Net.Dns.GetHostEntry(myHost).AddressList[i].ToString();
                    }

                }
                InitializeComponent();
                MessageBox.Show(myIp);
                IPEndPoint ip_end = new IPEndPoint(IPAddress.Parse(myIp), 8080);
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                server.Bind(ip_end);

                while (true)
                {

                    server.Listen(100);
                    client_sock = server.Accept();
                    HandleClient hc = new HandleClient();
                    IPEndPoint r = client_sock.RemoteEndPoint as IPEndPoint;
                    mapclass.map.Add(r.Address, client_sock);
                    
                    if (mapclass.map.ContainsKey(r.Address))
                        MessageBox.Show("HAS OB IN HASH");
                    hc.startClient(client_sock);


                }
            }
        catch(Exception we)
            {
                MessageBox.Show(we.Message);
            }
        }
        
        
        
        
        private void button1_Click(object sender, EventArgs e)
        {
           MessageBox.Show("Listening");
        }
    }
}

