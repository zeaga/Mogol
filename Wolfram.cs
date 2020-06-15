using Mogol.Util;

namespace Mogol {
	public class Wolfram : World {

		public Rule BirthRule => Rules[0];
		public Rule SurviveRule => Rules[1];
		public Rule[] Rules = new Rule[2];

		public Wolfram( int width, int height ) : base( width, height ) {
			maxValue = 1;
		}

		public Wolfram( int width, int height, Rule birthRule, Rule surviveRule ) : this( width, height ) {
			Rules[0] = birthRule;
			Rules[1] = surviveRule;
		}

		public override void Update( ) {
			for ( int x = 0; x < Width; x++ ) {
				for ( int y = 0; y < Height; y++ ) {
					OffGrid[x, y] = Rules[( Grid[x, y] > 0 ).ToInt( )][SumNeighbors( x, y )].ToInt( );
				}
			}
			Swap( );
		}

	}
}