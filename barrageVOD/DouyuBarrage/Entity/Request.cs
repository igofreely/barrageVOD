using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage.Entity
{
    public class Request
    {
        /**
     * 程序需要发送的各种请求正文
     */

        public static String gid(int roomId, String devid, String rt, String vk)
        {
            return String.Format("type@=loginreq/username@=/ct@=0/password@=/roomid@={0}/devid@={1}/rt@={2}/vk@={3}/ver@=20150929/", roomId, devid, rt, vk);
        }

        public static String danmakuLogin(int roomId)
        {
            return String.Format("type@=loginreq/username@=visitor34807350/password@=1234567890123456/roomid@={0}/", roomId);
        }

        public static String joinGroup(int rid, int gid)
        {
            return String.Format("type@=joingroup/rid@={0}/gid@={1}/", rid, gid);
        }

        public static String keepLive(int tick)
        {
            return String.Format("type@=keeplive/tick@={0}/", tick);
        }
    }
}
