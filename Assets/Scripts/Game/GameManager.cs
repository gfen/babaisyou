using Gfen.Game.Config;
using Gfen.Game.Logic;
using Gfen.Game.Manager;
using Gfen.Game.Presentation;
using Gfen.Game.UI;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace Gfen.Game
{
    public class GameManager : MonoBehaviour
    {
        public GameConfig gameConfig;

        public Camera gameCamera;

        public UIManager uiManager;

        private LevelManager m_levelManager;

        public LevelManager LevelManager { get { return m_levelManager; } }

        private LogicGameManager m_logicGameManager;

        private PresentationGameManager m_presentationGameManager;

        private bool m_isInGame;

        private bool m_isPause;

        private int m_currentChapterIndex;

        private int m_currentLevelIndex;

        private float m_lastInputTime;

        private void Start() 
        {
            gameConfig.Init();

            m_levelManager = new LevelManager();
            m_levelManager.Init(this);

            uiManager.Init(this);

            m_isInGame = false;
            m_isPause = false;
            m_logicGameManager = new LogicGameManager(this);
            m_presentationGameManager = new PresentationGameManager(this, m_logicGameManager);

            var stayChapterIndex = m_levelManager.GetStayChapterIndex();
            uiManager.ShowPage<ChapterPage>();
            if (stayChapterIndex >= 0)
            {
                var levelPage = uiManager.ShowPage<LevelPage>();
                levelPage.SetContent(stayChapterIndex);
            }
        }

        private void Update() 
        {
            if (m_isInGame)
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            var restart = CrossPlatformInputManager.GetButton("Restart");
            if (restart)
            {
                m_lastInputTime = 0f;
                RestartGame();
                return;
            }

            if (m_isPause)
            {
                return;
            }

            var pause = CrossPlatformInputManager.GetButton("Pause");
            if (pause)
            {
                m_lastInputTime = 0f;
                PauseGame();
                return;
            }

            var isTimeToInputDelay = (Time.unscaledTime - m_lastInputTime) >= gameConfig.inputRepeatDelay;

            var undo = CrossPlatformInputManager.GetButton("Undo");
            var redo = CrossPlatformInputManager.GetButton("Redo");
            if (undo)
            {
                if (isTimeToInputDelay)
                {
                    m_lastInputTime = Time.unscaledTime;
                    m_logicGameManager.Undo();
                    m_presentationGameManager.RefreshPresentation();
                }
            }
            else if (redo)
            {
                if (isTimeToInputDelay)
                {
                    m_lastInputTime = Time.unscaledTime;
                    m_logicGameManager.Redo();
                    m_presentationGameManager.RefreshPresentation();
                }
            }
            else
            {
                var operationType = GetLogicOperation();

                if (operationType != OperationType.None)
                {
                    if (isTimeToInputDelay)
                    {
                        m_lastInputTime = Time.unscaledTime;
                        m_logicGameManager.Tick(operationType);
                        m_presentationGameManager.RefreshPresentation();
                    }
                }
                else
                {
                    m_lastInputTime = 0f;
                }
            }
        }

        private OperationType GetLogicOperation()
        {
            var horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            var vertical = CrossPlatformInputManager.GetAxis("Vertical");
            var wait = CrossPlatformInputManager.GetButton("Wait");

            var operationType = OperationType.None;
            if (vertical > 0.1f && vertical >= Mathf.Abs(horizontal))
            {
                operationType = OperationType.Up;
            }
            else if (vertical < -0.1f && vertical <= -Mathf.Abs(horizontal))
            {
                operationType = OperationType.Down;
            }
            else if (horizontal < -0.1f && horizontal <= -Mathf.Abs(vertical))
            {
                operationType = OperationType.Left;
            }
            else if (horizontal > 0.1f && horizontal >= Mathf.Abs(vertical))
            {
                operationType = OperationType.Right;
            }
            else if (wait)
            {
                operationType = OperationType.Wait;
            }

            return operationType;
        }

        public void StartGame(int chapterIndex, int levelIndex)
        {
            uiManager.HideAllPages();

            m_logicGameManager.StartGame(gameConfig.chapterConfigs[chapterIndex].levelConfigs[levelIndex].map);
            m_presentationGameManager.StartPresent();
            m_isInGame = true;
            m_isPause = false;
            m_currentChapterIndex = chapterIndex;
            m_currentLevelIndex = levelIndex;

            m_logicGameManager.GameEnd += OnGameEnd;

            uiManager.ShowPage<GamePlayPage>();
        }

        public void StopGame()
        {
            m_logicGameManager.GameEnd -= OnGameEnd;

            m_presentationGameManager.StopPresent();
            m_logicGameManager.StopGame();
            m_isInGame = false;
            m_isPause = false;

            uiManager.HideAllPages();

            uiManager.ShowPage<ChapterPage>();
            var levelPage = uiManager.ShowPage<LevelPage>();
            levelPage.SetContent(m_currentChapterIndex);
        }

        public void RestartGame()
        {
            uiManager.HideAllPages();

            m_logicGameManager.RestartGame();
            m_presentationGameManager.RefreshPresentation();
            m_isPause = false;

            uiManager.ShowPage<GamePlayPage>();
        }

        public void PauseGame()
        {
            m_isPause = true;
            uiManager.ShowPage<InGameSettingsPage>();
        }

        public void ResumeGame()
        {
            m_isPause = false;
            uiManager.HidePage();
        }

        private void OnGameEnd(bool success)
        {
            if (success)
            {
                m_isInGame = false;
                m_levelManager.PassLevel(m_currentChapterIndex, m_currentLevelIndex);

                uiManager.ShowPage<GameSuccessPage>();
            }
        }
    }
}
