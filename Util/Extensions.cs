using System;
using System.Collections.Generic;
using System.Linq;

namespace Mogol.Util {
	public static class Extensions {

		public static int Mod( this int i, int iMax ) => ( ( i % iMax ) + iMax ) % iMax;
		public static float Mod( this float i, float iMax ) => ( ( i % iMax ) + iMax ) % iMax;
		private static Random rnd = new Random( );
		public static IEnumerable<T> Randomize<T>( this IEnumerable<T> source ) => source.OrderBy( ( item ) => rnd.Next( ) );
		public static int ToInt( this bool source, int i = 1 ) => source ? i : 0;

		public static IEnumerable<T> ForEach<T>( this IEnumerable<T> source, Action<T> action ) {
			foreach ( T item in source )
				action( item );
			return source;
		}

	}
}