using System;
using Gfen.Game.Common;

namespace Gfen.Game.Manager
{
    [Serializable]
    public class LevelManagerInfo
    {
        public int lastStayChapterIndex = -1;

        public SerializableDictionaryOfIntAndChapterInfo chapterInfoDict = new SerializableDictionaryOfIntAndChapterInfo();
    }

    public class SerializableDictionaryOfIntAndChapterInfo : SerializableDictionary<int, ChapterInfo> { }

    [Serializable]
    public class ChapterInfo
    {
        public SerializableDictionaryOfIntAndInt levelInfoDict = new SerializableDictionaryOfIntAndInt();
    }
}
