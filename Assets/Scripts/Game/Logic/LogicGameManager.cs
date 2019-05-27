using System;
using System.Collections.Generic;
using Gfen.Game.Common;
using Gfen.Game.Config;
using UnityEngine;

namespace Gfen.Game.Logic
{
    public class LogicGameManager
    {
        public Action<bool> GameEnd;

        public Action<Block> BlockCreated;

        public Action<Block> BlockPositionUpdated;

        public Action<Block> BlockDestroyed;

        private GameManager m_gameManager;

        public GameManager GameManager { get { return m_gameManager; } }

        private RuleAnalyzer m_ruleAnalyzer;

        private MapConfig m_mapConfig;

        private List<Block>[,] m_map;

        public List<Block>[,] Map { get { return m_map; } }

        private Dictionary<int, HashSet<AttributeCategory>> m_entityTypeAttributeDict = new Dictionary<int, HashSet<AttributeCategory>>();

        private Dictionary<EntityCategory, HashSet<AttributeCategory>> m_entityCategoryAttributeDict = new Dictionary<EntityCategory, HashSet<AttributeCategory>>();

        private Stack<Stack<Command>> m_tickCommandsStack = new Stack<Stack<Command>>();

        private Stack<Stack<Command>> m_redoCommandsStack = new Stack<Stack<Command>>();

        public LogicGameManager(GameManager gameManager)
        {
            m_gameManager = gameManager;

            m_ruleAnalyzer = new RuleAnalyzer(this);
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

                AddMapBlock(block);
            }

            RefreshAttributes();

            m_ruleAnalyzer.Apply(null);
        }

        private void StopGameCore()
        {
            m_entityTypeAttributeDict.Clear();
            m_entityCategoryAttributeDict.Clear();

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

            RefreshAttributes();

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

            RefreshAttributes();

            m_ruleAnalyzer.Refresh();
        }

        public void Tick(OperationType operationType)
        {
            m_redoCommandsStack.Clear();

            var tickCommands = new Stack<Command>();

            HandleAttributeYou(operationType, tickCommands);

            RefreshAttributes();

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
            if (gameResult != GameResult.Uncertain)
            {
                if (GameEnd != null)
                {
                    GameEnd(gameResult == GameResult.Success);
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

        private void HandleAttributeYou(OperationType operationType, Stack<Command> tickCommands)
        {
            if (operationType == OperationType.Wait)
            {
                return;
            }

            var displacement = GetOperationDisplacement(operationType);

            var youBlocks = ListPool<Block>.Get();

            GetBlocksWithAttribute(AttributeCategory.You, youBlocks);

            var handledYouBlocks = HashSetPool<Block>.Get();

            foreach (var youBlock in youBlocks)
            {
                if (handledYouBlocks.Contains(youBlock))
                {
                    continue;
                }

                var positiveEndPushPosition = youBlock.position;
                while (InMap(positiveEndPushPosition + displacement) && (HasAttribute(positiveEndPushPosition + displacement, AttributeCategory.Push) || HasAttribute(positiveEndPushPosition + displacement, AttributeCategory.You)))
                {
                    positiveEndPushPosition += displacement;
                }

                var negativeEndPullPosition = youBlock.position;
                while (InMap(negativeEndPullPosition - displacement) && HasAttribute(negativeEndPullPosition - displacement, AttributeCategory.Pull))
                {
                    negativeEndPullPosition -= displacement;
                }

                if (InMap(positiveEndPushPosition + displacement) && !HasAttribute(positiveEndPushPosition + displacement, AttributeCategory.Stop) && !HasAttribute(positiveEndPushPosition + displacement, AttributeCategory.Pull))
                {
                    var cachedCommands = ListPool<Command>.Get();

                    for (var position = youBlock.position + displacement; position != positiveEndPushPosition + displacement; position += displacement)
                    {
                        var attributeBlocks = ListPool<Block>.Get();

                        GetBlocksWithAttribute(position, AttributeCategory.Push, attributeBlocks);
                        AddMoveBlockCommandsByYou(attributeBlocks, displacement, handledYouBlocks, cachedCommands);

                        ListPool<Block>.Release(attributeBlocks);
                    }

                    for (var position = negativeEndPullPosition; position != youBlock.position; position += displacement)
                    {
                        var attributeBlocks = ListPool<Block>.Get();

                        GetBlocksWithAttribute(position, AttributeCategory.Pull, attributeBlocks);
                        AddMoveBlockCommandsByYou(attributeBlocks, displacement, handledYouBlocks, cachedCommands);

                        ListPool<Block>.Release(attributeBlocks);
                    }

                    AddMoveBlockCommand(youBlock, displacement, cachedCommands);

                    handledYouBlocks.Add(youBlock);

                    foreach (var cachedCommand in cachedCommands)
                    {
                        cachedCommand.Perform();
                        tickCommands.Push(cachedCommand);
                    }

                    ListPool<Command>.Release(cachedCommands);
                }
            }

            HashSetPool<Block>.Release(handledYouBlocks);

            ListPool<Block>.Release(youBlocks);
        }

        private void AddMoveBlockCommandsByYou(List<Block> blocks, Vector2Int displacement, HashSet<Block> handledYouBlocks, List<Command> tickCommands)
        {
            foreach (var block in blocks)
            {
                AddMoveBlockCommand(block, displacement, tickCommands);

                if (HasAttribute(block, AttributeCategory.You))
                {
                    handledYouBlocks.Add(block);    
                }
            }
        }

        private Vector2Int GetOperationDisplacement(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Up: return Vector2Int.up;
                case OperationType.Down: return Vector2Int.down;
                case OperationType.Left: return Vector2Int.left;
                case OperationType.Right: return Vector2Int.right;
                default: return Vector2Int.zero;
            }
        }

        public bool InMap(Vector2Int position)
        {
            return position.x >= 0 && position.x < m_map.GetLength(0) && position.y >= 0 && position.y < m_map.GetLength(1);
        }

        private void RefreshAttributes()
        {
            m_entityTypeAttributeDict.Clear();
            m_entityCategoryAttributeDict.Clear();

            foreach (var entityCategoryConfig in m_gameManager.gameConfig.entityCategoryConfigs)
            {
                foreach (var attributeCategory in entityCategoryConfig.inherentAttributeCategories)
                {
                    SetAttributeForEntityCategory(entityCategoryConfig.entityCategory, attributeCategory);
                }
            }
        }

        private void AddMoveBlockCommand(Block block, Vector2Int displacement, List<Command> cachedCommands)
        {
            var moveCommand = new MoveCommand(this, block, displacement);
            cachedCommands.Add(moveCommand);
        }

        public void SetBlockPosition(Block block, Vector2Int position)
        {
            RemoveMapBlock(block);
            block.position = position;
            AddMapBlock(block);

            if (BlockPositionUpdated != null)
            {
                BlockPositionUpdated(block);
            }
        }

        private void RemoveMapBlock(Block block)
        {
            var mapBlockList = m_map[block.position.x, block.position.y];
            var targetIndex = mapBlockList.IndexOf(block);
            if (targetIndex >= 0)
            {
                mapBlockList.RemoveAt(targetIndex);
            }
        }

        private void AddMapBlock(Block block)
        {
            var mapBlockList = m_map[block.position.x, block.position.y];
            mapBlockList.Add(block);
        }

        public void ConverseBlockEntityType(Block block, int targetEntityType)
        {
            block.entityType = targetEntityType;
        }

        public void SetAttributeForEntityType(int entityType, AttributeCategory attributeCategory)
        {
            if (!m_entityTypeAttributeDict.ContainsKey(entityType))
            {
                m_entityTypeAttributeDict[entityType] = new HashSet<AttributeCategory>();
            }

            var attributeCategories = m_entityTypeAttributeDict[entityType];
            if (!attributeCategories.Contains(attributeCategory))
            {
                attributeCategories.Add(attributeCategory);
            }
        }

        public void SetAttributeForEntityCategory(EntityCategory entityCategory, AttributeCategory attributeCategory)
        {
            if (!m_entityCategoryAttributeDict.ContainsKey(entityCategory))
            {
                m_entityCategoryAttributeDict[entityCategory] = new HashSet<AttributeCategory>();
            }

            var attributeCategories = m_entityCategoryAttributeDict[entityCategory];
            if (!attributeCategories.Contains(attributeCategory))
            {
                attributeCategories.Add(attributeCategory);
            }
        }

        private void GetAttributes(Block block, HashSet<AttributeCategory> attributeCategories)
        {
            var blockEntityCategory = m_gameManager.gameConfig.GetEntityConfig(block.entityType).category;
            var categoryAttributes = m_entityCategoryAttributeDict.ContainsKey(blockEntityCategory) ? m_entityCategoryAttributeDict[blockEntityCategory] : null;
            var typeAttributes = m_entityTypeAttributeDict.ContainsKey(block.entityType) ? m_entityTypeAttributeDict[block.entityType] : null;

            if (categoryAttributes != null)
            {
                foreach (var attributeCategory in categoryAttributes)
                {
                    if (!attributeCategories.Contains(attributeCategory))
                    {
                        attributeCategories.Add(attributeCategory);
                    }
                }
            }
            if (typeAttributes != null)
            {
                foreach (var attributeCategory in typeAttributes)
                {
                    if (!attributeCategories.Contains(attributeCategory))
                    {
                        attributeCategories.Add(attributeCategory);
                    }
                }
            }
        }

        private bool HasAttribute(Block block, AttributeCategory attributeCategory)
        {
            if (m_entityTypeAttributeDict.ContainsKey(block.entityType) && m_entityTypeAttributeDict[block.entityType].Contains(attributeCategory))
            {
                return true;
            }

            var blockEntityCategory = m_gameManager.gameConfig.GetEntityConfig(block.entityType).category;
            return m_entityCategoryAttributeDict.ContainsKey(blockEntityCategory) && m_entityCategoryAttributeDict[blockEntityCategory].Contains(attributeCategory);
        }

        private bool HasAttribute(Vector2Int position, AttributeCategory attributeCategory)
        {
            var mapBlockList = m_map[position.x, position.y];
            foreach (var block in mapBlockList)
            {
                if (HasAttribute(block, attributeCategory))
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
                        if (HasAttribute(block, attributeCategory))
                        {
                            blocks.Add(block);
                        }
                    }
                }
            }
        }

        private void GetBlocksWithAttribute(Vector2Int position, AttributeCategory attributeCategory, List<Block> blocks)
        {
            var mapBlockList = m_map[position.x, position.y];
            foreach (var block in mapBlockList)
            {
                if (HasAttribute(block, attributeCategory))
                {
                    blocks.Add(block);
                }
            }
        }
    }
}
