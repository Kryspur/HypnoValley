using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleHVMod
{
    public interface IHypnoValleyAPI
    {
        void ActivateTrance(string characterName, int level, int cost);
        void ModifyTrust(string characterName, int amount);
    }
}
