using UnityEngine;

namespace Gfen.Game.Manager
{
    public class LevelManager
    {
        private const string InfoKey = "LevelManagerInfo";

        private LevelManagerInfo m_managerInfo = new LevelManagerInfo();

        public void Init()
        {
            LoadInfo();
        }

        private void LoadInfo()
        {
            var json = PlayerPrefs.GetString(InfoKey, "");
            JsonUtility.FromJsonOverwrite(json, m_managerInfo);
        }

        private void SaveInfo()
        {
            var json = JsonUtility.ToJson(m_managerInfo);
            PlayerPrefs.SetString(InfoKey, json);
        }

        public bool IsLevelPassed(int levelId)
        {
            var isPassed = 0;
            if (m_managerInfo.levelInfo.TryGetValue(levelId, out isPassed))
            {
                return isPassed > 0;
            }

            return false;
        }

        public void PassLevel(int levelId)
        {
            m_managerInfo.levelInfo[levelId] = 1;

            SaveInfo();
        }
    }
}
