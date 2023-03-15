using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RD91SWinForm
{
    internal class NETTools
    {
        //public static String targetAddress = "http://10.255.255.235:8080/jeecg-boot/wash/api/sampleCode";
        public static String targetAddress= "http://192.168.2.208:80/jeecg-boot/wash/api/sampleCode";
        public static string HttpGet(string Url, string postDataStr)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ReadWriteTimeout = 500000000;
            string retString = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream myResponseStream = response.GetResponseStream())
                {
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
                    retString = myStreamReader.ReadToEnd();
                    myResponseStream.Flush();
                    myResponseStream.Close();
                    myStreamReader.Close();
                }

            }

            return retString;
        }
    }
}
