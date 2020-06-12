using Mogol.Util;
using Raylib_cs;
using System.Runtime.InteropServices;

namespace Mogol {
	internal class Program {

		private const bool DEBUG_BUTTONS = false;

		[DllImport( "shlwapi.dll" )]
		public static extern int ColorHLSToRGB( int H, int L, int S );

		public readonly static Viewport ViewWindow = new Viewport( 1600, 900 );
		public readonly static Viewport ViewGame = new Viewport( 0, 0, ViewWindow.Width, ViewWindow.Width / 2 );
		public readonly static Viewport ViewCtrls = new Viewport( 0, ViewGame.Height, ViewWindow.Width, ViewWindow.Height - ViewGame.Height );

		public static int MeasureText( string text ) => Raylib.MeasureText( text, FontHeight );
		public static int FontHeight => ViewWindow.Height / 45;
		public static int FontWidth => MeasureText( "M" );
		private static int PAUSED_SIZE = FontHeight * 10;

		public static World World = new World( 160, 80, new Rule( 3 ), new Rule( 2, 3 ) );

		public static readonly float CELL_WIDTH = ViewGame.Width / World.Width;
		public static readonly float CELL_HEIGHT = ViewGame.Height / World.Height;

		private static int MouseX => (int)( ( Raylib.GetMouseX( ) - ViewGame.X ) / CELL_WIDTH );
		private static int MouseY => (int)( ( Raylib.GetMouseY( ) - ViewGame.Y ) / CELL_HEIGHT );

		private static bool Playing = false;
		private static int Radius = 0;
		private static int TPS = 15;

		private static void Main( ) {
			Raylib.InitWindow( ViewWindow.Width, ViewWindow.Height, "Mogol" );
			Raylib.SetTargetFPS( int.MaxValue );
			float tickTimer = 0f;
			while ( !Raylib.WindowShouldClose( ) ) {
				Update( );
				if ( Playing ) {
					float tickTime = 1f / TPS;
					tickTimer += Raylib.GetFrameTime( );
					if ( tickTimer >= tickTime ) {
						FixedUpdate( );
						tickTimer -= tickTime;
					}
				}
				Raylib.BeginDrawing( );
				Draw( );
				Raylib.EndDrawing( );
			}
			Raylib.CloseWindow( );
		}

		static int maxRadius = ( World.Width < World.Height ? World.Width : World.Height ) / 2 - 1;
		private static void Update( ) {
			Radius += Raylib.GetMouseWheelMove( );
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
		private static void DrawCell( int x, int y ) => Raylib.DrawRectangle( ViewGame.Left + (int)( x * CELL_WIDTH ), ViewGame.Top + (int)( y * CELL_HEIGHT ), (int)CELL_WIDTH, (int)CELL_HEIGHT, Color.WHITE );
		private static void Draw( ) {
			// draw game
			Raylib.ClearBackground( Color.BLACK );
			for ( int x = 0; x < World.Width; x++ ) {
				for ( int y = 0; y < World.Height; y++ ) {
					if ( World.Grid[x, y] > 0 )
						DrawCell( x, y );
				}
			}
			int size = 2 * Radius + 1;
			Raylib.DrawRectangleLines( ViewGame.Left + (int)( ( MouseX - Radius ) * CELL_WIDTH ), ViewGame.Top + (int)( ( MouseY - Radius ) * CELL_HEIGHT ), (int)( size * CELL_WIDTH ), (int)( size * CELL_HEIGHT ), Color.GRAY );
			// draw ctrls
			int mn = 9;
			int i = 0;
			Raylib.DrawRectangle( ViewCtrls.X, ViewCtrls.Y, ViewCtrls.Width, ViewCtrls.Height, Color.DARKGRAY );
			Raylib.DrawText( $"{Raylib.GetFPS( )}FPS", ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, 20, Color.GRAY );
			Raylib.DrawText( $"{size}px", ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, FontHeight, Color.GRAY );
			DrawRule( 'S', ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, 1 );
			DrawRule( 'B', ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, 0 );
			string strTick = $"{TPS}tps";
			int totalLength = FontWidth * strTick.Length;
			Raylib.DrawText( strTick, ViewCtrls.Right - mn - totalLength, ViewCtrls.Top + mn, FontHeight, Color.GRAY );
			if ( AreaClicked( ViewCtrls.Right - mn - totalLength, ViewCtrls.Top + mn, totalLength, FontHeight ) )
				TPS = TPS < 60 ? TPS + 1 : 60;
			if ( AreaClicked( ViewCtrls.Right - mn - totalLength, ViewCtrls.Top + mn, totalLength, FontHeight, true ) )
				TPS = TPS > 0 ? TPS - 1 : 0;
			if ( !Playing )
				Raylib.DrawText( "PAUSED", ViewGame.Right - ( ViewGame.Width + Raylib.MeasureText( "PAUSED", PAUSED_SIZE ) ) / 2, ViewGame.Bottom - ( ViewGame.Height + PAUSED_SIZE ) / 2, PAUSED_SIZE, new Color( 127, 127, 127, 127 ) );
		}

		private static void DrawRule( char prefix, int posX, int posY, int ruleId ) {
			Raylib.DrawText( $"{prefix}:", posX, posY, FontHeight, Color.GRAY );
			posX += FontWidth * 2;
			for ( int i = 0; i <= 8; i++ ) {
				int x = posX + i * FontWidth;
				Raylib.DrawText( $"{8 - i}", x, posY, FontHeight, World.Rules[ruleId][8 - i] ? Color.LIGHTGRAY : Color.GRAY );
				if ( AreaClicked( x, posY, FontWidth - 1, FontHeight ) )
					World.Rules[ruleId][8 - i] ^= true;
			}
		}

		private static bool AreaClicked( int x, int y, int width, int height, bool right = false ) {
			if ( DEBUG_BUTTONS )
				Raylib.DrawRectangleLines( x, y, width, height, Color.RED );
			return Raylib.IsMouseButtonPressed( right ? MouseButton.MOUSE_RIGHT_BUTTON : MouseButton.MOUSE_LEFT_BUTTON ) && Raylib.GetMouseX( ) >= x && Raylib.GetMouseX( ) < x + width && Raylib.GetMouseY( ) >= y && Raylib.GetMouseY( ) < y + height;
		}
	}
}