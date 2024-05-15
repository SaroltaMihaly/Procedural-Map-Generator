using System.Collections.Generic;
using Godot;

namespace PluginPCG;

public interface IMapBuilder
{
    void Initialize(int width, int height, Dictionary<string, object> parameters);
    void GenerateMap(TileMap tileMap);
}
