using System;
using System.Collections.Generic;
using HypnoValley.Classes;
using StardewValley;

namespace HypnoValley.Trances.Effects
{
    public class Amnesia : ITranceCommand
    {
        public Amnesia()
        {
        }

        public override void Activate(TriggerVariant variant, NPC target, int level)
        {
            base.Activate(variant, target, level); //Removes trust, adds experience, and sets trance flags

            //Sets global variables
            Trance trance = LocalData.ModData.Trances[target.Name];
            Dialogue response;
            int cost = variant.TriggerCost;

            //Fail if trust is not high enough
            if (!HypnoValley.TrustCheck(trance.trustLevel, cost, Game1.player.getFriendshipHeartLevelForNPC(target.Name))) { FailedUse(target); return; }

            switch (variant.ID)
            {
                //Forgets Name
                case "Kryspur.HypnoValley_AmnesiaName":
                    //Perform/Set-up Action
                    target.displayName = "???";

                    //Queues up dialogue and skip if not found
                    response = target.TryGetDialogue("Kryspur.HypnoValley_AmnesiaName");
                    if (response != null) Game1.DrawDialogue(response);
                    break;

                //Forgets gifts
                case "Kryspur.HypnoValley_AmnesiaGift":
                    //Set local variables
                    Random rng = new();
                    Friendship friendship = Game1.player.friendshipData[target.Name];

                    //Perform Action
                    if (friendship.GiftsToday > 0 && level < 2) friendship.GiftsThisWeek--; //Removes one gift given this week if one has been given
                    else if (level >= 2) friendship.GiftsThisWeek = 0; //Removes all gifts given this week if trance is strong enough
                    friendship.GiftsToday = 0; //Removes all gifts given today
                    friendship.Points -= rng.Next(20, 80); //Removes some friendship points from target

                    //Queues up dialogue
                    /*To-Do: Add dialogue for level 3+*/
                    response = level < 2 ? target.TryGetDialogue("Kryspur.HypnoValley_AmnesiaGiftWeak") : target.TryGetDialogue("Kryspur.HypnoValley_AmnesiaGiftWeak");
                    if (response != null) Game1.DrawDialogue(response);
                    break;
            }
        }

        private static void FailedUse(NPC recipient)
        {
            Dialogue failResponse = recipient.TryGetDialogue("Kryspur.HypnoValley_AmnesiaFail");
            if (failResponse != null) Game1.DrawDialogue(failResponse);
        }
    }
}
