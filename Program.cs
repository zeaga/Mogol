using Mogol.Util;
using Raylib_cs;
using System;
using System.Runtime.InteropServices;

namespace Mogol {
	internal class Program {

		private const bool DEBUG_BUTTONS = false;

		public readonly static Viewport ViewWindow = new Viewport( 1600, 900 );
		public readonly static Viewport ViewGame = new Viewport( 0, 0, ViewWindow.Width, ViewWindow.Width / 2 );
		public readonly static Viewport ViewCtrls = new Viewport( 0, ViewGame.Height, ViewWindow.Width, ViewWindow.Height - ViewGame.Height );

		public static int MeasureText( string text ) => Raylib.MeasureText( text, FontHeight );
		public static int FontHeight => ViewWindow.Height / 45;
		public static int FontWidth => MeasureText( "M" );

		public static Wolfram World = new Wolfram( 160, 80, new Rule( 3 ), new Rule( 2, 3 ) );

		public static readonly float CELL_WIDTH = ViewGame.Width / World.Width;
		public static readonly float CELL_HEIGHT = ViewGame.Height / World.Height;

		private static int MouseX => (int)( ( Raylib.GetMouseX( ) - ViewGame.X ) / CELL_WIDTH );
		private static int MouseY => (int)( ( Raylib.GetMouseY( ) - ViewGame.Y ) / CELL_HEIGHT );

		private static bool Playing = false;
		private static bool ShowHelp = false;
		private static int BrushSize = 1;
		private static int TPS = 15;

		private static readonly Color ColorDebugOutline = Color.RED;
		private static readonly Color ColorGameBackground = Color.BLACK;
		private static readonly Color ColorGameCell = Color.RAYWHITE;
		private static readonly Color ColorGameCursor = Color.DARKGRAY;
		private static readonly Color ColorHelpBackground = new Color( 0, 0, 0, 223 );
		private static readonly Color ColorHelpText = Color.LIGHTGRAY;
		private static readonly Color ColorHelpTextActive = Color.RAYWHITE;
		private static readonly Color ColorCtrlsBackground = Color.DARKGRAY;
		private static readonly Color ColorCtrlsText = Color.GRAY;
		private static readonly Color ColorCtrlsTextActive = Color.LIGHTGRAY;
		private static readonly Color ColorPauseButton = new Color( 127, 127, 127, 223 );

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

		private static bool MouseInGame => !( MouseX < 0 || MouseX >= World.Width || MouseY < 0 || MouseY >= World.Height );

		static int maxBrushSize = ( World.Width > World.Height ? World.Width : World.Height ) / 2 - 1;
		private static void Update( ) {
			ShowHelp ^= Raylib.IsKeyPressed( KeyboardKey.KEY_F1 );
			Playing ^= Raylib.IsKeyPressed( KeyboardKey.KEY_SPACE );
			BrushSize += Raylib.GetMouseWheelMove( );
			BrushSize = BrushSize < 0 ? 0 : BrushSize > maxBrushSize ? maxBrushSize : BrushSize;
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_C ) )
				World.Clear( );
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_R ) )
				World.Randomize( );
			if ( Raylib.IsKeyPressed( KeyboardKey.KEY_W ) )
				World.Wrap ^= true;
			if ( !Playing && Raylib.IsKeyPressed( KeyboardKey.KEY_P ) )
				World.Update( );
			if ( ShowHelp || !MouseInGame )
				return;
			int set = Raylib.IsMouseButtonDown( MouseButton.MOUSE_LEFT_BUTTON ) ? 1 : Raylib.IsMouseButtonDown( MouseButton.MOUSE_RIGHT_BUTTON ) ? 0 : -1;
			if ( set < 0 )
				return;
			int from = -(int)( BrushSize / 2f + 0.5f ), to = BrushSize / 2;
			for ( int x = from; x <= to; x++ ) {
				for ( int y = from; y <= to; y++ ) {
					World.Set( MouseX + x, MouseY + y, set );
				}
			}
		}

		private static void FixedUpdate( ) => World.Update( );
		private static void DrawCell( int x, int y ) => Raylib.DrawRectangle( ViewGame.Left + (int)( x * CELL_WIDTH ), ViewGame.Top + (int)( y * CELL_HEIGHT ), (int)CELL_WIDTH, (int)CELL_HEIGHT, ColorGameCell );
		private static void Draw( ) {
			int mn = 9;
			int wd = FontWidth * 16;
			// draw game
			Raylib.ClearBackground( ColorGameBackground );
			for ( int x = 0; x < World.Width; x++ ) {
				for ( int y = 0; y < World.Height; y++ ) {
					if ( World.Grid[x, y] > 0 )
						DrawCell( x, y );
				}
			}
			if ( !ShowHelp && ViewGame.Inside( Raylib.GetMouseX( ), Raylib.GetMouseY( ) ) ) {
				int cursorX = (int)( (int)( MouseX - BrushSize / 2f ) * CELL_WIDTH );
				int cursorY = (int)( (int)( MouseY - BrushSize / 2f ) * CELL_HEIGHT );
				int cursorWidth = (int)( ( BrushSize + 1 ) * CELL_WIDTH );
				cursorWidth = cursorX + cursorWidth > ViewGame.Width ? ViewGame.Width - cursorX : cursorWidth;
				cursorWidth = cursorX < 0 ? cursorWidth + cursorX : cursorWidth;
				int cursorHeight = (int)( ( BrushSize + 1 ) * CELL_HEIGHT );
				cursorHeight = cursorY + cursorHeight > ViewGame.Height ? ViewGame.Height - cursorY : cursorHeight;
				cursorHeight = cursorY < 0 ? cursorHeight + cursorY : cursorHeight;
				Raylib.DrawRectangleLines( ViewGame.Left + ( cursorX < 0 ? 0 : cursorX ), ViewGame.Top + ( cursorY < 0 ? 0 : cursorY ), cursorWidth, cursorHeight, ColorGameCursor );
			}
			if ( !Playing ) {
				Raylib.DrawRectangle( ViewGame.Right - 112, 32, ViewGame.Top + 32, 80, ColorPauseButton );
				Raylib.DrawRectangle( ViewGame.Right - 64, ViewGame.Top + 32, 32, 80, ColorPauseButton );
			}
			//draw help
			if ( ShowHelp ) {
				int j = 1;
				int x = ViewGame.Left + mn;
				int y = ViewGame.Top + mn;
				Raylib.DrawRectangle( ViewGame.Left + mn / 2, ViewGame.Top + mn / 2, FontWidth * 64, mn + mn + FontHeight * 15, ColorHelpBackground );
				DrawText( "L/R click to set/unset cells", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( $"L/R click \"{TPS}tps\" to increase/decrease tickrate", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "Scrollwheel to change brush size", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "<R> to randomize cells", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "<C> to clear cells", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "<P> to step one generation (if paused)", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "<W> to toggle wrapping", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "<Space> to toggle pause", x, y + FontHeight * ++j, ColorHelpText );
				j++;
				DrawText( "You can modify the game rules in realtime", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "Click a digit to toggle its state in the rule", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "For more information Google \"2D Cellular Automation\"", x, y + FontHeight * ++j, ColorHelpText );
				DrawText( "Please direct feedback to Zeaga#5406", x, y + FontHeight * ++j, ColorHelpText );
				j = 0;
				x += FontWidth * 36;
				DrawText( "Interesting rules (click to set):", x, y + FontHeight * ++j, ColorHelpText );
				x += FontWidth * 4;
				DrawText( "Amoeba (S1358/B357)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( 1, 3, 5, 8 );
					World.BirthRule.Set( 3, 5, 7 );
				} );
				DrawText( "Brian's Brain (S-/B2)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( );
					World.BirthRule.Set( 2 );
				} );
				DrawText( "Conway's Game of Life (S23/B3)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( 2, 3 );
					World.BirthRule.Set( 3 );
				} );
				DrawText( "Day & Night (S34678/B3678)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( 3, 4, 6, 7, 8 );
					World.BirthRule.Set( 3, 6, 7, 8 );
					World.Wrap = true;
				} );
				DrawText( "High Life (S23/B36)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( 2, 3 );
					World.BirthRule.Set( 3, 6 );
				} );
				DrawText( "Majority (S5678/B5678)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( 4, 5, 6, 7, 8 );
					World.BirthRule.Set( 5, 6, 7, 8 );
					World.Wrap = true;
				} );
				DrawText( "Walled Cities (S2345/B45678)", x, y + FontHeight * ++j, ColorHelpTextActive, ( ) => {
					World.SurviveRule.Set( 2, 3, 4, 5 );
					World.BirthRule.Set( 4, 5, 6, 7, 8 );
				} );
			}
			DrawText( "<F1> to toggle help", ViewGame.Left + mn, ViewGame.Top + mn, ColorHelpText );
			// draw ctrls
			Raylib.DrawRectangle( ViewCtrls.X, ViewCtrls.Y, ViewCtrls.Width, ViewCtrls.Height, ColorCtrlsBackground );
			int yi = -1;
			int xi = 0;
			string text = $"{TPS}tps";
			int length = FontWidth * text.Length;
			Raylib.DrawText( text, ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn + FontHeight * ++yi, FontHeight, ColorCtrlsText );
			if ( AreaClicked( ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn, length, FontHeight ) )
				TPS = TPS < 60 ? TPS + 5 : 60;
			if ( AreaClicked( ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn, length, FontHeight, true ) )
				TPS = TPS > 5 ? TPS - 5 : 5;
			Raylib.DrawText( $"{BrushSize + 1}px", ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * ++yi, FontHeight, ColorCtrlsText );
			DrawRule( 'S', ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * ++yi, 1 );
			DrawRule( 'B', ViewCtrls.Left + mn, ViewCtrls.Top + mn + FontHeight * ++yi, 0 );
			yi = -1;
			xi++;
			DrawText( "Clear", ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn + FontHeight * ++yi, ( ) => World.Clear( ) );
			DrawText( "Randomize", ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn + FontHeight * ++yi, ( ) => World.Randomize( ) );
			DrawText( "Step", ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn + FontHeight * ++yi, ( ) => { if ( !Playing ) World.Update( ); } );
			DrawText( "Wrap", ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn + FontHeight * ++yi, World.Wrap ? ColorCtrlsTextActive : ColorCtrlsText, ( ) => World.Wrap ^= true );
			yi = -1;
			xi++;
			DrawText( $"{ Raylib.GetFPS( )}fps", ViewCtrls.Left + mn + wd * xi, ViewCtrls.Top + mn + FontHeight * ++yi, ( ) => World.Clear( ) );
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

		private static bool DrawText( string text, int x, int y ) => DrawText( text, x, y, ColorCtrlsText );
		private static bool DrawText( string text, int x, int y, Action action ) => DrawText( text, x, y, ColorCtrlsText, action );

		private static void DrawRule( char prefix, int posX, int posY, int ruleId ) {
			Raylib.DrawText( $"{prefix}:", posX, posY, FontHeight, ColorCtrlsText );
			posX += FontWidth * 2;
			for ( int i = 0; i <= 8; i++ ) {
				int x = posX + i * FontWidth;
				Raylib.DrawText( $"{8 - i}", x, posY, FontHeight, World.Rules[ruleId][8 - i] ? ColorCtrlsTextActive : ColorCtrlsText );
				if ( AreaClicked( x, posY, FontWidth - 1, FontHeight ) )
					World.Rules[ruleId][8 - i] ^= true;
			}
		}

#pragma warning disable CS0162 // Unreachable code detected
		private static bool AreaClicked( int x, int y, int width, int height, bool right = false ) {
			if ( DEBUG_BUTTONS )
				Raylib.DrawRectangleLines( x, y, width, height, ColorDebugOutline );
			return Raylib.IsMouseButtonPressed( right ? MouseButton.MOUSE_RIGHT_BUTTON : MouseButton.MOUSE_LEFT_BUTTON ) && Raylib.GetMouseX( ) >= x && Raylib.GetMouseX( ) < x + width && Raylib.GetMouseY( ) >= y && Raylib.GetMouseY( ) < y + height;

		}
#pragma warning restore CS0162 // Unreachable code detected
	}
}