using Mogol.Util;

namespace Mogol {
	public class Rule {

		private int flag;

		public bool this[int i] {
			get => ( ( flag >> i ) & 1 ) != 0;
			set => flag ^= ( value.ToInt( -1 ) ^ flag ) & ( 1 << i );
		}

		public void Set( params int[] items ) {
			flag = 0;
			foreach ( int item in items )
				flag |= 1 << item;
		}

		public Rule( params int[] items ) {
			flag = 0;
			foreach ( int item in items )
				flag |= 1 << item;
		}

	}
}