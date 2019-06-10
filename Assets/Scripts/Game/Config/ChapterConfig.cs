using System;
using UnityEngine;

namespace Gfen.Game.Config
{
    [Serializable]
    [CreateAssetMenu(fileName = "ChapterConfig", menuName = "babaisyou/ChapterConfig", order = 0)]
    public class ChapterConfig : ScriptableObject
    {
        public string chapterName;
        
        public LevelConfig[] levelConfigs;
    }
}
