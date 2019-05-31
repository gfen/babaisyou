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
                block.direction = mapBlockConfig.direction;

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
            HandleAttributeMove(tickCommands);

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

        private Direction GetOperationDirection(OperationType operationType)
        {
            switch (operationType)
            {
                case OperationType.Up: return Direction.Up;
                case OperationType.Down: return Direction.Down;
                case OperationType.Left: return Direction.Left;
                case OperationType.Right: return Direction.Right;
                default: return Direction.Up;
            }
        }

        private void HandleAttributeYou(OperationType operationType, Stack<Command> tickCommands)
        {
            if (operationType == OperationType.Wait)
            {
                return;
            }

            var direction = GetOperationDirection(operationType);
            var displacement = DirectionUtils.DirectionToDisplacement(direction);

            var youBlocks = ListPool<Block>.Get();

            GetBlocksWithAttribute(AttributeCategory.You, youBlocks);

            var handledYouBlocks = HashSetPool<Block>.Get();

            foreach (var youBlock in youBlocks)
            {
                if (handledYouBlocks.Contains(youBlock))
                {
                    continue;
                }

                var negativeEndPosition = youBlock.position - displacement;
                while (InMap(negativeEndPosition))
                {
                    negativeEndPosition -= displacement;
                }

                var positiveEndPosition = youBlock.position + displacement;
                while (InMap(positiveEndPosition))
                {
                    positiveEndPosition += displacement;
                }

                var impactBlocks = DictionaryPool<Block, int>.Get();

                for (var position = negativeEndPosition + displacement; position != positiveEndPosition; position += displacement)
                {
                    var hasYou = false;
                    {
                        var blocks = m_map[position.x, position.y];
                        foreach (var block in blocks)
                        {
                            if (HasAttribute(block, AttributeCategory.You))
                            {
                                impactBlocks[block] = impactBlocks.GetOrDefault(block, 0) | 1;
                                handledYouBlocks.Add(block);
                                hasYou = true;
                            }
                        }
                    }
                    if (hasYou)
                    {
                        for (var pullPosition = position - displacement; InMap(pullPosition); pullPosition -= displacement)
                        {
                            var blocks = m_map[pullPosition.x, pullPosition.y];
                            var hasPull = false;
                            foreach (var block in blocks)
                            {
                                if (HasAttribute(block, AttributeCategory.Pull))
                                {
                                    impactBlocks[block] = impactBlocks.GetOrDefault(block, 0) | 1;
                                    hasPull = true;
                                }
                            }
                            if (!hasPull)
                            {
                                break;
                            }
                        }
                        for (var pushPosition = position + displacement; InMap(pushPosition); pushPosition += displacement)
                        {
                            var blocks = m_map[pushPosition.x, pushPosition.y];
                            var hasPush = false;
                            foreach (var block in blocks)
                            {
                                if (HasAttribute(block, AttributeCategory.Push))
                                {
                                    impactBlocks[block] = impactBlocks.GetOrDefault(block, 0) | 1;
                                    hasPush = true;
                                }
                            }
                            if (!hasPush)
                            {
                                break;
                            }
                        }
                    }
                }

                {
                    var stopPosition = positiveEndPosition - displacement;
                    {
                        var blocks = m_map[stopPosition.x, stopPosition.y];
                        foreach (var block in blocks)
                        {
                            var impact = 0;
                            if (impactBlocks.TryGetValue(block, out impact))
                            {
                                impactBlocks[block] = 0;
                            }
                        }
                    }
                }
                for (var position = positiveEndPosition - displacement; position != negativeEndPosition + displacement; position -= displacement)
                {
                    var hasStop = false;
                    {
                        var blocks = m_map[position.x, position.y];
                        foreach (var block in blocks)
                        {
                            if (HasAttribute(block, AttributeCategory.Stop) || HasAttribute(block, AttributeCategory.Pull) || HasAttribute(block, AttributeCategory.Push))
                            {
                                if (impactBlocks.GetOrDefault(block, 0) == 0)
                                {
                                    hasStop = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (hasStop)
                    {
                        var stopPosition = position - displacement;
                        {
                            var blocks = m_map[stopPosition.x, stopPosition.y];
                            foreach (var block in blocks)
                            {
                                var impact = 0;
                                if (impactBlocks.TryGetValue(block, out impact))
                                {
                                    impactBlocks[block] = 0;
                                }
                            }
                        }
                    }
                }

                foreach (var impactBlockPair in impactBlocks)
                {
                    var block = impactBlockPair.Key;
                    var impact = impactBlockPair.Value;

                    if ((impact & 1) != 0)
                    {
                        MoveBlock(block, direction, 1, tickCommands);
                    }
                }

                DictionaryPool<Block, int>.Release(impactBlocks);
            }

            HashSetPool<Block>.Release(handledYouBlocks);

            ListPool<Block>.Release(youBlocks);
        }

        private void HandleAttributeMove(Stack<Command> tickCommands)
        {
            var moveBlocks = ListPool<Block>.Get();

            GetBlocksWithAttribute(AttributeCategory.Move, moveBlocks);

            var handledMoveBlocks = HashSetPool<Block>.Get();

            foreach (var moveBlock in moveBlocks)
            {
                if (handledMoveBlocks.Contains(moveBlock))
                {
                    continue;
                }

                var displacement = DirectionUtils.DirectionToDisplacement(moveBlock.direction);

                var negativeEndPosition = moveBlock.position - displacement;
                while (InMap(negativeEndPosition))
                {
                    negativeEndPosition -= displacement;
                }

                var positiveEndPosition = moveBlock.position + displacement;
                while (InMap(positiveEndPosition))
                {
                    positiveEndPosition += displacement;
                }

                var impactBlocks = DictionaryPool<Block, int>.Get();

                for (var position = negativeEndPosition + displacement; position != positiveEndPosition; position += displacement)
                {
                    var blocks = m_map[position.x, position.y];
                    foreach (var block in blocks)
                    {
                        if (!HasAttribute(block, AttributeCategory.Move) || !DirectionUtils.IsParallel(moveBlock.direction, block.direction))
                        {
                            continue;
                        }
                        var impactDirection = 1;
                        var impactDisplacement = displacement;
                        if (block.direction != moveBlock.direction)
                        {
                            impactDirection = 2;
                            impactDisplacement = Vector2Int.zero - displacement;
                        }
                        impactBlocks[block] = impactBlocks.GetOrDefault(block, 0) | impactDirection;
                        handledMoveBlocks.Add(block);
                        if (HasAttribute(block, AttributeCategory.Push))
                        {
                            for (var pushPosition = position + impactDisplacement; InMap(pushPosition); pushPosition += impactDisplacement)
                            {
                                var pushBlocks = m_map[pushPosition.x, pushPosition.y];
                                var hasPush = false;
                                foreach (var pushBlock in pushBlocks)
                                {
                                    if (HasAttribute(pushBlock, AttributeCategory.Push))
                                    {
                                        impactBlocks[pushBlock] = impactBlocks.GetOrDefault(pushBlock, 0) | impactDirection;
                                        hasPush = true;
                                    }
                                }
                                if (!hasPush)
                                {
                                    break;
                                }
                            }
                        }
                        if (HasAttribute(block, AttributeCategory.Pull))
                        {
                            for (var pullPosition = position - impactDisplacement; InMap(pullPosition); pullPosition -= impactDisplacement)
                            {
                                var pullBlocks = m_map[pullPosition.x, pullPosition.y];
                                var hasPull = false;
                                foreach (var pullBlock in pullBlocks)
                                {
                                    if (HasAttribute(pullBlock, AttributeCategory.Pull))
                                    {
                                        impactBlocks[pullBlock] = impactBlocks.GetOrDefault(pullBlock, 0) | impactDirection;
                                        hasPull = true;
                                    }
                                }
                                if (!hasPull)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }

                for (var position = positiveEndPosition - displacement; position != negativeEndPosition; position -= displacement)
                {
                    var blocks = m_map[position.x, position.y];
                    foreach (var block in blocks)
                    {
                        if (impactBlocks.GetOrDefault(block, 0) == 3)
                        {
                            impactBlocks[block] = 0;
                        }
                    }
                }
                {
                    var stopPosition = positiveEndPosition - displacement;
                    {
                        var blocks = m_map[stopPosition.x, stopPosition.y];
                        foreach (var block in blocks)
                        {
                            var impact = 0;
                            if (impactBlocks.TryGetValue(block, out impact))
                            {
                                impactBlocks[block] &= ~1;
                            }
                        }
                    }
                }
                {
                    var stopPosition = negativeEndPosition + displacement;
                    {
                        var blocks = m_map[stopPosition.x, stopPosition.y];
                        foreach (var block in blocks)
                        {
                            var impact = 0;
                            if (impactBlocks.TryGetValue(block, out impact))
                            {
                                impactBlocks[block] &= ~2;
                            }
                        }
                    }
                }
                for (var position = positiveEndPosition - displacement; position != negativeEndPosition + displacement; position -= displacement)
                {
                    var hasStop = false;
                    {
                        var blocks = m_map[position.x, position.y];
                        foreach (var block in blocks)
                        {
                            if (HasAttribute(block, AttributeCategory.Stop) || HasAttribute(block, AttributeCategory.Pull) || HasAttribute(block, AttributeCategory.Push))
                            {
                                if ((impactBlocks.GetOrDefault(block, 0) & 1) == 0)
                                {
                                    hasStop = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (hasStop)
                    {
                        var stopPosition = position - displacement;
                        {
                            var blocks = m_map[stopPosition.x, stopPosition.y];
                            foreach (var block in blocks)
                            {
                                var impact = 0;
                                if (impactBlocks.TryGetValue(block, out impact))
                                {
                                    impactBlocks[block] &= ~1;
                                }
                            }
                        }
                    }
                }
                for (var position = negativeEndPosition + displacement; position != positiveEndPosition - displacement; position += displacement)
                {
                    var hasStop = false;
                    {
                        var blocks = m_map[position.x, position.y];
                        foreach (var block in blocks)
                        {
                            if (HasAttribute(block, AttributeCategory.Stop) || HasAttribute(block, AttributeCategory.Pull) || HasAttribute(block, AttributeCategory.Push))
                            {
                                if ((impactBlocks.GetOrDefault(block, 0) & 2) == 0)
                                {
                                    hasStop = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (hasStop)
                    {
                        var stopPosition = position - displacement;
                        {
                            var blocks = m_map[stopPosition.x, stopPosition.y];
                            foreach (var block in blocks)
                            {
                                var impact = 0;
                                if (impactBlocks.TryGetValue(block, out impact))
                                {
                                    impactBlocks[block] &= ~2;
                                }
                            }
                        }
                    }
                }

                foreach (var impactBlockPair in impactBlocks)
                {
                    var block = impactBlockPair.Key;
                    var impact = impactBlockPair.Value;

                    if ((impact & 1) != 0)
                    {
                        MoveBlock(block, moveBlock.direction, 1, tickCommands);
                    }
                    else if ((impact & 2) != 0)
                    {
                        MoveBlock(block, DirectionUtils.GetOppositeDirection(moveBlock.direction), 1, tickCommands);
                    }
                }

                DictionaryPool<Block, int>.Release(impactBlocks);
            }

            HashSetPool<Block>.Release(handledMoveBlocks);

            ListPool<Block>.Release(moveBlocks);
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

        private void MoveBlock(Block block, Direction direction, int length, Stack<Command> tickCommands)
        {
            var moveCommand = new MoveCommand(this, block, direction, length);
            moveCommand.Perform();
            tickCommands.Push(moveCommand);
        }

        public void SetBlockPosition(Block block, Vector2Int position)
        {
            RemoveMapBlock(block);
            block.position = position;
            AddMapBlock(block);
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
