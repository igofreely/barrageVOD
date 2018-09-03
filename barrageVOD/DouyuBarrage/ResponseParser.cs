using DouyuBarrage.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DouyuBarrage
{
    class ResponseParser
    {
        private const string REGEX_ROOM_ID = "\"room_id\":(\\d*),";
        private const string REGEX_ROOM_STATUS = "\"show_status\":(\\d*),";
        private const string REGEX_SERVER = "%7B%22ip%22%3A%22(.*?)%22%2C%22port%22%3A%22(.*?)%22%7D%2C";
        private const string REGEX_GROUP_ID = "type@=setmsggroup.*/rid@=(\\d*?)/gid@=(\\d*?)/";
        private const string REGEX_DANMAKU_SERVER = "/ip@=(.*?)/port@=(\\d*?)/";
        private const string REGEX_CHAT_DANMAKU = "type@=chatmsg/.*rid@=(\\d*?)/.*uid@=(\\d*).*nn@=(.*?)/txt@=(.*?)/(.*)/";

        private static MatchCollection GetMatcher(string content, string regex)
        {
            return Regex.Matches(content, regex);
        }


        /// <summary>
        /// 从房间页面解析出roomId
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static int ParseRoomId(string content)
        {
            int rid = -1;
            if (content == null) return rid;

            MatchCollection matcher = GetMatcher(content, REGEX_ROOM_ID);
            if (matcher.Count > 0)
            {
                Match dd = matcher[0];
                rid = Convert.ToInt32(dd.Groups[1].Value);
            }
            return rid;
        }


        /// <summary>
        /// 解析当前直播状态
        /// </summary>
        /// <param name="content"></param>
        /// <returns>若room_status == 1 则正在直播</returns>
        public static bool ParseOnline(string content)
        {
            if (content == null) return false;

            MatchCollection matcher = GetMatcher(content, REGEX_ROOM_STATUS);
            return matcher.Count > 0 && 1 == Convert.ToInt32(matcher[0].Groups[1].Value);
        }

        /// <summary>
        /// 解析出服务器地址
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static List<ServerInfo> ParseServerInfo(string content)
        {
            if (content == null) return null;

            MatchCollection matcher = GetMatcher(content, REGEX_SERVER);
            List<ServerInfo> serverList = new List<ServerInfo>();

            foreach (Match item in matcher)
            {
                ServerInfo serverInfo = new ServerInfo(item.Groups[1].Value, Convert.ToInt32(item.Groups[2].Value));
                serverList.Add(serverInfo);
                Console.WriteLine("服务器地址：" + serverInfo.ToString());
            }
            return serverList;
        }

        /// <summary>
        /// 解析弹幕服务器地址
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static List<ServerInfo> ParseDanmakuServer(string content)
        {
            if (content == null) return null;

            MatchCollection matcher = GetMatcher(content, REGEX_DANMAKU_SERVER);
            List<ServerInfo> serverList = new List<ServerInfo>();

            foreach (Match item in matcher)
            {
                ServerInfo serverInfo = new ServerInfo(item.Groups[1].Value, Convert.ToInt32(item.Groups[2].Value));
                serverList.Add(serverInfo);
            }
            return serverList;
        }

        /// <summary>
        /// 解析  gid
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static int ParseID(string response)
        {
            if (response == null) return -1;

            MatchCollection matcher = GetMatcher(response, REGEX_GROUP_ID);
            int gid = -1;
            if (matcher.Count > 0)
            {
                gid = Convert.ToInt32(matcher[0].Groups[2].Value);
            }

            return gid;
        }

        /// <summary>
        /// 解析弹幕信息
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Danmaku parseDanmaku(string response)
        {
            if (response == null) return null;

            MatchCollection matcher = GetMatcher(response, REGEX_CHAT_DANMAKU);
            Danmaku danmaku = null;

            if (matcher.Count > 0)
            {
                danmaku = new Danmaku(Convert.ToInt32(matcher[0].Groups[2].Value),
                        matcher[0].Groups[3].Value,
                        matcher[0].Groups[4].Value,
                        Convert.ToInt32(matcher[0].Groups[1].Value));
            }

            return danmaku;
        }

    }
}
