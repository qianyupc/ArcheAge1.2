﻿using System;

using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.DoodadObj.Static;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.Skills.Effects.SpecialEffects
{
    public class AttachTo : SpecialEffectAction
    {
        protected override SpecialType SpecialEffectActionType => SpecialType.AttachTo;
        
        public override void Execute(IUnit caster, SkillCaster casterObj, IBaseUnit target, SkillCastTarget targetObj, CastAction castObj,
            Skill skill, SkillObject skillObject, DateTime time, int value1, int value2, int value3, int value4)
        {
            if (!(target is Slave slave))
                return;

            if (!(caster is Character character))
                return;

            SlaveManager.Instance.BindSlave(character, slave.ObjId, (AttachPointKind)value1, AttachUnitReason.NewMaster);
        }
    }
}
