using System;
using Godot;


namespace PluginPCG.WaveFunctionCollapse;

public class WFCCoordinates {
	public struct Coordinates{
		public int X;
		public int Y;

		public Vector2I AsVector2I => new(X, Y);

		public static Coordinates Up{ get; } = new(0, -1);
		public static Coordinates Right{ get; } = new(1, 0);
		public static Coordinates Left{ get; } = new(-1, 0);
		public static Coordinates Down{ get; } = new(0, 1);
		public static Coordinates UpLeft{ get; } = new(-1, -1);
		public static Coordinates UpRight{ get; } = new(1, -1);
		public static Coordinates DownLeft{ get; } = new(-1, 1);
		public static Coordinates DownRight{ get; } = new(1, 1);
		public static Coordinates[] Cardinals{ get; } ={ Up, Right, Down, Left };
		public static Coordinates[] Ordinals{ get; } ={ UpLeft, UpRight, DownRight, DownLeft };

		public static Coordinates[] Neighbours{ get; } =
			{ UpLeft, Up, UpRight, Left, Right, DownLeft, Down, DownRight };

		public bool Equals(Coordinates other){
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj){
			return obj is Coordinates other && Equals(other);
		}

		public override int GetHashCode(){
			return HashCode.Combine(X, Y);
		}

		public static Coordinates operator +(Coordinates a, Coordinates b){
			a.X += b.X;
			a.Y += b.Y;
			return a;
		}

		public static Coordinates operator -(Coordinates a, Coordinates b){
			a.X -= b.X;
			a.Y -= b.Y;
			return a;
		}

		public static bool operator ==(Coordinates a, Coordinates b) => a.X == b.X && a.Y == b.Y;

		public static bool operator !=(Coordinates a, Coordinates b) => a.X != b.X || a.Y != b.Y;

		public override string ToString(){
			return $"Coord ({X}, {Y})";
		}

		public Coordinates(){
			X = 0;
			Y = 0;
		}

		public Coordinates(int _x, int _y){
			X = _x;
			Y = _y;
		}
	}

	public struct RemovalUpdate{
		public int TileIndex;
		public Coordinates Coordinates;
	}

	public struct EntropyCoordinates{
		public double Entropy;
		public Coordinates Coordinates;

		public static EntropyCoordinates Invalid => new()
			{ Entropy = -1, Coordinates = new Coordinates() };
	}
}
