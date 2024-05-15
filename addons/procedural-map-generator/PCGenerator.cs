#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using PluginPCG.GramElites;
using PluginPCG.WaveFunctionCollapse;
using CellularAutomataBuilder = PluginPCG.CellularAutomataBuilder;
using MapBuilder = PluginPCG.MapBuilder;

[Tool]
public partial class PCGenerator : EditorPlugin
{
	#region VARIABLES
	
	private PackedScene _editorScene =
		ResourceLoader.Load<PackedScene>("res://addons/procedural-map-generator/PCEditor.tscn");

	private DropTileMapLabel _dropTileMapLabel;
	
	private DropTileMapLabel _sampleTileMapLabel;
	
	private Control _editorInstance;

	private Button _buttonCa, _buttonWfc, _buttonGe;
	
	private VBoxContainer _caContainer, _wfcContainer, _geContainer;

	private TileMap _tileMap;

	private TileMap _sample;
	
	private bool _isWillBeRemove = false;
	
	private bool _isWillBeRemoveGenerateCa = false, _isWillBeRemoveGenerateWfc = false, _isWillBeRemoveGenerateGe = false;

	#endregion
	
	#region EDITOR_PLUGIN
	public override void _EnterTree()
	{
		Init();
	}

	public override void _ExitTree()
	{
		_editorInstance.QueueFree();
	}
	
	public override bool _HasMainScreen(){
		return true;
	}
	
	public override void _MakeVisible(bool visible){
		if (_editorInstance != null)
		{
			_editorInstance.Visible = visible;
		}
	}
	
	public override string _GetPluginName(){
		return "PCG MAPS Editor";
	}

	public override Texture2D _GetPluginIcon(){
		return GetEditorInterface().GetBaseControl().GetThemeIcon("ResourcePreloader", "EditorIcons");
	}
	
	#endregion
	
	#region INITIALIZATION

	private void Init()
	{
		_editorInstance = (Control)_editorScene.Instantiate();
		// GetEditorInterface().GetEditorMainScreen().AddChild(_editorInstance);
		AddControlToDock(DockSlot.LeftBl, _editorInstance);
		_MakeVisible(false);

		SetupVariables();
		SetupCallbacks();
		_dropTileMapLabel = _editorInstance.GetNode<DropTileMapLabel>("Elements/DropData/MarginContainer/VBoxContainer/DropTileMapList");
		if (_dropTileMapLabel == null)
		{
			GD.PrintErr("DropTileMapLabel not found");
		}
		else
		{
			_dropTileMapLabel.DropTileMap += map => _tileMap = map;
		}
		
		_sampleTileMapLabel = _editorInstance.GetNode<DropTileMapLabel>("Elements/GramElites/MarginContainer/VBoxContainer/MarginContainer/VBoxContainer/DropTileMapList");
		if (_sampleTileMapLabel == null)
		{
			GD.PrintErr("DropTileMapLabel not found");
		}
		else
		{
			_sampleTileMapLabel.DropTileMap += map => _sample = map;
		}
	}

	private void SetupVariables()
	{
		_buttonCa = _editorInstance.GetNode("Elements/CenterUI/VBoxContainer/CAButton") as Button;
		_buttonWfc = _editorInstance.GetNode("Elements/CenterUI/VBoxContainer/WFCButton") as Button;
		_buttonGe = _editorInstance.GetNode("Elements/CenterUI/VBoxContainer/GEButton") as Button;
		
		_caContainer = _editorInstance.GetNode("Elements/CellularAutomata") as VBoxContainer;
		_wfcContainer = _editorInstance.GetNode("Elements/WFC") as VBoxContainer;
		_geContainer = _editorInstance.GetNode("Elements/GramElites") as VBoxContainer;
		
		_caContainer.Visible = false;
		_wfcContainer.Visible = false;
		_geContainer.Visible = false;
	}

	private void SetupCallbacks()
	{
		if(_isWillBeRemove) RemoveCallbacks();
		_buttonCa.Pressed += SetupCellularAutomata;
		_buttonWfc.Pressed += SetupWaveFunctionCollapse;
		_buttonGe.Pressed += SetupGramElites;
		_isWillBeRemove = true;
	}

	private void SetupCellularAutomata()
	{
		_caContainer.Visible = true;
		_wfcContainer.Visible = false;
		_geContainer.Visible = false;

		Button generateButton = _editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Button") as Button;
		if(_isWillBeRemoveGenerateCa) generateButton.Pressed -= GenerateMapWithCellularAutomata;
		generateButton.Pressed += GenerateMapWithCellularAutomata;
		_isWillBeRemoveGenerateCa = true;
	}

	private void GenerateMapWithCellularAutomata()
	{
		int width = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Width/SpinBox") as SpinBox).Value;
		int height = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Height/SpinBox") as SpinBox).Value;
		int borderWidth = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/BorderWidth/SpinBox") as SpinBox).Value;
		int density = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Density/SpinBox") as SpinBox).Value;
		int smoothing = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Smoothing/SpinBox") as SpinBox).Value;
		bool useRandomSeed = (_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/UseRandomSeed/CheckButton") as CheckButton).ButtonPressed;
		int seed = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Seed/SpinBox") as SpinBox).Value;
		
		int wallAtlasCoordX = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Wall/AtlasCoordX/SpinBox") as SpinBox).Value;
		int wallAtlasCoordY = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/Wall/AtlasCoordY/SpinBox") as SpinBox).Value;
		
		int nextToRoomAtlasCoordX = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/NextToRoom/AtlasCoordX/SpinBox") as SpinBox).Value;
		int	nextToRoomAtlasCoordY = (int)(_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/NextToRoom/AtlasCoordY/SpinBox") as SpinBox).Value;
		
		String tileSetFilePath = (_editorInstance.GetNode("Elements/CellularAutomata/MarginContainer/VBoxContainer/TileSetPath/LineEdit") as LineEdit).Text;
		TileSet tileSet = GD.Load<TileSet>(tileSetFilePath);
		
		_tileMap.TileSet = tileSet;
		
		_tileMap.Clear();
		
		new MapBuilder()
			.UseBuilder(new CellularAutomataBuilder())
			.Initialize(width, height, new Dictionary<string, object>
			{
				{"borderWidth", borderWidth},
				{"density", density},
				{"smoothing", smoothing},
				{"useRandomSeed", useRandomSeed},
				{"seed", seed},
				{"wallAtlasCoordX", wallAtlasCoordX},
				{"wallAtlasCoordY", wallAtlasCoordY},
				{"nextToRoomAtlasCoordX", nextToRoomAtlasCoordX},
				{"nextToRoomAtlasCoordY", nextToRoomAtlasCoordY}
			})
			.Build(_tileMap);
	}
	
	private void SetupWaveFunctionCollapse()
	{
		_caContainer.Visible = false;
		_wfcContainer.Visible = true;
		_geContainer.Visible = false;
		
		Button generateButton1 = _editorInstance.GetNode("Elements/WFC/MarginContainer/VBoxContainer/Button") as Button;
		if(_isWillBeRemoveGenerateWfc) generateButton1.Pressed -= GenerateMapWithWFC;
		generateButton1.Pressed += GenerateMapWithWFC;
		_isWillBeRemoveGenerateWfc = true;
	}

	private void GenerateMapWithWFC()
	{
		int width = (int)(_editorInstance.GetNode("Elements/WFC/MarginContainer/VBoxContainer/Width/SpinBox") as SpinBox).Value;
		int height = (int)(_editorInstance.GetNode("Elements/WFC/MarginContainer/VBoxContainer/Height/SpinBox") as SpinBox).Value;
		String rulesFilePath = (_editorInstance.GetNode("Elements/WFC/MarginContainer/VBoxContainer/RulaPath/LineEdit") as LineEdit).Text;
		String tileSetFilePath = (_editorInstance.GetNode("Elements/WFC/MarginContainer/VBoxContainer/TileSetPath/LineEdit") as LineEdit).Text;
		
		TileSet tileSet = GD.Load<TileSet>(tileSetFilePath);
		
		_tileMap.TileSet = tileSet;
		
		_tileMap.Clear();
		
		new MapBuilder()
			.UseBuilder(new WFCBuilder())
			.Initialize(width, height, new Dictionary<string, object>
			{
				{"rulePath", rulesFilePath},
				{"wrap", false}
			})
			.Build(_tileMap);
		
	}

	private void SetupGramElites()
	{
		_caContainer.Visible = false;
		_wfcContainer.Visible = false;
		_geContainer.Visible = true;
		
		Button generateButton2 = _editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/Button") as Button;
		Button saveButton = _editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/Save") as Button;
		if (_isWillBeRemoveGenerateGe)
		{
			generateButton2.Pressed -= GenerateMapWithGramElites;
			saveButton.Pressed -= SaveGramElitesLearningTilemap;
		}
		generateButton2.Pressed += GenerateMapWithGramElites;
		saveButton.Pressed += SaveGramElitesLearningTilemap;
		_isWillBeRemoveGenerateCa = true;
	}
	private void GenerateMapWithGramElites()
	{
		String pythonFilePath = (_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/PythonInterpreter/LineEdit") as LineEdit).Text;
		int height = (int)(_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/Height/SpinBox") as SpinBox).Value;
		int seed = (int)(_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/Seed/SpinBox") as SpinBox).Value;
		int startPopulationSize = (int)(_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/StartPopulationSize/SpinBox") as SpinBox).Value;
		int iterations = (int)(_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/Iterations/SpinBox") as SpinBox).Value;
		int atlasCoordX = (int)(_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/AtlasCoords/AtlasCoordX/SpinBox") as SpinBox).Value;
		int atlasCoordY = (int)(_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/AtlasCoords/AtlasCoordY/SpinBox") as SpinBox).Value;
		
		String tileSetFilePath = (_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/TileSetPath/LineEdit") as LineEdit).Text;
		
		TileSet tileSet = GD.Load<TileSet>(tileSetFilePath);
		
		_tileMap.TileSet = tileSet;
		
		_tileMap.Clear();
		
		int width = 0;
		
		new MapBuilder()
			.UseBuilder(new GramElitesBuilder())
			.Initialize(width, height, new Dictionary<string, object>
			{
				{"pythonPath", pythonFilePath},
				{"seed", seed},
				{"startPopulationSize", startPopulationSize},
				{"iterations", iterations},
				{"atlasCoordX", atlasCoordX},
				{"atlasCoordY", atlasCoordY}
			})
			.Build(_tileMap);
	}
	
	private void SaveGramElitesLearningTilemap()
	{
		String textName = (_editorInstance.GetNode("Elements/GramElites/MarginContainer/VBoxContainer/NameOfTheTilemap/LineEdit") as LineEdit).Text;
		new ParserTileMapToTxt().ParseTileMapToTxt(_sample, new Vector2I(0, 2), textName);
	}

	private void RemoveCallbacks()
	{
		_buttonCa.Pressed -= SetupCellularAutomata;
		_buttonWfc.Pressed -= SetupWaveFunctionCollapse;
		_buttonGe.Pressed -= SetupGramElites;
	}
	
	
	#endregion
}
#endif
