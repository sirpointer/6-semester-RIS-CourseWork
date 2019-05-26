using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;

namespace SeaBattleServer
{
    /// <summary>
    /// Представляет статический класс для расшифровки полученных сообщений.
    /// </summary>
    internal static class RequestHandler
    {
        /// <summary>
        /// Определяет тип запроса.
        /// </summary>
        /// <param name="jsonRequest">Запрос в JSON.</param>
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
        /// <param name="jsonRequest">Запрос в JSON.</param>
        public static string GetJsonRequestResult(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            }
            catch (JsonReaderException e)
            {
                Console.WriteLine(jsonRequest);
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
        /// <param name="jsonResult">Результат запроса в JSON.</param>
        public static BeginGame GetAddGameResult(string jsonResult)
        {
            if (string.IsNullOrWhiteSpace(jsonResult))
                return null;

            try
            {
                return Serializer<BeginGame>.Deserialize(jsonResult);
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
        public static BeginGame GetJoinTheGameResult(string jsonResult)
        {
            return GetAddGameResult(jsonResult);
        }

        /// <summary>
        /// Десереализует Location.
        /// </summary>
        public static Location GetLocation(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                return Serializer<Location>.Deserialize(json);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Десереализует GameField.
        /// </summary>
        public static GameField GetGameFieldResult(string jsonResult)
        {
            if (string.IsNullOrWhiteSpace(jsonResult))
                return null;

            try
            {
                List<Ship> ships = Serializer<List<Ship>>.Deserialize(jsonResult);
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
