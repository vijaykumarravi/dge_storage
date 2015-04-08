using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server_Application
{
    public class data
    {
        
        public const int buf_size = 8096;
        public byte[] buffer;
        public int length;
        public data()
        { 
            buffer= new byte[buf_size];
            buffer = null;
            length = 0;
        }


    }
}
