﻿using Obsidian.Chat;
using Obsidian.Net;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace Obsidian.Entities
{
    public abstract class Entity
    {
        public readonly Timer TickTimer = new Timer();

        public Entity()
        {
        }

        public EntityBitMask BitMask { get; set; }

        public int Air { get; set; } = 300;

        public ChatMessage CustomName { get; private set; }

        public bool CustomNameVisible { get; private set; }
        public bool Silent { get; private set; }
        public bool NoGravity { get; set; }

        public async Task WriteAsync(MinecraftStream stream)
        {
            await stream.WriteAsEntityMetdata(0, EntityMetadataType.Byte, (byte)BitMask);

            await stream.WriteAsEntityMetdata(1, EntityMetadataType.VarInt, Air);

            if (CustomName == null)
            {
                await stream.WriteAsEntityMetdata(2, EntityMetadataType.String, "test");
            }
            else
            {
                await stream.WriteAsEntityMetdata(2, EntityMetadataType.Chat, CustomName);
            }


            await stream.WriteAsEntityMetdata(3, EntityMetadataType.Boolean, CustomNameVisible);
            await stream.WriteAsEntityMetdata(4, EntityMetadataType.Boolean, Silent);
            await stream.WriteAsEntityMetdata(5, EntityMetadataType.Boolean, NoGravity);
        }
    }

    [Flags]
    public enum EntityBitMask : byte
    {
        None = 0x00,
        OnFire = 0x01,
        Crouched = 0x02,

        [Obsolete]
        Riding = 0x04,

        Sprinting = 0x08,
        Swimming = 0x10,
        Invisible = 0x20,
        Glowing = 0x40,
        Flying = 0x80
    }
}