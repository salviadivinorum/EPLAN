using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EPLAN.Model
{
	internal static class Extensions
	{
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

		/// <summary>
		/// Permutation function
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="collection"></param>
		/// <returns></returns>
		public static IEnumerable<IEnumerable<T>> GetPermutationsC<T>(this IEnumerable<T> collection)
		{
			var list = collection.ToList();
			List<List<T>> permutations = new List<List<T>>();
			GetPermutations(list, 0, permutations);
			return permutations;
		}

		private static void GetPermutations<T>(List<T> collection, int startIndex,
											   List<List<T>> permutations)
		{
			if (startIndex == collection.Count - 1)
			{
				permutations.Add(new List<T>(collection));
			}
			else
			{
				for (int i = startIndex; i < collection.Count; i++)
				{
					Swap(collection, startIndex, i);
					GetPermutations(collection, startIndex + 1, permutations);
					Swap(collection, startIndex, i);
				}
			}
		}

		private static void Swap<T>(List<T> collection, int index1, int index2)
		{
			T temp = collection[index1];
			collection[index1] = collection[index2];
			collection[index2] = temp;
		}
	}
}
