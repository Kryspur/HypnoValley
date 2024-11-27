using System;
using System.Collections.Generic;
using StardewValley;
using Microsoft.Xna.Framework;
using StardewValley.Network;
using StardewValley.GameData.Characters;
using StardewValley.Menus;
using HypnoValley.Classes;

namespace HypnoValley.Trances.Effects
{
    public class Lovers : ITranceCommand
    {
        public Lovers()
        {
        }

        public override void Activate(TriggerVariant variant, NPC target, int level)
        {
            base.Activate(variant, target, level);

            Friendship friendship = Game1.player.friendshipData[target.Name];
            Trance trance = LocalData.ModData.Trances[target.Name];
            int cost = variant.TriggerCost;
            Dialogue response;

            //Fails if trust is not high enough
            if (!HypnoValley.TrustCheck(trance.trustLevel, cost, Game1.player.getFriendshipHeartLevelForNPC(target.Name))){ FailedUse(target, friendship, trance, cost); return; }

            switch (variant.ID)
            {
                //Lets the farmer kiss the subject
                case "Kryspur.HypnoValley_LoveKiss":
                    //Sets up local variables
                    CharacterData data = Game1.characterData[target.Name];
                    //Fails if the character is not marriageable (will not have a kiss sprite)
                    if (!data.CanBeRomanced){ FailedUse(target, 4); return; }

                    //Enables the kiss
                    trance.LoversKiss = true;

                    //Queues up dialogue
                    response = target.TryGetDialogue("Kryspur.HypnoValley_LoveKiss");
                    Game1.DrawDialogue(response);
                    break;

                //Makes the subject believe they received an item
                case "Kryspur.HypnoValley_LoveGift":
                    //Sets up local variables
                    int points;

                    //Fails if a gift has already been given today or if 2 gifts have already been given this week unless it is the target's birthday
                    if (friendship.GiftsToday > 0 || (friendship.GiftsThisWeek >= 2 && !target.isBirthday())){ FailedUse(target, 1); return; }

                    //Sets point value based on trance level and if it is the target's birthday
                    points = level switch
                    {
                        0 => 10,
                        1 => 20,
                        2 => 45,
                        3 => 60,
                        4 => 80,
                        5 => 120,
                        _ => 250
                    };
                    points *= target.isBirthday() ? 8 : 1;

                    //Updates villager's friendship data
                    friendship.Points += points;
                    friendship.GiftsToday = 1;
                    friendship.GiftsThisWeek++;

                    //Queues up dialogue and skip if not found
                    response = target.TryGetDialogue("Kryspur.HypnoValley_LoveGiftStrong");
                    if (response != null) Game1.DrawDialogue(response);
                    break;
            }
        }

        private static void FailedUse(NPC recipient, Friendship friendship, Trance trance, int tranceReq)
        {
            Dialogue failResponse = recipient.TryGetDialogue("Kryspur.HypnoValley_LoveFail");
            if (failResponse != null) Game1.DrawDialogue(failResponse);

            friendship.Points -= 50;
            trance.ModifyTrust(Math.Max(-100, -1 * tranceReq / 2));
        }

        private static void FailedUse(NPC n, int code)
        {
            switch (code) 
            {
                case 1:
                    Game1.drawObjectDialogue($"{n.Name} has received too many gifts recently");
                    break;
                case 2:
                    Game1.drawObjectDialogue($"You cannot marry any villagers at this time");
                    break;
                case 3:
                    Game1.drawObjectDialogue($"Your house is not big enough to marry {n.Name}");
                    break;
                case 4:
                    Game1.drawObjectDialogue($"{n.Name} cannot be dated");
                    break;
                default:
                    Game1.drawObjectDialogue("This trance has not be added yet. Please complain about this loudly.");
                    break;
            }
        }

        public static void KissFarmer(NPC n, Farmer who)
        {
            LocalData.ModData.Trances[n.Name].LoversKiss = false;
            n.faceGeneralDirection(who.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
            who.faceGeneralDirection(n.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
            if (n.FacingDirection == 3 || n.FacingDirection == 1)
            {
                //Default index for the character's kissing frame
                int spouseFrame = 28;
                bool facingRight = true;

                //Updates the kissing frame and facing direction
                switch (n.Name)
                {
                    case "Maru":
                        spouseFrame = 28;
                        facingRight = false;
                        break;
                    case "Harvey":
                        spouseFrame = 31;
                        facingRight = false;
                        break;
                    case "Leah":
                        spouseFrame = 25;
                        facingRight = true;
                        break;
                    case "Elliott":
                        spouseFrame = 35;
                        facingRight = false;
                        break;
                    case "Sebastian":
                        spouseFrame = 40;
                        facingRight = false;
                        break;
                    case "Abigail":
                        spouseFrame = 33;
                        facingRight = false;
                        break;
                    case "Penny":
                        spouseFrame = 35;
                        facingRight = true;
                        break;
                    case "Alex":
                        spouseFrame = 42;
                        facingRight = true;
                        break;
                    case "Sam":
                        spouseFrame = 36;
                        facingRight = true;
                        break;
                    case "Shane":
                        spouseFrame = 34;
                        facingRight = false;
                        break;
                    case "Emily":
                        spouseFrame = 33;
                        facingRight = false;
                        break;
                    case "Krobus":
                        spouseFrame = 16;
                        facingRight = true;
                        break;
                    default:
                        break;
                }
                bool flip = (facingRight && n.FacingDirection == 3) || (!facingRight && n.FacingDirection == 1);
                int delay = (n.movementPause = (Game1.IsMultiplayer ? 1000 : 10));

                //Sets the farmer's animations
                n.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
                    {
                        new(spouseFrame, delay, secondaryArm: false, flip, n.haltMe, behaviorAtEndOfFrame: true)
                    });
                who.changeFriendship(10, n); //Adds friendship
                Game1.currentLocation.playSound("dwop", context:StardewValley.Audio.SoundContext.NPC); //Play the kiss audio
                who.exhausted.Value = false; //Removes exhaustion
                n.hasBeenKissedToday.Value = true; //Sets kiss value
                n.Sprite.UpdateSourceRect();
                int playerFaceDirection = 1;
                if ((facingRight && !flip) || (!facingRight && flip))
                {
                    playerFaceDirection = 3;
                }
                who.PerformKiss(playerFaceDirection);
            }
        }
    }
}
