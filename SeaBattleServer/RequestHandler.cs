using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;

namespace SeaBattleServer
{
    internal static class RequestHandler
    {
        /// <summary>
        /// Определяет тип запроса.
        /// </summary>
        /// <param name="jsonRequest"></param>
        /// <returns></returns>
        public static Request.RequestTypes GetRequestType(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            }
            catch (JsonReaderException e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine(jsonRequest);
                Console.WriteLine();
                return Request.RequestTypes.BadRequest;
            }
            
            if (jObject.ContainsKey(JsonStructInfo.Type))
            {
                string requestType = (string)jObject[JsonStructInfo.Type];
                return Request.JsonTypeToEnum(requestType);
            }
            else
            {
                return Request.RequestTypes.BadRequest;
            }
        }

        /// <summary>
        /// Возвращает тело запроса.
        /// </summary>
        /// <param name="jsonRequest"></param>
        /// <returns></returns>
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

            if (jObject.ContainsKey(JsonStructInfo.Result))
            {
                string result = (string)jObject[JsonStructInfo.Result];
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Десереализованный результат запроса AddGame.
        /// </summary>
        /// <param name="jsonResult"></param>
        /// <returns></returns>
        public static BeginGame GetAddGameResult(string jsonResult)
        {
            if (string.IsNullOrWhiteSpace(jsonResult))
                return null;

            try
            {
                return Serializer<BeginGame>.GetSerializedObject(jsonResult);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Десереализованный результат запроса AddGame.
        /// </summary>
        /// <param name="jsonResult"></param>
        /// <returns></returns>
        public static BeginGame GetJoinTheGameResult(string jsonResult)
        {
            return GetAddGameResult(jsonResult);
        }

        public static GameField GetGameFieldResult(string jsonResult)
        {
            if (string.IsNullOrWhiteSpace(jsonResult))
                return null;

            try
            {
                List<Ship> ships = Serializer<List<Ship>>.GetSerializedObject(jsonResult);
                return new GameField(ships);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }
}
