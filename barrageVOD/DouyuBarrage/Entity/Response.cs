using DouyuBarrage.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage.Entity
{
    public class Response
    {
        public List<string> responses { get; private set; }

        public Response(byte[] buffer)
        {
            responses=splitResponse(buffer);
        }

        public List<string> splitResponse(byte[] buffer)
        {
            List<string> resList = new List<string>();

            string hex= BitConverter.ToString(buffer,0).Replace("-",string.Empty).ToLower();

            string[] responseStrings= hex.Split(new String[] { "b2020000" }, StringSplitOptions.RemoveEmptyEntries);
            int end = 0;
            for (int i = 1; i < responseStrings.Length; i++)
            {
                if (!responseStrings[i].Contains("00")) continue;

                end = responseStrings[i].IndexOf("00");

                string str = responseStrings[i].Substring(0,  end);


                byte[] bytes = ByteTools.hexString2Bytes(str);
                if (bytes != null) resList.Add(Encoding.UTF8.GetString(bytes));
            }
            return resList;
        }


    }
}
