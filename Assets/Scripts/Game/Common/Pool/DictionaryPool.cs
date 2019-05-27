using System.Collections.Generic;

namespace Gfen.Game.Common
{
    public static class DictionaryPool<TKey, TValue>
    {
        private static readonly ObjectPool<Dictionary<TKey, TValue>> s_dictionaryPool = new ObjectPool<Dictionary<TKey, TValue>>(null, Clear);

        public static Dictionary<TKey, TValue> Get()
        {
            return s_dictionaryPool.Get();
        }

        public static void Release(Dictionary<TKey, TValue> toRelease)
        {
            s_dictionaryPool.Release(toRelease);
        }

        private static void Clear(Dictionary<TKey, TValue> dictionary)
        {
            dictionary.Clear();
        }
    }
}
