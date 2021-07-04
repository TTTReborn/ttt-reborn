using System.Collections.Generic;

using Sandbox;
using Sandbox.UI;

using TTTReborn.Items;
using TTTReborn.Player;

namespace TTTReborn.UI
{
    using System.Linq;

    public class Effects : Panel
    {
        private readonly List<Effect> _effectList = new();

        public Effects()
        {
            StyleSheet.Load("/ui/Effects.scss");

            if (Local.Pawn is not TTTPlayer player)
            {
                return;
            }

            PerksInventory perks = (player.Inventory as Inventory).Perks;
            for (int i = 0; i < perks.Count(); i++)
            {
                AddEffect(perks.Get(i));
            }
        }

        public void AddEffect(TTTPerk perk)
        {
            _effectList.Add(new Effect(this, perk));
        }

        public void RemoveEffect(TTTPerk perk)
        {
            foreach (Effect effect in _effectList.Where(effect => effect.Item.Name == perk.Name))
            {
                _effectList.Remove(effect);
                effect.Delete();
                return;
            }
        }
    }
}
