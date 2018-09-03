using DouyuBarrage.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage
{
    public class Events: EventArgs
    {
        public string message { get; set; }
        public Events() { }
        public Events(string msg)
        {
            message = msg;
        }
    }

    public class DanmakuEventArgs : EventArgs
    {
        public Danmaku Danmaku { get; set; }
        public DanmakuEventArgs() { }
        public DanmakuEventArgs(Danmaku Danmaku)
        {
            this.Danmaku = Danmaku;
        }
    }



}
