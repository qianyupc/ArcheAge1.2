﻿using System;
using System.Collections.Generic;

namespace AAEmu.Game.Core.Database.Sqllite;

public partial class QuestActConReportNpc
{
    public long? Id { get; set; }

    public long? NpcId { get; set; }

    public byte[] UseAlias { get; set; }

    public long? QuestActObjAliasId { get; set; }
}
