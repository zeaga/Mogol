using System;
using System.Runtime.CompilerServices;

namespace Mogol.Util {
	public struct Viewport : IEquatable<Viewport> {

		public int X;
		public int Y;
		public int Width;
		public int Height;

		public int Left { get => X; }
		public int Right { get => X + Width; }
		public int Top { get => Y; }
		public int Bottom { get => Y + Height; }

		public Viewport( int size ) : this( 0, 0, size, size ) { }
		public Viewport( int width, int height ) : this( 0, 0, width, height ) { }
		public Viewport( int x, int y, int size ) : this( x, y, size, size ) { }
		public Viewport( int x, int y, int width, int height ) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
			if ( Width < 0 ) {
				Width *= -1;
				X -= Width;
			}
			if ( Height < 0 ) {
				Height *= -1;
				Y -= Height;
			}
		}

		public override bool Equals( object other ) => other is Viewport && Equals( (Viewport)other );
		public bool Equals( Viewport other ) => X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
		public override int GetHashCode( ) => HashCode.Combine( X.GetHashCode( ), Y.GetHashCode( ), Width.GetHashCode( ), Height.GetHashCode( ) );

		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static bool operator ==( Viewport left, Viewport right ) => left.Equals( right );
		[MethodImpl( MethodImplOptions.AggressiveInlining )] public static bool operator !=( Viewport left, Viewport right ) => !( left == right );

	}
}