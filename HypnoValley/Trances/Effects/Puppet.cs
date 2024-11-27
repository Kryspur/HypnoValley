using HypnoValley.Classes;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HypnoValley.Trances.Effects
{
    public class Puppet : ITranceCommand
    {
        public Puppet()
        {
        }

        public override void Activate(TriggerVariant variant, NPC target, int level)
        {
            base.Activate(variant, target, level);

            //Set up variables
            Trance trance = LocalData.ModData.Trances[target.Name];
            int cost = variant.TriggerCost;
            Dialogue response;

            //Fail if trust is not high enough
            if (!HypnoValley.TrustCheck(trance.trustLevel, cost, Game1.player.getFriendshipHeartLevelForNPC(target.Name))) { FailedUse(target); return; }

            //Stops target
            target.ClearSchedule();
            target.stopWithoutChangingFrame();
            target.controller = null;
            target.faceTowardFarmer = false;

            //Queues up dialogue and skip if not found
            response = target.TryGetDialogue("Kryspur.HypnoValley_PuppetDoll");
            if (response != null) Game1.DrawDialogue(response);
        }

        private static void FailedUse (NPC target)
        {
            Dialogue failResponse = target.TryGetDialogue("Kryspur.HypnoValley_PuppetFail");
            if (failResponse != null) Game1.DrawDialogue(failResponse);
        }
    }
}
