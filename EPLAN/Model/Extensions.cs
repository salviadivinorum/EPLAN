using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EPLAN.Model
{
	internal static class Extensions
	{
		/// <summary>
		/// Permute generic function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nums"></param>
		/// <param name="length"></param>
		public static IEnumerable<IEnumerable<T>> Permute<T>(this T[] nums, int length)
		{
			var list = new List<IEnumerable<T>>();
			return DoPermute(nums, 0, length, list);
		}

		/// <summary>
		/// Permute generic function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="nums"></param>
		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="list"></param
		private static IEnumerable<IEnumerable<T>> DoPermute<T>(T[] nums, int start, int end, List<IEnumerable<T>> list)
		{
			if (start == end)
			{
				// We have one of our possible n! solutions,
				// add it to the list.
				list.Add(new List<T>(nums));
			}
			else
			{
				for (var i = start; i <= end; i++)
				{
					Swap(ref nums[start], ref nums[i]);
					DoPermute(nums, start + 1, end, list);
					Swap(ref nums[start], ref nums[i]);
				}
			}

			return list;
		}

		/// <summary>
		/// Swap 2 elements of same type, skip compile optimization
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="a"></param>
		/// <param name="b"></param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Swap<T>(ref T a, ref T b)
		{
			T temp = a;
			a = b;
			b = temp;
		}

		/// <summary>
		/// Combination function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="elements"></param>
		/// <param name="k"></param>
		public static List<List<T>> Combinations<T>(this List<T> elements, int k)
		{
			List<List<T>> result = new List<List<T>>();
			if (k == 0)
			{
				result.Add(new List<T>());
				return result;
			}

			for (int i = 0; i < elements.Count; i++)
			{
				T currentElement = elements[i];
				List<T> remainingElements = new List<T>(elements.GetRange(i + 1, elements.Count - (i + 1)));

				List<List<T>> subCombinations = Combinations(remainingElements, k - 1);
				foreach (List<T> subCombination in subCombinations)
				{
					subCombination.Insert(0, currentElement);
					result.Add(subCombination);
				}
			}
			return result;
		}
	}
}
