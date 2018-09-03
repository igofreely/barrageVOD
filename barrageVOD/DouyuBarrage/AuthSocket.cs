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

namespace DouyuBarrage
{
    /// <summary>
    /// 弹幕服务器认证
    /// </summary>
    public class AuthSocket
    {

        #region Events
        public event EventHandler OnReady;
        public virtual void OnOnReadyEvent()
        {
            OnReady?.Invoke(this, EventArgs.Empty);
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
        private readonly Socket _socket = null;
        //处理发送的SAEA处理，由于绑定不同的回调函数，因此需要不同的SAEA对象
        private SocketAsyncEventArgs _sendSaea;

        private List<ServerInfo> danmakuServers = new List<ServerInfo>();
        private int gid; 
        #endregion

        public int GID { get { return gid; } }
        public List<ServerInfo> DanmakuServers { get { return danmakuServers; } }
        public int RID { get { return rid; } }

        public AuthSocket()
        {
            thread = new Thread(work);
            thread.IsBackground = true;
            random = new Random();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        }

        public void Start()
        {
            thread.Start();
        }


        private void work()
        {
            string roomUrl = DouyuConfig.ROOT + DouyuConfig.room;
            //获取房间页面
            string html = HttpUtil.GetHtml(roomUrl);
            //获取直播房间ID
            rid = ResponseParser.ParseRoomId(html);

            //检查是否在线
            bool online = ResponseParser.ParseOnline(html);
            if (!online)
            {
                //主播没有在线
                OnErrorEvent("主播没有在线哦！");
                return;
            }

            //获取服务器列表
            List<ServerInfo> serverList = ResponseParser.ParseServerInfo(html);
            if (serverList == null || serverList.Count == 0)
            {
                //获取服务器列表失败
                OnErrorEvent("获取服务器列表失败！");
                return;
            }
            loginRequest(serverList);

        }

        private void loginRequest(List<ServerInfo> serverList)
        {
            try
            {
                OnLogEvent("开始连接服务器。。。");
                int i = random.Next(0, serverList.Count);
                ServerInfo server = serverList[i];

                IPAddress ipAddress = IPAddress.Parse(server.host);
                _endPoint = new IPEndPoint(ipAddress, server.port);

                _saea = new SocketAsyncEventArgs { RemoteEndPoint = _endPoint };
                _saea.Completed += OnConnectedCompleted;
                _socket.ConnectAsync(_saea);

            }
            catch (Exception ex)
            {
                //获取服务器列表失败
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
            var receiveBuffer = new byte[1024 * 4];
            receiveSaea.SetBuffer(receiveBuffer, 0, receiveBuffer.Length);       //设置消息的缓冲区大小
            receiveSaea.Completed += OnReceiveCompleted;         //绑定回调事件
            receiveSaea.RemoteEndPoint = _endPoint;
            _socket.ReceiveAsync(receiveSaea);

            string timestamp = MD5Utils.ConvertDateTimeInt(DateTime.Now) + "";
            string uuid = Guid.NewGuid().ToString().Replace("-", "").ToUpper();
            string vk = MD5Utils.GetMD5(timestamp + "7oE9nPEG9xXV69phU31FYCLUagKeYtsF" + uuid);

            SendMsg(Request.gid(rid, uuid, timestamp, vk));

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
                if (!danmakuListener(req.responses))
                {
                    socket.ReceiveAsync(e);
                }
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
        private bool danmakuListener(List<string> responses)
        {
            foreach (string response in responses)
            {
                //
                if (response.Contains("msgiplist"))
                {
                    //获取弹幕服务器地址
                    OnLogEvent("获取弹幕服务器地址 ..."); 
                    string danmakuServerStr = SttCode.deFilterStr(SttCode.deFilterStr(response));
                    danmakuServers = ResponseParser.ParseDanmakuServer(danmakuServerStr);
                    OnLogEvent("获取到 " + danmakuServers.Count + " 个服务器地址 ...");
                }

                if (response.Contains("setmsggroup"))
                {
                    //获取gid
                    OnLogEvent("获取弹幕群组ID ...");
                    gid = ResponseParser.ParseID(response);
                    OnLogEvent("弹幕群组ID：" + gid);

                    OnDisConnect();
                    OnOnReadyEvent();
                    return true;
                }
            }
            return false;
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

        public void OnDisConnect()
        {
            if (_socket != null)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    //_socket.DisconnectAsync(_saea);
                }
                catch (SocketException ex)
                {
                }
                finally
                {
                    _socket.Close();
                }
            }
        }
        

    }

}
