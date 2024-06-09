using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginPCG.WaveFunctionCollapse;

public class WFCOptions{
    public List<int> Up{ get; }
    public List<int> Right{ get; }
    public List<int> Down{ get; }
    public List<int> Left{ get; }

    public WFCOptions(){
        Up = new List<int>();
        Right = new List<int>();
        Down = new List<int>();
        Left = new List<int>();
    }
}
