﻿using AAEmu.Game.Models.Game.DoodadObj.Templates;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.DoodadObj.Funcs
{
    public class DoodadFuncRenewItem : DoodadFuncTemplate
    {
        public uint SkillId { get; set; }

        public override void Use(IUnit caster, Doodad owner, uint skillId, int nextPhase = 0)
        {
            _log.Debug("DoodadFuncRenewItem");
            owner.ToPhaseAndUse = false;
        }
    }
}
