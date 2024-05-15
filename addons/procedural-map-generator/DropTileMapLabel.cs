using Godot;
using Godot.Collections;

[Tool]
public partial class DropTileMapLabel : Label
{
    [Signal] public delegate void DropTileMapEventHandler(TileMap tileMap);

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        Dictionary path = data.AsGodotDictionary();
        Array p = path["nodes"].AsGodotArray();
        NodePath nodePath = p[0].ToString();
        TileMap tileMap = GetNode<TileMap>(nodePath);
        if (tileMap != null)
        {
            GD.Print("TileMap dropped successfully.");
            Text = "TileMap dropped successfully.";
            EmitSignal(SignalName.DropTileMap, tileMap);
        }
        else
        {
            GD.Print("Dropped data is not a TileMap. Ignoring...");
            Text = "Dropped data is not a TileMap. Ignoring...";
        }
    }
}
