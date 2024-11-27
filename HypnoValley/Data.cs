using System;
using System.Collections.Generic;
using HypnoValley.Classes;
using StardewModdingAPI;
using StardewValley;


namespace HypnoValley
{
    public class Data
    {
        public Dictionary<string, Trance> Trances = new();
        public int TranceEXP = 0;

        /// <summary>
        /// Creates new save data
        /// </summary>
        public Data()
        {
            //Create new dictionary to save trance data
            Trances = new Dictionary<string, Trance>();
        }

        /// <summary>
        /// Get trance information for characters
        /// </summary>
        /// <returns>Human readable trance data. Useful for debugging</returns>
        public override string ToString()
        {
            string output = null;
            foreach (string name in Trances.Keys)
                output += $"{name}: {Trances[name].trustLevel}\n";
            return output;
        }

        /// <summary>
        /// Returns the equivalent skill level based on the current player's trance experience
        /// </summary>
        /// <returns></returns>
        public int GetTranceLevel()
        {
            if (TranceEXP < 100)
                return 0;
            else if (TranceEXP < 380)
                return 1;
            else if (TranceEXP < 770)
                return 2;
            else if (TranceEXP < 1300)
                return 3;
            else if (TranceEXP < 2150)
                return 4;
            else if (TranceEXP < 3300)
                return 5;
            else if (TranceEXP < 4800)
                return 6;
            else if (TranceEXP < 6900)
                return 7;
            else if (TranceEXP < 10000)
                return 8;
            else if (TranceEXP < 15000)
                return 9;
            else
                return 10;
        }

        public static bool IsTriggerLearned(string triggerName)
        {
            return Game1.player.mailReceived.Contains($"Kryspur.HypnoValley_{triggerName}");
        }
    }

    public class LocalData
    {
        private static Dictionary<string, Trigger> masterTriggerList;
        public static Dictionary<string, Trigger> TriggerList { get => masterTriggerList; set => masterTriggerList = value; }

        private static Data modData;
        public static Data ModData { get => modData; set => modData = value; }

        static LocalData()
        {
        }

    }
}
