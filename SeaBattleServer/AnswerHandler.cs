using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SeaBattleClassLibrary.DataProvider;
using SeaBattleClassLibrary.Game;

namespace SeaBattleServer
{
    /// <summary>
    /// Представляет статический класс для генерации ответов от сервера.
    /// </summary>
    internal static class AnswerHandler
    {
        /// <summary>
        /// Возвращает сообщение об ошибке (AnswerTypes.Error).
        /// </summary>
        public static string GetErrorMessage()
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Error));
            jObject.Add(JsonStructInfo.Result, "");

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        /// <summary>
        /// Возвращает сообщение ("AnswerTypes.Ok").
        /// </summary>
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
        public static string GetShotResultMessage(Location shotLocation)
        {
            string location = Serializer<Location>.Serialize(shotLocation);

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.ShotOfTheEnemy));
            jObject.Add(JsonStructInfo.Result, Serializer<Location>.Serialize(shotLocation));

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        /// <summary>
        /// Возвращает сообщение с доступными играми.
        /// </summary>
        public static string GetGamesMessage(List<BeginGame> games)
        {
            string message = Serializer<List<BeginGame>>.Serialize(games);

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.Games));
            jObject.Add(JsonStructInfo.Result, message);

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        /// <summary>
        /// Возвращает сообщение о том, что игра сформирована (готова).
        /// </summary>
        /// <param name="yourTurn">Чей ход.</param>
        public static string GetGameReadyMessage(bool yourTurn)
        {
            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.GameReady));
            jObject.Add(JsonStructInfo.Result, Answer.EnumTypeToString(yourTurn ? Answer.AnswerTypes.Yes : Answer.AnswerTypes.No));

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        /// <summary>
        /// Возвращает сообщение с результатом выстрела.
        /// </summary>
        /// <param name="ship">Корабль по которому попали.</param>
        /// <param name="location">Позизия выстрела.</param>
        public static string GetShotResultMessage(Ship ship, Location location)
        {
            SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType type = SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Miss;

            if (ship != null)
            {
                if (ship.IsDead)
                    type = SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Kill;
                else
                    type = SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Damage;
            }

            ship = type == SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Kill ? ship : null;

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, Answer.EnumTypeToString(Answer.AnswerTypes.ShotResult));
            jObject.Add(JsonStructInfo.Result, SeaBattleClassLibrary.DataProvider.ShotResult.EnumTypeToString(type));
            jObject.Add(JsonStructInfo.AdditionalContent, Serializer<Ship>.Serialize(ship));
            jObject.Add(JsonStructInfo.Content, Serializer<Location>.Serialize(location));

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }

        /// <summary>
        /// Возвращает сообщение об ожидании второго игрока.
        /// </summary>
        public static string AwaitSecondPlayer()
        {
            Answer.AnswerTypes type = Answer.AnswerTypes.No;
            string typeStr = Answer.EnumTypeToString(type);

            JObject jObject = new JObject();
            jObject.Add(JsonStructInfo.Type, typeStr);
            jObject.Add(JsonStructInfo.Result, "");

            return jObject.ToString() + JsonStructInfo.EndOfMessage;
        }
    }
}