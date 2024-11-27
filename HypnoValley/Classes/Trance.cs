using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Extensions;
using System;

namespace HypnoValley
{
    public class Trance
    {
        /// <summary>
        /// Name of the character
        /// </summary>
        public readonly string NPCName;
        /// <summary>
        /// The character's trust level
        /// </summary>
        public int trustLevel;

        /// <summary>
        /// Whether the character has been hypnotized today
        /// </summary>
        public bool DailyTrance;
        /// <summary>
        /// Whether the character is currently in trance
        /// </summary>
        public bool CurrentTrance;
        /// <summary>
        /// The time the character's trance will end naturally
        /// </summary>
        public int TranceExpire;
        /// <summary>
        /// Whether a trigger has been used on the character today
        /// </summary>
        public bool DailyTrigger;
        /// <summary>
        /// Whether a deepener has been used on the character today
        /// </summary>
        public bool DailyDeepen;

        //Trance-related values
        /// <summary>
        /// Original name of the character. Will currently break on language change
        /// </summary>
        public readonly string DefaultDisplayName;
        /// <summary>
        /// Whether the character can be kissed because of hypnosis
        /// </summary>
        public bool LoversKiss;
        /// <summary>
        /// Whether the character can be pet pecause of hypnosis
        /// </summary>
        public bool CanPet;
        /// <summary>
        /// Wheter the character is currently following the player
        /// </summary>
        public bool IsFollowing;

        //Tech-related valuse
        public bool ChipImplanted;

        /// <summary>
        /// Adds trance data to NPC
        /// </summary>
        /// <param name="npcName">Name of NPC</param>
        public Trance(string npcName)
        {
            NPCName = npcName;
            trustLevel = 0;
            DailyTrance = false;
            DailyTrigger = false;

            DefaultDisplayName = Game1.getCharacterFromName(npcName).displayName;
            LoversKiss = false;
            CanPet = false;
            IsFollowing = false;

            ChipImplanted = false;
        }

        /// <summary>
        /// Increases or decreases trust level
        /// </summary>
        /// <param name="amount">Amount to change trust level by</param>
        public void ModifyTrust(int amount)
        {
            //Caps trust level between -2 and 14
            trustLevel = Math.Min(trustLevel + amount, 14 * 250);
            trustLevel = Math.Max(trustLevel, -2 * 250);
        }

        public void SetTrustLevel(int amount)
        {
            //Caps trust level between -2 and 14
            trustLevel = Math.Min(amount, 14 * 250);
            trustLevel = Math.Max(trustLevel, -2 * 250);
        }

        public void NewDay()
        {
            //Set trance variables to false
            DailyTrance = false;
            DailyTrigger = false;
            CurrentTrance = false;
            DailyDeepen = false;

            //Check if standard chip will break
            if (!ChipImplanted && trustLevel > 0) ModifyTrust(-2);
            Random chipBreak = Utility.CreateDaySaveRandom();
            if (chipBreak.NextBool() && ChipImplanted) { ChipImplanted = false; Game1.player.mailReceived.Add($"Kryspur.HypnoValley_{NPCName}ChipBroke"); }

            //Reset trigger variables
            Game1.getCharacterFromName(NPCName).displayName = DefaultDisplayName; //Allows NPC to remember their name
            LoversKiss = false; //Stops player from kissing NPC because of hypnosis
            CanPet = false; //Stops player from petting NPC
            IsFollowing = false; //Disables following for NPC
        }

        public void OnSecondUpdate()
        {
            if (Game1.timeOfDay == TranceExpire || DailyTrigger)
            {
                CurrentTrance = false;
                NPC npc = Game1.getCharacterFromName(NPCName);
                npc.addedSpeed = 0;
                if (npc.CurrentDialogue.Count > 0)
                    if (npc.CurrentDialogue.Peek().dialogues[0].Text == "...")
                        npc.CurrentDialogue.Pop();
            }

            if (Game1.player.mailReceived.Contains($"Kryspur.HypnoValley_{NPCName}ChipBroke"))
            {
                Game1.player.mailReceived.Remove($"Kryspur.HypnoValley_{NPCName}ChipBroke");
                string npcDisplayName = Game1.getCharacterFromName(NPCName).displayName;
                HUDMessage chipBroke = new("The chip on " + npcDisplayName + " broke overnight", HUDMessage.newQuest_type);
                Game1.addHUDMessage(chipBroke);
            }
        }

        public void OnFrameUpdate()
        {
            if (CurrentTrance && Game1.timeOfDay < TranceExpire)
            {
                NPC entranced = Game1.getCharacterFromName(NPCName);
                Dialogue asleepDialogue = new(entranced, "", "...$7");

                entranced.addedSpeed = entranced.speed * -1;
                if (entranced.CurrentEmoteIndex == -1 || entranced.CurrentEmoteIndex == 0)
                    entranced.doEmote(24);

                if (entranced.CurrentDialogue.Count == 0 || entranced.CurrentDialogue.Peek().dialogues[0].Text != "...")
                    entranced.CurrentDialogue.Push(asleepDialogue);
            }
        }
    }
}
