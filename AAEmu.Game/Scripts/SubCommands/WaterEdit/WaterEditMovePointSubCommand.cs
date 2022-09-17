﻿using System.Numerics;
using System.Collections.Generic;
using System.Drawing;
using AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Items;
using AAEmu.Game.Models.Game.Items.Actions;
using AAEmu.Game.Utils.Converters;
using AAEmu.Game.Utils.Scripts;
using AAEmu.Game.Utils.Scripts.SubCommands;

namespace AAEmu.Game.Scripts.Commands
{
    public class WaterEditMovePointSubCommand : SubCommandBase 
    {
        public WaterEditMovePointSubCommand()
        {
            Title = "[WaterEdit]";
            Description = "Move a point in the select body of water to your location.";
            CallPrefix = $"{CommandManager.CommandPrefix}wateredit movepoint";
            AddParameter(new NumericSubCommandParameter<int>("point", "point id", false));
        }

        public override void Execute(ICharacter character, string triggerArgument, string[] args) =>
            Execute(character, triggerArgument, new Dictionary<string, ParameterValue>());
        
        public override void Execute(ICharacter character, string triggerArgument, IDictionary<string, ParameterValue> parameters)
        {
            var world = WorldManager.Instance.GetWorld(character.Transform.WorldId);
            if (world == null)
            {
                character.SendMessage($"[WaterEdit] You are somehow not in a valid world!");
                return;
            }
            
            if (WaterEditCmd.SelectedWater == null)
            {
                character.SendMessage($"|cFFFF0000[WaterEdit] You need to select a water body first!|r");
                return;
            }
            
            var pointIndex = GetOptionalParameterValue<int>(parameters, "point", WaterEditCmd.NextPoint);

            if ((pointIndex >= WaterEditCmd.SelectedWater.Points.Count - 1) || (pointIndex < 0))
            {
                character.SendMessage($"|cFFFFFFFF[WaterEdit] {pointIndex} is not a valid point index (0~{WaterEditCmd.SelectedWater.Points.Count - 2})!|r");
                return;
            }

            var newPos = new Vector3(character.Transform.World.Position.X, character.Transform.World.Position.Y, WaterEditCmd.SelectedWater.Points[pointIndex].Z);
            lock (WaterEditCmd.SelectedWorld.Water._lock)
            {
                WaterEditCmd.SelectedWater.Points[pointIndex] = newPos;
                if (pointIndex == 0)
                    WaterEditCmd.SelectedWater.Points[^1] = newPos;
            }
            WaterEditCmd.NextPoint = pointIndex + 1;

            WaterEditCmd.ShowSelectedArea(character);
            character.SendMessage($"[WaterEdit] |cFFFFFFFF{WaterEditCmd.SelectedWater.Name} #{pointIndex}|r moved to set to |cFF00FF00{newPos}|r");
        }
    }
}
