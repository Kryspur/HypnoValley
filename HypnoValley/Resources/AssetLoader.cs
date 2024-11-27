using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;

namespace HypnoValley.Resources
{
    public class AssetLoader
    {
        public static bool CanLoad(IAssetName asset)
        {
            return asset.Name switch
            {
                "Kryspur.HypnoValley_TranceBar" or
                "Kryspur.HypnoValley_TranceBarOutline" => true,
                _ => false,
            };
        }

        public static void Load(AssetRequestedEventArgs asset)
        {
            switch (asset.Name.Name)
            {
                case "Kryspur.HypnoValley_TranceBar":
                    asset.LoadFromModFile<Texture2D>("assets/TranceBar.png", AssetLoadPriority.Exclusive);
                    break;
                case "Kryspur.HypnoValley_TranceBarOutline":
                    asset.LoadFromModFile<Texture2D>("assets/TranceBarOutline.png", AssetLoadPriority.Exclusive);
                    break;
                default:
                    break;
            }
        }
    }
}
