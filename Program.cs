using Mogol.Util;
using Raylib_cs;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Mogol {
	internal class Program {

		public const float TICK_TIME = 1 / 15f;

		[DllImport( "shlwapi.dll" )]
		public static extern int ColorHLSToRGB( int H, int L, int S );

		public readonly static Viewport ViewWindow = new Viewport( 1600, 900 );
		public readonly static Viewport ViewGame = new Viewport( 0, 0, ViewWindow.Width, ViewWindow.Width / 2 );
		public readonly static Viewport ViewCtrls = new Viewport( 0, ViewGame.Height, ViewWindow.Width, ViewWindow.Height - ViewGame.Height );

		public static World World = new World( 160, 80, new Rule( 3 ), new Rule( 2, 3 ) );

		public static readonly float CELL_WIDTH = ViewGame.Width / World.Width;
		public static readonly float CELL_HEIGHT = ViewGame.Height / World.Height;

		private static int MouseX => (int)( ( Raylib.GetMouseX( ) - ViewGame.X ) / CELL_WIDTH );
		private static int MouseY => (int)( ( Raylib.GetMouseY( ) - ViewGame.Y ) / CELL_HEIGHT );

		private static bool Playing = false;
		private static int Radius = 0;

		private static void Main( ) {
			Raylib.InitWindow( ViewWindow.Width, ViewWindow.Height, "Mogol" );
			Raylib.SetTargetFPS( int.MaxValue );
			float tickTimer = 0f;
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

		static int maxRadius = ( World.Width < World.Height ? World.Width : World.Height ) / 2 - 1;
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

		private static void DrawCell( int x, int y, Color color ) => Raylib.DrawRectangle( ViewGame.Left + (int)( x * CELL_WIDTH ), ViewGame.Top + (int)( y * CELL_HEIGHT ), (int)CELL_WIDTH, (int)CELL_HEIGHT, color );
		private static void Draw( ) {
			// draw game
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
			Raylib.DrawRectangleLines( ViewGame.Left + (int)( ( MouseX - Radius ) * CELL_WIDTH ), ViewGame.Top + (int)( ( MouseY - Radius ) * CELL_HEIGHT ), (int)( size * CELL_WIDTH ), (int)( size * CELL_HEIGHT ), Color.GRAY );
			// draw ctrls
			int mn = 9;
			int sz = 20;
			int i = 0;
			Raylib.DrawRectangle( ViewCtrls.X, ViewCtrls.Y, ViewCtrls.Width, ViewCtrls.Height, Color.DARKGRAY );
			Raylib.DrawText( $"{Raylib.GetFPS( )}FPS", ViewCtrls.Left + mn, ViewCtrls.Top + mn + sz * i++, 20, Color.GRAY );
			Raylib.DrawText( $"{size}px", ViewCtrls.Left + mn, ViewCtrls.Top + mn + sz * i++, sz, Color.GRAY );
			DrawRule( 'S', ViewCtrls.Left + mn, ViewCtrls.Top + mn + sz * i++, 20, 1 );
			DrawRule( 'B', ViewCtrls.Left + mn, ViewCtrls.Top + mn + sz * i++, 20, 0 );
		}

		private static void DrawRule( char prefix, int posX, int posY, int fontSize, int ruleId ) {
			Raylib.DrawText( $"{prefix}:", posX, posY, fontSize, Color.GRAY );
			for ( int i = 0; i <= 8; i++ ) {
				int x = posX + 26 + i * 12;
				Raylib.DrawText( $"{8 - i}", x, posY, 20, World.Rules[ruleId][8 - i] ? Color.LIGHTGRAY : Color.GRAY );
				if ( AreaClicked( x, posY, 11, 20 ) )
					World.Rules[ruleId][8 - i] ^= true;
			}
		}

		private static bool AreaClicked( int x, int y, int width, int height, bool right = false ) => Raylib.IsMouseButtonPressed( right ? MouseButton.MOUSE_RIGHT_BUTTON : MouseButton.MOUSE_LEFT_BUTTON ) && Raylib.GetMouseX( ) >= x && Raylib.GetMouseX( ) < x + width && Raylib.GetMouseY( ) >= y && Raylib.GetMouseY( ) < y + height;

	}
}