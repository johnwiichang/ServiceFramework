using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ServiceFramework
{
    public static class JsonHelper<T> where T : class
    {
        static DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(T));

        /// <summary>
        /// 序列化对象。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static String Serialize(T obj)
        {
            var Json = "";
            using (MemoryStream memStream = new MemoryStream())
            {
                Serializer.WriteObject(memStream, obj);
                memStream.Position = 0;
                using (StreamReader sr = new StreamReader(memStream, Encoding.UTF8))
                {
                    Json = sr.ReadToEnd();
                }
            }
            return Json;
        }


        /// <summary>
        /// 反序列化对象。
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static T DeSerialize(String str)
        {
            using (MemoryStream memStream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
            {
                return Serializer.ReadObject(memStream) as T;
            }
        }
    }
}
