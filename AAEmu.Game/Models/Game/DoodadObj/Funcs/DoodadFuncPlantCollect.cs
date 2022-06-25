﻿using AAEmu.Game.Models.Game.DoodadObj.Templates;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.DoodadObj.Funcs
{
    public class DoodadFuncPlantCollect : DoodadFuncTemplate
    {
        public override void Use(IUnit caster, Doodad owner, uint skillId, int nextPhase = 0)
        {
            _log.Debug("DoodadFuncPlantCollect");
            owner.ToPhaseAndUse = false;
        }
    }
}
