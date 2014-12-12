using Dungeon.Generator.Generation.Generators;
using Dungeon.Generator.Generation.Generators.GridBased;
using Dungeon.Generator.Generation.Generators.RoomBased;
using Dungeon.Generator.Navigation;
using System;

namespace Dungeon.Generator.Generation
{
    public class Generator
    {
        public static ITileMap GenerateGridBased(MapSize size, uint seed = 1024u)
        {
            return Generate(size, seed, typeof(GridBased));
        }

        public static ITileMap GenerateRoomBased(MapSize size, uint seed = 1024u)
        {
            return Generate(size, seed, typeof(RoomBased));
        }


        private static ITileMap Generate(MapSize size, uint seed, Type dungeon)
        {
            var dim = size.ToDimensions();
            var map = new Dungeon(dim.X, dim.Y);
            var random = new MersennePrimeRandom(seed);

            var temp = new Type[] { typeof(MersennePrimeRandom), typeof(ITileMap) };
            var ctor = dungeon.GetConstructor(temp);
            var strategy = (DungeonGeneration)ctor.Invoke(new object[] { random, map });
            strategy.Execute();

            return map;
            
        }
    }

}