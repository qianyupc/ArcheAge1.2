﻿using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.DoodadObj.Templates;
using AAEmu.Game.Models.Game.Items.Actions;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.DoodadObj.Funcs
{
    public class DoodadFuncRemoveItem : DoodadFuncTemplate
    {
        public uint ItemId { get; set; }
        public int Count { get; set; }

        public override void Use(IUnit caster, Doodad owner, uint skillId, int nextPhase = 0)
        {
            _log.Debug("DoodadFuncRemoveItem: ItemId {0}, Count {1}", ItemId, Count);

            var character = (Character)caster;
            var balans = character?.Inventory.Bag.ConsumeItem(ItemTaskType.DoodadItemChanger, ItemId, Count, null); // DoodadItemChanger right for this ?
            //character?.Inventory.RemoveItem(ItemId, Count);
            owner.ToPhaseAndUse = balans > 0;
        }
    }
}
