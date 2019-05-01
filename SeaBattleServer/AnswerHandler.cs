using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using System.Runtime.Serialization.Json;
using System.IO;

namespace SeaBattleServer
{
    internal class AnswerHandler
    {
        public static string GetErrorMessage()
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Error));
            jObject.Add(JsonStructInfo.Result, "");

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        public static string GetOkMessage()
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Ok));
            jObject.Add(JsonStructInfo.Result, "");

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        /// <summary>
        /// Возвращает готовый JSON документ с информацией о позиции выстрела.
        /// </summary>
        /// <param name="shootLocation"></param>
        /// <returns></returns>
        public static string GetShootMessage(SeaBattleClassLibrary.Game.Location shootLocation)
        {
            MemoryStream ms = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(SeaBattleClassLibrary.Game.Location));

            try
            {
                ser.WriteObject(ms, shootLocation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return GetErrorMessage();
            }

            byte[] locationData = ms.ToArray();
            ms.Close();
            string location = Encoding.UTF8.GetString(locationData, 0, locationData.Length);

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Ok));
            jObject.Add(JsonStructInfo.Result, location);

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        public static string GetShootResult(ShootResult.ShootResultType shootResult)
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.ShootResult));
            jObject.Add(JsonStructInfo.Result, ShootResult.EnumTypeToString(shootResult));

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        public static string GetGamesMessage(List<BeginGame> games)
        {
            string message = Serializer<List<BeginGame>>.SetSerializedObject(games);

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Games));
            jObject.Add(JsonStructInfo.Result, message);

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }
    }
}
