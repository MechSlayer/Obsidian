﻿
using Obsidian.Blocks;
using System.Runtime.CompilerServices;

namespace Obsidian.WorldData;

internal class WorldLight
{
    private readonly World world;

    public WorldLight(World world)
    {
        this.world = world;
    }

    public async Task ProcessSkyLightForChunk(Chunk chunk)
    {
        // No skylight for nether/end
        if (world.Name != "overworld") { return; }

        for (int x = 0; x < 16; x++)
        {
            for (int z = 0; z < 16; z++)
            {
                var surfaceY = chunk.Heightmaps[ChunkData.HeightmapType.MotionBlocking].GetHeight(x, z);
                for (int y = 319; y > surfaceY; y--)
                {
                    var secIndex = (y >> 4) + 4;
                    if (chunk.Sections[secIndex].IsEmpty)
                    {
                        y -= 15;
                    }
                    else
                    {
                        chunk.SetLightLevel(x, y, z, LightType.Sky, 15);
                    }
                }

                await SetLightAndSpread(new Vector(x, surfaceY, z), LightType.Sky, 15, chunk);
            }
        }

        //TODO: Check neighboring chunks (if exist) for light on edges
    }

    public async Task SetLightAndSpread(Vector pos, LightType lt, int level, Chunk chunk)
    {
        if (chunk is null) { return; }
        if (!chunk.isGenerated) { return; }

        int curLevel = chunk.GetLightLevel(pos, lt);
        if (level <= curLevel) { return; }

        chunk.SetLightLevel(pos, lt, level);

        // Can spread up with no loss of level
        // as long as there is a neighbor that's non-transparent.
        for (int spreadY = 1; spreadY < 320 - pos.Y; spreadY++)
        {
            var secIndex = ((pos.Y + spreadY) >> 4) + 4;
            if (chunk.Sections[secIndex].IsEmpty) { break; }

            foreach (Vector dir in Vector.CardinalDirs)
            {
                if (!chunk.GetBlock(pos + (0, spreadY, 0) + dir).IsTransparent)
                {
                    chunk.SetLightLevel(pos + (0, spreadY, 0), lt, level);
                    break;
                }
            }
        }

        level--;

        if (level == 0) { return; }

        // Can spread in any cardinal direction and up/down.
        // No level lost for travelling vertically.
        foreach (Vector dir in Vector.CardinalDirs)
        {
            // If light would propogate to another chunk, bail out now
            // TODO: don't bail out lol - get new chunk ref
            if (pos.X == 0 && dir == Vector.West ||
                pos.X == 15 && dir == Vector.East ||
                pos.Z == 0 && dir == Vector.North ||
                pos.Z == 15 && dir == Vector.South)
            {
                continue;
            }

            var highY = chunk.Heightmaps[ChunkData.HeightmapType.MotionBlocking].GetHeight(pos.X, pos.Z) + 1;

            // Spread up
            for (int spreadY = 1; spreadY < (highY - pos.Y); spreadY++)
            {
                // To spread up, there must only be transparent blocks above the source
                var upBlock = chunk.GetBlock(pos + (0, spreadY, 0));
                if (!upBlock.IsTransparent) { break; }
                
                var scanPos = pos + dir + (0, spreadY, 0);
                if (chunk.GetBlock(scanPos).IsTransparent)
                {
                    if (!chunk.GetBlock(scanPos + Vector.Down).IsTransparent)
                    {
                        await SetLightAndSpread(scanPos + Vector.Down, lt, level, chunk);
                    }
                }
            }

            // Spread down
            // To spread down, the block above the adjacent must be transparent
            if (!chunk.GetBlock(pos + dir + Vector.Up).IsTransparent) { continue; }

            // Find the first non-transparent block and set level
            for (int spreadY = 0; spreadY > (-64 - pos.Y); spreadY--)
            {
                var scanPos = pos + dir + (0, spreadY, 0);
                if (!chunk.GetBlock(scanPos).IsTransparent)
                {
                    await SetLightAndSpread(scanPos, lt, level, chunk);
                    break;
                }
            }
        }
    }
}
