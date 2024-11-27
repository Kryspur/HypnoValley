using System;
using System.Collections.Generic;
using HypnoValley.Classes;
using HypnoValley.Trances;
using HypnoValley.Trances.Effects;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.GameData.Objects;
using StardewValley.Pathfinding;
using StardewValley.Triggers;

namespace HypnoValley
{
    public class ModEntry : Mod
    {
        private const bool isDebug = false;
        private static IMonitor log;
        public static IMonitor Log { get => log; set => log = value; }

        private Point warpPos = new(0, 0);

        /// <summary>
        /// Called when the mod is first loaded.
        /// </summary>
        /// <param name="helper"></param>
        public override void Entry(IModHelper helper)
        {
            Log = Monitor;

            helper.Events.Content.AssetRequested += Content_OnAssetRequested; //Adds event for loading data
            helper.Events.Input.ButtonPressed += Input_ButtonPressed; //Adds event for player input
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking; //Adds event for frame updates
            helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked; //Adds event for after game updates every second
            helper.Events.Player.Warped += Player_Warped; //Adds event for changing location
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded; //Adds event for leading save
            helper.Events.GameLoop.Saving += GameLoop_Saving; //Adds event for saving
            helper.Events.GameLoop.DayEnding += GameLoop_DayEnding; //Adds event for the end of day

            GameStateQuery.Register("Kryspur.HypnoValley_Hypnotized", new GameStateQueryDelegate(HVQuery_IsHypnotized)); //Adds game state query to check if character is hypnotized
            GameStateQuery.Register("Kryspur.HypnoValley_Learned", new GameStateQueryDelegate(HVQuery_LearnedCommand)); //Adds game state query to check if trigger is learned

            TriggerActionManager.RegisterAction("Kryspur.HypnoValley_TriggerAction", TriggerAction); //Adds trigger action to perform the action given by a trigger (This naming scheme is too confusing I might change it later)
            
            helper.ConsoleCommands.Add("trance_setlevel", "Sets the NPC's trust level.\n\nUsage: trance_setlevel <name> <amount>\n- name: the name of the NPC\n- amount: the integer level", SetLevel); //Adds a SMAPI console command to set trance level
            helper.ConsoleCommands.Add("trance_addallcommands", "Makes player learn all commands", AddAllCommands); //Adds a SMAPI console command to learn all commands

            LocalData.ModData = new Data();
        }

        /// <summary>
        /// Sets up the API to allow for custom interactions with this mod
        /// </summary>
        /// <returns></returns>
        public override object GetApi()
        {
            return new HVApi();
        }

        #region Asset editor
        /// <summary>
        /// Raised when the game attempts to load an external asset from the content or mod folders
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="e">The event data</param>
        private void Content_OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (Resources.AssetLoader.CanLoad(e.Name)) Resources.AssetLoader.Load(e);
        }
        #endregion

        #region Game Events
        /// <summary>
        /// Called when the player presses a button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady) return; //Ignores if save isn't loaded
            if (!Context.IsPlayerFree) return; //Ignores if player is in a menu
            if (Game1.isFestival()) return; //Ignores if a festival is active (Just feels wrong to mess up the holidays. Plus things break because of the festival coding)

            //This is just stuff for testing ignore this part. You can't access it anyways
            if (isDebug)
            {
                string testNPCName = "Pierre"; //Set NPC to run tests with
                string testItemID = "HypnoPendulum"; //Set item to run tests with
                if (e.Button == SButton.NumPad0) //Reset test NPC friendship and add test items to player inventory
                {
                    //Teleport test NPC to player
                    NPC testNPC = Game1.getCharacterFromName(testNPCName);
                    testNPC.currentLocation.characters.Remove(testNPC);
                    testNPC.currentLocation = Game1.player.currentLocation;
                    testNPC.Position = Game1.player.Position;
                    Game1.player.currentLocation.characters.Add(testNPC);

                    //Reset NPC friendship so more gifts can be given
                    if (Game1.player.friendshipData.ContainsKey(testNPCName))
                        Game1.player.friendshipData.Remove(testNPCName);
                    Game1.player.friendshipData.Add(testNPCName, new Friendship());
                    Game1.player.friendshipData[testNPCName].GiftsToday = 0;
                    Game1.player.friendshipData[testNPCName].GiftsThisWeek = 0;

                    //Give player test items and max out inventory size
                    Game1.player.setInventory(new List<Item>());
                    Game1.player.increaseBackpackSize(24);
                    Game1.player.addItemToInventory(ItemRegistry.Create($"Kryspur.HypnoValley_{testItemID}"));
                }
                else if (e.Button == SButton.NumPad1) //Opens the trigger menu for the Test NPC
                {
                    if (!LocalData.ModData.Trances.ContainsKey(testNPCName))
                        LocalData.ModData.Trances.Add(testNPCName, new(testNPCName));

                    AddAllCommands("", null);
                    LocalData.ModData.Trances[testNPCName].SetTrustLevel(2500);

                    LocalData.ModData.Trances[testNPCName].DailyTrance = true;
                    LocalData.ModData.Trances[testNPCName].CurrentTrance = true;
                    Game1.activeClickableMenu = new UI.TranceMenu(Game1.getCharacterFromName(testNPCName), 5, TranceManager.OnAction);
                }
                else if (e.Button == SButton.NumPad2)
                {
                    Game1.player.warpFarmer(new Warp(0, 0, "Saloon", 27, 13, false));
                    Game1.PlayEvent("Kryspur.HypnoValley_FarmerChickenShow", false, false);
                }
                else if (e.Button == SButton.NumPad3)
                    Game1.PlayEvent("Kryspur.HypnoValley_FarmerChickenShow", false, false);
                else if (e.Button == SButton.NumPad9) //Warp player to test NPC
                {
                    //if (!LocalData.ModData.Trances.ContainsKey(testNPCName))
                    //    LocalData.ModData.Trances.Add(testNPCName, new(testNPCName));

                    NPC testNPC = Game1.getCharacterFromName(testNPCName);
                    Game1.player.warpFarmer(new Warp(0, 0, testNPC.currentLocation.Name, testNPC.TilePoint.X, testNPC.TilePoint.Y, false));
                    Game1.player.Position = testNPC.Position;

                    //AddAllCommands("", null);
                    //LocalData.ModData.Trances[testNPCName].SetTrustLevel(2500);
                    //LocalData.ModData.Trances[testNPCName].DailyTrigger = false;
                }
            }

            //Use Hypno Tool
            //Checks if player is using tool and clicking
            if (Game1.player.CurrentTool != null &&
                e.Button.IsUseToolButton())
            {
                Tool heldTool = Game1.player.CurrentTool; //Set held tool
                NPC clickNPC = Game1.currentLocation.isCharacterAtTile(e.Cursor.GrabTile); //Checks if NPC is being clicked
                clickNPC ??= Game1.currentLocation.isCharacterAtTile(new(e.Cursor.GrabTile.X, e.Cursor.GrabTile.Y + 1)); //Checks if you're aiming for the head

                //Checks if tool has modded data, if that modded data includes the hypno tool flag, and if the NPC clicked is valid
                if (heldTool.modData != null &&
                    heldTool.modData.ContainsKey("Kryspur.HypnoValley_IsHypnoTool") &&
                    clickNPC != null
                    )
                    //Move to trance manager to manage tool action
                    if (heldTool.modData["Kryspur.HypnoValley_IsHypnoTool"] == "True") TranceManager.TryToUse(clickNPC, heldTool.GetToolData());
            }

            //Use Hypno Item
            //Checks if player is using an item
            if (Game1.player.CurrentItem != null &&
                (e.Button.IsActionButton() ||
                e.Button.IsUseToolButton()))
            {
                Item heldItem = Game1.player.CurrentItem; //Grab player's currently held item

                //Try to grab the source data for the item
                ObjectData giftData;
                try{ giftData = (ObjectData)ItemRegistry.GetData(heldItem.ItemId).RawData; }
                catch (Exception){ giftData = null; }

                if (giftData != null && 
                    giftData.CustomFields != null) //Ignore if held item is an error item or is a vanilla item
                {
                    NPC clickNPC = Game1.currentLocation.isCharacterAtTile(e.Cursor.GrabTile); //See if NPC is being clicked

                    //Checks if held item is a giftable hypno item and if NPC is being clicked
                    if (giftData.CustomFields.ContainsKey("Kryspur.HypnoValley_IsHypnoGift") &&
                        clickNPC != null)
                    {
                        //Move to trance manager to manage gift action
                        if (giftData.CustomFields["Kryspur.HypnoValley_IsHypnoGift"] == "True") TranceManager.TryToGift(clickNPC, giftData);
                    }

                    //Checks if held item is a lesson book
                    else if (giftData.CustomFields.ContainsKey("Kryspur.HypnoValley_IsLesson"))
                    {

                    }
                }
            }

            //Checks if hypnosis actions can be used
            if (e.Button.IsActionButton() || e.Button.IsUseToolButton())
            {
                NPC activeNPC = Game1.currentLocation.isCharacterAtTile(e.Cursor.GrabTile); //Finds the character being interacted with
                if (activeNPC == null) return; //Ends if no character is found

                if (!LocalData.ModData.Trances.ContainsKey(activeNPC.Name)) return; //Ends if character has never been hypnotised

                #region Love 1: Kiss NPC
                if (LocalData.ModData.Trances[activeNPC.Name].LoversKiss && activeNPC.CurrentDialogue.Count == 0) //Checks if NPC has the trigger and has no queued dialogue
                    Lovers.KissFarmer(activeNPC, Game1.player);
                #endregion
                #region Pet 1: Pet NPC
                if (LocalData.ModData.Trances[activeNPC.Name].CanPet && activeNPC.CurrentDialogue.Count == 0) //Checks if NPC has the trigger and has no queued dialogue
                    Pet.PetNPC(activeNPC, LocalData.ModData.Trances[activeNPC.Name]);
                #endregion
            }
        }
        
        /// <summary>
        /// Called every frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return; //Ignore if save is not loaded or if player is in menu

            //Frame update logic
            foreach (Trance t in LocalData.ModData.Trances.Values)
            {
                t.OnFrameUpdate();

                //Following logic
                #region Following
                if (!t.IsFollowing) continue; //Checks if NPC should be following player

                NPC target = Game1.getCharacterFromName(t.NPCName); //Sets NPC variable

                Point expectedLocation = new(0, 0); //Sets up next expected point to go to

                target.addedSpeed = ((target.Tile - Game1.player.Tile).Length() > 2) ? 4 : Game1.player.running ? 2.5f : 0; //Checks what the NPC's walking speed should be to ensure they don't fall too far behind player

                if ((Game1.player.Position - target.Position).Length() < 0.5) //Don't move NPC if they're within half a tile of the player
                    expectedLocation = target.TilePoint;
                else if (Game1.player.currentLocation.NameOrUniqueName != target.currentLocation.NameOrUniqueName)  //Check if NPC is not in the same map as the player
                {
                    if (Game1.player.TilePoint != warpPos)
                    {
                        target.currentLocation.characters.Remove(target);
                        target.currentLocation = Game1.player.currentLocation;
                        target.currentLocation.characters.Add(target);
                        target.setTilePosition(warpPos);
                        expectedLocation = warpPos;
                    } //Check if the player has moved away from the point they warped in, if so warp the NPC to that point
                }
                else //Normal pathing behavior
                {
                    expectedLocation = target.TilePoint - Game1.player.TilePoint; //Draw a line from the NPC to the Player
                    Vector2 temp = expectedLocation.ToVector2(); //Convert line to Vector
                    if (temp.X != 0 || temp.Y != 0) //Set the distance of the line to 1 tile. Ignore if player and NPC are on the same spot (Sometimes math breaks and tries to move NPC anyways, even though this shouldn't run if player/NPC are within 0.5 tiles)
                        temp.Normalize();
                    expectedLocation = temp.ToPoint(); //Convert vector back to point line
                    expectedLocation += Game1.player.TilePoint; //Set NPC moving to next tile in direction of player
                }

                Stack<Point> path = PathFindController.findPathForNPCSchedules(target.TilePoint, expectedLocation, Game1.player.currentLocation, int.MaxValue); //Uses built-in pathing data to move NPC to next point

                if (path == null || path.Count < 2) //Ignore if there are no spots to move between current spot and next 
                    continue;

                Point dir = path.Pop() - path.Pop(); //Find the direction NPC should be moving next (Up/Down/Left/Right)

                //Face the NPC in the direction they should be moving.
                //Try to center them along the tile (Pathfinding assumes they're aligned with the tile but player can be misaligned, causing NPC to misalign and pathing to break. This will correct that)
                //Start moving NPC in the necessary direction
                if (dir.X == 0)
                {
                    if (dir.Y < 0)
                    {
                        target.faceDirection(2);
                        target.Position = target.Position;
                        if ((target.Tile.X * 64 - target.Position.X) < 0)
                            target.Position = new Vector2(target.Position.X - 1, target.Position.Y);
                        else if ((target.Tile.X * 64 - target.Position.X) > 0)
                            target.Position = new Vector2(target.Position.X + 1, target.Position.Y);
                        target.tryToMoveInDirection(2, false, 0, false);
                    }
                    else
                    {
                        target.faceDirection(0);
                        if ((target.Tile.X * 64 - target.Position.X) < 0)
                            target.Position = new Vector2(target.Position.X - 1, target.Position.Y);
                        else if ((target.Tile.X * 64 - target.Position.X) > 0)
                            target.Position = new Vector2(target.Position.X + 1, target.Position.Y);
                        target.tryToMoveInDirection(0, false, 0, false);
                    }
                }
                else
                {
                    if (dir.X < 0)
                    {
                        target.faceDirection(1);
                        if ((target.Tile.Y * 64 - target.Position.Y) < 0)
                            target.Position = new Vector2(target.Position.X, target.Position.Y - 1);
                        else if ((target.Tile.Y * 64 - target.Position.Y) > 0)
                            target.Position = new Vector2(target.Position.X, target.Position.Y + 1);
                        target.tryToMoveInDirection(1, false, 0, false);
                    }
                    else
                    {
                        target.faceDirection(3);
                        if ((target.Tile.Y * 64 - target.Position.Y) < 0)
                            target.Position = new Vector2(target.Position.X, target.Position.Y - 1);
                        else if ((target.Tile.Y * 64 - target.Position.Y) > 0)
                            target.Position = new Vector2(target.Position.X, target.Position.Y + 1);
                        target.tryToMoveInDirection(3, false, 0, false);
                    }
                }

                //Animate the walking sprite
                target.animateInFacingDirection(Game1.currentGameTime);
                #endregion
            }
        }
        /// <summary>
        /// Called every second after game updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree) return; //Ignore if player cannot move or game is not loaded

            //Check if any trances have updates available
            foreach (Trance t in LocalData.ModData.Trances.Values)
                t.OnSecondUpdate();
        }
        
        /// <summary>
        /// Called when farmer enters new location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            warpPos = e.Player.TilePoint; //Sets as part of following logic
        }

        /// <summary>
        /// Called when the player goes to sleep
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            LocalData.TriggerList ??= Game1.content.Load<Dictionary<string, Trigger>>("Mods/Kryspur.HypnoValley_Triggers"); //Loads list of available triggers if starting a new save file

            foreach (Trance t in LocalData.ModData.Trances.Values)
                t.NewDay();
        }
        /// <summary>
        /// Called when game is saving at the end of the day
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_Saving(object sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("ModData", LocalData.ModData);
        }
        /// <summary>
        /// Called when a save file is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            LocalData.ModData = Helper.Data.ReadSaveData<Data>("moddata") ?? new Data();
            LocalData.TriggerList = Game1.content.Load<Dictionary<string, Trigger>>("Mods/Kryspur.HypnoValley_Triggers");
        }
        #endregion

        #region Custom Queries
        /// <summary>
        /// Game State Query to check if farmer has hypnotized the specified character
        /// </summary>
        /// <param name="npcName">The name of the character to check</param>
        /// <returns></returns>
        private bool HVQuery_IsHypnotized(string[] query, GameStateQueryContext context)
        {
            string npcName = query[1];
            if (!LocalData.ModData.Trances.ContainsKey(npcName)) return false;
            if (!LocalData.ModData.Trances[npcName].CurrentTrance) return false;

            return true;
        }
        /// <summary>
        /// Game State Query to check if farmer has learned the specified command
        /// </summary>
        /// <param name="query"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private bool HVQuery_LearnedCommand(string[] query, GameStateQueryContext context)
        {
            if (!ArgUtility.TryGet(query, 1, out string requiredItem, out _, allowBlank: true))
                return false;

            if (requiredItem == "")
                return true;

            if (Game1.player.stats.Get(requiredItem) > 0)
                return true;

            return false;
        }
        #endregion

        #region Custom Booleans
        public static bool IsCommandLearned(string requiredItem)
        {
            if (requiredItem == "")
                return true;

            if (Game1.player.stats.Get(requiredItem) > 0)
                return true;

            return false;
        }
        #endregion

        #region Custom Actions
        public static bool TriggerAction(string[] args, TriggerActionContext context, out string error)
        {
            if (!ArgUtility.TryGet(args, 1, out string variantName, out error, allowBlank: false))
                return false;
            if (!ArgUtility.TryGet(args, 2, out string npcName, out error, allowBlank: false))
                return false;
            if (!ArgUtility.TryGet(args, 3, out string triggerLevel, out error, allowBlank: false))
                return false;

            NPC target = Game1.getCharacterFromName(npcName);
            if (target == null){ error = $"Cannot find NPC: {npcName}"; return false; }

            if (!int.TryParse(triggerLevel, out int level)){ error = $"{triggerLevel} is not a valid integer"; return false; }

            ITranceCommand trigger;
            string[] triggerName = variantName.Split('/');
            trigger = triggerName[0] switch
            {
                "Deepener" => new Mindless(),
                "Amnesia" => new Amnesia(),
                "Animal" => new Pet(),
                "Devotion" => new Devotion(),
                "Love" => new Lovers(),
                "Puppet" => new Puppet(),
                _ => null
            };
            if (trigger == null) { error = $"Cannot find trigger: {variantName}"; return false; }


            trigger.Activate(LocalData.TriggerList[triggerName[0]].Variants[triggerName[1]], Game1.getCharacterFromName(npcName), level);
            return true;
        }
        #endregion

        #region Custom Commands
        /// <summary>
        /// Sets the trance level for the specified NPC
        /// </summary>
        /// <param name="command"></param>
        /// <param name="args"></param>
        private void SetLevel(string command, string[] args)
        {
            int amount;
            if (!Context.IsWorldReady) return; //Does nothing if no save is loaded
            if (args.Length == 0) //Error if no NPC name is listed
            { Monitor.Log("Please specify an NPC's name. (Name must be the default English name)", LogLevel.Error); return; }
            else if (args.Length == 1) //Error if no number is listed
            { Monitor.Log("Please specify a number to set the NPC's trust to.", LogLevel.Error); return; }
            else if (!LocalData.ModData.Trances.ContainsKey(args[0])) //Error if NPC is not valid
            { Monitor.Log("NPC trance data not found. You might need to hypnotize them once first, or they name is not the default English name", LogLevel.Error); return; }
            else if (!int.TryParse(args[1], out amount)) //Error if trust value cannot be read as a number
            { Monitor.Log("Trust level must be a number", LogLevel.Error); return; }

            string NPC = args[0];
            LocalData.ModData.Trances[NPC].SetTrustLevel(amount);
            Monitor.Log($"The trust level for {NPC} is now set to {LocalData.ModData.Trances[NPC].trustLevel}", LogLevel.Info);
        }

        private void AddAllCommands(string command, string[] args)
        {
            if (!Context.IsWorldReady) { Monitor.Log("Load a save file first"); return; }

            foreach (Trigger t in LocalData.TriggerList.Values)
                Game1.player.stats.Set(t.RequiredBook, 1);
        }
        #endregion
    }
}
