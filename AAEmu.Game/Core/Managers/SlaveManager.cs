using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using AAEmu.Commons.IO;
using AAEmu.Commons.Utils;
using AAEmu.Commons.Utils.DB;
using AAEmu.Game.Core.Managers.AAEmu.Game.Core.Managers;
using AAEmu.Game.Core.Managers.Id;
using AAEmu.Game.Core.Managers.UnitManagers;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Network.Connections;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.DoodadObj;
using AAEmu.Game.Models.Game.DoodadObj.Static;
using AAEmu.Game.Models.Game.Items;
using AAEmu.Game.Models.Game.Items.Actions;
using AAEmu.Game.Models.Game.Items.Templates;
using AAEmu.Game.Models.Game.Skills;
using AAEmu.Game.Models.Game.Skills.Buffs;
using AAEmu.Game.Models.Game.Slaves;
using AAEmu.Game.Models.Game.Units;
using AAEmu.Game.Models.Game.World.Transform;
using AAEmu.Game.Models.Tasks.Slave;
using AAEmu.Game.Utils;
using AAEmu.Game.Utils.DB;
using Microsoft.VisualBasic.CompilerServices;
using NLog;
using NLog.LayoutRenderers;

namespace AAEmu.Game.Core.Managers;

public class SlaveManager : Singleton<SlaveManager>
{
    private static Logger Logger { get; } = LogManager.GetCurrentClassLogger();
    private Dictionary<uint, SlaveTemplate> _slaveTemplates;
    private Dictionary<uint, Slave> _activeSlaves;
    private Dictionary<uint, Slave> _tlSlaves;
    public Dictionary<uint, Dictionary<AttachPointKind, WorldSpawnPosition>> _attachPoints;
    public Dictionary<uint, List<SlaveInitialItems>> _slaveInitialItems; // PackId and List<Slot/ItemData>
    private object _slaveListLock;

    public bool Exist(uint templateId)
    {
        return _slaveTemplates.ContainsKey(templateId);
    }

    public SlaveTemplate GetSlaveTemplate(uint id)
    {
        return _slaveTemplates.ContainsKey(id) ? _slaveTemplates[id] : null;
    }

    public Slave GetActiveSlaveByOwnerObjId(uint objId)
    {
        lock (_slaveListLock)
            return _activeSlaves.ContainsKey(objId) ? _activeSlaves[objId] : null;
    }

    /// <summary>
    /// Returns a list of all Slaves of specific SlaveKind
    /// </summary>
    /// <param name="kind"></param>
    /// <param name="worldId">When set, only return from specific world</param>
    /// <returns></returns>
    public IEnumerable<Slave> GetActiveSlavesByKind(SlaveKind kind, uint worldId = uint.MaxValue)
    {
        lock (_slaveListLock)
        {
            if (worldId >= uint.MaxValue)
                return _activeSlaves.Select(i => i.Value).Where(s => s.Template.SlaveKind == kind);

            return _activeSlaves.Select(i => i.Value).Where(s => (s.Template.SlaveKind == kind) && (s.Transform.WorldId == worldId));
        }
    }

    /// <summary>
    /// Returns a list of all Slaves of specific SlaveKind
    /// </summary>
    /// <param name="kinds"></param>
    /// <param name="worldId">When set, only return from specific world</param>
    /// <returns></returns>
    public IEnumerable<Slave> GetActiveSlavesByKinds(SlaveKind[] kinds, uint worldId = uint.MaxValue)
    {
        lock (_slaveListLock)
        {
            if (worldId >= uint.MaxValue)
                return _activeSlaves.Where(s => kinds.Contains(s.Value.Template.SlaveKind))
                    .Select(s => s.Value);

            return _activeSlaves.Where(s => kinds.Contains(s.Value.Template.SlaveKind))
                .Where(s => s.Value?.Transform.WorldId == worldId)
                .Select(s => s.Value);
        }
    }

    public Slave GetActiveSlaveByObjId(uint objId)
    {
        lock (_slaveListLock)
        {
            foreach (var slave in _activeSlaves.Values)
            {
                if (slave.ObjId == objId) return slave;
            }
        }
        return null;
    }

    /* Unused
    private Slave GetActiveSlaveByTlId(uint tlId)
    {
        lock (_slaveListLock)
        {
            foreach (var slave in _activeSlaves.Values)
            {
                if (slave.TlId == tlId) return slave;
            }
        }

        return null;
    }*/

    public void UnbindSlave(Character character, uint tlId, AttachUnitReason reason)
    {

        Slave slave;
        lock (_slaveListLock)
            slave = _tlSlaves[tlId];
        var attachPoint = slave.AttachedCharacters.FirstOrDefault(x => x.Value == character).Key;
        if (attachPoint != default)
        {
            slave.AttachedCharacters.Remove(attachPoint);
            character.Transform.Parent = null;
            character.Transform.StickyParent = null;
        }

        character.Buffs.TriggerRemoveOn(BuffRemoveOn.Unmount);

        character.BroadcastPacket(new SCUnitDetachedPacket(character.ObjId, reason), true);
    }

    public void BindSlave(Character character, uint objId, AttachPointKind attachPoint, AttachUnitReason bondKind)
    {
        // Check if the target spot is already taken
        Slave slave;
        lock (_slaveListLock)
            slave = _tlSlaves.FirstOrDefault(x => x.Value.ObjId == objId).Value;
        //var slave = GetActiveSlaveByObjId(objId);
        if ((slave == null) || (slave.AttachedCharacters.ContainsKey(attachPoint)))
            return;

        character.BroadcastPacket(new SCUnitAttachedPacket(character.ObjId, attachPoint, bondKind, objId), true);
        switch (attachPoint)
        {
            case AttachPointKind.Driver:
                character.BroadcastPacket(new SCSlaveBoundPacket(character.Id, objId), true);
                break;
        }

        slave.AttachedCharacters.Add(attachPoint, character);
        character.Transform.Parent = slave.Transform;
        // TODO: move to attach point's position
        character.Transform.Local.SetPosition(0, 0, 0, 0, 0, 0);
    }

    public void BindSlave(GameConnection connection, uint tlId)
    {
        var unit = connection.ActiveChar;
        Slave slave;
        lock (_slaveListLock)
            slave = _tlSlaves[tlId];

        BindSlave(unit, slave.ObjId, AttachPointKind.Driver, AttachUnitReason.NewMaster);
    }

    // TODO - GameConnection connection
    /// <summary>
    /// Removes a slave from the world
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="objId"></param>
    public void Delete(Character owner, uint objId)
    {
        var activeSlaveInfo = GetActiveSlaveByObjId(objId);
        if (activeSlaveInfo == null) return;
        activeSlaveInfo.Save();

        // Remove passengers
        foreach (var character in activeSlaveInfo.AttachedCharacters.Values.ToList())
            UnbindSlave(character, activeSlaveInfo.TlId, AttachUnitReason.SlaveBinding);

        // Check if one of the slave doodads is holding a item
        foreach (var doodad in activeSlaveInfo.AttachedDoodads)
        {
            if ((doodad.ItemId != 0) || (doodad.ItemTemplateId != 0))
            {
                owner?.SendErrorMessage(ErrorMessageType.SlaveEquipmentLoadedItem); // TODO: Do we need this error? Client already mentions it.
                return; // don't allow un-summon if some it's holding a item (should be a trade-pack)
            }
        }

        var despawnDelayedTime = DateTime.UtcNow.AddSeconds(activeSlaveInfo.Template.PortalTime - 0.5f);

        activeSlaveInfo.Transform.DetachAll();

        foreach (var doodad in activeSlaveInfo.AttachedDoodads)
        {
            // Note, we un-check the persistent flag here, or else the doodad will delete itself from DB as well
            // This is not desired for player owned slaves
            if (owner != null)
                doodad.IsPersistent = false;
            doodad.Despawn = despawnDelayedTime;
            SpawnManager.Instance.AddDespawn(doodad);
            // doodad.Delete();
        }

        foreach (var attachedSlave in activeSlaveInfo.AttachedSlaves)
        {
            lock (_slaveListLock)
                _tlSlaves.Remove(attachedSlave.TlId);
            attachedSlave.Despawn = despawnDelayedTime;
            SpawnManager.Instance.AddDespawn(attachedSlave);
            //attachedSlave.Delete();
        }

        var world = WorldManager.Instance.GetWorld(activeSlaveInfo.Transform.WorldId);
        world.Physics.RemoveShip(activeSlaveInfo);
        owner.BroadcastPacket(new SCSlaveDespawnPacket(objId), true);
        owner.BroadcastPacket(new SCSlaveRemovedPacket(owner.ObjId, activeSlaveInfo.TlId), true);
        lock (_slaveListLock)
            _activeSlaves.Remove(owner.ObjId);

        activeSlaveInfo.Despawn = DateTime.UtcNow.AddSeconds(activeSlaveInfo.Template.PortalTime + 0.5f);
        SpawnManager.Instance.AddDespawn(activeSlaveInfo);
    }

    /// <summary>
    /// Slave created from spawn effect
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="skillData"></param>
    /// <param name="hideSpawnEffect"></param>
    /// <param name="positionOverride"></param>
    public void Create(Character owner, SkillItem skillData, bool hideSpawnEffect = false, Transform positionOverride = null)
    {
        var activeSlaveInfo = GetActiveSlaveByOwnerObjId(owner.ObjId);
        if (activeSlaveInfo != null)
        {
            activeSlaveInfo.Save();
            // TODO: If too far away, don't delete
            Delete(owner, activeSlaveInfo.ObjId);
            return;
        }

        var item = owner.Inventory.GetItemById(skillData.ItemId);
        if (item == null) return;

        var itemTemplate = (SummonSlaveTemplate)ItemManager.Instance.GetTemplate(item.TemplateId);
        if (itemTemplate == null) return;

        Create(owner, itemTemplate.SlaveId, item, hideSpawnEffect, positionOverride); // TODO replace the underlying code with this call
    }

    // added "/slave spawn <templateId>" to be called from the script command
    /// <summary>
    /// Slave created by player or spawn effect
    /// </summary>
    /// <param name="owner"></param>
    /// <param name="templateId"></param>
    /// <param name="item"></param>
    /// <param name="hideSpawnEffect"></param>
    /// <param name="positionOverride"></param>
    public void Create(Character owner, uint templateId, Item item = null, bool hideSpawnEffect = false, Transform positionOverride = null)
    {
        var slaveTemplate = GetSlaveTemplate(templateId);
        if (slaveTemplate == null) return;

        var tlId = (ushort)TlIdManager.Instance.GetNextId();
        var objId = ObjectIdManager.Instance.GetNextId();

        Transform spawnPos;
        var spawnOffsetPos = new Vector3();


        var dbId = 0u;
        var slaveName = string.Empty;
        var slaveHp = 1;
        var slaveMp = 1;
        var isLoadedPlayerSlave = false;
        if ((owner.Id > 0) && (item?.Id > 0))
        {
            using var connection = MySQL.CreateConnection();
            using (var command = connection.CreateCommand())
            {
                // Sorting required to make make sure parenting doesn't produce invalid parents (normally)

                command.CommandText = "SELECT * FROM slaves  WHERE (owner = @playerId) AND (item_id = @itemId) LIMIT 1";
                command.Parameters.AddWithValue("playerId", owner.Id);
                command.Parameters.AddWithValue("itemId", item.Id);
                command.Prepare();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        dbId = reader.GetUInt32("id");
                        // var slaveItemId = reader.GetUInt32("item_id");
                        // var slaveOwnerId = reader.GetUInt32("owner");
                        slaveName = reader.GetString("name");
                        // var slaveCreatedAt = reader.GetDateTime("created_at");
                        // var slaveUpdatedAt = reader.GetDateTime("updated_at");
                        slaveHp = reader.GetInt32("hp");
                        slaveMp = reader.GetInt32("mp");
                        // Coords are saved, but not really used when summoning and are only required to show vehicle
                        // location after a server restart (if it was still summoned)
                        // var slaveX = reader.GetFloat("x");
                        // var slaveY = reader.GetFloat("y");
                        // var slaveZ = reader.GetFloat("z");
                        isLoadedPlayerSlave = true;
                        break;
                    }
                }
            }
        }

        // TODO: Attach Slave's DbId to the Item Details
        // We currently fake the DbId using TlId instead

        if (item is SummonSlave slaveSummonItem)
        {
            slaveSummonItem.SlaveType = 0x02;
            slaveSummonItem.SlaveDbId = dbId;
            slaveSummonItem.IsDirty = true;
            owner.SendPacket(new SCItemTaskSuccessPacket(ItemTaskType.UpdateSummonMateItem, new ItemUpdate(item), new List<ulong>()));
        }

        // Replacing the position with the new coordinates from the method call parameters
        if (positionOverride != null)
        {
            // If manually defined a spawn location (i.e. created from ShipYard), use that location instead
            spawnPos = positionOverride.CloneDetached();
        }
        else
        {
            spawnPos = owner.Transform.CloneDetached();

            // If no spawn position override has been provided, then handle normal spawning algorithm

            // owner.SendMessage("SlaveSpawnOffset: x:{0} y:{1}", slaveTemplate.SpawnXOffset, slaveTemplate.SpawnYOffset);
            spawnPos.Local.AddDistanceToFront(Math.Clamp(slaveTemplate.SpawnYOffset, 5f, 50f));
            // INFO: Seems like X offset is defined as the size of the vehicle summoned, but visually it's nicer if we just ignore this 
            // spawnPos.Local.AddDistanceToRight(slaveTemplate.SpawnXOffset);
            if (slaveTemplate.IsABoat())
            {
                // If we're spawning a boat, put it at the water level regardless of our own height
                // TODO: if not at ocean level, get actual target location water body height (for example rivers)
                var world = WorldManager.Instance.GetWorld(spawnPos.WorldId);
                if (world == null)
                {
                    Logger.Fatal($"Unable to find world to spawn in {spawnPos.WorldId}");
                    return;
                }

                var worldWaterLevel = world.Water.GetWaterSurface(spawnPos.World.Position);
                spawnPos.Local.SetHeight(worldWaterLevel);

                // temporary grab ship information so that we can use it to find a suitable spot in front to summon it
                var tempShipModel = ModelManager.Instance.GetShipModel(slaveTemplate.ModelId);
                var minDepth = tempShipModel.MassBoxSizeZ - tempShipModel.MassCenterZ + 1f;

                // Somehow take into account where the ship will end up related to it's mass center (also check boat physics)
                spawnOffsetPos.Z += (tempShipModel.MassCenterZ < 0f ? (tempShipModel.MassCenterZ / 2f) : 0f) -
                                    tempShipModel.KeelHeight;

                for (var inFront = 0f; inFront < (50f + tempShipModel.MassBoxSizeX); inFront += 1f)
                {
                    using var depthCheckPos = spawnPos.CloneDetached();
                    depthCheckPos.Local.AddDistanceToFront(inFront);
                    var floorHeight = WorldManager.Instance.GetHeight(depthCheckPos);
                    if (floorHeight > 0f)
                    {
                        var surfaceHeight = world.Water.GetWaterSurface(depthCheckPos.World.Position);
                        var delta = surfaceHeight - floorHeight;
                        if (delta > minDepth)
                        {
                            //owner.SendMessage("Extra inFront = {0}, required Depth = {1}", inFront, minDepth);
                            spawnPos.Dispose();

                            spawnPos = depthCheckPos.CloneDetached();
                            break;
                        }
                    }
                }

                spawnPos.Local.Position += spawnOffsetPos;
            }
            else
            {
                // If a land vehicle, put it a the ground level of it's target spawn location
                // TODO: check for maximum height difference for summoning
                var h = WorldManager.Instance.GetHeight(spawnPos);
                if (h > 0f)
                    spawnPos.Local.SetHeight(h);
            }

            // Always spawn horizontal(level) and 90° CCW of the player
            spawnPos.Local.SetRotation(0f, 0f, owner.Transform.World.Rotation.Z + (MathF.PI / 2));
        }

        owner.BroadcastPacket(new SCSlaveCreatedPacket(owner.ObjId, tlId, objId, hideSpawnEffect, 0, owner.Name), true);

        // Get new Id to save if it has a player as owner
        if ((owner?.Id > 0) && (dbId <= 0))
            dbId = CharacterIdManager.Instance.GetNextId(); // dbId = SlaveIdManager.Instance.GetNextId();

        if (slaveHp <= 0)
            slaveHp = 1;

        var summonedSlave = new Slave
        {
            TlId = tlId,
            ObjId = objId,
            TemplateId = slaveTemplate.Id,
            Name = string.IsNullOrWhiteSpace(slaveName) ? slaveTemplate.Name : slaveName,
            Level = (byte)slaveTemplate.Level,
            ModelId = slaveTemplate.ModelId,
            Template = slaveTemplate,
            Hp = slaveHp,
            Mp = slaveMp,
            ModelParams = new UnitCustomModelParams(),
            Faction = owner.Faction,
            Id = dbId,
            Summoner = owner,
            SummoningItem = item,
            SpawnTime = DateTime.UtcNow
        };

        if (_slaveInitialItems.TryGetValue(summonedSlave.Template.SlaveInitialItemPackId, out var itemPack))
        {
            foreach (var initialItem in itemPack)
            {
                // var newItem = new Item(WorldManager.DefaultWorldId,ItemManager.Instance.GetTemplate(initialItem.itemId),1);
                var newItem = ItemManager.Instance.Create(initialItem.itemId, 1, 0, false);
                summonedSlave.Equipment.AddOrMoveExistingItem(ItemTaskType.Invalid, newItem, initialItem.equipSlotId);
            }
        }

        // Add Passive buffs
        foreach (var buff in summonedSlave.Template.PassiveBuffs)
        {
            var passive = SkillManager.Instance.GetPassiveBuffTemplate(buff.PassiveBuffId);
            summonedSlave.Buffs.AddBuff(passive.BuffId, summonedSlave);
        }

        // Add Normal initial buffs
        foreach (var buff in summonedSlave.Template.InitialBuffs)
            summonedSlave.Buffs.AddBuff(buff.BuffId, summonedSlave);

        summonedSlave.Hp = summonedSlave.MaxHp;
        summonedSlave.Mp = summonedSlave.MaxMp;

        summonedSlave.Transform = spawnPos.CloneDetached(summonedSlave);
        summonedSlave.Spawn();

        // TODO - DOODAD SERVER SIDE
        if (isLoadedPlayerSlave == false)
        {
            // This is a new slave, create all doodads
            foreach (var doodadBinding in summonedSlave.Template.DoodadBindings)
            {
                var doodad = new Doodad
                {
                    ObjId = ObjectIdManager.Instance.GetNextId(),
                    TemplateId = doodadBinding.DoodadId,
                    OwnerObjId = owner.ObjId,
                    ParentObjId = summonedSlave.ObjId,
                    AttachPoint = doodadBinding.AttachPointId,
                    OwnerId = owner.Id,
                    PlantTime = summonedSlave.SpawnTime,
                    OwnerType = DoodadOwnerType.Slave,
                    OwnerDbId = summonedSlave.Id,
                    Template = DoodadManager.Instance.GetTemplate(doodadBinding.DoodadId),
                    Data = (byte)doodadBinding.AttachPointId, // copy of AttachPointId
                    ParentObj = summonedSlave,
                    Faction = summonedSlave.Faction,
                    Type2 = 1u, // Flag: No idea why it's 1 for slave's doodads, seems to be 0 for everything else
                    Spawner = null
                };

                doodad.SetScale(doodadBinding.Scale);

                doodad.FuncGroupId = doodad.GetFuncGroupId();
                doodad.Transform = summonedSlave.Transform.CloneAttached(doodad);
                doodad.Transform.Parent = summonedSlave.Transform;

                // NOTE: In 1.2 we can't replace slave parts like sail, so just apply it to all of the doodads on spawn)
                // Should probably have a check somewhere if a doodad can have the UCC applied or not
                if (item != null && item.HasFlag(ItemFlag.HasUCC) && (item.UccId > 0))
                    doodad.UccId = item.UccId;

                if (_attachPoints.ContainsKey(summonedSlave.ModelId))
                {
                    if (_attachPoints[summonedSlave.ModelId].ContainsKey(doodadBinding.AttachPointId))
                    {
                        doodad.Transform = summonedSlave.Transform.CloneAttached(doodad);
                        doodad.Transform.Parent = summonedSlave.Transform;
                        doodad.Transform.Local.Translate(
                            _attachPoints[summonedSlave.ModelId][doodadBinding.AttachPointId]
                                .AsPositionVector());
                        doodad.Transform.Local.SetRotation(
                            _attachPoints[summonedSlave.ModelId][doodadBinding.AttachPointId].Roll,
                            _attachPoints[summonedSlave.ModelId][doodadBinding.AttachPointId].Pitch,
                            _attachPoints[summonedSlave.ModelId][doodadBinding.AttachPointId].Yaw);
                        Logger.Debug("Model id: {0} attachment {1} => pos {2} = {3}", summonedSlave.ModelId,
                            doodadBinding.AttachPointId,
                            _attachPoints[summonedSlave.ModelId][doodadBinding.AttachPointId],
                            doodad.Transform);
                    }
                    else
                    {
                        Logger.Warn("Model id: {0} incomplete attach point information", summonedSlave.ModelId);
                    }
                }
                else
                {
                    doodad.Transform = new Transform(doodad);
                    Logger.Warn("Model id: {0} has no attach point information", summonedSlave.ModelId);
                }

                summonedSlave.AttachedDoodads.Add(doodad);
                doodad.Spawn();
                if ((owner?.Id > 0) && (item?.Id > 0))
                {
                    doodad.IsPersistent = true;
                    doodad.Save();
                }
            }
        }
        else
        {
            // This was a previously saved slave, load doodads from DB instead
            var doodadSpawnCount = SpawnManager.Instance.SpawnPersistentDoodads(DoodadOwnerType.Slave, (int)summonedSlave.Id, summonedSlave, true);
            Logger.Debug($"Loaded {doodadSpawnCount} doodads from DB for Slave {summonedSlave.ObjId} (Db: {summonedSlave.Id}");
        }

        foreach (var slaveBinding in summonedSlave.Template.SlaveBindings)
        {
            var childSlaveTemplate = GetSlaveTemplate(slaveBinding.SlaveId);
            var ctlId = (ushort)TlIdManager.Instance.GetNextId();
            var cobjId = ObjectIdManager.Instance.GetNextId();
            var childSlave = new Slave()
            {
                TlId = ctlId,
                ObjId = cobjId,
                ParentObj = summonedSlave,
                TemplateId = childSlaveTemplate.Id,
                Name = childSlaveTemplate.Name,
                Level = (byte)childSlaveTemplate.Level,
                ModelId = childSlaveTemplate.ModelId,
                Template = childSlaveTemplate,
                Hp = 1,
                Mp = 1,
                ModelParams = new UnitCustomModelParams(),
                Faction = owner.Faction,
                Id = 11, // TODO
                Summoner = owner,
                SpawnTime = DateTime.UtcNow,
                AttachPointId = (sbyte)slaveBinding.AttachPointId,
                OwnerObjId = summonedSlave.ObjId
            };
            childSlave.Hp = childSlave.MaxHp;
            childSlave.Mp = childSlave.MaxMp;
            childSlave.Transform = spawnPos.CloneDetached(childSlave);
            childSlave.Transform.Parent = summonedSlave.Transform;

            if (_attachPoints.ContainsKey(summonedSlave.ModelId))
            {
                if (_attachPoints[summonedSlave.ModelId].ContainsKey(slaveBinding.AttachPointId))
                {
                    var attachPoint = _attachPoints[summonedSlave.ModelId][slaveBinding.AttachPointId];
                    // childSlave.AttachPosition = _attachPoints[template.ModelId][(int) slaveBinding.AttachPointId];
                    childSlave.Transform = summonedSlave.Transform.CloneAttached(childSlave);
                    childSlave.Transform.Parent = summonedSlave.Transform;
                    childSlave.Transform.Local.Translate(attachPoint.AsPositionVector());
                    childSlave.Transform.Local.Rotate(attachPoint.Roll, attachPoint.Pitch, attachPoint.Yaw);
                }
                else
                {
                    Logger.Warn("Model id: {0} incomplete attach point information");
                }
            }

            summonedSlave.AttachedSlaves.Add(childSlave);
            lock (_slaveListLock)
                _tlSlaves.Add(childSlave.TlId, childSlave);
            childSlave.Spawn();
        }

        lock (_slaveListLock)
        {
            _tlSlaves.Add(summonedSlave.TlId, summonedSlave);
            _activeSlaves.Add(owner.ObjId, summonedSlave);
        }

        if (summonedSlave.Template.IsABoat())
        {
            var world = WorldManager.Instance.GetWorld(owner.Transform.WorldId);
            world.Physics.AddShip(summonedSlave);
        }

        owner.SendPacket(new SCMySlavePacket(summonedSlave.ObjId, summonedSlave.TlId, summonedSlave.Name,
            summonedSlave.TemplateId,
            summonedSlave.Hp, summonedSlave.MaxHp,
            summonedSlave.Transform.World.Position.X,
            summonedSlave.Transform.World.Position.Y,
            summonedSlave.Transform.World.Position.Z
        ));

        // Save to DB
        summonedSlave.Save();
    }

    public Slave Create(SlaveSpawner spawner, Item item = null, bool hideSpawnEffect = false, Transform positionOverride = null)
    {
        var slaveTemplate = GetSlaveTemplate(spawner.UnitId);
        if (slaveTemplate == null) return null;

        var tlId = (ushort)TlIdManager.Instance.GetNextId();
        var objId = ObjectIdManager.Instance.GetNextId();

        var slave = new Slave();
        slave.TlId = tlId;
        slave.ObjId = objId;
        slave.TemplateId = slaveTemplate.Id;
        slave.Name = slaveTemplate.Name;
        slave.Level = (byte)slaveTemplate.Level;
        slave.ModelId = slaveTemplate.ModelId;
        slave.Template = slaveTemplate;
        slave.Hp = 1;
        slave.Mp = 1;
        slave.ModelParams = new UnitCustomModelParams();
        slave.Faction = FactionManager.Instance.GetFaction(slaveTemplate.FactionId);
        slave.Id = 0; // TODO (previously set to 10 which prevented the use of the slave doodads 
        slave.Summoner = new Character(new UnitCustomModelParams()); // ?
        slave.SpawnTime = DateTime.UtcNow;

        slave.Transform.ApplyWorldSpawnPosition(spawner.Position);
        if (slave.Transform == null)
        {
            Logger.Error($"Can't spawn slave {spawner.UnitId}");
            return null;
        }

#pragma warning disable CA2000 // Dispose objects before losing scope

        Transform spawnPos;

        // Replacing the position with the new coordinates from the method call parameters

        if (positionOverride != null)
        {
            // If manually defined a spawn location (i.e. created from ShipYard), use that location instead
            spawnPos = positionOverride.CloneDetached();
        }
        else
        {
            spawnPos = slave.Transform;
            // If no spawn position override has been provided, then handle normal spawning algorithm

            // owner.SendMessage("SlaveSpawnOffset: x:{0} y:{1}", slaveTemplate.SpawnXOffset, slaveTemplate.SpawnYOffset);
            //spawnPos.Local.AddDistanceToFront(Math.Clamp(slaveTemplate.SpawnYOffset, 5f, 50f));
            // INFO: Seems like X offset is defined as the size of the vehicle summoned, but visually it's nicer if we just ignore this 
            // spawnPos.Local.AddDistanceToRight(slaveTemplate.SpawnXOffset);
            if (slaveTemplate.IsABoat())
            {
                // If we're spawning a boat, put it at the water level regardless of our own height
                // TODO: if not at ocean level, get actual target location water body height (for example rivers)
                var worldWaterLevel = WorldManager.Instance.GetWorld(spawnPos.WorldId)?.OceanLevel ?? 100f;
                spawnPos.Local.SetHeight(worldWaterLevel);

                // temporary grab ship information so that we can use it to find a suitable spot in front to summon it
                var tempShipModel = ModelManager.Instance.GetShipModel(slaveTemplate.ModelId);
                var minDepth = tempShipModel.MassBoxSizeZ - tempShipModel.MassCenterZ + 1f;
                for (var inFront = 0f; inFront < (50f + tempShipModel.MassBoxSizeX); inFront += 1f)
                {
                    using var depthCheckPos = spawnPos.CloneDetached();
                    depthCheckPos.Local.AddDistanceToFront(inFront);
                    var h = WorldManager.Instance.GetHeight(depthCheckPos);
                    if (h > 0f)
                    {
                        var d = worldWaterLevel - h;
                        if (d > minDepth)
                        {
                            //owner.SendMessage("Extra inFront = {0}, required Depth = {1}", inFront, minDepth);
                            spawnPos = depthCheckPos.CloneDetached();
                            break;
                        }
                    }
                }
            }
            else
            {
                // If a land vehicle, put it a the ground level of it's target spawn location
                // TODO: check for maximum height difference for summoning
                var h = WorldManager.Instance.GetHeight(spawnPos);
                if (h > 0f)
                    spawnPos.Local.SetHeight(h);
            }

            // Always spawn horizontal(level) and 90° CCW of the player
            //spawnPos.Local.SetRotation(0f, 0f, slave.Transform.World.Rotation.Z + (MathF.PI / 2)); 
        }
#pragma warning restore CA2000 // Dispose objects before losing scope

        // TODO
        slave.BroadcastPacket(new SCSlaveCreatedPacket(slave.ObjId, tlId, objId, hideSpawnEffect, 0, slave.Name), true);

        if (_slaveInitialItems.TryGetValue(slave.Template.SlaveInitialItemPackId, out var itemPack))
        {
            foreach (var initialItem in itemPack)
            {
                // var newItem = new Item(WorldManager.DefaultWorldId,ItemManager.Instance.GetTemplate(initialItem.itemId),1);
                var newItem = ItemManager.Instance.Create(initialItem.itemId, 1, 0, false);
                slave.Equipment.AddOrMoveExistingItem(ItemTaskType.Invalid, newItem, initialItem.equipSlotId);
            }
        }

        foreach (var buff in slave.Template.InitialBuffs)
        {
            slave.Buffs.AddBuff(buff.BuffId, slave);
        }

        slave.Hp = slave.MaxHp;
        slave.Mp = slave.MaxMp;

        slave.Transform = spawnPos.CloneDetached(slave);
        slave.Spawn();

        // TODO - DOODAD SERVER SIDE
        foreach (var doodadBinding in slave.Template.DoodadBindings)
        {
            var doodad = new Doodad();
            doodad.ObjId = ObjectIdManager.Instance.GetNextId();
            doodad.TemplateId = doodadBinding.DoodadId;
            doodad.OwnerObjId = slave.ObjId;
            doodad.ParentObjId = slave.ObjId;
            doodad.AttachPoint = doodadBinding.AttachPointId;
            doodad.OwnerId = slave.Id;
            doodad.PlantTime = DateTime.UtcNow;
            doodad.OwnerType = DoodadOwnerType.Slave;
            doodad.OwnerDbId = slave.Id;
            doodad.Template = DoodadManager.Instance.GetTemplate(doodadBinding.DoodadId);
            doodad.Data = (byte)doodadBinding.AttachPointId;
            doodad.ParentObj = slave;

            doodad.SetScale(doodadBinding.Scale);

            doodad.FuncGroupId = doodad.GetFuncGroupId();
            doodad.Transform = slave.Transform.CloneAttached(doodad);
            doodad.Transform.Parent = slave.Transform;

            // NOTE: In 1.2 we can't replace slave parts like sail, so just apply it to all of the doodads on spawn)
            // Should probably have a check somewhere if a doodad can have the UCC applied or not
            if (item != null && item.HasFlag(ItemFlag.HasUCC) && (item.UccId > 0))
                doodad.UccId = item.UccId;

            if (_attachPoints.ContainsKey(slave.ModelId))
            {
                if (_attachPoints[slave.ModelId].ContainsKey(doodadBinding.AttachPointId))
                {
                    doodad.Transform = slave.Transform.CloneAttached(doodad);
                    doodad.Transform.Parent = slave.Transform;
                    doodad.Transform.Local.Translate(_attachPoints[slave.ModelId][doodadBinding.AttachPointId]
                        .AsPositionVector());
                    doodad.Transform.Local.SetRotation(
                        _attachPoints[slave.ModelId][doodadBinding.AttachPointId].Roll,
                        _attachPoints[slave.ModelId][doodadBinding.AttachPointId].Pitch,
                        _attachPoints[slave.ModelId][doodadBinding.AttachPointId].Yaw);
                    Logger.Debug("Model id: {0} attachment {1} => pos {2} = {3}", slave.ModelId,
                        doodadBinding.AttachPointId, _attachPoints[slave.ModelId][doodadBinding.AttachPointId],
                        doodad.Transform);
                }
                else
                {
                    Logger.Warn("Model id: {0} incomplete attach point information", slave.ModelId);
                }
            }
            else
            {
                doodad.Transform = new Transform(doodad);
                Logger.Warn("Model id: {0} has no attach point information", slave.ModelId);
            }

            slave.AttachedDoodads.Add(doodad);

            doodad.Spawn();
        }

        foreach (var slaveBinding in slave.Template.SlaveBindings)
        {
            var childSlaveTemplate = GetSlaveTemplate(slaveBinding.SlaveId);
            var ctlId = (ushort)TlIdManager.Instance.GetNextId();
            var cobjId = ObjectIdManager.Instance.GetNextId();
            var childSlave = new Slave();
            childSlave.TlId = ctlId;
            childSlave.ObjId = cobjId;
            childSlave.ParentObj = slave;
            childSlave.TemplateId = childSlaveTemplate.Id;
            childSlave.Name = childSlaveTemplate.Name;
            childSlave.Level = (byte)childSlaveTemplate.Level;
            childSlave.ModelId = childSlaveTemplate.ModelId;
            childSlave.Template = childSlaveTemplate;
            childSlave.Hp = 1;
            childSlave.Mp = 1;
            childSlave.ModelParams = new UnitCustomModelParams();
            childSlave.Faction = slave.Faction;
            childSlave.Id = 11; // TODO
            childSlave.Summoner = slave.Summoner;
            childSlave.SpawnTime = DateTime.UtcNow;
            childSlave.AttachPointId = (sbyte)slaveBinding.AttachPointId;
            childSlave.OwnerObjId = slave.ObjId;
            childSlave.Hp = childSlave.MaxHp;
            childSlave.Mp = childSlave.MaxMp;
            childSlave.Transform = spawnPos.CloneDetached(childSlave);
            childSlave.Transform.Parent = slave.Transform;

            if (_attachPoints.ContainsKey(slave.ModelId))
            {
                if (_attachPoints[slave.ModelId].ContainsKey(slaveBinding.AttachPointId))
                {
                    var attachPoint = _attachPoints[slave.ModelId][slaveBinding.AttachPointId];
                    // childSlave.AttachPosition = _attachPoints[template.ModelId][(int) slaveBinding.AttachPointId];
                    childSlave.Transform = slave.Transform.CloneAttached(childSlave);
                    childSlave.Transform.Parent = slave.Transform;
                    childSlave.Transform.Local.Translate(attachPoint.AsPositionVector());
                    childSlave.Transform.Local.Rotate(attachPoint.Roll, attachPoint.Pitch, attachPoint.Yaw);
                }
                else
                {
                    Logger.Warn("Model id: {0} incomplete attach point information");
                }
            }

            slave.AttachedSlaves.Add(childSlave);
            lock (_slaveListLock)
                _tlSlaves.Add(childSlave.TlId, childSlave);
            childSlave.Spawn();
        }

        lock (_slaveListLock)
        {
            _tlSlaves.Add(slave.TlId, slave);
            _activeSlaves.Add(slave.ObjId, slave);
        }

        if (slave.Template.IsABoat())
        {
            var world = WorldManager.Instance.GetWorld(slave.Summoner.Transform.WorldId);
            world.Physics.AddShip(slave);
        }

        slave.SendPacket(new SCMySlavePacket(slave.ObjId, slave.TlId, slave.Name, slave.TemplateId,
            slave.Hp, slave.MaxHp,
            slave.Transform.World.Position.X,
            slave.Transform.World.Position.Y,
            slave.Transform.World.Position.Z));

        return slave;
    }

    public void LoadSlaveAttachmentPointLocations()
    {
        Logger.Info("Loading Slave Model Attach Points...");

        var filePath = Path.Combine(FileManager.AppPath, "Data", "slave_attach_points.json");
        var contents = FileManager.GetFileContents(filePath);
        if (string.IsNullOrWhiteSpace(contents))
            throw new IOException(
                $"File {filePath} doesn't exists or is empty.");

        if (JsonHelper.TryDeserializeObject(contents, out List<SlaveModelAttachPoint> attachPoints, out _))
            Logger.Info("Slave model attach points loaded...");
        else
            Logger.Warn("Slave model attach points not loaded...");

        // Convert degrees from json to radian
        foreach (var vehicle in attachPoints)
        {
            foreach (var pos in vehicle.AttachPoints)
            {
                pos.Value.Roll = pos.Value.Roll.DegToRad();
                pos.Value.Pitch = pos.Value.Pitch.DegToRad();
                pos.Value.Yaw = pos.Value.Yaw.DegToRad();
            }
        }

        _attachPoints = new Dictionary<uint, Dictionary<AttachPointKind, WorldSpawnPosition>>();
        foreach (var set in attachPoints)
        {
            _attachPoints[set.ModelId] = set.AttachPoints;
        }
    }

    public void Load()
    {
        _slaveListLock = new object();
        _slaveTemplates = new Dictionary<uint, SlaveTemplate>();
        lock (_slaveListLock)
        {
            _activeSlaves = new Dictionary<uint, Slave>();
            _tlSlaves = new Dictionary<uint, Slave>();
        }
        _slaveInitialItems = new Dictionary<uint, List<SlaveInitialItems>>();

        #region SQLLite

        using (var connection = SQLite.CreateConnection())
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM slaves";
                command.Prepare();
                using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        var template = new SlaveTemplate
                        {
                            Id = reader.GetUInt32("id"),
                            Name =
                                LocalizationManager.Instance.Get("slaves", "name", reader.GetUInt32("id"),
                                    reader.GetString("name")),
                            ModelId = reader.GetUInt32("model_id"),
                            Mountable = reader.GetBoolean("mountable"),
                            SpawnXOffset = reader.GetFloat("spawn_x_offset"),
                            SpawnYOffset = reader.GetFloat("spawn_y_offset"),
                            FactionId = reader.GetUInt32("faction_id", 0),
                            Level = reader.GetUInt32("level"),
                            Cost = reader.GetInt32("cost"),
                            SlaveKind = (SlaveKind)reader.GetUInt32("slave_kind_id"),
                            SpawnValidAreaRance = reader.GetUInt32("spawn_valid_area_range", 0),
                            SlaveInitialItemPackId = reader.GetUInt32("slave_initial_item_pack_id", 0),
                            SlaveCustomizingId = reader.GetUInt32("slave_customizing_id", 0),
                            Customizable = reader.GetBoolean("customizable", false),
                            PortalTime = reader.GetFloat("portal_time")
                        };
                        _slaveTemplates.Add(template.Id, template);
                    }
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM slave_initial_items";
                command.Prepare();

                using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        var ItemPackId = reader.GetUInt32("slave_initial_item_pack_id");
                        var SlotId = reader.GetByte("equip_slot_id");
                        var item = reader.GetUInt32("item_id");

                        if (_slaveInitialItems.TryGetValue(ItemPackId, out var key))
                        {
                            key.Add(new SlaveInitialItems() { slaveInitialItemPackId = ItemPackId, equipSlotId = SlotId, itemId = item });
                        }
                        else
                        {
                            var newPack = new List<SlaveInitialItems>();
                            var newKey = new SlaveInitialItems
                            {
                                slaveInitialItemPackId = ItemPackId,
                                equipSlotId = SlotId,
                                itemId = item
                            };
                            newPack.Add(newKey);

                            _slaveInitialItems.Add(ItemPackId, newPack);
                        }
                    }
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM slave_initial_buffs";
                command.Prepare();

                using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        var template = new SlaveInitialBuffs
                        {
                            Id = reader.GetUInt32("id"),
                            SlaveId = reader.GetUInt32("slave_id"),
                            BuffId = reader.GetUInt32("buff_id")
                        };
                        if (_slaveTemplates.ContainsKey(template.SlaveId))
                        {
                            _slaveTemplates[template.SlaveId].InitialBuffs.Add(template);
                        }
                    }
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM slave_passive_buffs";
                command.Prepare();

                using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        var template = new SlavePassiveBuffs
                        {
                            Id = reader.GetUInt32("id"),
                            OwnerId = reader.GetUInt32("owner_id"),
                            OwnerType = reader.GetString("owner_type"),
                            PassiveBuffId = reader.GetUInt32("passive_buff_id")
                        };
                        if (_slaveTemplates.ContainsKey(template.OwnerId))
                        {
                            _slaveTemplates[template.OwnerId].PassiveBuffs.Add(template);
                        }
                    }
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM slave_doodad_bindings";
                command.Prepare();

                using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        var template = new SlaveDoodadBindings
                        {
                            Id = reader.GetUInt32("id"),
                            OwnerId = reader.GetUInt32("owner_id"),
                            OwnerType = reader.GetString("owner_type"),
                            AttachPointId = (AttachPointKind)reader.GetInt32("attach_point_id"),
                            DoodadId = reader.GetUInt32("doodad_id"),
                            Persist = reader.GetBoolean("persist"),
                            Scale = reader.GetFloat("scale")
                        };
                        if (_slaveTemplates.ContainsKey(template.OwnerId))
                        {
                            _slaveTemplates[template.OwnerId].DoodadBindings.Add(template);
                        }
                    }
                }
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM slave_bindings";
                command.Prepare();

                using (var reader = new SQLiteWrapperReader(command.ExecuteReader()))
                {
                    while (reader.Read())
                    {
                        var template = new SlaveBindings()
                        {
                            Id = reader.GetUInt32("id"),
                            OwnerId = reader.GetUInt32("owner_id"),
                            OwnerType = reader.GetString("owner_type"),
                            AttachPointId = (AttachPointKind)reader.GetUInt32("attach_point_id"),
                            SlaveId = reader.GetUInt32("slave_id")
                        };

                        if (_slaveTemplates.ContainsKey(template.OwnerId))
                        {
                            _slaveTemplates[template.OwnerId].SlaveBindings.Add(template);
                        }
                    }
                }
            }
        }
        #endregion

        LoadSlaveAttachmentPointLocations();
    }

    public static void Initialize()
    {
        var sendMySlaveTask = new SendMySlaveTask();
        TaskManager.Instance.Schedule(sendMySlaveTask, TimeSpan.Zero, TimeSpan.FromSeconds(5));
    }

    public void SendMySlavePacketToAllOwners()
    {
        Dictionary<uint, Slave> slaveList = null;
        lock (_slaveListLock)
            slaveList = _activeSlaves;

        foreach (var (ownerObjId, slave) in slaveList)
        {
            var owner = WorldManager.Instance.GetCharacterByObjId(ownerObjId);
            owner?.SendPacket(new SCMySlavePacket(slave.ObjId, slave.TlId, slave.Name, slave.TemplateId,
                slave.Hp, slave.MaxHp,
                slave.Transform.World.Position.X,
                slave.Transform.World.Position.Y,
                slave.Transform.World.Position.Z));
        }
    }

    public Slave GetIsMounted(uint objId, out AttachPointKind attachPoint)
    {
        attachPoint = AttachPointKind.None;
        lock (_slaveListLock)
        {
            foreach (var slave in _activeSlaves.Values)
                foreach (var unit in slave.AttachedCharacters)
                {
                    if (unit.Value.ObjId == objId)
                    {
                        attachPoint = unit.Key;
                        return slave;
                    }
                }
        }

        return null;
    }
}
