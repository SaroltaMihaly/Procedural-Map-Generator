using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Godot;
using Godot.Collections;

namespace PluginPCG.WaveFunctionCollapse;

[Serializable]
public partial class WFCRule{
    public int Frequency{ get; set; }
    public WFCOptions Options{ get; set; } = new();
    
    public static List<WFCRule> FromJSONFile(string path){
        string json = File.ReadAllText(path);
        Godot.Collections.Array a = Json.ParseString(json).AsGodotArray();
        List<WFCRule> rules = new();
        for (int i = 0; i < a.Count; i++){
            Dictionary d = a[i].AsGodotDictionary();
            rules.Add(new WFCRule());
            rules[i].Frequency = d["Frequency"].AsInt32();
            foreach (KeyValuePair<Variant, Variant> k in d["Options"].AsGodotDictionary()){
                switch (k.Key.ToString()){
                    case "Up":
                        foreach (string o in k.Value.AsStringArray()){
                            rules[i].Options.Up.Add(int.Parse(o));
                        }

                        break;
                    case "Right":
                        foreach (string o in k.Value.AsStringArray()){
                            rules[i].Options.Right.Add(int.Parse(o));
                        }

                        break;
                    case "Down":
                        foreach (string o in k.Value.AsStringArray()){
                            rules[i].Options.Down.Add(int.Parse(o));
                        }

                        break;
                    case "Left":
                        foreach (string o in k.Value.AsStringArray()){
                            rules[i].Options.Left.Add(int.Parse(o));
                        }

                        break;
                }
            }
        }

        return rules;
    }
    
    public static int[,,] ToAdjacencyRules(List<WFCRule> _ruleList){
        int[,,] adjacencyRules = new int[_ruleList.Count, 4, _ruleList.Count];
        for (int r = 0; r < adjacencyRules.GetLength(0); r++){
            for (int d = 0; d < adjacencyRules.GetLength(1); d++){
                for (int o = 0; o < adjacencyRules.GetLength(2); o++){
                    adjacencyRules[r, d, o] = d switch{
                        0 => _ruleList[r].Options.Up.Contains(o) ? 1 : 0,
                        1 => _ruleList[r].Options.Right.Contains(o) ? 1 : 0,
                        2 => _ruleList[r].Options.Down.Contains(o) ? 1 : 0,
                        3 => _ruleList[r].Options.Left.Contains(o) ? 1 : 0,
                        _ => throw new ArgumentOutOfRangeException()
                    };
                }
            }
        }

        return adjacencyRules;
    }
}

public static class Serialize{
    public static string ToJSON(this List<WFCRule> self){
        string json = JsonSerializer.Serialize(self, new JsonSerializerOptions{ WriteIndented = true });
        return json;
    }
}