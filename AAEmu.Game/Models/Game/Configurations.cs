﻿using System.Collections.Generic;
using AAEmu.Commons.Network;

namespace AAEmu.Game.Models.Game;

public class Configurations : PacketMarshaler
{
    public string Key { get; set; }
    public string Value { get; set; }
}

public class WorldConfig
{
    public string MOTD { get; set; } = "";
    public string LogoutMessage { get; set; } = "";
    public double AutoSaveInterval { get; set; } = 5.0;
    public double ExpRate { get; set; } = 1.0;
    public double HonorRate { get; set; } = 1.0;
    public double VocationRate { get; set; } = 1.0;
    public double LootRate { get; set; } = 1.0;
    public double GoldLootMultiplier { get; set; } = 1.0;
    public double GrowthRate { get; set; } = 1.0;
    public bool GodMode { get; set; } = false;
    public bool GeoDataMode { get; set; } = false;
}

public class AccountDeleteDelayTiming
{
    public int Level { get; set; }
    public int Delay { get; set; }
}

public class AccountConfig
{
    public string NameRegex { get; set; } = "^[a-zA-Z0-9]{1,18}$";
    public bool DeleteReleaseName { get; set; } = false;
    public List<AccountDeleteDelayTiming> DeleteTimings { get; set; } = new List<AccountDeleteDelayTiming>();
}

public class SpecialtyConfig
{
    public int MaxSpecialtyRatio { get; set; } = 130;
    public int MinSpecialtyRatio { get; set; } = 70;
    public double RatioDecreasePerPack { get; set; } = 0.5f;
    public double RatioIncreasePerTick { get; set; } = 5.0;
    public double RatioDecreaseTickMinutes { get; set; } = 1f;
    public double RatioRegenTickMinutes { get; set; } = 60f;
}

public class ScriptsConfig
{
    public LoadStrategyType LoadStrategy { get; set; } = LoadStrategyType.Reflection;

    public enum LoadStrategyType
    {
        Compilation,
        Reflection
    }
}
