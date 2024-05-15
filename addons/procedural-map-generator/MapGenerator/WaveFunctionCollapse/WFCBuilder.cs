using System.Collections.Generic;
using Godot;

namespace PluginPCG.WaveFunctionCollapse;

public class WFCBuilder : IMapBuilder {

    #region VARIABLES
    
    private int _width;
    
    private int _height;

    private string _rulePath;
    
    private bool _wrap;
    
    private WFCGrid _grid;
    
    private TileSetAtlasSource _source;
    
    private TileMap _targetTileMap;
    
    #endregion

    #region INITIALIZATION
    
    public void Initialize(int width, int height, Dictionary<string, object> parameters) {
        _width = width;
        _height = height;
        _rulePath = (string)parameters["rulePath"];
        _wrap = (bool)parameters["wrap"];
        
        GD.Print("width: " + width + " height: " + height + " rulePath: " + _rulePath + " wrap: " + _wrap);
    }

    #endregion

    #region GENERATION
    
    public void GenerateMap(TileMap tileMap) {
        _targetTileMap = tileMap;
        List<WFCRule> rules = WFCRule.FromJSONFile(ProjectSettings.GlobalizePath(_rulePath));
        _grid = new WFCGrid(_width, _height, rules);
        _grid.onComplete += OnGenerationComplete;
        GenerateGrid();
    }
    
    private void GenerateGrid(){
        if (_grid.Busy) return;
        ClearTilemap();
        _grid.TryCollapse(_wrap);
    }
    
    private void OnGenerationComplete(WFCResult result){
        if (!result.Success) return;
        StartPopulatingTilemap(result.Grid);
        //WFCGrid.onComplete -= OnGenerationComplete;
    }
    
    private void StartPopulatingTilemap(WFCGrid _grid)
    {
        _source = _targetTileMap.TileSet.GetSource(0) as TileSetAtlasSource;
        PopulateTilemap(_grid);
    }
    
    private void PopulateTilemap(WFCGrid _grid){
        foreach (var c in _grid.AnimationCoordinates){
            SetNextCell(c.AsVector2I);
        }
        GD.Print("Tilemap population complete");
    }
    
    private void SetNextCell(Vector2I c){
        _targetTileMap.EraseCell(0, c);
        if (_grid[c.X, c.Y].TileIndex == -1) return;
        var tileId = _source.GetTileId(_grid[c.X, c.Y].TileIndex);
        _targetTileMap.SetCell(0, c, 0, tileId);
    }
    
    private void ClearTilemap(){
        _targetTileMap.Clear();
    }
    
    #endregion
    
}