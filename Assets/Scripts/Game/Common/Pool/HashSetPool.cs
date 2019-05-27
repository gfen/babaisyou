using System.Collections.Generic;

namespace Gfen.Game.Common
{
    public static class HashSetPool<T>
    {
        private static readonly ObjectPool<HashSet<T>> s_hashSetPool = new ObjectPool<HashSet<T>>(null, Clear);

        public static HashSet<T> Get()
        {
            return s_hashSetPool.Get();
        }

        public static void Release(HashSet<T> toRelease)
        {
            s_hashSetPool.Release(toRelease);
        }

        private static void Clear(HashSet<T> hashSet)
        { 
            hashSet.Clear(); 
        }
    }
}
