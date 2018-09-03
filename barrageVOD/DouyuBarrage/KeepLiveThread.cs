using DouyuBarrage.Entity;
using DouyuBarrage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DouyuBarrage
{
    class KeepLiveThread
    {
        private Thread thread;
        private CrawlerThread socket;
        private bool IsRunning = true;
        public KeepLiveThread(CrawlerThread socket)
        {
            this.socket = socket;
            thread = new Thread(work);
            thread.IsBackground = true;
        }

        public void Start()
        {
            IsRunning = true;
            thread.Start();
        }

        private void work()
        {
            Console.WriteLine("心跳包线程启动");
            while (IsRunning)
            {
                Thread.Sleep(40000);
                string message = Request.keepLive(MD5Utils.ConvertDateTimeInt(DateTime.Now));
                if (IsRunning)
                    socket.SendMsg(message);
            }
        }

        public void Stop()
        {
            IsRunning = false;
        }

    }
}
