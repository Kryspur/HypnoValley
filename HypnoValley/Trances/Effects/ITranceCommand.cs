using System;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using HypnoValley.Classes;

namespace HypnoValley.Trances.Effects
{
    public abstract class ITranceCommand
    {
        public virtual void Activate(TriggerVariant variant, NPC target, int level)
        {
            Trance activeTrance = LocalData.ModData.Trances[target.Name];

            if (level != 5)
                activeTrance.ModifyTrust(-1 * variant.TriggerCost / 2);
            LocalData.ModData.TranceEXP += variant.TriggerCost / 100;

            activeTrance.DailyTrigger = true;
        }
    }
}
