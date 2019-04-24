using System;

namespace SeaBattleClassLibrary.DataProvider
{
    /// <summary>
    /// Типы ответов от сервера.
    /// </summary>
    public class Answer
    {
        /// <summary>
        /// Типы ответов.
        /// </summary>
        public enum AnswerTypes
        {
            Ok = 0,
            Error = 1,
            ShotOfTheEnemy = 2,
            GameReady = 4,
            ShootResult = 8
        }

        /// <summary>
        /// Типы запросов для JSON.
        /// </summary>
        public static class AnswerJsonTypes
        {
            public const string Ok = "ok";
            public const string Error = "error";
            public const string ShotOfTheEnemy = "shotOfTheEnemy";
            public const string GameReady = "gameReady";
            public const string ShootResult = "shootResult";
        }

        /// <summary>
        /// Конвертирует тип из JSON строки в AnswerTypes.
        /// </summary>
        public static AnswerTypes JsonTypeToEnum(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("message", nameof(type));

            switch (type)
            {
                case AnswerJsonTypes.Error:
                    return AnswerTypes.Error;
                case AnswerJsonTypes.GameReady:
                    return AnswerTypes.GameReady;
                case AnswerJsonTypes.Ok:
                    return AnswerTypes.Ok;
                case AnswerJsonTypes.ShotOfTheEnemy:
                    return AnswerTypes.ShotOfTheEnemy;
                case AnswerJsonTypes.ShootResult:
                    return AnswerTypes.ShootResult;
            }

            throw new ArgumentException($"Неопределенный тип {nameof(type)} = {type}", nameof(type));
        }

        /// <summary>
        /// Конвертирует AnswerTypes в строку для JSON запроса.
        /// </summary>
        public static string EnumTypeToString(AnswerTypes type)
        {
            switch (type)
            {
                case AnswerTypes.Ok:
                    return AnswerJsonTypes.Ok;
                case AnswerTypes.Error:
                    return AnswerJsonTypes.Error;
                case AnswerTypes.ShotOfTheEnemy:
                    return AnswerJsonTypes.ShotOfTheEnemy;
                case AnswerTypes.GameReady:
                    return AnswerJsonTypes.GameReady;
                case AnswerTypes.ShootResult:
                    return AnswerJsonTypes.ShootResult;
            }

            throw new ArgumentException($"Тип не определён, тип - {type.ToString()}.", nameof(type));
        }
    }
}
