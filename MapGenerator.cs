using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject endPrefab;
    public GameObject floor;

    GameObject spawnedPlayer;
    GameObject spawnedEnd;
    public int width;
    public int height;
    public int squareSize;
    
    public Location startLocation;
    public Location endLocation;

    public Material[] wallMaterials;
    public Material[] floorMaterials;

    public enum Level_Type
    {
        Arid,
        Desert,
        Water,
        Ice,
        Lava,
        Plains,
        Forest,
        White,
        Abyss
    }

    public enum Location
    {
        none = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
        BottomLeft = 4
    }

    [Range(1, 10)]
    public int borderSize;
    [Range(1, 25)]
    public int wallHeight;
    public string seed;
    public bool useRandomSeed;
    public bool useRandomSizer;
    [Range(1, 10)]
    public int smoothMultiplier;
   
    public bool noMinuscules;
    [Range(0, 100)]
    public int minusculeWallThreshhold;
    [Range(0, 100)]
    public int minusculeRoomThreshhold;
    [Range(45, 55)]
    public int fillModifier;
    [Range(1, 5)]
    public int passageSize;
    public bool displayMapGizmo;
    int[,] map;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateMap();
        }
    }

    public void GenerateMap()
    {

        Destroy(spawnedPlayer);
        Destroy(spawnedEnd);
        startLocation = Location.none;
        endLocation = Location.none;
        map = new int[width, height];
        if (useRandomSizer)
        {
            System.Random r = new System.Random();
            fillModifier = r.Next(45, 55);
        }
        RandomFillMap();

        for (int i = 0; i < smoothMultiplier; i++)
        {
            SmoothMap();
        }
        ProcessMap();
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }

        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, squareSize ,(float)wallHeight);
        playerPrefab.transform.position = GetStartLocation();
        endPrefab.transform.position = GetFinishLocation();
        //Vector3 spawnPoint = GetStartLocation();
        //Vector3 endPoint = GetFinishLocation();
        //SpawnPlayer(spawnPoint);
        //SpawnEnd(endPoint);
        floor.transform.position = new Vector3(0, -wallHeight, 0);
    }

    public Vector3 FindSpot(Location location)
    {
        Vector3 spot;
        spot = new Vector3(0f, 0f, 0f);

        if (location == Location.BottomLeft)
        {
            for (int x = 0; x < map.GetLength(0) - 1; x++)
            {
                for (int y = 0; y < map.GetLength(1) - 1; y++)
                {
                    //Debug.Log("BOTTOMLEFT ORDER : [" + x + "," + y + "] = " + map[x,y]);
                    if (map[x,y] == 0)
                    {
                        Coord c = new Coord(x, y);
                        return PlayerCoordToWorldPoint(c);  
                    }
                }
            }
        }
        if (location == Location.BottomRight)
        {
            for (int x = map.GetLength(0) - 1; x > 0; x--)
            {
                for (int y = 0; y < map.GetLength(1) - 1; y++)
                {
                    //Debug.Log("BOTTOMRIGHT ORDER : [" + x + "," + y + "] = " + map[x, y]);
                    if (map[x, y] == 0)
                    {
                        Coord c = new Coord(x, y);
                        return PlayerCoordToWorldPoint(c);
                    }
                }
            }
        }
        if (location == Location.TopRight)
        {
            for (int x = map.GetLength(0) - 1; x > 0; x--)
            {
                for (int y = map.GetLength(1) - 1; y > 0; y--)
                {
                    //Debug.Log("TOPRIGHT ORDER : [" + x + "," + y + "] = " + map[x, y]);
                    if (map[x, y] == 0)
                    {
                        Coord c = new Coord(x, y);
                        return PlayerCoordToWorldPoint(c);
                    }
                }
            }
        }
        if (location == Location.TopLeft)
        {
            for (int x = 0; x < map.GetLength(0) - 1; x++)
            {
                for (int y = map.GetLength(1) - 1; y > 0; y--)
                {
                    //Debug.Log("TOPLEFT ORDER : [" + x + "," + y + "] = " + map[x, y]);
                    if (map[x, y] == 0)
                    {
                        Coord c = new Coord(x, y);
                        return PlayerCoordToWorldPoint(c);
                    }
                }
            }
        }
        return spot;
    }

    public Vector3 GetStartLocation()
    {
        System.Random r = new System.Random();
        startLocation = (Location)r.Next(1, 4);
        if (startLocation == Location.none)
        {
            startLocation = (Location)r.Next(1, 4);
        }

        return FindSpot(startLocation);
    }

    public Vector3 GetFinishLocation()
    {
        System.Random r = new System.Random();
        if (startLocation == Location.TopLeft)
        {
            endLocation = Location.BottomRight;
        }
        else if (startLocation == Location.TopRight)
        {
            endLocation = Location.BottomLeft;
        }
        else if (startLocation == Location.BottomRight)
        {
            endLocation = Location.TopLeft;
        }
        else if (startLocation == Location.BottomLeft)
        {
            endLocation = Location.TopRight;
        }
        return FindSpot(endLocation);
    }

    public void SpawnPlayer(Vector3 sp)
    {
        if (playerPrefab != null)
        {
            spawnedPlayer = Instantiate(playerPrefab,sp,Quaternion.identity,transform) as GameObject;
            Debug.Log("Spawn Point at : " + sp);
        }
    }

    public void SpawnEnd(Vector3 ep)
    {
        spawnedEnd = Instantiate(endPrefab, ep, Quaternion.identity, transform) as GameObject;
        Debug.Log("End Point at : " + ep);
    }

    public void ProcessMap()
    {
        int wallThresholdSize = 0;
        int roomThreshholdSize = 0;
        if (noMinuscules)
        {
            wallThresholdSize = minusculeWallThreshhold;
            roomThreshholdSize = minusculeRoomThreshhold;
        }
        List<List<Coord>> wallRegions = GetRegions(1);

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);

        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThreshholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        survivingRooms.Sort();
        survivingRooms[0].isMainRoom = true;
        survivingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(survivingRooms);
    }

    public void ConnectClosestRooms(List<Room> allRooms,bool forceAccessibilityFromMainRoom = false)
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMainRoom)
        {
            foreach(Room room in allRooms)
            {
                if (room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnectionFound = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if(roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            foreach (Room roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];
                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));
                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAccessibilityFromMainRoom)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }



    public void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        //Debug.DrawLine(CoordToWorldPoint(tileA), CoordToWorldPoint(tileB), Color.green, 5f);

        List<Coord> line = GetLine(tileA, tileB);
        foreach(Coord c in line)
        {
            DrawCircle(c, passageSize);
        }
    }

    public void DrawCircle(Coord c, int r)
    {
        for(int x = -r; x<= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if(x*x + y*y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    public List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for(int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));
            if (inverted)
            {
                y += step;
            }else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if(gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    public Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2f + 0.5f + tile.tileX, 2f, -height / 2f + 0.5f + tile.tileY);
    }

    public Vector3 PlayerCoordToWorldPoint(Coord tile)
    {
        float size = squareSize;
        float mapWidth = map.GetLength(0) * size;
        float mapHeight = map.GetLength(1) * size;
        
        return new Vector3(-mapWidth / 2 + tile.tileX * size + size / 2, -wallHeight + 1f, -mapHeight / 2 + tile.tileY * size + size / 2);
    }


    public List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);
                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    public List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    public bool IsInMapRange(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    public void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < fillModifier) ? 1 : 0;
                }
            }
        }
    }

    public void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neigbourWallTiles = GetSurroundingWallCount(x, y);
                if (neigbourWallTiles > 4)
                {
                    map[x, y] = 1;
                }
                else if (neigbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    public int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallcount = 0;
        for (int x = gridX - 1; x <= gridX + 1; x++)
        {
            for (int y = gridY - 1; y <= gridY + 1; y++)
            {
                if (IsInMapRange(x, y))
                {
                    if (x != gridX || y != gridY)
                    {
                        wallcount += map[x, y];
                    }
                }
                else
                {
                    wallcount++;
                }
            }
        }
        return wallcount;
    }

    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }

    }

    public class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        public int roomSize;

        public Room()
        {

        }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }
        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            }
            else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
        public void SetAccessibleFromMainRoom()
        {
            if (!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach(Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }
    }


    public void OnDrawGizmos()
    {
        if (displayMapGizmo)
        {
            if (map != null)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                        Vector3 pos = CoordToWorldPoint(new Coord(x, y));
                        Gizmos.DrawCube(pos, Vector3.one);
                    }
                }
            }
        }
    }

}
