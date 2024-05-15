using System;
using System.Collections.Generic;
using Godot;

namespace PluginPCG;

public class CellularAutomataBuilder : IMapBuilder
{
    #region VARIABLES
    
    private int _width;
    private int _height;
    private int _borderWidth;
    private int _density;
    private int _smoothing;
    private int _minRegionSize;
    private int _seed;
    private bool _useRandomSeed;
    private Vector2I _wallTile;
    private Vector2I _nextToRoomTile;
    
    private int currentSmoothing;
    
    private const int Wallthreshold = 4;
    
    private int[,] _map;
    private Random _pseudoRandom;

    #endregion
    
    #region INITIALIZATION
    
    public void Initialize(int width, int height, Dictionary<string, object> parameters)
    {
        _width = width;
        _height = height;
        _borderWidth = Convert.ToInt32(parameters["borderWidth"]);
        _density = Convert.ToInt32(parameters["density"]);
        _smoothing = Convert.ToInt32(parameters["smoothing"]);
        // _minRegionSize = Convert.ToInt32(parameters["minRegionSize"]);
        _useRandomSeed = Convert.ToBoolean(parameters["useRandomSeed"]);
        _seed = Convert.ToInt32(parameters["seed"]);

        _map = new int[_width, _height];
        _pseudoRandom = _useRandomSeed ? new Random() : new Random(_seed);
        
        int wallTileX = Convert.ToInt32(parameters["wallAtlasCoordX"]);
        int wallTileY = Convert.ToInt32(parameters["wallAtlasCoordY"]);
        int nextToRoomTileX = Convert.ToInt32(parameters["nextToRoomAtlasCoordX"]);
        int nextToRoomTileY = Convert.ToInt32(parameters["nextToRoomAtlasCoordY"]);
        
        _wallTile = new Vector2I(wallTileX, wallTileY);
        _nextToRoomTile = new Vector2I(nextToRoomTileX, nextToRoomTileY);
        
        currentSmoothing = 0;
    }
    
    #endregion

    #region GENERATE_MAP
    /**
     * Generate the map, iterates over the tiles, checks if the tile is a border tile, if so, it sets it to a wall,
     * if not, it sets it to a floor
     * After that, it smooths the map and cleans up the regions
     */
    public void GenerateMap(TileMap tileMap)
    {
        int index = 0;
        
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (IsBorderTile(x, y))
                {
                    _map[x, y] = 1;
                }
                else
                {
                    _map[x, y] = _pseudoRandom.Next(0, 100) <= _density ? 1 : 0;
                }
            }
        }
        
        while (currentSmoothing < _smoothing)
        {
            SmoothMap();
            currentSmoothing++;
        }

        // CleanupRegions();
        
        GenerateDijkstraMap(new Vector2I(_width / 2, _height / 2));
        
        ChangeTilesNextToRooms();

        UpdateTilemap(tileMap);
    }
    
    /**
     * Check if the tile is a border tile
     *
     * @param x The x coordinate of the tile
     * @param y The y coordinate of the tile
     */
    private bool IsBorderTile(int x, int y)
    {
        return
            x < _borderWidth ||
            x >= _width - _borderWidth ||
            y < _borderWidth ||
            y >= _height - _borderWidth;
    }
    
    /**
     * Smooths the map, iterates over the tiles, checks if the number of neighbours is greater the Wallthreshold,
     * if so, it sets the tile to a wall, if it is lower, it sets it to a floor, if it is equal, it leaves it as it is
     */
    private void SmoothMap()
    {
        int[,] temp = new int[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                int nWalls1 = NeighbouringWallCount(x, y);
                temp[x, y] = nWalls1 switch
                {
                    < Wallthreshold => 0,
                    > Wallthreshold => 1,
                    _ => _map[x, y]
                };
            }
        }

        _map = temp;
    }
    
    /**
     * Get the number of neighbouring walls of a tile
     * It iterates over the neighbours of the tile and checks if it is in bounds,
     * if it is, it adds the value of the tile to the count
     * if it is not, it adds 1 to the count
     *
     * @param x The x coordinate of the tile
     * @param y The y coordinate of the tile
     */
    private int NeighbouringWallCount(int x, int y, int radius = 1)
    {
        int count = 0;
        for (int nY = y + radius; nY >= y - radius; nY--)
        {
            for (int nX = x + radius; nX >= x - radius; nX--)
            {
                if (IsInBounds(nX, nY))
                {
                    if (nY == y && nX == x)
                    {
                        continue;
                    }
                    count += _map[nX, nY];
                }
                else
                {
                    count++;
                }
            }
        }
        return count;
    }
    
    /**
     * Check if the tile is in bounds
     *
     * @param x The x coordinate of the tile
     * @param y The y coordinate of the tile
     */
    private bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < _width && y >= 0 && y < _height;
    }
    
    /**
     * Get the region of a tile
     * It iterates over the tiles, checks if the tile is in bounds, if it is, it checks if the tile is a same type as the original tile,
     * if it is, it adds it to the region and enqueues the neighbours
     * After that, it returns the region
     *
     * @param x The x coordinate of the tile
     * @param y The y coordinate of the tile
     */
    private List<Vector2I> GetRegion(int x, int y)
    {
        List<Vector2I> temp = new List<Vector2I>();
        int[,] flags = new int[_width, _height];
        int tileType = _map[x, y];
        Queue<Vector2I> queue = new Queue<Vector2I>();
        queue.Enqueue(new Vector2I(x, y));
        flags[x, y] = 1;
        
        // BFS to get the region
        while (queue.Count > 0)
        {
            Vector2I current = queue.Dequeue();
            temp.Add(current);
            for (int tileX = current.X - 1; tileX <= current.X + 1; tileX++)
            {
                for (int tileY = current.Y - 1; tileY <= current.Y + 1; tileY++)
                {
                    if (!IsInBounds(tileX, tileY))
                    {
                        continue;
                    }
                    if (current.X != tileX && current.Y != tileY)
                    {
                        continue;
                    }
                    if (flags[tileX, tileY] != 0 || _map[tileX, tileY] != tileType)
                    {
                        continue;
                    }
                    flags[tileX, tileY] = 1;
                    queue.Enqueue(new Vector2I(tileX, tileY));
                }
            }
        }

        return temp;
    }
    
    /**
     * Get the regions of the map
     * It iterates over the tiles, checks if the tile is of the specified type, if it is,
     * it gets the region of the tile and adds it to the list of regions
     * After that, it returns the list of regions
     * 
     * @param tileType The type of the tile. 0 for room, 1 for wall
     */
    private List<List<Vector2I>> GetRegions(int tileType)
    {
        List<List<Vector2I>> temp = new List<List<Vector2I>>();
        int[,] flags = new int[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (flags[x, y] != 0 || _map[x, y] != tileType)
                {
                    continue;
                }
                List<Vector2I> region = GetRegion(x, y);
                temp.Add(region);
                foreach (Vector2I tile in region)
                {
                    flags[tile.X, tile.Y] = 1;
                }
            }
        }
        return temp;
    }

    /**
     * Cleans up the regions of the map
     * It gets the regions of the map, checks if the region is smaller than the minimum region size,
     * if it is, it sets the tiles of the region to the opposite type
     */
    private void CleanupRegions()
    {
        List<List<Vector2I>> wallRegions = GetRegions(1);
        List<List<Vector2I>> roomRegions = GetRegions(0);

        foreach (List<Vector2I> region in wallRegions)
        {
            if (region.Count >= _minRegionSize) continue;
            foreach (Vector2I tile in region)
            {
                _map[tile.X, tile.Y] = 0;
            }
        }

        foreach (List<Vector2I> region in roomRegions)
        {
            if (region.Count >= _minRegionSize) continue;
            foreach (Vector2I tile in region)
            {
                _map[tile.X, tile.Y] = 1;
            }
        }
    }
    
    /**
     * Change tiles next to rooms to third style tiles
     */
    
    private void ChangeTilesNextToRooms()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_map[x, y] == 0)
                {
                    for (int nY = y + 1; nY >= y - 1; nY--)
                    {
                        for (int nX = x + 1; nX >= x - 1; nX--)
                        {
                            if (IsInBounds(nX, nY))
                            {
                                if (nY == y && nX == x)
                                {
                                    continue;
                                }

                                if (_map[nX, nY] == 1)
                                {
                                    _map[nX, nY] = 2;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    public void GenerateDijkstraMap(Vector2I startPoint)
    {
        int[,] dijkstraMap = new int[_width, _height];
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                dijkstraMap[x, y] = int.MaxValue;
            }
        }
        dijkstraMap[startPoint.X, startPoint.Y] = 0;

        PriorityQueue<Vector2I, int> queue = new PriorityQueue<Vector2I, int>();
        queue.Enqueue(startPoint, 0);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            int currentDist = dijkstraMap[current.X, current.Y];
            List<Vector2I> neighbours = GetNeighbours(current);

            foreach (var neighbour in neighbours)
            {
                if (IsInBounds(neighbour.X, neighbour.Y) && _map[neighbour.X, neighbour.Y] == 0)
                {
                    int newDist = currentDist + 1; 
                    if (newDist < dijkstraMap[neighbour.X, neighbour.Y])
                    {
                        dijkstraMap[neighbour.X, neighbour.Y] = newDist;
                        queue.Enqueue(neighbour, newDist);
                    }
                }
            }
        }
        UpdateTilemapBasedOnDijkstraMap(dijkstraMap);
    }
    
    private void UpdateTilemapBasedOnDijkstraMap(int[,] dijkstraMap)
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (dijkstraMap[x, y] == int.MaxValue && _map[x, y] == 0)
                {
                    _map[x, y] = 1;
                }
            }
        }
    }
    
    private List<Vector2I> GetNeighbours(Vector2I cell)
    {
        List<Vector2I> neighbours = new List<Vector2I>();
        neighbours.Add(new Vector2I(cell.X + 1, cell.Y));
        neighbours.Add(new Vector2I(cell.X - 1, cell.Y));
        neighbours.Add(new Vector2I(cell.X, cell.Y + 1));
        neighbours.Add(new Vector2I(cell.X, cell.Y - 1));
        return neighbours;
    }
    
    /**
     * Updates the tilemap
     * It iterates over the tiles, checks if the tile is a wall, if it is, it sets the tile to a wall
     * if it is not, it sets the tile to a floor
     *
     * @param _tileMap The tilemap to update
     */
    private void UpdateTilemap(TileMap _tileMap)
    {
        _tileMap.Clear();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (_map[x, y] == 0) continue;
                if (_map[x, y] == 1) _tileMap.SetCell(0, new Vector2I(x, y), 0, _wallTile);
                if (_map[x, y] == 2) _tileMap.SetCell(0, new Vector2I(x, y), 0, _nextToRoomTile);
                
            }
        }
    }
    
    #endregion
}