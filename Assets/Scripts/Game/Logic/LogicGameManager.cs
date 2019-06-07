using System;
using System.Collections.Generic;
using Gfen.Game.Common;
using Gfen.Game.Config;
using Gfen.Game.Utility;
using UnityEngine;

namespace Gfen.Game.Logic
{
    public class LogicGameManager
    {
        public Action<bool> GameEnd;

        public Action<Block> BlockMoved;

        public Action<Block> BlockTeleported;

        public Action<Block> BlockDestroyed;

        private GameManager m_gameManager;

        public GameManager GameManager { get { return m_gameManager; } }

        private RuleAnalyzer m_ruleAnalyzer;

        private AttirbuteHandler m_attributeHandler;

        public AttirbuteHandler AttributeHandler { get { return m_attributeHandler; } }

        private MapConfig m_mapConfig;

        private List<Block>[,] m_map;

        public List<Block>[,] Map { get { return m_map; } }

        private Stack<Stack<Command>> m_tickCommandsStack = new Stack<Stack<Command>>();

        private Stack<Stack<Command>> m_redoCommandsStack = new Stack<Stack<Command>>();

        public LogicGameManager(GameManager gameManager)
        {
            m_gameManager = gameManager;

            m_ruleAnalyzer = new RuleAnalyzer(this);
            m_attributeHandler = new AttirbuteHandler(this);
        }

        public void StartGame(MapConfig mapConfig)
        {
            m_mapConfig = mapConfig;

            StartGameCore();
        }

        public void StopGame()
        {
            StopGameCore();
        }

        public void RestartGame()
        {
            StopGameCore();
            StartGameCore();
        }

        private void StartGameCore()
        {
            m_map = new List<Block>[m_mapConfig.size.x, m_mapConfig.size.y];
            for (var i = 0; i < m_mapConfig.size.x; i++)
            {
                for (var j = 0; j < m_mapConfig.size.y; j++)
                {
                    m_map[i, j] = new List<Block>();
                }
            }

            foreach (var mapBlockConfig in m_mapConfig.blocks)
            {
                var block = new Block();
                block.entityType = mapBlockConfig.entityType;
                block.position = mapBlockConfig.position;
                block.direction = mapBlockConfig.direction;

                AddBlock(block);
            }

            m_attributeHandler.RefreshAttributes();

            m_ruleAnalyzer.Apply(null);
        }

        private void StopGameCore()
        {
            m_attributeHandler.Clear();

            m_tickCommandsStack.Clear();
            m_redoCommandsStack.Clear();

            m_ruleAnalyzer.Clear();
        }

        public void Undo()
        {
            if (m_tickCommandsStack.Count <= 0)
            {
                return;
            }

            var redoCommands = new Stack<Command>();

            var lastTickCommands = m_tickCommandsStack.Pop();
            while (lastTickCommands.Count > 0)
            {
                var command = lastTickCommands.Pop();
                command.Undo();

                redoCommands.Push(command);
            }

            m_redoCommandsStack.Push(redoCommands);

            m_attributeHandler.RefreshAttributes();

            m_ruleAnalyzer.Refresh();
        }

        public void Redo()
        {
            if (m_redoCommandsStack.Count <= 0)
            {
                return;
            }

            var tickCommands = new Stack<Command>();

            var redoCommands = m_redoCommandsStack.Pop();
            while (redoCommands.Count > 0)
            {
                var command = redoCommands.Pop();
                command.Perform();

                tickCommands.Push(command);
            }

            m_tickCommandsStack.Push(tickCommands);

            m_attributeHandler.RefreshAttributes();

            m_ruleAnalyzer.Refresh();
        }

        public void Tick(OperationType operationType)
        {
            m_redoCommandsStack.Clear();

            var tickCommands = new Stack<Command>();

            m_attributeHandler.HandleAttributeYou(operationType, tickCommands);
            m_attributeHandler.HandleAttributeMove(tickCommands);

            m_attributeHandler.HandleAttributeDefeat(tickCommands);
            m_attributeHandler.HandleAttributeSink(tickCommands);
            m_attributeHandler.HandleAttributeHot(tickCommands);

            m_attributeHandler.RefreshAttributes();

            m_ruleAnalyzer.Apply(tickCommands);

            if (tickCommands.Count > 0)
            {
                m_tickCommandsStack.Push(tickCommands);
            }

            CheckGameResult();
        }

        private void CheckGameResult()
        {
            var gameResult = GetGameResult();
            if (gameResult == GameResult.Success)
            {
                if (GameEnd != null)
                {
                    GameEnd(true);
                }
            }
        }

        private GameResult GetGameResult()
        {
            var gameResult = GameResult.Uncertain;
            ForeachMapPosition(position =>
            {
                if (HasAttribute(position, AttributeCategory.You))
                {
                    if (HasAttribute(position, AttributeCategory.Win))
                    {
                        gameResult = GameResult.Success;
                        return false;
                    }
                    if (HasAttribute(position, AttributeCategory.Defeat))
                    {
                        gameResult = GameResult.Failure;
                    }
                }

                return true;
            });

            return gameResult;
        }

        public void ForeachMapPosition(Func<Vector2Int, bool> positionHandler)
        {
            var mapXLength = m_map.GetLength(0);
            var mapYLength = m_map.GetLength(1);

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    if (!positionHandler(new Vector2Int(i, j)))
                    {
                        return;
                    }
                }
            }
        }

        public void ForeachMapBlock(Func<Block, bool> blockHandler)
        {
            var mapXLength = m_map.GetLength(0);
            var mapYLength = m_map.GetLength(1);

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    foreach (var block in m_map[i, j])
                    {
                        if (!blockHandler(block))
                        {
                            return;
                        }
                    }
                }
            }
        }

        public bool InMap(Vector2Int position)
        {
            return position.x >= 0 && position.x < m_map.GetLength(0) && position.y >= 0 && position.y < m_map.GetLength(1);
        }

        public void MoveBlock(Block block, Vector2Int position)
        {
            RemoveBlock(block);
            block.position = position;
            AddBlock(block);
        }

        public void RemoveBlock(Block block)
        {
            var mapBlockList = m_map[block.position.x, block.position.y];
            var targetIndex = mapBlockList.IndexOf(block);
            if (targetIndex >= 0)
            {
                mapBlockList.RemoveAt(targetIndex);
            }
        }

        public void AddBlock(Block block)
        {
            var mapBlockList = m_map[block.position.x, block.position.y];
            mapBlockList.Add(block);
        }

        public void ConverseBlockEntityType(Block block, int targetEntityType)
        {
            block.entityType = targetEntityType;
        }

        public bool HasAttribute(Vector2Int position, AttributeCategory attributeCategory)
        {
            var blocks = m_map[position.x, position.y];
            foreach (var block in blocks)
            {
                if (m_attributeHandler.HasAttribute(block, attributeCategory))
                {
                    return true;
                }
            }

            return false;
        }

        private void GetBlocksWithAttribute(AttributeCategory attributeCategory, List<Block> blocks)
        {
            var mapXLength = m_map.GetLength(0);
            var mapYLength = m_map.GetLength(1);

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    foreach (var block in m_map[i, j])
                    {
                        if (m_attributeHandler.HasAttribute(block, attributeCategory))
                        {
                            blocks.Add(block);
                        }
                    }
                }
            }
        }

        private void GetBlocksWithAttribute(Vector2Int position, AttributeCategory attributeCategory, List<Block> resultBlocks)
        {
            var blocks = m_map[position.x, position.y];
            foreach (var block in blocks)
            {
                if (m_attributeHandler.HasAttribute(block, attributeCategory))
                {
                    resultBlocks.Add(block);
                }
            }
        }
    }
}
