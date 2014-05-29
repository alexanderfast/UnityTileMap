using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

/// <summary>
/// A good and complete dungeon generation algorithm is out of the scope for this showcase,
/// it focuses instead on the TileMap aspect of a roguelike game.
/// But something, even if minimal and naive, is needed to provide the levels.
/// </summary>
public class DungeonGenerator
{
    private readonly Grid<TileType> m_grid = new Grid<TileType>();

    public Grid<TileType> Generate(int sizeX, int sizeY)
    {
        m_grid.SetSize(sizeX, sizeY, TileType.Empty);

        // divide the map into four quadrants for four rooms, once on x and once on y
        var divX = Random.Range((sizeX / 2) - 2, (sizeX / 2) + 3);
        var divY = Random.Range((sizeY / 2) - 2, (sizeY / 2) + 3);

        // create four rooms
        var bottomLeft = CreateRandomRoom(0, divX - 1, 0, divY - 1);
        var bottomRight = CreateRandomRoom(divX + 1, sizeX - 1, 0, divY - 1);
        var upperLeft = CreateRandomRoom(0, divX - 1, divY + 1, sizeY - 1);
        var upperRight = CreateRandomRoom(divX + 1, sizeX - 1, divY + 1, sizeY - 1);

        // create four corridors between the rooms
        CreateRandomCorridor(bottomLeft, upperLeft);
        CreateRandomCorridor(bottomRight, upperRight);
        CreateRandomCorridor(bottomLeft, bottomRight);
        CreateRandomCorridor(upperLeft, upperRight);

        return m_grid;
    }

    private Room CreateRandomRoom(int x1, int x2, int y1, int y2)
    {
        x1 += Random.Range(0, 2);
        x2 -= Random.Range(0, 2);
        y1 += Random.Range(0, 2);
        y2 -= Random.Range(0, 2);
        return CreateRoom(x1, x2, y1, y2);
    }

    private Room CreateRoom(int x1, int x2, int y1, int y2)
    {
        //Debug.Log(string.Format("Room x {0} -> {1} y {2} -> {3}", x1, x2, y1, y2));
        var room = new Room(x1, x2, y1, y2);
        foreach (var c in room.GetFloorTiles())
            m_grid[c.X, c.Y] = TileType.Floor;
        foreach (var c in room.GetWallTiles())
            m_grid[c.X, c.Y] = TileType.Wall;
        return room;
    }

    private void CreateRandomCorridor(Room a, Room b)
    {
        var sharedX = a.IntersectX(b).ToList();
        if (sharedX.Count > 0)
        {
            var x = sharedX[Random.Range(0, sharedX.Count)];
            CreateCorridor(x - 1, x + 1, a.Y1, b.Y2);
            return;
        }
        var sharedY = a.IntersectY(b).ToList();
        if (sharedY.Count > 0)
        {
            var y = sharedY[Random.Range(0, sharedY.Count)];
            CreateCorridor(a.X1, b.X2, y - 1, y + 1);
            return;
        }
    }

    private void CreateCorridor(int x1, int x2, int y1, int y2)
    {
        //Debug.Log(string.Format("Corridor x {0} -> {1} y {2} -> {3}", x1, x2, y1, y2));
        var room = new Room(x1, x2, y1, y2);
        foreach (var c in room.GetFloorTiles())
            m_grid[c.X, c.Y] = TileType.Floor;
        foreach (var c in room.GetWallTiles())
        {
            if (m_grid[c.X, c.Y] != TileType.Empty)
                continue;
            m_grid[c.X, c.Y] = TileType.Wall;
        }
    }

    private class Room
    {
        public int X1;
        public int X2;
        public int Y1;
        public int Y2;

        public Room(int x1, int x2, int y1, int y2)
        {
            X1 = x1;
            X2 = x2;
            Y1 = y1;
            Y2 = y2;
        }

        public IEnumerable<int> IntersectX(Room other)
        {
            for (int x = X1 + 1; x < X2; x++)
            {
                if (x > other.X1 && x < other.X2)
                    yield return x;
            }
        }

        public IEnumerable<int> IntersectY(Room other)
        {
            for (int y = Y1 + 1; y < Y2; y++)
            {
                if (y > other.Y1 && y < other.Y2)
                    yield return y;
            }
        }

        public IEnumerable<Coord> GetWallTiles()
        {
            for (int x = X1; x <= X2; x++)
            {
                yield return new Coord(x, Y1);
                yield return new Coord(x, Y2);
            }
            for (int y = Y1 + 1; y < Y2; y++)
            {
                yield return new Coord(X1, y);
                yield return new Coord(X2, y);
            }
        }

        public IEnumerable<Coord> GetFloorTiles()
        {
            for (int y = Y1 + 1; y < Y2; y++)
            {
                for (int x = X1 + 1; x < X2; x++)
                    yield return new Coord(x, y);
            }
        }
    }

    private class Coord
    {
        public int X;
        public int Y;

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
