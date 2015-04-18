using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Server_Application
{
    static class mapclass
    {
        public static Dictionary<IPAddress, Socket> map = new Dictionary<IPAddress, Socket>();
    }

}
