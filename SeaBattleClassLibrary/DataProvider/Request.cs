using System;

namespace SeaBattleClassLibrary.DataProvider
{
    public class Request
    {
        /// <summary>
        /// Типы запросов.
        /// </summary>
        public enum RequestTypes
        {
            AddGame = 0,
            Shot = 1,
            SetField = 2,
            BadRequest = 4
        }

        /// <summary>
        /// Типы запросов для JSON.
        /// </summary>
        public static class RequestJsonTypes
        {
            public const string SetField = "setField";
            public const string Shot = "shot";
            public const string AddGame = "addGame";
        }

        /// <summary>
        /// Конвертирует тип из JSON строки в RequestTypes.
        /// </summary>
        public static RequestTypes JsonTypeToEnum(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return RequestTypes.BadRequest;

            switch (type)
            {
                case RequestJsonTypes.SetField:
                    return RequestTypes.SetField;
                case RequestJsonTypes.Shot:
                    return RequestTypes.Shot;
                case RequestJsonTypes.AddGame:
                    return RequestTypes.AddGame;
                default:
                    return RequestTypes.BadRequest;
            }
        }

        /// <summary>
        /// Конвертирует RequestTypes в строку для JSON запроса.
        /// </summary>
        public static string EnumTypeToString(RequestTypes type)
        {
            switch (type)
            {
                case RequestTypes.AddGame:
                    return RequestJsonTypes.AddGame;
                case RequestTypes.Shot:
                    return RequestJsonTypes.Shot;
                case RequestTypes.SetField:
                    return RequestJsonTypes.SetField;
                case RequestTypes.BadRequest:
                    throw new ArgumentException("Невозможно преобразовать BadRequest", nameof(type));
            }

            throw new ArgumentException($"Тип не определён, тип - {type.ToString()}.", nameof(type));
        }
    }
}