using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.Common
{
    public class ObjectPool<T> where T : new()
    {
        private readonly Stack<T> m_stack = new Stack<T>();

        private readonly Action<T> m_onGet;

        private readonly Action<T> m_onRelease;

        public int countAll { get; private set; }

        public int countActive { get { return countAll - countInactive; } }

        public int countInactive { get { return m_stack.Count; } }

        public ObjectPool(Action<T> onGet, Action<T> onRelease)
        {
            m_onGet = onGet;
            m_onRelease = onRelease;
        }

        public T Get()
        {
            T element;
            if (m_stack.Count == 0)
            {
                element = new T();
                countAll++;
            }
            else
            {
                element = m_stack.Pop();
            }

            if (m_onGet != null)
            {
                m_onGet(element);
            }

            return element;
        }

        public void Release(T element)
        {
            if (m_stack.Count > 0 && ReferenceEquals(m_stack.Peek(), element))
            {
                Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
            }

            if (m_onRelease != null)
            {
                m_onRelease(element);
            }

            m_stack.Push(element);
        }
    }
}
