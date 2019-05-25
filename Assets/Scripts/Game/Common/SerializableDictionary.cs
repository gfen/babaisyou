using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.Common
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<TKey> m_keys = new List<TKey>();

        [SerializeField]
        private List<TValue> m_values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            m_keys.Clear();
            m_values.Clear();

            foreach (var pair in this)
            {
                m_keys.Add(pair.Key);
                m_values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            var count = Math.Min(m_keys.Count, m_values.Count);
            for (var i = 0; i < count; i++)
            {
                this[m_keys[i]] = m_values[i];
            }
        }
    }
}
