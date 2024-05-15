using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Godot;

namespace PluginPCG.GramElites;

public class GramElitesBuilder : IMapBuilder
{
    #region VARIABLES

    private int _width;
    private int _height;
    private int _seed;
    private int _startPopulationSize;
    private int _iterations;
    private String _pythonPath;
    private String _executablePythonFilePath;
    private Vector2I _coordWallInTileSet;

    #endregion
    
    #region INITIALIZATION
    
    public void Initialize(int width, int height, Dictionary<string, object> parameters)
    {
        _width = width;
        _height = height;
        _pythonPath = (String)parameters["pythonPath"];
        _seed = (int)parameters["seed"];
        _startPopulationSize = (int)parameters["startPopulationSize"];
        _iterations = (int)parameters["iterations"];
        int atlasCoordX = (int)parameters["atlasCoordX"];
        int atlasCoordY = (int)parameters["atlasCoordY"];
        
        _coordWallInTileSet = new Vector2I(atlasCoordX, atlasCoordY);
        
        String currentDirectory = Directory.GetCurrentDirectory();
        String relativePath = "addons\\procedural-map-generator\\MapGenerator\\GramElites\\main.py --seed " + _seed + " --start_strand_size " + _height + " --start_population_size " + _startPopulationSize + " --iterations " + _iterations;
        _executablePythonFilePath = Path.Combine(currentDirectory, relativePath);
        
    }
    
    #endregion
    
    #region GENERATION
    
    public void GenerateMap(TileMap tileMap)
    {
        Run_Cmd(_pythonPath, _executablePythonFilePath);
        
        String folderPath = Directory.GetCurrentDirectory();
        String relativePath = "addons\\procedural-map-generator\\MapGenerator\\GramElites\\IcarusData\\gram_elites\\levels";
        
        String textFolder = Path.Combine(folderPath, relativePath);
        
        String[] files = Directory.GetFiles(textFolder, "*.txt");
        String text = files[0];
        String textPath = Path.Combine(folderPath, text);
        
        tileMap.Clear();
        
        new ParserTxtToTileMap().ParseTxtToTileMap(tileMap, textPath, tileMap.TileSet, _coordWallInTileSet);
    }
    
    static void Run_Cmd(string cmd, string args)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = cmd;//cmd is full path to python.exe
        start.Arguments = args;//args is path to .py file and any cmd line args
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.StandardOutputEncoding = Encoding.UTF8;
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                GD.Print(result);
            }
        }
    }
    
    #endregion
}