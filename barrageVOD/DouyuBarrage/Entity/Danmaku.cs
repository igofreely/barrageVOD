using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage.Entity
{
    public class Danmaku
    {
        public int uid { get; set; }
        public string snick { get; set; }
        public string content { get; set; }
        public DateTime date { get; set; }
        public int rid { get; set; }

        public Danmaku()
        {

        }
        public Danmaku(int uid, string snick, string content, int rid)
        {
            this.uid = uid;
            this.snick = snick;
            this.content = content;
            this.date = DateTime.Now;
            this.rid = rid;
        }

        public override string ToString()
        {
            return "Danmaku{" +
                    "uid=" + uid +
                    ", snick='" + snick + '\'' +
                    ", content='" + content + '\'' +
                    ", date=" + date +
                    ", rid=" + rid +
                    '}';
        }

    }
}
