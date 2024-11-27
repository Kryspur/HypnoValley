using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HypnoValley.Classes
{
    public class HVApi
    {
        /// <summary>
        /// This activates during all of the vanilla trances so call this if you want normal behavior
        /// </summary>
        /// <param name="characterName">Name of the character being entranced</param>
        /// <param name="level">Tool level being used</param>
        /// <param name="cost">Cost of the trance variant</param>
        public void ActivateTrance(string characterName, int level, int cost)
        {
            Trance activeTrance = LocalData.ModData.Trances[characterName];

            if (level != 5)
                activeTrance.ModifyTrust(-1 * cost / 2);
            LocalData.ModData.TranceEXP += cost / 100;

            activeTrance.DailyTrigger = true;
        }

        /// <summary>
        /// Modifies trust level for the specified character
        /// </summary>
        /// <param name="characterName">Name of the character whose trust is being edited</param>
        /// <param name="amount">Amount to modify trust by</param>
        public void ModifyTrust(string characterName, int amount)
        {
            Trance activeTrance = LocalData.ModData.Trances[characterName];
            activeTrance.ModifyTrust(amount);
        }
    }
}
