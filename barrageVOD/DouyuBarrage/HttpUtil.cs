using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage
{
    public class HttpUtil
    {
        public static string GetHtml(string url)
        {
            WebClient http = new WebClient();
            http.Encoding = Encoding.UTF8;
            return http.DownloadString(url);
        }


    }
}
