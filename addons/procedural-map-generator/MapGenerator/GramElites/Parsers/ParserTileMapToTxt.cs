using System;
using System.IO;
using Godot;

namespace PluginPCG.GramElites;

public class ParserTileMapToTxt
{
    public void ParseTileMapToTxt(TileMap tileMap, Vector2I coordWallInTileSet, String textName)
    {
        GD.Print("Parsing tile map to txt...");
        String currentDirectory = Directory.GetCurrentDirectory();
        String relativePath = "addons\\procedural-map-generator\\MapGenerator\\GramElites\\vglc_levels\\Icarus";
        String textNamePath = textName + ".txt";
		
        var output = Path.Combine(currentDirectory, relativePath, textNamePath);
        var outputStream = new StreamWriter(output);
        
        

        int width = (int)tileMap.GetUsedRect().Size.X;
        int height = (int)tileMap.GetUsedRect().Size.Y;
        
        GD.Print("Width: " + width + " Height: " + height);
        
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Vector2I coord = new Vector2I(j, i);
                Vector2I cell = tileMap.GetCellAtlasCoords(0, coord);
                if (cell == coordWallInTileSet)
                {
                    outputStream.Write("#");
                    outputStream.Flush();
                }
                else
                {
                    outputStream.Write("-");
                    outputStream.Flush();
                }
            }
            outputStream.WriteLine();
            outputStream.Flush();
        }
        
        outputStream.Close();
    }
}