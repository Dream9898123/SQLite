using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bingo.Extensions
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// 对source中的每个元素串行调用指定的操作
        /// 即，实现对foreach的封装
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="sequence">数据源</param>
        /// <param name="action">操作</param>
        public static void ForAll<T>(this IEnumerable<T> sequence, Action<T> action)
        {
            if (sequence == null || sequence.Count() <= 0)
                return;
            foreach (T item in sequence)
                action(item);
        }
    }
}
