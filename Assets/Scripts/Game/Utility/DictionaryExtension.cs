using System;
using System.Collections.Generic;

namespace Gfen.Game.Utility
{
    public static class DictionaryExtension
    {
        public static TValue GetOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> thisDictionary, TKey key, TValue defaultValue = default(TValue))
        {
            TValue value;
            if (thisDictionary.TryGetValue(key, out value))
            {
                return value;
            }

            return defaultValue;
        }

        public static TValue GetOrSet<TKey, TValue>(this Dictionary<TKey, TValue> thisDictionary, TKey key, Func<TValue> valueGetter)
        {
            TValue value;
            if (thisDictionary.TryGetValue(key, out value))
            {
                return value;
            }

            value = valueGetter();
            thisDictionary[key] = value;

            return value;
        }
    }
}
