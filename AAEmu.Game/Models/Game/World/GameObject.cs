﻿using System;
using AAEmu.Game.Core.Managers.World;
using AAEmu.Game.Core.Network.Game;
using AAEmu.Game.Core.Packets.G2C;
using AAEmu.Game.Models.Game.Char;
using AAEmu.Game.Models.Game.Chat;
using AAEmu.Game.Utils;
using AAEmu.Game.Models.Game.World.Transform;

namespace AAEmu.Game.Models.Game.World
{
    public class GameObject
    {
        public Guid Guid { get; set; } = Guid.NewGuid();
        public uint ObjId { get; set; }
        public uint InstanceId { get; set; } = WorldManager.DefaultInstanceId ;
        public bool DisabledSetPosition { get; set; }
        /// <summary>
        /// Contains position, rotation, zone and instance information
        /// </summary>
        public Transform.Transform Transform { get; set; }
        //public Point Position { get; set; }
        /// <summary>
        /// When not null, this is the location where the character last entered a instance from
        /// </summary>
        public Transform.Transform MainWorldPosition { get; set; }
        public Region Region { get; set; }
        public DateTime Respawn { get; set; }
        public DateTime Despawn { get; set; }
        public virtual bool IsVisible { get; set; }
        public GameObject ParentObj { get; set; }
        public virtual float ModelSize { get; set; } = 0f; 

        public GameObject()
        {
            Transform = new Transform.Transform(this,null);
        }

        public virtual void SetPosition(float x, float y, float z, float rotationX, float rotationY, float rotationZ)
        {
            if (DisabledSetPosition)
                return;

            //var rX = MathUtil.ConvertDirectionToRadian((sbyte)MathF.Round(rotationX));
            //var rY = MathUtil.ConvertDirectionToRadian((sbyte)MathF.Round(rotationY));
            //var rZ = MathUtil.ConvertDirectionToRadian((sbyte)MathF.Round(rotationZ));

            /*
            if (this is Character c)
            {
                c.SendMessage("SetPositionRaw(x{0:0.##} y{1:0.##} z{2:0.##} rx{3:0.##} ry{4:0.##} rz{5:0.##})", x, y, z,
                    rotationX.RadToDeg(), rotationY.RadToDeg(), rotationZ.RadToDeg());
                c.SendMessage("SetPosition(x{0:0.##} y{1:0.##} z{2:0.##} rx{3:0.##} ry{4:0.##} rz{5:0.##})", x, y, z, rX, rY, rZ);
            }
            */

            Transform.Local.SetPosition(x, y, z, rotationX, rotationY, rotationZ);
            //Transform.Local.SetPosition(x, y, z, (float)rX, (float)rY , (float)rZ);
            WorldManager.Instance.AddVisibleObject(this);
        }

        public virtual void Spawn()
        {
            WorldManager.Instance.AddObject(this);
            Show();
        }

        public virtual void Delete()
        {
            Hide();
            WorldManager.Instance.RemoveObject(this);
        }

        public virtual void Show()
        {
            IsVisible = true;
            WorldManager.Instance.AddVisibleObject(this);
        }

        public virtual void Hide()
        {
            IsVisible = false;
            WorldManager.Instance.RemoveVisibleObject(this);
        }

        /// <summary>
        /// Broadcasts packet to all players near this GameObject
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="self">Include sending to self (only for if called from Character)</param>
        public virtual void BroadcastPacket(GamePacket packet, bool self)
        {
            foreach (var character in WorldManager.Instance.GetAround<Character>(this))
                character.SendPacket(packet);
            if ((self) && (this is Character chr))
                chr.SendPacket(packet);
        }

        public virtual void AddVisibleObject(Character character)
        {
            if ((Transform != null) && (Transform.Children.Count > 0))
                foreach (var child in Transform.Children.ToArray())
                    child?.GameObject?.AddVisibleObject(character);
        }

        public virtual void RemoveVisibleObject(Character character)
        {
            if ((character.CurrentTarget != null) && (character.CurrentTarget == this))
            {
                character.CurrentTarget = null;
                character.SendPacket(new SCTargetChangedPacket(character.ObjId, 0));
            }
            if ((Transform != null) && (Transform.Children.Count > 0))
                foreach (var child in Transform.Children.ToArray())
                    child?.GameObject?.RemoveVisibleObject(character);
        }
    }
}
