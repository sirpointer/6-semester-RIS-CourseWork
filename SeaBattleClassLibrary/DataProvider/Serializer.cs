using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SeaBattleClassLibrary.DataProvider
{
    public static class Serializer<T> where T : class
    {
        public static T GetSerializedObject(string json)
        {
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            T deserialized = ser.ReadObject(ms) as T;
            ms.Close();
            return deserialized;
        }

        public static string SetSerializedObject(T obj)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, obj);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json);
        }
    }
}
