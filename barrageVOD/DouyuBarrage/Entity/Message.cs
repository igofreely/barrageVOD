using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage.Entity
{
    public class Message
    {
        /**
    * 请求消息体包含五部分：
    * 1.计算后四部分的字节长度，占4个字节
    * 2.内容设置和第一部分一样
    * 3.请求代码，固定，发到斗鱼是0xb1,0x02,0x00,0x00,接收是0xb2,0x02,0x00,0x00，4个字节
    * 4.消息正文
    * 5.尾部1个空字节
    */

        public int length { get; set; }
        public int code { get; set; }

        public byte[] magic = { 0xb1, 0x02, 0x00, 0x00 };
        public string content { get; set; }

        public byte[] end = { 0x00 };

        public Message(string content)
        {
            length = 4 + 4 + content.Length + 1;
            code = 4 + 4 + content.Length + 1;
            this.content = content;
        }

        /// <summary>
        /// 将Message对象转化为字节数组
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            byte[] buff = null;
            using( MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(length);
                    writer.Write(code);

                    writer.Write(magic);

                    byte[] buf = Encoding.UTF8.GetBytes(content);

                    writer.Write(buf);

                    writer.Write(end);
                }
                buff = stream.ToArray();

            }
            return buff;
        }
        
    }
}
