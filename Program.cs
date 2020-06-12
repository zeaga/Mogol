using Mogol.Util;
using Raylib_cs;
using System;
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
		private static bool ShowHelp = false;
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
						if ( !ShowHelp )
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
			ShowHelp ^= Raylib.IsKeyPressed( KeyboardKey.KEY_F1 );
			if ( ShowHelp )
				return;
			Radius += Raylib.GetMouseWheelMove( );
			Radius = Radius < 0 ? 0 : Radius > maxRadius ? maxRadius : Radius;
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_C ) )
				World.Clear( );
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_R ) )
				World.Randomize( );
			Playing ^= Raylib.IsKeyPressed( KeyboardKey.KEY_SPACE );
			if ( !Playing && Raylib.IsKeyPressed( KeyboardKey.KEY_P ) )
				World.Update( );
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
			int mn = 9;
			int wd = FontWidth * 16;
			// draw game
			Raylib.ClearBackground( Color.BLACK );
			for ( int x = 0; x < World.Width; x++ ) {
				for ( int y = 0; y < World.Height; y++ ) {
					if ( World.Grid[x, y] > 0 )
						DrawCell( x, y );
				}
			}
			int size = 2 * Radius + 1;
			if ( !ShowHelp )
				Raylib.DrawRectangleLines( ViewGame.Left + (int)( ( MouseX - Radius ) * CELL_WIDTH ), ViewGame.Top + (int)( ( MouseY - Radius ) * CELL_HEIGHT ), (int)( size * CELL_WIDTH ), (int)( size * CELL_HEIGHT ), Color.GRAY );
			if ( !Playing && !ShowHelp )
				Raylib.DrawText( "PAUSED", ViewGame.Right - ( ViewGame.Width + Raylib.MeasureText( "PAUSED", PAUSED_SIZE ) ) / 2, ViewGame.Bottom - ( ViewGame.Height + PAUSED_SIZE ) / 2, PAUSED_SIZE, new Color( 127, 127, 127, 127 ) );
			//draw help
			if ( ShowHelp ) {
				int j = 1;
				int x = ViewGame.Left + mn;
				int y = ViewGame.Top + mn;
				Raylib.DrawRectangle( ViewGame.Left + mn / 2, ViewGame.Top + mn / 2, FontWidth * 64, mn + mn + FontHeight * 14, new Color( 0, 0, 0, 223 ) );
				DrawText( "L/R click to set/unset cells", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( $"L/R click \"{TPS}tps\" to increase/decrease tickrate", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "Scrollwheel to change brush size", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "<R> to randomize cells", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "<C> to clear cells", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "<P> to step one generation (if paused)", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "<Space> to toggle pause", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				j++;
				DrawText( "You can modify the game rules in realtime", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "Click a digit to toggle its state in the rule", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "For more information Google \"2D Cellular Automation\"", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				DrawText( "Please direct feedback to Zeaga#5406", x, y + FontHeight * ++j, Color.LIGHTGRAY );
				j = 0;
				x += FontWidth * 36;
				DrawText( "Interesting rules (click to set):", x, y + FontHeight * ++j );
				x += FontWidth * 4;
				DrawText( "Amoeba (S1358/B357)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( 1, 3, 5, 8 );
					World.BirthRule.Set( 3, 5, 7 );
				} );
				DrawText( "Brian's Brain (S-/B2)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( );
					World.BirthRule.Set( 2 );
				} );
				DrawText( "Conway's Game of Life (S23/B3)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( 2, 3 );
					World.BirthRule.Set( 3 );
				} );
				DrawText( "Day & Night (S34678/B3678)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( 3, 4, 6, 7, 8 );
					World.BirthRule.Set( 3, 6, 7, 8 );
				} );
				DrawText( "High Life (S23/B36)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( 2, 3 );
					World.BirthRule.Set( 3, 6 );
				} );
				DrawText( "Majority (S5678/B5678)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( 4, 5, 6, 7, 8 );
					World.BirthRule.Set( 5, 6, 7, 8 );
				} );
				DrawText( "Walled Cities (S2345/B45678)", x, y + FontHeight * ++j, Color.WHITE, ( ) => {
					World.SurviveRule.Set( 2, 3, 4, 5 );
					World.BirthRule.Set( 4, 5, 6, 7, 8 );
				} );
			}
			DrawText( "<F1> to toggle help", ViewGame.Left + mn, ViewGame.Top + mn, Color.LIGHTGRAY );
			// draw ctrls
			Raylib.DrawRectangle( ViewCtrls.X, ViewCtrls.Y, ViewCtrls.Width, ViewCtrls.Height, Color.DARKGRAY );
			string text = $"{TPS}tps";
			int length = FontWidth * text.Length;
			DrawText( $"{ Raylib.GetFPS( )}fps", ViewCtrls.Left + mn + wd, ViewCtrls.Top + mn );
			Raylib.DrawText( text, ViewCtrls.Left + mn, ViewCtrls.Top + mn, FontHeight, Color.GRAY );
			if ( AreaClicked( ViewCtrls.Left + mn, ViewCtrls.Top + mn, length, FontHeight ) )
				TPS = TPS < 60 ? TPS + 5 : 60;
			if ( AreaClicked( ViewCtrls.Left + mn, ViewCtrls.Top + mn, length, FontHeight, true ) )
				TPS = TPS > 5 ? TPS - 5 : 5;
			int i = 1;
			DrawText( "Clear", ViewCtrls.Left + mn + wd, ViewCtrls.Top + mn + FontHeight * i, ( ) => World.Clear( ) );
			Raylib.DrawText( $"{size}px", ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, FontHeight, Color.GRAY );
			DrawText( "Randomize", ViewCtrls.Left + mn + wd, ViewCtrls.Top + mn + FontHeight * i, ( ) => World.Randomize( ) );
			DrawRule( 'S', ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, 1 );
			DrawText( "Step", ViewCtrls.Left + mn + wd, ViewCtrls.Top + mn + FontHeight * i, ( ) => { if ( !Playing ) World.Update( ); } );
			DrawRule( 'B', ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * i++, 0 );
		}

		private static bool DrawText( string text, int x, int y, Color color, Action action ) {
			Raylib.DrawText( text, x, y, FontHeight, color );
			bool clicked = AreaClicked( x, y, MeasureText( text ), FontHeight );
			if ( clicked )
				action( );
			return clicked;
		}

		private static bool DrawText( string text, int x, int y, Color color ) {
			Raylib.DrawText( text, x, y, FontHeight, color );
			return AreaClicked( x, y, MeasureText( text ), FontHeight );
		}

		private static bool DrawText( string text, int x, int y ) => DrawText( text, x, y, Color.GRAY );
		private static bool DrawText( string text, int x, int y, Action action ) => DrawText( text, x, y, Color.GRAY, action );

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

#pragma warning disable CS0162 // Unreachable code detected
		private static bool AreaClicked( int x, int y, int width, int height, bool right = false ) {
			if ( DEBUG_BUTTONS )
				Raylib.DrawRectangleLines( x, y, width, height, Color.RED );
			return Raylib.IsMouseButtonPressed( right ? MouseButton.MOUSE_RIGHT_BUTTON : MouseButton.MOUSE_LEFT_BUTTON ) && Raylib.GetMouseX( ) >= x && Raylib.GetMouseX( ) < x + width && Raylib.GetMouseY( ) >= y && Raylib.GetMouseY( ) < y + height;

		}
#pragma warning restore CS0162 // Unreachable code detected
	}
}