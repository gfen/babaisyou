using System.Collections.Generic;

namespace Gfen.Game.Common
{
    public static class StackPool<T>
    {
        private static readonly ObjectPool<Stack<T>> s_stackPool = new ObjectPool<Stack<T>>(null, Clear);

        public static Stack<T> Get()
        {
            return s_stackPool.Get();
        }

        public static void Release(Stack<T> toRelease)
        {
            s_stackPool.Release(toRelease);
        }

        private static void Clear(Stack<T> stack)
        { 
            stack.Clear(); 
        }
    }
}
