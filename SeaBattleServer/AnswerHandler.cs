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
using SeaBattleClassLibrary.Game;

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
        /// <param name="shotLocation"></param>
        /// <returns></returns>
        public static string GetShotResultMessage(Location shotLocation)
        {
            string location = Serializer<Location>.SetSerializedObject(shotLocation);

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.ShotOfTheEnemy));
            jObject.Add(JsonStructInfo.Result, Serializer<Location>.SetSerializedObject(shotLocation));

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

        public static string GetGameReadyMessage(bool yourTurn)
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.GameReady));
            jObject.Add(JsonStructInfo.Result, Answer.EnumTypeToString(yourTurn ? Answer.AnswerTypes.Yes : Answer.AnswerTypes.No));

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        public static string GetShotResultMessage(Ship ship)
        {
            ShotResult.ShotResultType type = ShotResult.ShotResultType.Miss;

            if (ship != null)
            {
                if (ship.IsDead)
                    type = ShotResult.ShotResultType.Kill;
                else
                    type = ShotResult.ShotResultType.Damage;
            }

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.ShotResult));
            jObject.Add(JsonStructInfo.Result, ShotResult.EnumTypeToString(type));
            jObject.Add(JsonStructInfo.AdditionalContent, Serializer<Ship>.SetSerializedObject(ship));

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }
    }
}