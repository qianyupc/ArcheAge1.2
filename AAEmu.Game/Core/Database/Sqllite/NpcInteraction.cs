﻿using System;
using System.Collections.Generic;

namespace AAEmu.Game.Core.Database.Sqllite;

public partial class NpcInteraction
{
    public long? Id { get; set; }

    public long? NpcInteractionSetId { get; set; }

    public long? SkillId { get; set; }
}
