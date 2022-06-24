﻿using System;
using AAEmu.Game.Core.Packets;
using AAEmu.Game.Models.Game.Skills.Templates;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.Skills.Effects
{
    public class TrainCraftEffect : EffectTemplate
    {
        public uint CraftId { get; set; }

        public override bool OnActionTime => false;

        public override void Apply(IUnit caster, SkillCaster casterObj, IBaseUnit target, SkillCastTarget targetObj,
            CastAction castObj,
            EffectSource source, SkillObject skillObject, DateTime time, CompressedGamePackets packetBuilder = null)
        {
            _log.Debug("TrainCraftEffect");
        }
    }
}
