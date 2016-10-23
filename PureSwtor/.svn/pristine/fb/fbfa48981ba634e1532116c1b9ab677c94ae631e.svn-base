using System.Linq;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;

namespace PureSWTor.Managers
{
    public static class Dispel
    {
        public static Debuff[] DebuffList = {
                                            new Debuff("Crushing Affliction (Force)", 0),
                                            new Debuff("Crushing Affliction (All)", 0),
                                            new Debuff("Corrosive Slime", 0),
                                            new Debuff("Laser", 4)                                            
                                            };

        public static bool NeedsCleanse(this TorCharacter p)
        {
            foreach (Debuff d in DebuffList)
            {
                if (d.Stacks == 0 && p.HasDebuff(d.Name))
                    return true;

                if (d.Stacks > 0 && p.HasDebuffCount(d.Name, d.Stacks))
                    return true;
            }
            return false;
        }

        public static bool HasDebuffCount(this TorCharacter p, string debuff, int stacks)
        {
            return !p.HasDebuff(debuff) ? false : p.Debuffs.Any(d => d.Name.Contains(debuff) && d.GetStacks() >= stacks);
        }
    }

    
    public class Debuff
    {
        public string Name;
        public int Stacks;

        public Debuff(string _name, int _stacks)
        {
            Name = _name;
            Stacks = _stacks;
        }
    }
}
