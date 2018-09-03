using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage.Utils
{
    /// <summary>
    /// /服务器返回数据 编解码， 根据JS代码转化过来的
    /// </summary>
    public class SttCode
    {
        public static string encode(string content)
        {
            if (content == null || content.Length <= 0) return null;
            return null;
        }

        public static Dictionary<string, string> decode(string content)
        {
            if (content == null || content.Length <= 0) return null;


            string[] strs = content.Split('/');
            Dictionary<string, string> map = new Dictionary<string, string>();
            foreach (string str in strs)
            {
                if (str.Contains("@="))
                {
                    string[] kv = str.Split(new string[] { "@=" }, StringSplitOptions.RemoveEmptyEntries);
                    if (kv.Length < 2) kv = new string[] { kv[0], "" };
                    map.Add(deFilterStr(kv[0]), deFilterStr(kv[1]));
                }
                else
                {
                    map.Add("", deFilterStr(str));
                }
            }

            return map;
        }

        public static string displayMap(Dictionary<string, string> map)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in map.Keys)
            {
                sb.Append(s).Append(":").Append(map[s]).Append("\n");
            }

            return sb.ToString();
        }

        public static string deFilterStr(string str)
        {
            if (str == null) return null;
            return str.Trim().Replace("@A", "@").Replace("@S", "/");
        }

        public static string filterStr(string str)
        {
            if (str == null) return null;
            return str.Trim().Replace("@", "@A").Replace("/", "@S");
        }
    }
}
