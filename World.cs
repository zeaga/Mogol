using Mogol.Util;
using Raylib_cs;

namespace Mogol {
	public class World {

		private int[,] grid1;
		private int[,] grid2;
		private bool which;

		public int[,] Grid => which ? grid1 : grid2;
		public int[,] OffGrid => which ? grid2 : grid1;
		public readonly int Width;
		public readonly int Height;
		public bool Wrap = false;
		public int CellsOn { get; private set; }
		public int CellsOff => Width * Height - CellsOn;

		public Rule BirthRule => Rules[0];
		public Rule SurviveRule => Rules[1];

		public Rule[] Rules = new Rule[2];

		public World( int width, int height ) {
			Width = width;
			Height = height;
			Rules[0] = new Rule( );
			Rules[1] = new Rule( );
			grid1 = new int[width, height];
			grid2 = new int[width, height];
		}

		public World( int width, int height, Rule birthRule, Rule surviveRule ) {
			Width = width;
			Height = height;
			Rules[0] = birthRule;
			Rules[1] = surviveRule;
			grid1 = new int[width, height];
			grid2 = new int[width, height];
		}

		public int Get( int x, int y, bool offGrid = false ) => x < 0 || x >= Width || y < 0 || y >= Height ? 0 : offGrid ? OffGrid[x, y] : Grid[x, y];
		public void Set( int x, int y, int value, bool offGrid = false ) {
			if ( x < 0 || x >= Width || y < 0 || y >= Height )
				return;
			if ( offGrid )
				OffGrid[x, y] = value;
			else
				Grid[x, y] = value;
		}

		public int[,] Swap( ) {
			which ^= true;
			return Grid;
		}

		public void Clear( ) {
			CellsOn = 0;
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					grid1[x, y] = grid2[x, y] = 0;
				}
			}
		}

		public void Randomize( ) {
			CellsOn = 0;
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					grid1[x, y] = grid2[x, y] = Raylib.GetRandomValue( 0, 1 );
					CellsOn += grid1[x, y];
				}
			}
		}

		public void Update( ) {
			CellsOn = 0;
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					OffGrid[x, y] = Rules[( Grid[x, y] > 0 ).ToInt( )][SumNeighbors( x, y )].ToInt( );
					CellsOn += OffGrid[x, y];
				}
			}
			Swap( );
		}

		public int IsOn( int x, int y ) => Get( x, y );

		public int SumNeighbors( int x, int y ) {
			int l = x - 1;
			int r = x + 1;
			int u = y - 1;
			int d = y + 1;
			if ( Wrap ) {
				l = l.Mod( Width );
				r = r.Mod( Width );
				u = u.Mod( Height );
				d = d.Mod( Height );
			}
			return Get( l, u ) + Get( x, u ) + Get( r, u ) + Get( l, y ) + Get( r, y ) + Get( l, d ) + Get( x, d ) + Get( r, d );
		}

	}
}