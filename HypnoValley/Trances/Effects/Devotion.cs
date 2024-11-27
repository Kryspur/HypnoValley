using HypnoValley.Classes;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HypnoValley.Trances.Effects
{
    public class Devotion : ITranceCommand
    {
        public Devotion()
        {
        }

        public override void Activate(TriggerVariant variant, NPC target, int level)
        {
            base.Activate(variant, target, level);

            //Set up variables
            Trance trance = LocalData.ModData.Trances[target.Name];
            int cost = variant.TriggerCost;
            Friendship friendship = Game1.player.friendshipData[target.Name];
            Dialogue response;

            //Fails if the trust is not high enough
            if (!HypnoValley.TrustCheck(trance.trustLevel, cost, Game1.player.getFriendshipHeartLevelForNPC(target.Name))){ FailedUse(target, friendship, trance, cost); return; }

            //Fails if other condition
            
            if (!target.GetData().CustomFields.ContainsKey("Kryspur.HypnoValley_Inventory"))
                return;
            Item gift = ItemRegistry.Create(target.GetData().CustomFields["Kryspur.HypnoValley_Inventory"], 1, 0, true);

            if (!Game1.player.couldInventoryAcceptThisItem(gift)){ Game1.drawDialogueBox("Cannot hold more items"); return; }

            //Perform/Set-up Action
            if (gift == null)
                ModEntry.Log.Log($"Item {target.GetData().CustomFields["Kryspur.HypnoValley_Inventory"]} does not exist", StardewModdingAPI.LogLevel.Error);
            else
                Game1.player.addItemToInventory(ItemRegistry.Create(target.GetData().CustomFields["Kryspur.HypnoValley_Inventory"]));

            //Queues up dialogue and skips if not found
            response = target.TryGetDialogue("Kryspur.HypnoValley_DevotionGiftWeak");
            if (response != null) Game1.DrawDialogue(response);
        }

        public static void FailedUse(NPC recipient, Friendship friendship, Trance trance, int tranceReq)
        {
            Dialogue failResponse = recipient.TryGetDialogue("Kryspur.HypnoValley_DevotionFail");
            if (failResponse != null) Game1.DrawDialogue(failResponse);

            friendship.Points -= 50;
            trance.ModifyTrust(Math.Max(-100, -1 * tranceReq / 2));
        }
    }
}
