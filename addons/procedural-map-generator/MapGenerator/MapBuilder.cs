using System.Collections.Generic;
using Godot;

namespace PluginPCG;

public class MapBuilder
{
    private IMapBuilder _currentBuilder;
    
    public MapBuilder UseBuilder(IMapBuilder builder)
    {
        _currentBuilder = builder;
        return this;
    }
    
    public MapBuilder Initialize(int width, int height, Dictionary<string, object> parameters)
    {
        _currentBuilder?.Initialize(width, height, parameters);
        return this;
    }
    
    public void Build(TileMap tileMap)
    {
       _currentBuilder?.GenerateMap(tileMap);
    }
}
