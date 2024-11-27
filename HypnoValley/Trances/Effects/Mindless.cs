using System;
using System.Collections.Generic;
using StardewValley;
using HypnoValley.Classes;
using Microsoft.Xna.Framework;

namespace HypnoValley.Trances.Effects
{
    public class Mindless : ITranceCommand
    {
        public Mindless()
        {
        }

        public override void Activate(TriggerVariant variant, NPC target, int level)
        {
            base.Activate(variant, target, level); //Removes trust, adds experience, and sets trance flags

            Trance trance = LocalData.ModData.Trances[target.Name];
            int cost = variant.TriggerCost;

            //Fail if trust is not high enough
            if (!HypnoValley.TrustCheck(trance.trustLevel, cost, Game1.player.getFriendshipHeartLevelForNPC(target.Name))) { OnFail(target, trance); return; }

            //Increase the target's trust
            trance.ModifyTrust(10 + level * 10);

            //Try to get dialogue, if not skip this step
            Dialogue response = target.TryGetDialogue("Kryspur.HypnoValley_Mindless");
            if (response != null)
                Game1.DrawDialogue(response);
        }

        public static void OnFail(NPC recipient, Trance trance)
        {
            trance.ModifyTrust(-100);
            Game1.DrawDialogue(recipient.TryGetDialogue("Kryspur.HypnoValley_MindlessFail"));
        }
    }
}
