namespace MetalStorage.Utils
{
    public static class Constants
    {
        /// <summary>
        /// Mod information
        /// </summary>
        public const string MOD_NAME = "MetalStorage";
        public const string MOD_VERSION = "1.0.0";
        public const string MOD_AUTHOR = "HazDS";
        public const string MOD_DESCRIPTION = "Adds metal variants of storage racks to the game.";

        /// <summary>
        /// MelonPreferences configuration
        /// </summary>
        public const string PREFERENCES_CATEGORY = MOD_NAME;

        /// <summary>
        /// Game-related constants
        /// </summary>
        public static class Game
        {
            public const string GAME_STUDIO = "TVGS";
            public const string GAME_NAME = "Schedule I";
        }

        /// <summary>
        /// Original storage rack IDs
        /// </summary>
        public static class StorageRacks
        {
            public const string SMALL = "smallstoragerack";
            public const string MEDIUM = "mediumstoragerack";
            public const string LARGE = "largestoragerack";
        }

        /// <summary>
        /// Metal storage rack IDs
        /// </summary>
        public static class ItemIds
        {
            public const string SMALL = "metalsmallstoragerack";
            public const string MEDIUM = "metalmediumstoragerack";
            public const string LARGE = "metallargestoragerack";
        }
    }
}
