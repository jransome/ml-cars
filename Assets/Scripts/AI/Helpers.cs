using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

namespace RansomeCorp
{
    public static class ListExtensions
    {
        /// <summary>
        /// Applies a function to a randomly selected percentage of a collection's elements. Will always apply the function to at least 1 element.
        /// </summary>
        public static List<T> GuaranteedApplyToPercentage<T>(this List<T> list, float percentage, Func<T, T> operation) // where T : class
        {
            int elementsCount = list.Count;
            int nWeightsToMutate = Mathf.CeilToInt(percentage * elementsCount);

            HashSet<int> indexesToOperateOn = new HashSet<int>();
            do
            {
                indexesToOperateOn.Add(UnityEngine.Random.Range(0, elementsCount));
            } while (indexesToOperateOn.Count != nWeightsToMutate);

            List<T> result = new List<T>(list);
            foreach (int i in indexesToOperateOn)
            {
                result[i] = operation(result[i]);
            }

            return result;
        }

        /// <summary>
        /// Crosses a list of T with another at a single point to produce two new lists. Iterations must 
        /// be an odd number or there is a possibility that the result will be the same as the original.
        /// </summary>
        public static List<List<T>> SinglePointCrossover<T>(this List<T> list1, List<T> list2, int iterations = 1, int previousSlicePoint = -1)
        {
            int elementsCount = list1.Count;
            if (elementsCount == 1)
            {
                Debug.LogWarning("Lists not long enough to be crossed!");
                return new List<List<T>>(2) { list2, list1 };
            }

            if (iterations % 2 == 0) iterations++; // make iterations odd

            return SinglePointCrossoverRecursive(list1, list2, iterations);
        }

        private static List<List<T>> SinglePointCrossoverRecursive<T>(List<T> list1, List<T> list2, int iterations)
        {
            int elementsCount = list1.Count;
            int slicePoint = UnityEngine.Random.Range(1, elementsCount - 1);

            var crossed = new List<List<T>>(2)
            {
                list1.Take(slicePoint).Concat(list2.Skip(slicePoint)).ToList(),
                list2.Take(slicePoint).Concat(list1.Skip(slicePoint)).ToList(),
            };

            return --iterations == 0 ? crossed : SinglePointCrossoverRecursive(crossed[0], crossed[1], iterations);
        }
    }

}
