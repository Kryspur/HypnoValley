using HypnoValley.Classes;
using HypnoValley.UI;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Tools;
using StardewValley.Triggers;
using System;
using System.Threading;

namespace HypnoValley.Trances
{
    public class TranceManager
    {
        /// <summary>
        /// Parse action for using hypnosis tool
        /// </summary>
        /// <param name="target">NPC being targeted by tool</param>
        /// <param name="hypnoToolUsed">Tool being used</param>
        public static void TryToUse(NPC target, ToolData hypnoToolData)
        {
            bool isFirstTime = false;

            //Checks if character has been hypnotized before
            if (!LocalData.ModData.Trances.ContainsKey(target.Name))
            {
                //Check if character can be hypnotized
                //Adds data if they have not been hypnotized before
                CharacterData data = target.GetData();

                //Check if character has the "CanTrance" property and if it's true, then add new trance save data

                if (data != null &&
                    data.CustomFields != null &&
                    data.CustomFields.ContainsKey("Kryspur.HypnoValley_CanTrance"))
                {
                    if (data.CustomFields["Kryspur.HypnoValley_CanTrance"] == "True")
                    {
                        LocalData.ModData.Trances.Add(target.Name, new Trance(target.Name));
                        isFirstTime = true;
                    }
                    else
                    {
                        Game1.addHUDMessage(new($"{target.Name} cannot be tranced", 3));
                        return;
                    }
                }
                else
                {
                    Game1.addHUDMessage(new($"{target.Name} cannot be tranced", 3));
                    return;
                }
            }

            Trance targetTrance = LocalData.ModData.Trances[target.Name];

            //Checks if character has been given a command today
            if (targetTrance.DailyTrigger) return;

            //Checks if character is currently hypnotized
            if (targetTrance.CurrentTrance)
            {
                if (!hypnoToolData.ModData.ContainsKey("Kryspur.HypnoValley_MaxTranceLevel"))
                { ModEntry.Log.Log("Hypnosis tool does not try to define trigger strength", StardewModdingAPI.LogLevel.Error); return; }

                if (!int.TryParse(hypnoToolData.ModData["Kryspur.HypnoValley_MaxTranceLevel"], out int maxTranceLevel))
                    maxTranceLevel = 0;

                Game1.activeClickableMenu = new TranceMenu(target, maxTranceLevel, OnAction);
                return;
            }

            //Get mod data for the tool
            int trustUp = int.Parse(hypnoToolData.ModData["Kryspur.HypnoValley_BaseTrustIncrease"]);

            #region To-do: Change region to hypnosis minigame
            targetTrance.ModifyTrust(trustUp);
            targetTrance.DailyTrance = true;
            targetTrance.CurrentTrance = true;
            targetTrance.TranceExpire = Game1.timeOfDay + 100;
            target.ChooseAppearance();

            Dialogue toolReaction;
            if (isFirstTime)
                toolReaction = target.TryGetDialogue("Kryspur.HypnoValley_FirstTime");
            else
                toolReaction = target.TryGetDialogue(hypnoToolData.Name);

            if (toolReaction != null) Game1.DrawDialogue(toolReaction);
            #endregion

            //Game1.addHUDMessage(new($"Hypnosis Complete. Current trust: {targetTrance.TrustLevel}", 1));
        }
        /// <summary>
        /// Parse action for using hypnotic gift
        /// </summary>
        /// <param name="target">NPC being gifted</param>
        /// <param name="giftData">Object data of the gift</param>
        public static void TryToGift(NPC target, ObjectData giftData)
        {
            //Checks if character has been hypnotized before
            if (!LocalData.ModData.Trances.ContainsKey(target.Name))
            {
                //Check if character can be hypnotized
                //Adds data if they have not been hypnotized before
                CharacterData data = target.GetData();

                //Check if character has the "CanTrance" property and if it's true, then add new trance save data
                if (data != null && data.CustomFields.ContainsKey("Kryspur.HypnoValley_CanTrance"))
                    if (data.CustomFields["Kryspur.HypnoValley_CanTrance"] == "True")
                        LocalData.ModData.Trances.Add(target.Name, new Trance(target.Name));
                    else
                        return;
            }

            Trance targetTrance = LocalData.ModData.Trances[target.Name];

            //Increase trust levels
            _ = int.TryParse(giftData.CustomFields["Kryspur.HypnoValley_TrustUp"], out int trustUp);
            targetTrance.ModifyTrust(trustUp);

            //Increase trance experience
            _ = int.TryParse(giftData.CustomFields["Kryspur.HypnoValley_TranceExp"], out int expUp);
            LocalData.ModData.TranceEXP += expUp;

            //Custom actions
            switch (giftData.Name)
            {
                case "Kryspur.HypnoValley_HypnoChip":
                    targetTrance.ChipImplanted = true;
                    break;
                default:
                    break;
            }

            //Game1.addHUDMessage(new($"Hypnosis Complete. Current trust: {targetTrance.TrustLevel}. Current Exp: {LocalData.ModData.TranceEXP}", 1));
        }

        public static void OnAction(NPC target, TriggerVariant trigger, int toolLevel)
        {
            //Ensures the NPC has friendship data
            if (!Game1.player.friendshipData.ContainsKey(target.Name))
                Game1.player.friendshipData.Add(target.Name, new Friendship());

            if (!TriggerActionManager.TryRunAction(trigger.Action + ' ' + target.Name + ' ' + toolLevel, out string error, out Exception ex))
                ModEntry.Log.Log(error + ex, StardewModdingAPI.LogLevel.Error);
        }
    }
}
