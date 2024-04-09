using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace CleverGirl.Helper;

public static class HelperExtensions
{
    public static bool MaxBy<T, V>([NotNull] this IEnumerable<T> source, Func<T, V> selector, out T match)
        where V : IComparable<V>
    {
        match = default;
        if (!source.Any())
        {
            return false;
        }

        bool first = true;
        V maxKey = default;
        foreach (var item in source)
        {
            if (first)
            {
                match = item;
                maxKey = selector(match);
                first = false;
            }
            else
            {
                V currentKey = selector(item);
                if (currentKey.CompareTo(maxKey) > 0)
                {
                    maxKey = currentKey;
                    match = item;
                }
            }
        }

        return first;
    }

    public static bool First<T>([NotNull] this IEnumerable<T> source, Func<T, bool> selector, out T first)
    {
        first = default;
        foreach (T element in source)
        {
            if (selector(element))
            {
                first = element;
                return true;
            }
        }
        return false;
    }
}