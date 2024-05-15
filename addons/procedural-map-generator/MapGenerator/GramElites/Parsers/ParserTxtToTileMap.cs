using System;
using Godot;

namespace PluginPCG.GramElites;

public class ParserTxtToTileMap
{
    public void ParseTxtToTileMap(TileMap tileMap, String textPath, TileSet tileSet, Vector2I coordWallTileInTileSet)
    {
        tileMap.TileSet = tileSet;
        // read txt file to a matrix of integers
        int[,] matrix;
        // example of txt file:
        // ----####----####
        // ################
        // ####---#####-###
        // #------------###
        GD.Print("Text path: " + textPath);
        using (System.IO.StreamReader sr = new System.IO.StreamReader(textPath))
        {
            int width = GetWidth(textPath);
            int height = GetHeight(textPath);
            matrix = new int[width, height];
            
            for (int i = 0; i < height; i++)
            {
                string line = sr.ReadLine();
                for (int j = 0; j < width; j++)
                {
                    if (line[j] == '#' || line[j] == 'T')
                    {
                        matrix[j, i] = 1;
                    }
                    else
                    {
                        matrix[j, i] = 0;
                    }
                }
            }
            
            GD.Print("Width: " + matrix.GetLength(0) + " Height: " + matrix.GetLength(1));
            // create tile map
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    if (matrix[i, j] == 1)
                    {
                        tileMap.SetCell(0, new Vector2I(i, j), 0, coordWallTileInTileSet);
                    }
                }
            }
        }
        
    }
    
    private int GetWidth(String textPath)
    {
        using (System.IO.StreamReader sr = new System.IO.StreamReader(textPath))
        {
            string line = sr.ReadLine();
            return line.Length;
        }
    }
    
    private int GetHeight(String textPath)
    {
        using (System.IO.StreamReader sr = new System.IO.StreamReader(textPath))
        {
            int height = 0;
            while (sr.ReadLine() != null)
            {
                height++;
            }
            return height;
        }
    }
}