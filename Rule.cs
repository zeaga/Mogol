using Mogol.Util;

namespace Mogol {
	public class Rule {

		public int Flag;

		public bool this[int i] {
			get => ( ( Flag >> i ) & 1 ) != 0;
			set => Flag ^= ( value.ToInt( -1 ) ^ Flag ) & ( 1 << i );
		}

		public void Set( params int[] items ) {
			Flag = 0;
			foreach ( int item in items )
				Flag |= 1 << item;
		}

		public Rule( params int[] items ) {
			Flag = 0;
			foreach ( int item in items )
				Flag |= 1 << item;
		}

	}
}