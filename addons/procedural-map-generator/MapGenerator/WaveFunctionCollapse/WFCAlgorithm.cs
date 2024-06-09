using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;
using PluginPCG.WaveFunctionCollapse;

namespace PluginPCG;

public class WFCCell
{
    public int TileIndex { get; set; }
    public bool isCollapsed { get; set; }
    public bool[] Options { get; }
    public WFCCoordinates.Coordinates Position { get; }
    public double Entropy => CalculateEntropy();
    
    private readonly int[] _frequencies;
    private int _sumOfPossibleFrequencies;
    private readonly double _entropyNoise;

    public WFCCell(WFCCoordinates.Coordinates position, int[] _frequencies)
    {
        TileIndex = -1;
        isCollapsed = false;
        Position = position;
        
        this._frequencies = _frequencies;
        Options = new bool[this._frequencies.Length];
        
        for (int i = 0; i < Options.Length; i++)
        {
            Options[i] = true;
        }
        
        _entropyNoise = randomEntropyNoise();
        _sumOfPossibleFrequencies = _frequencies.Sum();
    }

    private double randomEntropyNoise()
    {
        var random = new Random();
        return random.NextDouble() * 0.1;
    }
    
    private double CalculateEntropy()
    {
        double possibleFrequencyLogFrequencies = 0;
        for (int i = 0; i < Options.Length; i++)
        {
            if (Options[i])
            {
                possibleFrequencyLogFrequencies += Math.Log2(_frequencies[i]) * _frequencies[i];
            }
        }
        return Math.Log2(_sumOfPossibleFrequencies) - possibleFrequencyLogFrequencies / _sumOfPossibleFrequencies + _entropyNoise;
    }

    public void RemoveOption(int i)
    {
        Options[i] = false;
        _sumOfPossibleFrequencies -= _frequencies[i];
    }
    
    private int RandomFromSum(int _sum)
    {
        var random = new Random();
        return random.Next(0, _sum);
    }

    private int RandomIndex()
    {
        int pointer = 0;
        int randomFromSum = RandomFromSum(_sumOfPossibleFrequencies);
        
        for (int i = 0; i < Options.Length; i++)
        {
            if (Options[i])
            {
                pointer += _frequencies[i];
                if (pointer >= randomFromSum)
                {
                    return i;
                }
            }
        }
        
        return -1;
    }

    public int Collapse()
    {
        isCollapsed = true;
        
        int randomIndex = RandomIndex();
        TileIndex = randomIndex;
        
        for (int i = 0; i < Options.Length; i++)
        {
            Options[i] = (i == TileIndex);
        }

        return randomIndex;
    }
}

public class WFCGrid
{
    public int Width;
    public int Height;
    
    public bool Busy;
    public readonly Queue<WFCCoordinates.Coordinates> GridCoordinates = new();
    
    private WFCCell[,] _cells;
    private int _remainingUncollapsedCells;
    private bool _validCollapse = true;
    private int[] _frequencies;
    private EntropyHeap _heap;
    private int[,,] _adjacencyRules;
    private Stack<WFCCoordinates.RemovalUpdate> _removalUpdates;
    private bool _suppressNotifications;
    
    public WFCGrid(int _width, int _height, List<WFCRule> _rules, bool _suppressNotifications = false)
    {
        onComplete += NotifyComplete;
        this._suppressNotifications = _suppressNotifications;
        _cells = new WFCCell[_width, _height];
        Width = _width;
        Height = _height;
        _remainingUncollapsedCells = _cells.Length;
        _frequencies = new int[_rules.Count];
        for (int i = 0; i < _frequencies.Length; i++)
        {
            _frequencies[i] = _rules[i].Frequency;
        }

        _adjacencyRules = WFCRule.ToAdjacencyRules(_rules);
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                _cells[x, y] = new WFCCell(new WFCCoordinates.Coordinates(x, y), _frequencies);
            }
        }

        _removalUpdates = new Stack<WFCCoordinates.RemovalUpdate>();
        _heap = new EntropyHeap(Width * Height);
    }
    
    private static Random pseudoRandom;

    public Delegates.OnComplete onComplete;

    private bool IsInBounds(WFCCoordinates.Coordinates c)
    {
        return c.X >= 0 && c.X < Width && c.Y >= 0 && c.Y < Height;
    }

    private void NotifyComplete(bool success)
    {
        if (!_suppressNotifications)
        {
            GD.Print("Successfully generated map");
        }
    }

    private void Reset(bool _resetAttempts = false)
    {
        _cells = new WFCCell[Width, Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                _cells[x, y] = new WFCCell(new WFCCoordinates.Coordinates(x, y), _frequencies);
            }
        }

        _remainingUncollapsedCells = _cells.Length;
        _removalUpdates.Clear();
        _heap = new EntropyHeap(Width * Height);
        _validCollapse = true;
        Busy = false;
        GridCoordinates.Clear();
    }
    
    private WFCCoordinates.EntropyCoordinates Observe()
    {
        while (!_heap.IsEmpty())
        {
            WFCCoordinates.EntropyCoordinates coords = _heap.Pop();
            if (!_cells[coords.Coordinates.X, coords.Coordinates.Y].isCollapsed) return coords;
        }

        return WFCCoordinates.EntropyCoordinates.Invalid;
    }

    private void Collapse(WFCCoordinates.Coordinates _coords)
    {
        int collapsedIndex = _cells[_coords.X, _coords.Y].Collapse();
        GridCoordinates.Enqueue(_coords);
        _removalUpdates.Push(new WFCCoordinates.RemovalUpdate()
        {
            Coordinates = _coords,
            TileIndex = collapsedIndex
        });
        _remainingUncollapsedCells--;
    }

    private void Propagate()
    {
        while (_removalUpdates.Count > 0)
        {
            WFCCoordinates.RemovalUpdate update = _removalUpdates.Pop();
            if (update.TileIndex == -1)
            {
                _validCollapse = false;
                return;
            }

            WFCCoordinates.Coordinates[] cardinals = WFCCoordinates.Coordinates.Cardinals;
            for (int d = 0; d < _adjacencyRules.GetLength(1); d++)
            {
                WFCCoordinates.Coordinates current = cardinals[d] + update.Coordinates;
                if (!IsInBounds(current))
                {
                    continue;
                }

                WFCCell currentCell = _cells[current.X, current.Y];
                if (currentCell.isCollapsed) continue;
                for (int o = 0; o < _adjacencyRules.GetLength(2); o++)
                {
                    if (_adjacencyRules[update.TileIndex, d, o] == 0 && currentCell.Options[o])
                    {
                        currentCell.RemoveOption(o);
                    }
                }

                _heap.Push(new WFCCoordinates.EntropyCoordinates()
                {
                    Coordinates = currentCell.Position,
                    Entropy = currentCell.Entropy
                });
            }
        }
    }

    public WFCCell GetRandomCell()
    {
        var random = new Random();
        int x = random.Next(0, _cells.GetLength(0));
        int y = random.Next(0, _cells.GetLength(1));
        return _cells[x, y];
    }

    public void TryCollapse(int _maxAttempts = 1000)
    {
        Reset(true);
        Busy = true;
        Stopwatch timer = Stopwatch.StartNew();
        for (int i = 0; i < _maxAttempts; i++)
        {
            WFCCell cell = GetRandomCell();
            _heap.Push(new WFCCoordinates.EntropyCoordinates()
            {
                Coordinates = cell.Position,
                Entropy = cell.Entropy
            });

            while (_remainingUncollapsedCells > 0 && _validCollapse)
            {
                WFCCoordinates.EntropyCoordinates e = Observe();
                Collapse(e.Coordinates);
                Propagate();
            }

            if (!_validCollapse && i < _maxAttempts - 1)
            {
                Reset();
            }
            else
            {
                break;
            }
        }

        timer.Stop();
        onComplete?.Invoke(_validCollapse);
        Busy = false;
    }

    public WFCCell this[int x, int y]
    {
        get => _cells[x, y];
    }
}

public class EntropyHeap
{
    private readonly WFCCoordinates.EntropyCoordinates[] coords;
    private int size;

    public EntropyHeap(int capacity)
    {
        coords = new WFCCoordinates.EntropyCoordinates[capacity];
    }

    public bool IsEmpty()
    {
        return size == 0;
    }

    public WFCCoordinates.EntropyCoordinates Pop()
    {
        WFCCoordinates.EntropyCoordinates result = coords[0];
        coords[0] = coords[size - 1];
        size--;
        HeapifyDown();
        
        return result;
    }

    public void Push(WFCCoordinates.EntropyCoordinates _coords)
    { 
        coords[size] = _coords;
        size++;
        HeapifyUp();
    }

    private void HeapifyDown()
    {
        int index = 0;
        while (2 * index + 1 < size)
        {
            int i = 2 * index + 1;
            
            if (2 * index + 2 < size &&
                coords[2 * index + 2].Entropy < coords[2 * index + 1].Entropy)
            {
                i = 2 * index + 2;
            }

            if (coords[i].Entropy >= coords[index].Entropy)
            {
                break;
            }
            
            (coords[i], coords[index]) = (coords[index], coords[i]);
            index = i;
        }
    }

    private void HeapifyUp()
    {
        int index = size - 1;
        
        while (index != 0 && 
               coords[index].Entropy < coords[(index - 1) / 2].Entropy)
        {
            int parentIndex = (index - 1) / 2;
            (coords[parentIndex], coords[index]) = (coords[index], coords[parentIndex]);
            index = parentIndex;
        }
    }
}

public abstract class Delegates
{
    public delegate void OnComplete(bool success);
}
