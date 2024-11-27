using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Triggers;
using StardewValley.Delegates;
using StardewValley.GameData.Shops;

namespace ExampleHVMod
{
    internal sealed class ModEntry : Mod
    {
        IHypnoValleyAPI? hvApi;
        readonly string rawMantra = "Live to obey. Love to obey.$7";
        readonly List<NPC> mantraActive = new();

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            helper.Events.Content.AssetRequested += Content_AssetRequested;

            TriggerActionManager.RegisterAction("Kryspur.HypnoValleyMantra_Action", MantraAction); //Adds trigger action to perform the action given by a trigger (This naming scheme is too confusing I might change it later)
        }

        private void Content_AssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            //if (e.Name.IsEquivalentTo("Data/Shops"))
            //{
            //    e.Edit(asset =>
            //    {
            //        var data = asset.AsDictionary<string, ShopData>().Data;
            //        ShopItemData MantraLesson = new()
            //        {
            //            Id = "Kryspur.HypnoValleyMantra_MantraLessonShop",
            //            ItemId = "Kryspur.HypnoValleyMantra_MantraLesson",
            //            Condition = "True",
            //            Price = 1000,
            //            AvailableStock = 1
            //        };
            //        data["Kryspur.HypnoValley_KrisShop"].Items.Add(MantraLesson);
            //    }, AssetEditPriority.Default);
            //}
        }

        private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (mantraActive.Count == 0)
                return;

            foreach (NPC n in mantraActive)
            {
                n.TryLoadPortraits($"Mods/Kryspur.HypnoValley/{n.Name}_Entranced", out string error);
                if (error != null)
                    Monitor.Log(error, LogLevel.Error);

                n.CurrentDialogue.Clear();
                n.CurrentDialogue.Push(new(n, "", rawMantra));
            }
        }

        private void GameLoop_GameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            hvApi = Helper.ModRegistry.GetApi<IHypnoValleyAPI>("Kryspur.HypnoValley");
            if(hvApi == null)
                Monitor.Log("Cannot find mod API for Kryspur.HypnoValley", LogLevel.Error);
        }

        public bool MantraAction(string[] args, TriggerActionContext context, out string error)
        {
            if (!ArgUtility.TryGet(args, 1, out string variantName, out error, allowBlank: false))
                return false;
            if (!ArgUtility.TryGet(args, 2, out string npcName, out error, allowBlank: false))
                return false;
            if (!ArgUtility.TryGet(args, 3, out string triggerLevel, out error, allowBlank: false))
                return false;

            NPC target = Game1.getCharacterFromName(npcName);
            if (target == null) { error = $"Cannot find NPC: {npcName}"; return false; }

            if (!int.TryParse(triggerLevel, out int level)) { error = $"{triggerLevel} is not a valid integer"; return false; }

            if (hvApi == null)
                return false;

            hvApi.ActivateTrance(npcName, level, 200);

            switch (variantName)
            {
                case "Mantra/DailyMantra":
                    mantraActive.Add(target);
                    break;
                default:
                    break;
            }

            return true;
        }
    }
}