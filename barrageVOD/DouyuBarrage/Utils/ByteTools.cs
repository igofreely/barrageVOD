using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DouyuBarrage.Utils
{
    public class ByteTools
    {
        public static byte[] hexString2Bytes(string hex)
        {
            var inputByteArray = new byte[hex.Length / 2];
            for (var x = 0; x < inputByteArray.Length; x++)
            {
                var i = Convert.ToInt32(hex.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            return inputByteArray;
        }

    }
}
