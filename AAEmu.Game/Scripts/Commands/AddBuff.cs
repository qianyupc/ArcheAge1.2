﻿using System;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Skills;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Tasks.Skills;

namespace AAEmu.Game.Scripts.Commands
{
    public class AddBuff : ICommand
    {
        public void OnLoad()
        {
            string[] name = { "addbuff", "add_buff", "buff" };
            CommandManager.Instance.Register(name, this);
        }

        public string GetCommandLineHelp()
        {
            return "";
        }

        public string GetCommandHelpText()
        {
            return "addbuff <buffId> <ab_level>";
        }

        public void Execute(Character character, string[] args)
        {
            if (args.Length < 1 || !uint.TryParse(args[0], out var buffId))
                return;

            if (args.Length < 2 || !uint.TryParse(args[1], out var abLevel))
                abLevel = 1u;

            var buffTemplate = SkillManager.Instance.GetBuffTemplate(buffId);
            if (buffTemplate == null)
                return;

            var newEffect =
                new Buff(character, character, new SkillCasterUnit(), buffTemplate, null, DateTime.Now)
                {
                    AbLevel = abLevel
                };

            character.Buffs.AddBuff(newEffect);
        }
    }
}
