﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.World;

namespace AAEmu.Game.Models.Game.Skills.Plots.Tree
{
    public class PlotState
    {
        private bool _cancellationRequest;
        public Dictionary<uint, int> Tickets { get; set; }
        public int[] Variables { get; set; }
        public byte CombatDiceRoll { get; set; }
        public bool IsCasting { get; set; }

        public Skill ActiveSkill { get; set; }
        public IUnit Caster { get; set; }
        public SkillCaster CasterCaster { get; set; }
        public IBaseUnit Target { get; set; }
        public SkillCastTarget TargetCaster { get; set; }
        public SkillObject SkillObject { get; set; }
        public List<(IBaseUnit unit, uint buffId)> ChanneledBuffs { get; set; }

        public Dictionary<uint,List<IGameObject>> HitObjects { get; set; }

        public PlotState(IUnit caster, SkillCaster casterCaster, IBaseUnit target, SkillCastTarget targetCaster, SkillObject skillObject, Skill skill)
        {
            _cancellationRequest = false;

            Caster = caster;
            CasterCaster = casterCaster;
            Target = target;
            TargetCaster = targetCaster;
            SkillObject = skillObject;
            ActiveSkill = skill;
            
            HitObjects = new Dictionary<uint, List<IGameObject>>();
            Tickets = new Dictionary<uint, int>();
            ChanneledBuffs = new List<(IBaseUnit, uint)>();
            Variables = new int[12];
        }

        public bool CancellationRequested() => _cancellationRequest;
        public bool RequestCancellation() => _cancellationRequest = true;
    }
}
