using System;
using System.Collections.Generic;

namespace Actor.UI.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Executes an <paramref name="action"/> over all enumerable elements
        /// </summary>
        /// <param name="enumerable">The source collection</param>
        /// <param name="action">The action to execute (element)</param>
        /// <typeparam name="T">The generic type of the <paramref name="enumerable"/></typeparam>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        /// Executes an <paramref name="action"/> over all enumerable elements, giving back also the index of the item in the given <paramref name="enumerable"/>
        /// </summary>
        /// <param name="enumerable">The source collection</param>
        /// <param name="action">The action to execute (index, element)</param>
        /// <typeparam name="T">The generic type of the <paramref name="enumerable"/></typeparam>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<int, T> action)
        {
            var i = 0;
            foreach (var item in enumerable)
            {
                action(i++, item);
            }
        }
    }
}