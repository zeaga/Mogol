using Raylib_cs;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Mogol {
	internal class Program {

		[DllImport( "shlwapi.dll" )]
		public static extern int ColorHLSToRGB( int H, int L, int S );

		public const int WINDOW_WIDTH = 1600;
		public const int WINDOW_HEIGHT = 900;

		public const int GRID_WIDTH = 160;
		public const int GRID_HEIGHT = 90;

		public static readonly int CELL_WIDTH = WINDOW_WIDTH / GRID_WIDTH;
		public static readonly int CELL_HEIGHT = WINDOW_HEIGHT / GRID_HEIGHT;

		public const float TICK_TIME = 1 / 15f;

		public static bool Playing = false;

		private static int Radius = 0;

		public static World World = new World( GRID_WIDTH, GRID_HEIGHT );

		private static int MouseX => Raylib.GetMouseX( ) / CELL_WIDTH;
		private static int MouseY => Raylib.GetMouseY( ) / CELL_HEIGHT;

		private static void Main( ) {
			World.BirthRule.Set( 3 );
			World.SurviveRule.Set( 2, 3 );
			Raylib.InitWindow( WINDOW_WIDTH, WINDOW_HEIGHT, "Mogol" );
			Raylib.SetTargetFPS( 1000000000 );
			float tickTimer = 0f;
			Console.WriteLine( Vector2.UnitX );
			while ( !Raylib.WindowShouldClose( ) ) {
				Update( );
				if ( Playing ) {
					tickTimer += Raylib.GetFrameTime( );
					if ( tickTimer >= TICK_TIME ) {
						FixedUpdate( );
						tickTimer -= TICK_TIME;
					}
				}
				Raylib.BeginDrawing( );
				Draw( );
				Raylib.EndDrawing( );
			}
			Raylib.CloseWindow( );
		}

		static int maxRadius = ( GRID_WIDTH < GRID_HEIGHT ? GRID_WIDTH : GRID_HEIGHT ) / 2 - 1;
		private static KeyboardKey radiusDec = KeyboardKey.KEY_LEFT_BRACKET;
		private static KeyboardKey radiusInc = KeyboardKey.KEY_RIGHT_BRACKET;
		private static KeyboardKey modifier = KeyboardKey.KEY_LEFT_SHIFT;
		private static void Update( ) {
			if ( Raylib.IsKeyDown( modifier ) && Raylib.IsKeyDown( radiusDec ) || Raylib.IsKeyPressed( radiusDec ) )
				Radius--;
			if ( Raylib.IsKeyDown( modifier ) && Raylib.IsKeyDown( radiusInc ) || Raylib.IsKeyPressed( radiusInc ) )
				Radius++;
			Radius = Radius < 0 ? 0 : Radius > maxRadius ? maxRadius : Radius;
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_C ) )
				World.Clear( );
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_R ) )
				World.Randomize( );
			if ( !Playing && Raylib.IsKeyPressed( KeyboardKey.KEY_P ) )
				World.Update( );
			Playing ^= Raylib.IsKeyPressed( KeyboardKey.KEY_SPACE );
			int set = Raylib.IsMouseButtonDown( MouseButton.MOUSE_LEFT_BUTTON ) ? 1 : Raylib.IsMouseButtonDown( MouseButton.MOUSE_RIGHT_BUTTON ) ? 0 : -1;
			if ( set >= 0 ) {
				for ( int x = -Radius; x <= Radius; x++ ) {
					for ( int y = -Radius; y <= Radius; y++ ) {
						World.Set( MouseX + x, MouseY + y, set );
					}
				}
			}
		}

		private static void FixedUpdate( ) => World.Update( );

		private static void DrawCell( float x, float y, Color color ) => Raylib.DrawRectangle( (int)x * CELL_WIDTH, (int)y * CELL_HEIGHT, CELL_WIDTH, CELL_HEIGHT, color );
		private static void Draw( ) {
			Raylib.ClearBackground( Color.BLACK );
			for ( int x = 0; x < World.Width; x++ ) {
				for ( int y = 0; y < World.Height; y++ ) {
					int v = World.Grid[x, y];
					if ( v > 0 ) {
						//v = ( v - 80 ).Mod( 160 );
						//int c = ColorHLSToRGB( v < 160 ? v : 160, v < 160 ? 120 : v < 280 ? v - 40 : 240, 240 );
						//int c = ColorHLSToRGB( v.Mod( 240 ), 120 + ( v / 120 ), 240 );
						//DrawCell( x, y, new Color( c & 0xFF, ( c >> 8 ) & 0xFF, ( c >> 16 ) & 0xFF, 0xFF ) );
						DrawCell( x, y, Color.RAYWHITE );
					}
				}
			}
			int size = 2 * Radius + 1;
			Raylib.DrawRectangleLines( ( MouseX - Radius ) * CELL_WIDTH, ( MouseY - Radius ) * CELL_HEIGHT, size * CELL_WIDTH, size * CELL_HEIGHT, Color.GRAY );
			Raylib.DrawText( $"{Raylib.GetFPS( )}FPS", 8, 8, 20, Color.GREEN );
			Raylib.DrawText( $"{size}px", 8, 32, 20, Color.GREEN );
			DrawRule( 'S', 8, 56, 20, World.SurviveRule );
			DrawRule( 'B', 8, 80, 20, World.BirthRule );
		}

		private static void DrawRule( char prefix, int posX, int posY, int fontSize, Rule rule ) {
			Raylib.DrawText( $"{prefix}:", posX, posY, fontSize, Color.GREEN );
			for ( int i = 0; i <= 8; i++ )
				Raylib.DrawText( $"{8-i}", i * 12 + 26 + posX, posY, 20, rule[8-i] ? Color.GREEN : Color.DARKGREEN );
		}

	}
}