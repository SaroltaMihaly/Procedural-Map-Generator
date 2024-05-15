using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginPCG.WaveFunctionCollapse;

[Serializable]
public class WFCOptions{
    public List<int> Up{ get; private set; }
    public List<int> Right{ get; private set; }
    public List<int> Down{ get; private set; }
    public List<int> Left{ get; private set; }

    public WFCOptions(){
        Up = new List<int>();
        Right = new List<int>();
        Down = new List<int>();
        Left = new List<int>();
    }

    public WFCOptions(int maxEntropy){
        Up = new List<int>();
        Right = new List<int>();
        Down = new List<int>();
        Left = new List<int>();
        for (int i = 0; i < maxEntropy; i++){
            Up.Add(i);
            Right.Add(i);
            Down.Add(i);
            Left.Add(i);
        }
    }

    public void Toggle(NeighbourDirections direction, int tileIndex, bool toggled = true){
        switch (direction){
            case NeighbourDirections.Up:
                if (toggled){
                    Up.Add(tileIndex);
                }
                else{
                    Up.Remove(tileIndex);
                }

                break;
            case NeighbourDirections.Right:
                if (toggled){
                    Right.Add(tileIndex);
                }
                else{
                    Right.Remove(tileIndex);
                }

                break;
            case NeighbourDirections.Down:
                if (toggled){
                    Down.Add(tileIndex);
                }
                else{
                    Down.Remove(tileIndex);
                }

                break;
            case NeighbourDirections.Left:
                if (toggled){
                    Left.Add(tileIndex);
                }
                else{
                    Left.Remove(tileIndex);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
}

public enum NeighbourDirections{
    Up,
    Right,
    Down,
    Left
}