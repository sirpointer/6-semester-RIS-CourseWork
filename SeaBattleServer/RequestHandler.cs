using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;

namespace SeaBattleServer
{
    internal static class RequestHandler
    {
        public static Request.RequestTypes GetRequestType(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            }
            catch (JsonReaderException e)
            {
                Console.WriteLine(e);
                return Request.RequestTypes.BadRequest;
            }
            
            if (jObject.ContainsKey(JsonBaseStruct.Type))
            {
                string requestType = (string)jObject[JsonBaseStruct.Type];
                return Request.JsonTypeToEnum(requestType);
            }
            else
            {
                return Request.RequestTypes.BadRequest;
            }
        }

        public static string GetJsonRequestResult(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            }
            catch (JsonReaderException e)
            {
                Console.WriteLine(e);
                return null;
            }

            if (jObject.ContainsKey(JsonBaseStruct.Result))
            {
                string result = (string)jObject[JsonBaseStruct.Result];
                return result;
            }
            else
                return null;
        }
    }
}
