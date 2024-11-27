using System;
using StardewValley;

namespace HypnoValley
{
    [Flags]
    public enum Triggers
    {
        None = 0,
        Lovers = 1,
        Amnesia = 2,
        Animal = 4,
        Puppet = 8,
        Mindless = 16,
        Devotion = 32,
    }

    public static class Converter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="days">Number of days to add</param>
        /// <returns>World Date representing x days after the current in-game day</returns>
        public static WorldDate AddDaysToCurrentWorldDate(int days)
        {
            WorldDate currentDay = Game1.Date;
            WorldDate newDay = currentDay;

            //Advance day of month
            newDay.DayOfMonth = currentDay.DayOfMonth + days;
            if (newDay.DayOfMonth <= 28) { return newDay; }

            //Advance season
            int seasonIndex = currentDay.SeasonIndex + (int)Math.Floor(newDay.DayOfMonth / 28d);
            newDay.DayOfMonth %= 28; newDay.DayOfMonth++;
            if (seasonIndex >= 4)
            {
                //Advance year
                newDay.Year = currentDay.Year + (int)Math.Floor((seasonIndex + 1) / 4d);
                seasonIndex %= 4;
            }
            switch (seasonIndex)
            {
                case 0:
                    newDay.Season = Season.Spring;
                    break;
                case 1:
                    newDay.Season = Season.Summer;
                    break;
                case 2:
                    newDay.Season = Season.Fall;
                    break;
                case 3:
                    newDay.Season = Season.Winter;
                    break;
            }
            return newDay;
        }
    }

    public static class HypnoValley
    {
        public static bool TrustCheck(int TrustLevel, int ReqTrust, int HeartLevel)
        {
            Random rng = new();
            if (TrustLevel <= 0) return false; //For certain heart levels, a negative trust level can lead to a high rate of success. 0 or lower should always fail
            if (Math.Pow((double)TrustLevel / (double)ReqTrust, 6d / (HeartLevel + 1d)) >= rng.NextDouble() || TrustLevel >= ReqTrust)
                return true;
            return false;
        }

        public static int SuccessChance(int TrustLevel, int ReqTrust, int HeartLevel)
        {
            if (TrustLevel >= ReqTrust) return 100;
            else if (TrustLevel <= 0) return 0;
            else return (int)Math.Floor(Math.Pow((double)TrustLevel / (double)ReqTrust, 6d / (HeartLevel + 1d)) * 100);
        }
    }
}
