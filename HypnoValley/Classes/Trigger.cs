using Microsoft.Xna.Framework.Content;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HypnoValley.Classes
{
    public class Trigger
    {
        /// <summary>
        /// A tokenizable string for Trigger's name
        /// </summary>
        public string ID;
        /// <summary>
        /// A tokenizable string for the Trigger's display name
        /// </summary>
        public string DisplayName;
        public string Description;
        public string RequiredBook = null;
        [ContentSerializer(Optional = true)]
        public Dictionary<string, TriggerVariant> Variants;
    }

    public class TriggerVariant
    {
        public string ID;
        public string DisplayName;
        public string Description;
        [ContentSerializer(Optional = true)]
        public int MinimumLevel = 0;
        [ContentSerializer(Optional = true)]
        public int TriggerCost = 0;
        public string Action;
    }
}
