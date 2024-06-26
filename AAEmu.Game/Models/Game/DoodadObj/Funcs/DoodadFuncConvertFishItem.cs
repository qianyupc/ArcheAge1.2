﻿using AAEmu.Game.Models.Game.DoodadObj.Templates;
using AAEmu.Game.Models.Game.Units;

namespace AAEmu.Game.Models.Game.DoodadObj.Funcs;

public class DoodadFuncConvertFishItem : DoodadPhaseFuncTemplate
{
    public uint DoodadFuncConvertFishId { get; set; }
    public uint ItemId { get; set; }
    public uint LootPackId { get; set; }
    public override bool Use(BaseUnit caster, Doodad owner)
    {
        Logger.Trace("DoodadFuncConvertFishItem");
        return false;
    }
}
