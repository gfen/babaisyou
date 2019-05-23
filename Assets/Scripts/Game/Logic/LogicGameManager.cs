using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gfen.Game.Logic
{
    public class LogicGameManager
    {
        public Action GameStarted;

        public Action GameEnded;

        public Action GameRestarted;

        public Action<Block> BlockCreated;

        public Action<Block> BlockPositionUpdated;

        public Action<Block> BlockDestroyed;

        private GameManager m_gameManager;

        public GameManager GameManager { get { return m_gameManager; } }

        private RuleAnalyzer m_ruleAnalyzer;

        private int m_mapId;

        private List<Block>[,] m_map;

        public List<Block>[,] Map { get { return m_map; } }

        private Dictionary<int, HashSet<AttributeCategory>> m_entityTypeAttributeDict = new Dictionary<int, HashSet<AttributeCategory>>();

        private Dictionary<EntityCategory, HashSet<AttributeCategory>> m_entityCategoryAttributeDict = new Dictionary<EntityCategory, HashSet<AttributeCategory>>();

        private Stack<Stack<Command>> m_tickCommandsStack = new Stack<Stack<Command>>();

        private Stack<Stack<Command>> m_redoCommandsStack = new Stack<Stack<Command>>();

        private List<Block> m_cachedBlocks = new List<Block>();

        private HashSet<AttributeCategory> m_cachedConversionAttributes = new HashSet<AttributeCategory>();

        private Stack<Command> m_cachedCommands = new Stack<Command>();

        public LogicGameManager(GameManager gameManager)
        {
            m_gameManager = gameManager;

            m_ruleAnalyzer = new RuleAnalyzer(this);
        }

        public void StartGame(int mapId)
        {
            m_mapId = mapId;

            StartGameCore(m_mapId);

            if (GameStarted != null)
            {
                GameStarted();
            }
        }

        public void EndGame()
        {
            EndGameCore();

            if (GameEnded != null)
            {
                GameEnded();
            }
        }

        public void RestartGame()
        {
            EndGameCore();
            StartGameCore(m_mapId);

            if (GameRestarted != null)
            {
                GameRestarted();
            }
        }

        private void StartGameCore(int mapId)
        {
            var mapConfig = m_gameManager.configSet.GetMapConfig(mapId);

            m_map = new List<Block>[mapConfig.size.x, mapConfig.size.y];
            for (var i = 0; i < mapConfig.size.x; i++)
            {
                for (var j = 0; j < mapConfig.size.y; j++)
                {
                    m_map[i, j] = new List<Block>();
                }
            }

            foreach (var mapBlockConfig in mapConfig.blocks)
            {
                var block = new Block();
                block.entityType = mapBlockConfig.entityType;
                block.position = mapBlockConfig.position;

                AddMapBlock(block);
            }

            RefreshAttributes();

            m_ruleAnalyzer.Apply(null);
        }

        private void EndGameCore()
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

            m_tickCommandsStack.Push(tickCommands);
        }

        public void ForeachMapBlock(Action<Block> blockHandler)
        {
            var mapXLength = m_map.GetLength(0);
            var mapYLength = m_map.GetLength(1);

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    foreach (var block in m_map[i, j])
                    {
                        blockHandler(block);
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

            ForeachMapBlock(block => 
            {
                if (!HasAttribute(block, AttributeCategory.You))
                {
                    return;
                }

                var youBlock = block;
                var positiveEndPushPosition = youBlock.position;
                while (InMap(positiveEndPushPosition + displacement) && HasAttribute(positiveEndPushPosition + displacement, AttributeCategory.Push))
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
                    for (var position = youBlock.position + displacement; position != positiveEndPushPosition + displacement; position += displacement)
                    {
                        GetBlocksWithAttribute(position, AttributeCategory.Push, m_cachedBlocks);
                        MoveBlocks(m_cachedBlocks, displacement, m_cachedCommands);

                        m_cachedBlocks.Clear();
                    }

                    for (var position = negativeEndPullPosition; position != youBlock.position; position += displacement)
                    {
                        GetBlocksWithAttribute(position, AttributeCategory.Pull, m_cachedBlocks);
                        MoveBlocks(m_cachedBlocks, displacement, m_cachedCommands);

                        m_cachedBlocks.Clear();
                    }

                    MoveBlock(youBlock, displacement, m_cachedCommands);
                }
            });

            while (m_cachedCommands.Count > 0)
            {
                var command = m_cachedCommands.Pop();
                command.Perform();
                tickCommands.Push(command);
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

            foreach (var entityCategoryConfig in m_gameManager.configSet.entityCategoryConfigs)
            {
                foreach (var attributeCategory in entityCategoryConfig.inherentAttributeCategories)
                {
                    SetAttributeForEntityCategory(entityCategoryConfig.entityCategory, attributeCategory);
                }
            }
        }

        private void MoveBlock(Block block, Vector2Int displacement, Stack<Command> tickCommands)
        {
            var moveCommand = new MoveCommand(this, block, displacement);
            // moveCommand.Perform();
            tickCommands.Push(moveCommand);
        }

        private void MoveBlocks(List<Block> blocks, Vector2Int displacement, Stack<Command> tickCommands)
        {
            foreach (var block in blocks)
            {
                MoveBlock(block, displacement, tickCommands);
            }
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
            var blockEntityCategory = m_gameManager.configSet.GetEntityConfig(block.entityType).category;
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

            var blockEntityCategory = m_gameManager.configSet.GetEntityConfig(block.entityType).category;
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
