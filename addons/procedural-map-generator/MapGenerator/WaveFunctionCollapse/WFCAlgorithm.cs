using System;
using System.Collections;
using Godot;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using PluginPCG.WaveFunctionCollapse;

namespace PluginPCG;
public class WFCCell {
    public int TileIndex { get; set; }
    public bool isCollapsed { get; set; }
    public bool[] Options { get; }
    public WFCCoordinates.Coordinates Position { get; }
    public double Entropy => CalculateEntropy();
    
    private readonly int[] _frequencies;
    private int _sumOfPossibleFrequencies;
    private readonly double _entropyNoise;

    public WFCCell(WFCCoordinates.Coordinates position, int[] _frequencies) {
        TileIndex = -1;
        isCollapsed = false;
        Position = position;
        
        this._frequencies = _frequencies;
        Options = new bool[this._frequencies.Length];
        
        for (int i = 0; i < Options.Length; i++){
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
        for (int i = 0; i < Options.Length; i++) {
            if (Options[i]) {
                possibleFrequencyLogFrequencies += Math.Log2(_frequencies[i]) * _frequencies[i];
            }
        }
        return Math.Log2(_sumOfPossibleFrequencies) - possibleFrequencyLogFrequencies / _sumOfPossibleFrequencies + _entropyNoise;
    }

    public void RemoveOption(int i) {
        Options[i] = false;
        _sumOfPossibleFrequencies -= _frequencies[i];
    }
    
    private int RandomFromSum(int _sum) {
        var random = new Random();
        return random.Next(0, _sum);
    }

    private int RandomIndex() {
        
        int pointer = 0;
        int randomFromSum = RandomFromSum(_sumOfPossibleFrequencies);
        
        for (int i = 0; i < Options.Length; i++) {
            if (Options[i])
            {
                pointer += _frequencies[i];
                if (pointer >= randomFromSum) {
                    return i;
                }
            }
        }
        
        return -1;
    }

    public int Collapse() {
        
        isCollapsed = true;
        
        int randomIndex = RandomIndex();
        TileIndex = randomIndex;
        
        for (int i = 0; i < Options.Length; i++) {
            if(i == TileIndex) {
                Options[i] = true;
            }
            else {
                Options[i] = false;
            }
        }

        return randomIndex;
    }
}

sealed partial class WFCGrid{
    
    public bool Busy;
    public readonly Queue<WFCCoordinates.Coordinates> AnimationCoordinates = new();

    private int currentAttempt;
    private WFCCell[,] cells;
    private int remainingUncollapsedCells;
    private bool validCollapse = true;
    private readonly int[] rawFrequencies;
    private EntropyHeap entropyHeap;
    private readonly int[,,] adjacencyRules;
    private readonly Stack<WFCCoordinates.RemovalUpdate> removalUpdates;
    private readonly bool suppressNotifications;
    
    private int width;
    private int height;

    public int Width{
        get{
            if (width == 0){
                width = cells.GetLength(0);
            }

            return width;
        }
    }
    
    public int Height{
        get{
            if (height == 0){
                height = cells.GetLength(1);
            }

            return height;
        }
    }
    
    private static Random pseudoRandom;

    public Delegates.OnComplete onComplete;

    private bool IsInBounds(WFCCoordinates.Coordinates c){
        return c.X >= 0 && c.X < Width && c.Y >= 0 && c.Y < Height;
    }
    
    public WFCGrid(int _width, int _height, List<WFCRule> _rules, bool _suppressNotifications = false){
        onComplete += NotifyComplete;
        suppressNotifications = _suppressNotifications;
        cells = new WFCCell[_width, _height];
        remainingUncollapsedCells = cells.Length;
        rawFrequencies = new int[_rules.Count];
        for (int i = 0; i < rawFrequencies.Length; i++){
            rawFrequencies[i] = _rules[i].Frequency;
        }

        adjacencyRules = WFCRule.ToAdjacencyRules(_rules);
        for (int x = 0; x < _width; x++){
            for (int y = 0; y < _height; y++){
                cells[x, y] = new WFCCell(new WFCCoordinates.Coordinates(x, y), rawFrequencies);
            }
        }

        removalUpdates = new Stack<WFCCoordinates.RemovalUpdate>();
        entropyHeap = new EntropyHeap(Width * Height);
    }


    private void NotifyComplete(WFCResult result){
        if (!suppressNotifications){
            GD.Print(result);
        }
    }

    private void Reset(bool _resetAttempts = false){
        cells = new WFCCell[Width, Height];
        for (int x = 0; x < Width; x++){
            for (int y = 0; y < Height; y++){
                cells[x, y] = new WFCCell(new WFCCoordinates.Coordinates(x, y), rawFrequencies);
            }
        }

        remainingUncollapsedCells = cells.Length;
        removalUpdates.Clear();
        entropyHeap = new EntropyHeap(Width * Height);
        validCollapse = true;
        Busy = false;
        currentAttempt = _resetAttempts ? 0 : currentAttempt;
        AnimationCoordinates.Clear();
    }
}

public partial class WFCGrid{
    private WFCCoordinates.EntropyCoordinates Observe(){
        while (!entropyHeap.IsEmpty){
            WFCCoordinates.EntropyCoordinates coords = entropyHeap.Pop();
            if (!cells[coords.Coordinates.X, coords.Coordinates.Y].isCollapsed) return coords;
        }

        return WFCCoordinates.EntropyCoordinates.Invalid;
    }

    private void Collapse(WFCCoordinates.Coordinates _coords){
        int collapsedIndex = cells[_coords.X, _coords.Y].Collapse();
        AnimationCoordinates.Enqueue(_coords);
        removalUpdates.Push(new WFCCoordinates.RemovalUpdate(){
            Coordinates = _coords,
            TileIndex = collapsedIndex
        });
        remainingUncollapsedCells--;
    }

    private void Propagate(){
        while (removalUpdates.Count > 0){
            WFCCoordinates.RemovalUpdate update = removalUpdates.Pop();
            if (update.TileIndex == -1){
                validCollapse = false;
                return;
            }

            WFCCoordinates.Coordinates[] cardinals = WFCCoordinates.Coordinates.Cardinals;
            for (int d = 0; d < adjacencyRules.GetLength(1); d++){
                WFCCoordinates.Coordinates current = cardinals[d] + update.Coordinates;
                if (!IsInBounds(current)){
                    continue;
                }

                WFCCell currentCell = cells[current.X, current.Y];
                if (currentCell.isCollapsed) continue;
                for (int o = 0; o < adjacencyRules.GetLength(2); o++){
                    if (adjacencyRules[update.TileIndex, d, o] == 0 && currentCell.Options[o]){
                        currentCell.RemoveOption(o);
                    }
                }

                entropyHeap.Push(new WFCCoordinates.EntropyCoordinates(){
                    Coordinates = currentCell.Position,
                    Entropy = currentCell.Entropy
                });
            }
        }
    }
    
    public WFCCell GetRandomCell() {
        var random = new Random();
        int x = random.Next(0, cells.GetLength(0));
        int y = random.Next(0, cells.GetLength(1));
        return cells[x, y];
    }

    public void TryCollapse(int _maxAttempts = 1000){
        Reset(true);
        Busy = true;
        Stopwatch timer = Stopwatch.StartNew();
        for (int i = 0; i < _maxAttempts; i++){
            currentAttempt++;
            WFCCell cell = GetRandomCell();
            entropyHeap.Push(new WFCCoordinates.EntropyCoordinates(){
                Coordinates = cell.Position,
                Entropy = cell.Entropy
            });

            while (remainingUncollapsedCells > 0 && validCollapse){
                WFCCoordinates.EntropyCoordinates e = Observe();
                Collapse(e.Coordinates);
                Propagate();
            }

            if (!validCollapse && i < _maxAttempts - 1){
                Reset();
            }
            else{
                break;
            }
        }

        timer.Stop();
        WFCResult result = new(){
            Grid = this,
            Success = validCollapse,
            Attempts = currentAttempt,
            ElapsedMilliseconds = timer.ElapsedMilliseconds
        };
        onComplete?.Invoke(result);
        Busy = false;
    }
}

public partial class WFCGrid : ICollection{
    
    public WFCCell this[int x, int y]{
        get => cells[x, y];
        set => cells[x, y] = value;
    }

    
    public WFCCell this[int i]{
        get => cells[i % Width, i / Width];
        set => cells[i % Width, i / Width] = value;
    }
    
    IEnumerator IEnumerable.GetEnumerator(){
        return new CellEnumerator(cells);
    }
    
    public void CopyTo(System.Array array, int index){
        foreach (WFCCell c in cells){
            array.SetValue(c, index);
            index++;
        }
    }
    
    bool ICollection.IsSynchronized => false;

    object ICollection.SyncRoot => this;
    int ICollection.Count => cells.Length;
    
}

public class CellEnumerator : IEnumerator{
    private readonly WFCCell[,] cells;
    private int Cursor;
    private readonly int Count;
    
    public CellEnumerator(WFCCell[,] _cells){
        cells = _cells;
        Cursor = -1;
        Count = _cells.GetLength(0) * cells.GetLength(1);
    }
    
    public void Reset(){
        Cursor = -1;
    }
    
    public bool MoveNext(){
        if (Cursor < Count)
            Cursor++;
        return Cursor != Count;
    }
    
    public object Current{
        get{
            if (Cursor < 0 || Cursor == Count)
                throw new InvalidOperationException();
            return cells[Cursor % cells.GetLength(0), Cursor / cells.GetLength(0)];
        }
    }
}

public class EntropyHeap{
    private readonly WFCCoordinates.EntropyCoordinates[] coords;
    private int size;

    public EntropyHeap(int capacity){
        coords = new WFCCoordinates.EntropyCoordinates[capacity];
    }

    private int GetLeftChildIndex(int i) => 2 * i + 1;
    private int GetRightChildIndex(int i) => 2 * i + 2;
    private int GetParentIndex(int i) => (i - 1) / 2;

    private bool HasLeftChild(int i) => GetLeftChildIndex(i) < size;
    private bool HasRightChild(int i) => GetRightChildIndex(i) < size;
    private bool IsRoot(int i) => i == 0;

    private WFCCoordinates.EntropyCoordinates GetLeftChild(int i) => coords[GetLeftChildIndex(i)];
    private WFCCoordinates.EntropyCoordinates GetRightChild(int i) => coords[GetRightChildIndex(i)];
    private WFCCoordinates.EntropyCoordinates GetParent(int i) => coords[GetParentIndex(i)];

    public bool IsEmpty => size == 0;

    private void Swap(int a, int b) => (coords[a], coords[b]) = (coords[b], coords[a]);

    public WFCCoordinates.EntropyCoordinates Pop(){
        if (size == 0) throw new IndexOutOfRangeException();
        WFCCoordinates.EntropyCoordinates result = coords[0];
        coords[0] = coords[size - 1];
        size--;
        RecalculateDown();
        return result;
    }

    public void Push(WFCCoordinates.EntropyCoordinates _coords){
        if (size == coords.Length) throw new IndexOutOfRangeException();
        coords[size] = _coords;
        size++;
        RecalculateUp();
    }

    private void RecalculateDown(){
        int index = 0;
        while (HasLeftChild(index)){
            int lesserIndex = GetLeftChildIndex(index);
            if (HasRightChild(index) && GetRightChild(index).Entropy < GetLeftChild(index).Entropy){
                lesserIndex = GetRightChildIndex(index);
            }

            if (coords[lesserIndex].Entropy >= coords[index].Entropy){
                break;
            }

            Swap(lesserIndex, index);
            index = lesserIndex;
        }
    }

    private void RecalculateUp(){
        int index = size - 1;
        while (!IsRoot(index) && coords[index].Entropy < GetParent(index).Entropy){
            int parentIndex = GetParentIndex(index);
            Swap(parentIndex, index);
            index = parentIndex;
        }
    }
}

public abstract class Delegates{
    public delegate void OnComplete(WFCResult result);
}

public struct WFCResult{
    public WFCGrid Grid;
    public bool Success;
    public int Attempts;
    public long ElapsedMilliseconds;

    public override string ToString(){
        StringBuilder s = new();
        s.Append("Result: ");
        s.Append(Success ? "Successful\n" : "Contradiction Failure\n");
        s.Append($"Attempts: {Attempts}\n");
        s.Append($"Elapsed Time: {ElapsedMilliseconds}ms\n");
        return s.ToString();
    }
}