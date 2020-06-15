using Mogol.Util;

namespace Mogol {
	public class Wolfram : World {

		public Rule BirthRule => Rules[0];
		public Rule SurviveRule => Rules[1];
		public Rule[] Rules = new Rule[] { new Rule( ), new Rule( ) };

		public Wolfram( int width, int height ) : base( width, height ) { }
		public Wolfram( int width, int height, Rule birthRule, Rule surviveRule ) : this( width, height ) => Rules = new Rule[] { birthRule, surviveRule };

		public override int UpdateCell( ) => Rules[( Here > 0 ).ToInt( )][NeighborSum].ToInt( );

	}
}