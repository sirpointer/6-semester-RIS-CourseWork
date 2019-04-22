using System;

namespace SeaBattleClassLibrary.DataProvider
{
    public class ShootResult
    {
        /// <summary>
        /// Типы выстрелов.
        /// </summary>
        public enum ShootResultType
        {
            Miss = 0,
            Damage = 1,
            Kill = 2
        }

        public static class ShootResultJsonTypes
        {
            public const string Miss = "miss";
            public const string Damage = "damage";
            public const string Kill = "kill";
        }

        public static ShootResultType JsonTypeToEnum(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("message", nameof(type));

            switch (type)
            {
                case ShootResultJsonTypes.Miss:
                    return ShootResultType.Miss;
                case ShootResultJsonTypes.Damage:
                    return ShootResultType.Damage;
                case ShootResultJsonTypes.Kill:
                    return ShootResultType.Kill;
            }

            throw new ArgumentException($"Неопределенный тип {nameof(type)} = {type}", nameof(type));
        }
        
        public static string EnumTypeToString(ShootResultType type)
        {
            switch (type)
            {
                case ShootResultType.Miss:
                    return ShootResultJsonTypes.Miss;
                case ShootResultType.Damage:
                    return ShootResultJsonTypes.Damage;
                case ShootResultType.Kill:
                    return ShootResultJsonTypes.Kill;
            }

            throw new ArgumentException($"Тип не определён, тип - {type.ToString()}.", nameof(type));
        }
    }

}
