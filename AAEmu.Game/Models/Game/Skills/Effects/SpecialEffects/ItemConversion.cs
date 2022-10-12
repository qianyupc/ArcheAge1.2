﻿using System;
using AAEmu.Commons.Utils;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.GameData;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Items.Actions;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.Skills.Effects.SpecialEffects
{
    public class ItemConversion : SpecialEffectAction
    {
        protected override SpecialType SpecialEffectActionType => SpecialType.ItemConversion;
        public override void Execute(Unit caster,
            SkillCaster casterObj,
            BaseUnit target,
            SkillCastTarget targetObj,
            CastAction castObj,
            Skill skill,
            SkillObject skillObject,
            DateTime time,
            int value1,
            int value2,
            int value3,
            int value4)
        {
            // TODO ...
            if (caster is Character) { _log.Debug("Special effects: ItemConversion value1 {0}, value2 {1}, value3 {2}, value4 {3}", value1, value2, value3, value4); }

            if ((!(caster is Character character)) || (character == null))
            {
                return;
            }

            if (!(targetObj is SkillCastItemTarget itemTarget) || (itemTarget == null))
            {
                return;
            }

            var targetItem = character.Inventory.GetItemById(itemTarget.Id);
            if (targetItem == null)
            {
                return;
            }

            if (!targetItem.Template.Disenchantable)
            {
                return;
            }

            var id = targetItem.TemplateId;
            var reagent = ItemConversionGameData.Instance.GetReagentForItem(targetItem.Grade, targetItem.Template.ImplId, id, targetItem.Template.Level);
            if (reagent == null)
            {
                _log.Error($"Couldn't find Reagent for item {id}");
                return;
            }

            var product = ItemConversionGameData.Instance.GetProductFromReagent(reagent);
            if (product == null)
            {
                _log.Error($"Couldn't find Product from Reagent for item {id}");
                return;
            }

            var productRoll = Rand.Next(0, 10000);
            var productChance = product.ChanceRate;
            if (productRoll < productChance)
            {
                // give product
                // TODO: add in weights
                int value = Rand.Next(product.MinOutput, product.MaxOutput + 1);
                character.Inventory.Bag.AcquireDefaultItem(ItemTaskType.Conversion, (uint) product.OuputItemId, value);
            }

            // destroy item
            targetItem._holdingContainer.RemoveItem(ItemTaskType.Conversion, targetItem, true);

            character.BroadcastPacket(new SCSkillEndedPacket(skill.TlId), true);
        }
    }
}
