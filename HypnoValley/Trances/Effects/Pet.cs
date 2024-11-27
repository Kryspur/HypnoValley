using HypnoValley.Classes;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HypnoValley.Trances.Effects
{
    public class Pet : ITranceCommand
    {
        public Pet()
        {
        }

        public override void Activate(TriggerVariant variant, NPC target, int level)
        {
            base.Activate(variant, target, level);  //Removes trust, adds experience, and sets trance flags

            //Sets variables
            Trance trance = LocalData.ModData.Trances[target.Name];
            int cost = variant.TriggerCost;
            Dialogue response;

            //Fails if the trust is not high enough
            if (!HypnoValley.TrustCheck(trance.trustLevel, cost, Game1.player.getFriendshipHeartLevelForNPC(target.Name))){ FailedUse(target); return; }
                   
            //Performs Action
            trance.CanPet = true; //Enables petting
            trance.IsFollowing = level >= 2; //Sets target to follow player if a strong enough trance was used
            target.ClearSchedule(); //Clears the target's schedule
            target.controller = null; //Stops all movement currently in progress

            //Queue up dialogue and skip if none found
            response = level switch
            {
                0 or 1 => target.TryGetDialogue("Kryspur.HypnoValley_AnimalPetWeak"),
                _ => target.TryGetDialogue("Kryspur.HypnoValley_AnimalPet"),
            };
            if (response != null) Game1.DrawDialogue(response);
        }

        public static void FailedUse(NPC recipient)
        {
            Dialogue failResponse = recipient.TryGetDialogue("Kryspur.HypnoValley_AnimalFail");
            if (failResponse != null) Game1.DrawDialogue(failResponse);
        }

        public static void PetNPC(NPC target, Trance trance)
        {
            target.doEmote(20);
            Game1.player.changeFriendship(25, target);
            trance.CanPet = false;
        }
    }
}
