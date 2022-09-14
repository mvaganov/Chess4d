using System.Collections.Generic;

public class Counting {
	private static char[] chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="value"></param>
	/// <param name="digits"></param>
	/// <returns>little endian</returns>
	public static List<T> Count<T>(long value, T[] digits) {
		List<T> list = new List<T>();
		while (value > 0) {
			long digit = value % digits.Length;
			value = value / digits.Length;
			list.Add(digits[digit]);
		}
		return list;
	}

	// TODO long ValueFromCombination(IEnumerable<T> sequence, IList<T> options)
	public static List<T> Combination<T>(long value, T[] options, int digits) {
		List<T> list = new List<T>();
		for(int i = 0; i < digits; ++i) {
			long digit = value % options.Length;
			value = value / options.Length;
			list.Add(options[digit]);
		}
		return list;
	}

	// TODO long ValueFromIncrementingCombination(IEnumerable<T> sequence, IList<T> options)
	public static List<T> IncrementingCombination<T>(long value, T[] options) {
		int places = 1;
		long countable = options.Length;
		while (value >= countable) {
			value -= countable;
			countable *= countable;
			++places;
		}
		return Combination(value, options, places);
	}

	/// <summary>
	/// 0: a, 26: aa
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static string Alpha(long value) {
		List<char> letters = IncrementingCombination(value, chars);
		letters.Reverse();
		return new string(letters.ToArray());
	}
}
