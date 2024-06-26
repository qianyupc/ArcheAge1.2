﻿using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Utils;
using AAEmu.Game.Utils.Scripts;
using NLog;

namespace AAEmu.Game.Scripts.Commands;

public class Sphere : ICommand
{
    protected static Logger Logger = LogManager.GetCurrentClassLogger();

    public void OnLoad()
    {
        CommandManager.Instance.Register("sphere", this);
    }

    public string GetCommandLineHelp()
    {
        return "<list||add||remove>";
    }

    public string GetCommandHelpText()
    {
        return "/sphere ";
    }

    public void Execute(Character character, string[] args, IMessageOutput messageOutput)
    {
        if (args.Length < 1)
        {
            character.SendMessage("/sphere <add/remove/list>");
            return;
        }

        SphereCommandUtil.GetCommandChoice(character, args[0], args);
    }
}
