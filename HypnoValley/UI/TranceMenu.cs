using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using HypnoValley.Trances.Effects;
using StardewValley.BellsAndWhistles;
using HypnoValley.Classes;
using System.Diagnostics.SymbolStore;

namespace HypnoValley.UI
{
    internal class TranceMenu : IClickableMenu
    {
        #region Fields
        /// <summary>
        /// The name of the NPC being entranced
        /// </summary>
        private readonly string NPCName;
        /// <summary>
        /// The trance data for the entranced NPC
        /// </summary>
        private readonly Trance currentTrance;

        private Dictionary<string, Trigger> triggerList;
        /// <summary>
        /// List of names of selectable trances
        /// </summary>
        private readonly List<string> triggerNameList = new();
        private readonly List<string> strengthList = new();
        /// <summary>
        /// The highest level of trance available with the current tool
        /// </summary>
        private readonly int maxSelectedLevel = 0;
        /// <summary>
        /// Index of currently selected trance
        /// </summary>
        private int selectedTrigger = 0;
        private int selectedLevel = 0;
        private int suggestionTrustReq = 0;
        private Color suggestionTextColor = SpriteText.color_Default;
        private Color strengthTextColor = SpriteText.color_Black;
        private Color confirmTextColor = SpriteText.color_Black;
        /// <summary>
        /// If the nighttime background should load
        /// </summary>
        private readonly bool isNight = false;
        /// <summary>
        /// Is the mouse hovering over the NPC icon
        /// </summary>
        private bool isNPCHover = false;
        /// <summary>
        /// Frame of NPC walking animation (for hover)
        /// </summary>
        private double frame = 0;
        /// <summary>
        /// Text to display in the hover box
        /// </summary>
        private string hoverText = "";
        private string chanceOfSuccess = "";

        /// <summary>
        /// The size of the menu
        /// </summary>
        private static readonly Vector2 size = new(1240, 680);
        /// <summary>
        /// The size and position of the Suggestion button
        /// </summary>
        private Rectangle suggestionBounds = new();
        private Rectangle strengthBounds = new();
        private Rectangle confirmBounds = new();

        private Rectangle suggestionAdvance = new();
        private Rectangle suggestionRevert = new();
        private Rectangle triggerAdvance = new();
        private Rectangle triggerRevert = new();
        #endregion

        #region UI Components
        /// <summary>
        /// Purely decorative elements in the UI
        /// </summary>
        private readonly List<ClickableTextureComponent> DecorTextures = new();
        private readonly List<ClickableTextureComponent> TriggerIndicators = new();
        private readonly List<ClickableTextureComponent> MovementButtons = new();
        #endregion

        private readonly Action<NPC, TriggerVariant, int> OnConfirm;
        private bool isReady;

        #region Public Methods
        /// <summary>
        /// Sets up trance menu
        /// </summary>
        /// <param name="target">NPC being entranced</param>
        public TranceMenu(NPC target, int toolLevel, Action<NPC, TriggerVariant, int> onConfirm)
            : base(Game1.viewport.Width / 2 - (int)size.X / 2, Game1.viewport.Height / 2 - (int)size.Y / 2, (int)size.X, (int)size.Y, true)
        {
            isReady = false;

            NPCName = target.Name;
            currentTrance = LocalData.ModData.Trances[NPCName];
            //maxSelectedLevel = currentTrance.TrustLevel;

            maxSelectedLevel = toolLevel;

            SetUpTranceList();
            isNight = Game1.timeOfDay >= 2000;

            OnConfirm = onConfirm;

            SetUpPositions();
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            xPositionOnScreen = Game1.viewport.Width / 2 - (int)size.X / 2;
            yPositionOnScreen = Game1.viewport.Height / 2 - (int)size.Y / 2;
            SetUpPositions();
        }
        #endregion

        #region Private Methods
        private void SetUpPositions()
        {
            this.initializeUpperRightCloseButton();
            this.upperRightCloseButton.setPosition(new(upperRightCloseButton.bounds.X - 24, upperRightCloseButton.bounds.Y + 84));

            DecorTextures.Clear();
            TriggerIndicators.Clear();
            MovementButtons.Clear();

            suggestionBounds = new(xPositionOnScreen + 16 + 81 + 256 + 16 + 44 + 16 + (0 * Game1.pixelZoom), yPositionOnScreen + 80 + 88 + 40, width - 48 - 16 - 81 - 256 - 16 - 44 - 16 - (0 * Game1.pixelZoom), 92);
            strengthBounds = new(suggestionBounds.X, suggestionBounds.Y + suggestionBounds.Height, suggestionBounds.Width, suggestionBounds.Height);
            confirmBounds = new(strengthBounds.X, yPositionOnScreen + 80 + 88 + 384 - strengthBounds.Height, strengthBounds.Width, strengthBounds.Height);

            suggestionAdvance = new(suggestionBounds.X + (6 * Game1.pixelZoom), suggestionBounds.Y + suggestionBounds.Height / 2 - (6 * Game1.pixelZoom), 16 * Game1.pixelZoom, 16 * Game1.pixelZoom);
            suggestionRevert = new(suggestionBounds.X + suggestionBounds.Width - (18 * Game1.pixelZoom), suggestionBounds.Y + suggestionBounds.Height / 2 - (6 * Game1.pixelZoom), 16 * Game1.pixelZoom, 16 * Game1.pixelZoom);
            triggerAdvance = new(strengthBounds.X + (6 * Game1.pixelZoom), strengthBounds.Y + strengthBounds.Height / 2 - (6 * Game1.pixelZoom), 16 * Game1.pixelZoom, 16 * Game1.pixelZoom);
            triggerRevert = new(strengthBounds.X + strengthBounds.Width - (18 * Game1.pixelZoom), strengthBounds.Y + strengthBounds.Height / 2 - (6 * Game1.pixelZoom), 16 * Game1.pixelZoom, 16 * Game1.pixelZoom);

            string charBkgAssetName = isNight ? "LooseSprites/nightbg" : "LooseSprites/daybg";
            DecorTextures.Add(new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 16 + 81, yPositionOnScreen + 80 + 88, 256, 384), Game1.content.Load<Texture2D>(charBkgAssetName), new Rectangle(0, 0, 128, 192), 2)); //Character background
            DecorTextures.Add(new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 16 + 81 + 68, yPositionOnScreen + 80 + 88 + 75, 120, 240), Game1.getCharacterFromName(NPCName).Sprite.spriteTexture, new Rectangle(0, 0, 16, 32), 7.5f)); //Character

            TriggerIndicators.Add(new ClickableTextureComponent("TranceBar", new Rectangle(xPositionOnScreen + 16 + 81 + 256 + 16 + 8, yPositionOnScreen + 346 * (int)Math.Max(2500 - currentTrance.trustLevel, 0) / 2500 + 192, 28, (int)(336 * Math.Min(currentTrance.trustLevel / 2500d, 1))), "", "", Game1.content.Load<Texture2D>("Kryspur.HypnoValley_TranceBar"), new Rectangle(0, 0, 28, (int)(336 * Math.Min(currentTrance.trustLevel / 2500d, 1))), 1));
            TriggerIndicators.Add(new ClickableTextureComponent("TranceBarReqs", new Rectangle(xPositionOnScreen + 16 + 81 + 256 + 16 + 8, yPositionOnScreen + 346 * (2500 - Math.Min(suggestionTrustReq, 2501)) / 2500 + 192, 28, 336 * suggestionTrustReq / 2500), "", "", Game1.content.Load<Texture2D>("Kryspur.HypnoValley_TranceBar"), new Rectangle(0, 336 * (2500 - Math.Min(suggestionTrustReq, 2501)) / 2500, 28, 336 * Math.Min(suggestionTrustReq, 2501) / 2500), 1));
            TriggerIndicators.Add(new ClickableTextureComponent("TranceBarOutline", new Rectangle(xPositionOnScreen + 16 + 81 + 256 + 16, yPositionOnScreen + 80 + 88, 44, 384), "", $"Current trust level:\n{Math.Floor(currentTrance.trustLevel / 250d)}/10", Game1.content.Load<Texture2D>("Kryspur.HypnoValley_TranceBarOutline"), new Rectangle(), 1));

            MovementButtons.Add(new ClickableTextureComponent("SuggestionAdvance", suggestionAdvance, "", "", Game1.mouseCursors, new Rectangle(352, 495, 12, 12), Game1.pixelZoom));
            MovementButtons.Add(new ClickableTextureComponent("SuggestionRevert", suggestionRevert, "", "", Game1.mouseCursors, new Rectangle(365, 495, 12, 12), Game1.pixelZoom));
            MovementButtons.Add(new ClickableTextureComponent("TriggerAdvance", triggerAdvance, "", "", Game1.mouseCursors, new Rectangle(352, 495, 12, 12), Game1.pixelZoom));
            MovementButtons.Add(new ClickableTextureComponent("TriggerRevert", triggerRevert, "", "", Game1.mouseCursors, new Rectangle(365, 495, 12, 12), Game1.pixelZoom));
        }

        private void SetUpTranceList()
        {
            triggerList = LocalData.TriggerList;
            foreach (string s in triggerList.Keys)
            {
                if (ModEntry.IsCommandLearned(triggerList[s].RequiredBook))
                    triggerNameList.Add(s);
            }

            if (triggerNameList.Count == 0) //If player knows 0 commands
            {
                HUDMessage noCommandsLearnedError = new("You don't know any commands", HUDMessage.error_type); //Set up error message

                Game1.addHUDMessage(noCommandsLearnedError);//Display the error
                Game1.activeClickableMenu = null; //Exit the menu
                return;
            }

            ReloadStrengthList(true);
        }

        private void ReloadStrengthList(bool reloadDir)
        {
            selectedLevel = 0;
            strengthList.Clear();

            Trigger currentTrigger = triggerList[triggerNameList[selectedTrigger]];

            foreach (string s in currentTrigger.Variants.Keys)
                strengthList.Add(s);

            AdvanceTrigger(false, true, reloadDir);
        }

        private void AdvanceSuggestion(bool isUp)
        {
            selectedTrigger += isUp ? 1 : -1;
            if (selectedTrigger < 0) selectedTrigger = triggerNameList.Count - 1;
            else selectedTrigger %= triggerNameList.Count;

            ReloadStrengthList(isUp);
        }

        private void AdvanceTrigger(bool isUp, bool isReload, bool reloadDir)
        {
            int move = isUp ? 1 : -1;
            if (isReload) move = 0;

            selectedLevel += move;
            if (selectedLevel < 0) selectedLevel = strengthList.Count - 1;
            else selectedLevel %= strengthList.Count;

            if (triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[selectedLevel]].MinimumLevel > maxSelectedLevel)
            {
                for (int i = selectedLevel + move; i != selectedLevel; i += move)
                {
                    if (i < 0) i = strengthList.Count - 1;
                    else i %= strengthList.Count;

                    if (triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[i]].MinimumLevel <= maxSelectedLevel)
                    {
                        selectedLevel = i;
                        return;
                    }
                }
                AdvanceSuggestion(reloadDir);
            }
        }
        #endregion


        #region Overrides
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            if (!isReady) return; //Prevents the game from registering the click to open the menu within the menu itself

            if (suggestionAdvance.Contains(x, y))
            { Game1.playSound("smallSelect"); AdvanceSuggestion(false); }
            else if (suggestionRevert.Contains(x, y))
                {Game1.playSound("smallSelect"); AdvanceSuggestion(true); }

            if (triggerAdvance.Contains(x, y))
                {Game1.playSound("smallSelect"); AdvanceTrigger(false, false, false); }
            else if (triggerRevert.Contains(x, y))
                {Game1.playSound("smallSelect"); AdvanceTrigger(true, false, false); }

            if (confirmBounds.Contains(x, y))
            {
                OnConfirm(Game1.getCharacterFromName(NPCName), triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[selectedLevel]], maxSelectedLevel);
                exitThisMenu();
            }

        }

        public override void releaseLeftClick(int x, int y)
        {
            if (!isReady) isReady = true;
            base.releaseLeftClick(x, y);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            base.receiveRightClick(x, y, playSound);
        }

        public override void performHoverAction(int x, int y)
        {
            hoverText = "";

            if (triggerNameList.Count == 0) return;

            //Checks if hovering over NPC sprite
            isNPCHover = false;
            if (DecorTextures[1].containsPoint(x, y))
            {
                hoverText += Game1.getCharacterFromName(NPCName).displayName + ": \nTrust Level: " + currentTrance.trustLevel;
                isNPCHover = true;
            }

            //Checks if hovering over suggestion box
            suggestionTextColor = SpriteText.color_Black;
            if (suggestionBounds.Contains(x, y))
            {
                suggestionTextColor = SpriteText.color_JojaBlue;
                hoverText += triggerList[triggerNameList[selectedTrigger]].DisplayName + "\n     " + triggerList[triggerNameList[selectedTrigger]].Description;
            }

            //Checks if hovering over trance level box
            strengthTextColor = SpriteText.color_Black;
            if (strengthBounds.Contains(x, y))
            {
                strengthTextColor = SpriteText.color_JojaBlue;
                hoverText += triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[selectedLevel]].DisplayName + "\n     " + triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[selectedLevel]].Description;
            }

            //Checks if hovering over confirm button
            confirmTextColor = SpriteText.color_Black;
            if (confirmBounds.Contains(x, y))
                confirmTextColor = SpriteText.color_Green;

            base.performHoverAction(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            if (triggerNameList.Count == 0) //Ignore the draw function until commands have been loaded
                return;

            drawTextureBox(b, 0, 0, Game1.graphics.PreferredBackBufferWidth, Game1.graphics.PreferredBackBufferHeight, new Color(0f, 0f, 0f, 0.2f));

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true); //Draws menu background
            SpriteText.drawStringHorizontallyCenteredAt(b, "Select A Suggestion", xPositionOnScreen + (width + 81 + 256) / 2, yPositionOnScreen + 120, color: SpriteText.color_Gray);

            //Draws decorative elements
            foreach (ClickableTextureComponent t in DecorTextures)
                t.draw(b);

            foreach (ClickableTextureComponent button in TriggerIndicators)
            {
                switch (button.name)
                {
                    case "TranceBarReqs":
                        button.draw(b, suggestionTrustReq > currentTrance.trustLevel ? Color.Red * 0.75f : Color.Blue * 1f, 1f);
                        break;
                    case "TranceBar":
                        button.draw(b, suggestionTrustReq > currentTrance.trustLevel ? Color.Black : new Color(139, 229, 52), 1);
                        break;
                    default:
                        button.draw(b);
                        break;
                }
            }

            //Utility.drawTextWithShadow(b, "Personality", Game1.dialogueFont, new(xPositionOnScreen + 16 + 36 + 169 + (width - 16 - 36 - 169) / 2 - 196 / 2, yPositionOnScreen + 80 + 200 + 30 - 32 + -20), Game1.textColor);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), suggestionBounds.X, suggestionBounds.Y, suggestionBounds.Width, suggestionBounds.Height, Color.White, 4f, false); //Selection Box
            SpriteText.drawStringHorizontallyCenteredAt(b, "Suggestion: " + triggerList[triggerNameList[selectedTrigger]].DisplayName, suggestionBounds.X + suggestionBounds.Width / 2, suggestionBounds.Y + suggestionBounds.Height / 2 - 25, color: suggestionTextColor);

            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), strengthBounds.X, strengthBounds.Y, strengthBounds.Width, strengthBounds.Height, Color.White, 4f, false); //Selection Box
            SpriteText.drawStringHorizontallyCenteredAt(b, "Trigger: " + triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[selectedLevel]].DisplayName, strengthBounds.X + strengthBounds.Width / 2, strengthBounds.Y + strengthBounds.Height / 2 - 25, color: strengthTextColor);

            drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 396, 15, 15), confirmBounds.X, confirmBounds.Y, confirmBounds.Width, confirmBounds.Height, Color.White, 4f, false); //Selection Box
            SpriteText.drawStringHorizontallyCenteredAt(b, $"Confirm ({chanceOfSuccess})", confirmBounds.X + confirmBounds.Width / 2, confirmBounds.Y + confirmBounds.Height / 2 - 25, color: confirmTextColor);

            foreach (ClickableTextureComponent t in MovementButtons)
                t.draw(b);

            if (hoverText != "") drawHoverText(b, hoverText, Game1.smallFont); //Draws the hover text box and text

            base.draw(b);

            drawMouse(b); //Draws the mouse
        }

        public override void update(GameTime time)
        {
            //Force close the menu if no commands have been learned (You can't close in the setup apparently)
            if (triggerNameList.Count == 0) exitThisMenuNoSound();
            else
            {

                //Animate NPC image if mouse is hovering over it
                DecorTextures[1].sourceRect = new Rectangle((int)frame * 16, 0, 16, 32);
                if (isNPCHover)
                    frame += 0.09;
                else
                    frame = 0;
                frame %= 4;

                //Update chance of success
                suggestionTrustReq = triggerList[triggerNameList[selectedTrigger]].Variants[strengthList[selectedLevel]].TriggerCost;
                int chance = HypnoValley.SuccessChance(currentTrance.trustLevel, suggestionTrustReq, Game1.player.getFriendshipHeartLevelForNPC(NPCName));

                if (chance == 100)
                    chanceOfSuccess = "Guaranteed";
                else if (chance >= 90)
                    chanceOfSuccess = "Highly Likely";
                else if (chance >= 70)
                    chanceOfSuccess = "Likely";
                else
                    chanceOfSuccess = "Unlikely";

                TriggerIndicators[1].bounds = new Rectangle(xPositionOnScreen + 378, yPositionOnScreen + 336 * (2500 - Math.Min(suggestionTrustReq, 2501)) / 2500 + 202, 28, 336 * Math.Min(suggestionTrustReq, 2501) / 2500);
                TriggerIndicators[1].sourceRect = new Rectangle(0, 336 * (2500 - Math.Min(suggestionTrustReq, 2501)) / 2500, 28, 336 * Math.Min(suggestionTrustReq, 2501) / 2500);

                base.update(time);
            }
        }
        #endregion
    }
}
