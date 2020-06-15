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
		public int EdgeCell = 0;
		public bool Wrap {
			get => EdgeCell < 0;
			set {
				if ( value != Wrap )
					EdgeCell = ~EdgeCell;
			}
		}
		public Color[] Colors = new Color[] { Color.BLACK, Color.RAYWHITE };

		public int CurrentX { get; private set; }
		public int CurrentY { get; private set; }

		protected int AboveLeft => Get( CurrentX - 1, CurrentY - 1 );
		protected int Above => Get( CurrentX, CurrentY - 1 );
		protected int AboveRight => Get( CurrentX + 1, CurrentY - 1 );
		protected int Left => Get( CurrentX - 1, CurrentY );
		protected int Here => Get( CurrentX, CurrentY );
		protected int Right => Get( CurrentX + 1, CurrentY );
		protected int BelowLeft => Get( CurrentX - 1, CurrentY + 1 );
		protected int Below => Get( CurrentX, CurrentY + 1 );
		protected int BelowRight => Get( CurrentX + 1, CurrentY + 1 );

		public int NeighborSum => AboveLeft + Above + AboveRight + Left + Right + BelowLeft + Below + BelowRight;

		public World( int width, int height ) {
			Width = width;
			Height = height;
			grid1 = new int[width, height];
			grid2 = new int[width, height];
			Clear( );
		}

		public Color GetColor( int x, int y ) => Colors[Get( x, y ) % Colors.Length];

		public int Get( int x, int y, bool offGrid = false ) => Wrap ? offGrid ? OffGrid[x.Mod( Width ), y.Mod( Height )] : Grid[x.Mod( Width ), y.Mod( Height )] : x < 0 || x >= Width || y < 0 || y >= Height ? EdgeCell : offGrid ? OffGrid[x, y] : Grid[x, y];
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
					CurrentX = x;
					CurrentY = y;
					grid1[x, y] = grid2[x, y] = ClearCell( );
				}
			}
		}

		public void Randomize( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					CurrentX = x;
					CurrentY = y;
					grid1[x, y] = grid2[x, y] = RandomizeCell( );
				}
			}
		}

		public void Update( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					CurrentX = x;
					CurrentY = y;
					OffGrid[x, y] = UpdateCell( );
				}
			}
			Swap( );
		}

		public virtual int ClearCell( ) => 0;
		public virtual int RandomizeCell( ) => Raylib.GetRandomValue( 0, 1 );
		public virtual int UpdateCell( ) => Here;

		public bool IsOn( int x, int y ) => Get( x, y ) != 0;

	}
}