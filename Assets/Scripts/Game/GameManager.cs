using Gfen.Game.Config;
using Gfen.Game.Logic;
using Gfen.Game.Manager;
using Gfen.Game.Presentation;
using Gfen.Game.UI;
using UnityEngine;

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

        private int m_currentLevelIndex;

        private void Start() 
        {
            gameConfig.Init();

            m_levelManager = new LevelManager();
            m_levelManager.Init();

            uiManager.Init(this);

            m_isInGame = false;
            m_logicGameManager = new LogicGameManager(this);
            m_presentationGameManager = new PresentationGameManager(this, m_logicGameManager);

            uiManager.ShowPage<LevelPage>();
        }

        private void Update() 
        {
            if (m_isInGame)
            {
                var operationType = OperationType.None;
                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    operationType = OperationType.Up;
                }
                else if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    operationType = OperationType.Down;
                }
                else if (Input.GetKeyUp(KeyCode.LeftArrow))
                {
                    operationType = OperationType.Left;
                }
                else if (Input.GetKeyUp(KeyCode.RightArrow))
                {
                    operationType = OperationType.Right;
                }
                else if (Input.GetKeyUp(KeyCode.Space))
                {
                    operationType = OperationType.Wait;
                }

                if (operationType != OperationType.None)
                {
                    m_logicGameManager.Tick(operationType);
                    m_presentationGameManager.RefreshPresentation();
                }

                if (Input.GetKeyUp(KeyCode.Z))
                {
                    m_logicGameManager.Undo();
                    m_presentationGameManager.RefreshPresentation();
                }
                else if (Input.GetKeyUp(KeyCode.R))
                {
                    m_logicGameManager.Redo();
                    m_presentationGameManager.RefreshPresentation();
                }
                else if (Input.GetKeyUp(KeyCode.Q))
                {
                    m_logicGameManager.RestartGame();
                    m_presentationGameManager.RefreshPresentation();
                }

                if (Input.GetKeyUp(KeyCode.Escape))
                {
                    uiManager.ShowPage<InGameSettingsPage>();
                }
            }
        }

        public void StartGame(int levelIndex)
        {
            m_logicGameManager.StartGame(gameConfig.levelConfigs[levelIndex].map);
            m_presentationGameManager.StartPresent();
            m_isInGame = true;
            m_currentLevelIndex = levelIndex;

            m_logicGameManager.GameEnd += OnGameEnd;
        }

        public void StopGame()
        {
            m_logicGameManager.GameEnd -= OnGameEnd;

            m_presentationGameManager.StopPresent();
            m_logicGameManager.StopGame();
            m_isInGame = false;
        }

        private void OnGameEnd(bool success)
        {
            if (success)
            {
                m_isInGame = false;
                m_levelManager.PassLevel(m_currentLevelIndex);
                uiManager.ShowPage<GameSuccessPage>();
            }
        }
    }
}
