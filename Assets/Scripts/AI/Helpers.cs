using System.Collections.Generic;
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
            while (indexesToOperateOn.Count != nWeightsToMutate)
            {
                indexesToOperateOn.Add(UnityEngine.Random.Range(0, elementsCount));
            }

            List<T> result = new List<T>(list);
            foreach (int i in indexesToOperateOn)
            {
                result[i] = operation(result[i]);
            }

            return result;
        }
    }

}
