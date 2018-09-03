using DouyuBarrage.Entity;
using DouyuBarrage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
namespace DouyuBarrage
{
    public class CrawlerThread
    {

        #region Events
        public event EventHandler<DanmakuEventArgs> OnDanmaku;
        public virtual void OnDanmakuEvent(Danmaku danmaku)
        {
            OnDanmaku?.Invoke(this, new DanmakuEventArgs(danmaku));
        }

        public event EventHandler<EventArgs> DisConnectHandler;
        public virtual void OnDisConnectEvent()
        {
            DisConnectHandler?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<Events> ErrorHandler;
        public virtual void OnErrorEvent(string msg)
        {
            ErrorHandler?.Invoke(this, new Events(msg));
        }

        public event EventHandler<Events> LogHandler;
        public virtual void OnLogEvent(string msg)
        {
            LogHandler?.Invoke(this, new Events(msg));
        }
        #endregion


        #region 私有属性
        private Thread thread;
        private int rid;
        private Random random;

        private SocketAsyncEventArgs _saea;        //处理连接和接收SAEA对象
        private EndPoint _endPoint;      //远端地址
        readonly Socket _socket = null;
        //处理发送的SAEA处理，由于绑定不同的回调函数，因此需要不同的SAEA对象
        private SocketAsyncEventArgs _sendSaea;

        private List<ServerInfo> danmakuServers;
        private int gid;
        private KeepLiveThread keepLiveThread;
        #endregion

        //创建日志记录组件实例
        ILog log = log4net.LogManager.GetLogger(typeof(CrawlerThread));
        //       //记录错误日志
        //       log.Error("发生了错误：", new Exception("log4net的测试错误信息"));
        ////记录致命的错误
        //log.Fatal("发生了致命的错误：", new Exception("log4net测试致命信息"));
        ////记录一般信息
        //log.Info("log4net的一般信息");
        ////记录调试信息
        //log.Debug("log4net的调试信息");
        ////记录警告信息
        //log.Warn("log4net警告信息");
        //Console.WriteLine("ok");
        //Console.ReadKey();
        public CrawlerThread(List<ServerInfo> danmakuServers, int gid, int rid)
        {
            this.danmakuServers = danmakuServers;
            this.gid = gid;
            this.rid = rid;
            thread = new Thread(work);
            thread.IsBackground = true;
            random = new Random();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            log4net.Config.XmlConfigurator.Configure();
        }

        public void Start()
        {
            thread.Start();
        }


        private void work()
        {
            int i = random.Next(0, danmakuServers.Count);
            ServerInfo danmakuServer = danmakuServers[i];

            try
            {
                OnLogEvent("登陆到弹幕服务器 " + danmakuServer.host + ":" + danmakuServer.port);
                IPAddress[] ips= Dns.GetHostAddresses(danmakuServer.host);
                IPAddress ipAddress = ips[random.Next(0,ips.Length)];
                _endPoint = new IPEndPoint(ipAddress, danmakuServer.port);

                _saea = new SocketAsyncEventArgs { RemoteEndPoint = _endPoint };
                _saea.Completed += OnConnectedCompleted;
                _socket.ConnectAsync(_saea);

            }
            catch (Exception ex)
            {
                OnErrorEvent(ex.Message);
            }
        }


        private void OnConnectedCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;

            Socket socket = sender as Socket;

            OnLogEvent("服务器连接成功。。。");

            //开启新的接受消息异步操作事件
            var receiveSaea = new SocketAsyncEventArgs();
            var receiveBuffer = new byte[1024 * 10];
            receiveSaea.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);       //设置消息的缓冲区大小
            receiveSaea.Completed += OnReceiveCompleted;         //绑定回调事件
            receiveSaea.RemoteEndPoint = _endPoint;
            _socket.ReceiveAsync(receiveSaea);

            SendMsg(Request.danmakuLogin(rid));
            OnLogEvent("进入 " + rid + " 号房间， " + gid + " 号弹幕群组 ...");
            Thread.Sleep(100);
            SendMsg(Request.joinGroup(rid, gid));

            //心跳包线程启动
            keepLiveThread= new KeepLiveThread(this);
            keepLiveThread.Start();
            OnLogEvent("开始接收弹幕");
            OnLogEvent("-----------------------------------------------");
        }

        /// <summary>
        /// 接受消息的回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.OperationAborted) return;
            var socket = sender as Socket;
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                int lengthBuffer = e.BytesTransferred;
                byte[] receiveBuffer = e.Buffer;
                byte[] buffer = new byte[lengthBuffer];
                Buffer.BlockCopy(receiveBuffer, 0, buffer, 0, lengthBuffer);

                Response req = new Response(buffer);
                danmakuListener(req.responses);

                socket.ReceiveAsync(e);
            }
            else if (e.SocketError == SocketError.ConnectionReset && e.BytesTransferred == 0)
            {
                OnErrorEvent("服务器已经断开连接");
            }
            else
            {
                return;
            }

        }
        /// <summary>
        /// 请求弹幕服务器回调
        /// </summary>
        /// <param name="responses"></param>
        private void danmakuListener(List<string> responses)
        {
            foreach (string response in responses)
            {
                //type@=uenter/rid@=783827/uid@=20887267/nn@=mt德芙要吃吗/level@=9/ic@=avatar@Sdefault@S12/rni@=0/el@=/sahf@=0/wgei@=0/
                log.InfoFormat("Receive Response:" + response);
                if (!response.Contains("chatmsg")) continue;

                //解析弹幕
                Danmaku danmaku = ResponseParser.parseDanmaku(response);
                if (danmaku == null) continue;
                OnDanmakuEvent(danmaku);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="content"></param>
        public void SendMsg(string content)
        {
           
            Message message = new Message(content);
            byte[] sendBuffer = message.GetBytes();
            if (_sendSaea == null)
            {
                _sendSaea = new SocketAsyncEventArgs { RemoteEndPoint = _endPoint };
                _sendSaea.Completed += OnSendCompleted;
            }
            _sendSaea.SetBuffer(sendBuffer, 0, sendBuffer.Length);
            if (_socket != null) _socket.SendAsync(_sendSaea);
        }

        /// <summary>
        /// 发送消息回调函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success) return;
            var socket = sender as Socket;

        }

        public void DisConnect()
        {
            if (_socket != null)
            {
                try
                {
                    if (keepLiveThread != null)
                        keepLiveThread.Stop();
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException ex)
                {
                }
                finally
                {
                    OnDisConnectEvent();
                    _socket.Close();
                }
            }
        }

    }
}
