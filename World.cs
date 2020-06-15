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
		protected int maxValue = 1;
		public int RandomValue => Raylib.GetRandomValue( 0, maxValue );

		public World( int width, int height ) {
			Width = width;
			Height = height;
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
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					grid1[x, y] = grid2[x, y] = 0;
				}
			}
		}

		public void Randomize( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					grid1[x, y] = grid2[x, y] = RandomValue;
				}
			}
		}

		public virtual void Update( ) { }

		public bool IsOn( int x, int y ) => Get( x, y ) != 0;

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