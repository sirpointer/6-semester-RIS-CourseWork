using System;

namespace SeaBattleClassLibrary.DataProvider
{
    /// <summary>
    /// Типы попаданий.
    /// </summary>
    public static class ShotResult
    {
        /// <summary>
        /// Типы попаданий.
        /// </summary>
        public enum ShotResultType
        {
            Miss = 0,
            Damage = 1,
            Kill = 2
        }

        /// <summary>
        /// Типы попаданий для json.
        /// </summary>
        public static class ShootResultJsonTypes
        {
            public const string Miss = "miss";
            public const string Damage = "damage";
            public const string Kill = "kill";
        }
        
        public static ShotResultType JsonTypeToEnum(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                throw new ArgumentException("message", nameof(type));

            switch (type)
            {
                case ShootResultJsonTypes.Miss:
                    return ShotResultType.Miss;
                case ShootResultJsonTypes.Damage:
                    return ShotResultType.Damage;
                case ShootResultJsonTypes.Kill:
                    return ShotResultType.Kill;
            }

            throw new ArgumentException($"Неопределенный тип {nameof(type)} = {type}", nameof(type));
        }
        
        public static string EnumTypeToString(ShotResultType type)
        {
            switch (type)
            {
                case ShotResultType.Miss:
                    return ShootResultJsonTypes.Miss;
                case ShotResultType.Damage:
                    return ShootResultJsonTypes.Damage;
                case ShotResultType.Kill:
                    return ShootResultJsonTypes.Kill;
            }

            throw new ArgumentException($"Тип не определён, тип - {type.ToString()}.", nameof(type));
        }
    }

}
