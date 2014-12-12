using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using Dungeon.Generator.Navigation;

namespace Dungeon.Generator.Generation.Generators.RoomBased
{
    public class RoomBased : DungeonGeneration
    {
        private Room[,] _roomGrid;


        public RoomBased(MersennePrimeRandom random, ITileMap map)
            : base(random, map)
        {
            _roomGrid = new Room[_dimensions.X, _dimensions.Y];
        }

        public override void Execute()
        {
            _roomGrid = new Room[_map.Width/GridSize, _map.Height/GridSize];
            var location = new Point {
                X = _roomGrid.GetLength(0)/2,
                Y = _roomGrid.GetLength(1)/2
            };

            var centerRoom = _roomGrid[location.X, location.Y] = new Room { 
                Location = location,
                Type = RoomType.Room
            };

            var unprocessed = new Queue<Room>();
            unprocessed.Enqueue(centerRoom);

            var directions = DirectionHelpers.Values();

            var totalCount = 1;

            // for each unprocessed room
            while (unprocessed.Count > 0)
            {
                var room = unprocessed.Dequeue();

                // decide which directions to spawn rooms in
                var newRooms = directions
                    .Where(x => {
                        // get the new room location
                        var newRoomLocation = room.Location.Move(x);

                        var canSpawnRoomAtLocation = _map.Contains(newRoomLocation, GridSize) 
                                                  && _roomGrid[newRoomLocation.X, newRoomLocation.Y] == default(Room);

                        var shouldSpawn = Chance(75 - totalCount);

                        if (room.Type == RoomType.Corridor)
                            return shouldSpawn && x == room.Cardinality && canSpawnRoomAtLocation;

                        // can spawn if its a valid location and there is a chance to spawn said room
                        return shouldSpawn && canSpawnRoomAtLocation;
                    })
                    // spawn rooms in those directions
                    .Select(direction => {
                        var newLocation = room.Location.Move(direction);

                        // carve the outlets for the new rooms
                        _map.Carve(room.GetCenterWallPoint(direction, GridSize), 1, 1, 1);

                        totalCount++;

                         Room newRoom = Room.CreateRoom(newLocation, direction);
                        
                        return _roomGrid[newLocation.X, newLocation.Y] = newRoom;
                    });


                newRooms.Aggregate(unprocessed, (acc, spawnedRoom) => {
                    // add them to the unprocessed list
                    acc.Enqueue(spawnedRoom);
                    return acc;
                });



                // carve our room
                room.Carve(_map, GridSize);
            }
        }
    }
}