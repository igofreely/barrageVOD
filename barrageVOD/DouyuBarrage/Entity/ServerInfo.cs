using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DouyuBarrage.Entity
{
    public class ServerInfo
    {
        public string host { get; set; }
        public int port { get; set; }

        public ServerInfo()
        {

        }

        public ServerInfo(string host,int port)
        {
            this.host = host;
            this.port = port;
        }


        public override string ToString()
        {
            return "ServerInfo{" +
               "host=" + host +
               ", port=" + port +
               '}';
        }
    }
}
