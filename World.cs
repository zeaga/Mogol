using Mogol.Util;
using Raylib_cs;

namespace Mogol {
	public class World {

		private int[,] grid1;
		private int[,] grid2;
		private bool which;

		public int[,] Grid => which ? grid1 : grid2;
		public int[,] OffGrid => which ? grid2 : grid1;
		public int Width;
		public int Height;

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

		public int Get( int x, int y ) => Grid[x.Mod( Width ), y.Mod( Height )];
		public void Set( int x, int y, int value ) => Grid[x.Mod( Width ), y.Mod( Height )] = value;

		public int[,] Swap( ) {
			which ^= true;
			return Grid;
		}

		public void Clear( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					grid1[x, y] = grid2[x, y] = 0;
				}
			}
		}

		public void Randomize( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					grid1[x, y] = grid2[x, y] = Raylib.GetRandomValue( 0, 1 );
				}
			}
		}

		public void Update( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					OffGrid[x, y] = Rules[( Grid[x, y] > 0 ).ToInt( )][SumNeighbors( x, y )].ToInt( );
				}
			}
			Swap( );
		}

		public int IsOn( int x, int y ) => Grid[x, y] == 0 ? 0 : 1;

		public int SumNeighbors( int x, int y ) {
			int l = ( x - 1 ).Mod( Width );
			int r = ( x + 1 ).Mod( Width );
			int u = ( y - 1 ).Mod( Height );
			int d = ( y + 1 ).Mod( Height );
			return IsOn( l, u ) + IsOn( x, u ) + IsOn( r, u ) + IsOn( l, y ) + IsOn( r, y ) + IsOn( l, d ) + IsOn( x, d ) + IsOn( r, d );
		}

	}
}