using System.Collections.Generic;
using Gfen.Game.Common;
using Gfen.Game.Utility;
using UnityEngine;

namespace Gfen.Game.Logic
{
    public class AttirbuteHandler
    {
        private LogicGameManager m_logicGameManager;

        private Dictionary<int, HashSet<AttributeCategory>> m_entityTypeAttributeDict = new Dictionary<int, HashSet<AttributeCategory>>();

        private Dictionary<EntityCategory, HashSet<AttributeCategory>> m_entityCategoryAttributeDict = new Dictionary<EntityCategory, HashSet<AttributeCategory>>();

        public AttirbuteHandler(LogicGameManager logicGameManager)
        {
            m_logicGameManager = logicGameManager;
        }

        public void Clear()
        {
            m_entityTypeAttributeDict.Clear();
            m_entityCategoryAttributeDict.Clear();
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
            var blockEntityCategory = m_logicGameManager.GameManager.gameConfig.GetEntityConfig(block.entityType).category;
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

        public bool HasAttribute(Block block, AttributeCategory attributeCategory)
        {
            if (m_entityTypeAttributeDict.ContainsKey(block.entityType) && m_entityTypeAttributeDict[block.entityType].Contains(attributeCategory))
            {
                return true;
            }

            var blockEntityCategory = m_logicGameManager.GameManager.gameConfig.GetEntityConfig(block.entityType).category;
            return m_entityCategoryAttributeDict.ContainsKey(blockEntityCategory) && m_entityCategoryAttributeDict[blockEntityCategory].Contains(attributeCategory);
        }

        public void RefreshAttributes()
        {
            Clear();

            foreach (var entityCategoryConfig in m_logicGameManager.GameManager.gameConfig.entityCategoryConfigs)
            {
                foreach (var attributeCategory in entityCategoryConfig.inherentAttributeCategories)
                {
                    SetAttributeForEntityCategory(entityCategoryConfig.entityCategory, attributeCategory);
                }
            }
        }

        private void PerformMoveBlockCommand(Block block, Direction direction, int length, Stack<Command> tickCommands)
        {
            var moveCommand = new MoveCommand(m_logicGameManager, block, direction, length);
            moveCommand.Perform();
            tickCommands.Push(moveCommand);
        }

        private void PerformDestroyBlockCommand(Block block, Stack<Command> tickCommands)
        {
            var destroyCommand = new DestroyCommand(m_logicGameManager, block);
            destroyCommand.Perform();
            tickCommands.Push(destroyCommand);
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

        public void HandleAttributeYou(OperationType operationType, Stack<Command> tickCommands)
        {
            if (operationType == OperationType.Wait)
            {
                return;
            }

            var moveDirection = GetOperationDirection(operationType);

            var mapXLength = m_logicGameManager.Map.GetLength(0);
            var mapYLength = m_logicGameManager.Map.GetLength(1);

            var scanDirection = Direction.Up;
            if (DirectionUtils.IsParallel(scanDirection, moveDirection))
            {
                for (var i = 0; i < mapXLength; i++)
                {
                    if (moveDirection == scanDirection)
                    {
                        HandleDirectionYou(moveDirection, new Vector2Int(i, -1), new Vector2Int(i, mapYLength), tickCommands);
                    }
                    else
                    {
                        HandleDirectionYou(moveDirection, new Vector2Int(i, mapYLength), new Vector2Int(i, -1), tickCommands);
                    }
                }
            }
            
            scanDirection = Direction.Right;
            if (DirectionUtils.IsParallel(scanDirection, moveDirection))
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    if (moveDirection == scanDirection)
                    {
                        HandleDirectionYou(moveDirection, new Vector2Int(-1, j), new Vector2Int(mapXLength, j), tickCommands);
                    }
                    else
                    {
                        HandleDirectionYou(moveDirection, new Vector2Int(mapXLength, j), new Vector2Int(-1, j), tickCommands);
                    }
                }
            }
        }

        private void HandleDirectionYou(Direction moveDirection, Vector2Int negativeEndPosition, Vector2Int positiveEndPosition, Stack<Command> tickCommands)
        {
            var displacement = DirectionUtils.DirectionToDisplacement(moveDirection);

             var impactBlocks = DictionaryPool<Block, int>.Get();

            for (var position = negativeEndPosition + displacement; position != positiveEndPosition; position += displacement)
            {
                var hasYou = false;
                {
                    var blocks = m_logicGameManager.Map[position.x, position.y];
                    foreach (var block in blocks)
                    {
                        if (HasAttribute(block, AttributeCategory.You))
                        {
                            impactBlocks[block] = impactBlocks.GetOrDefault(block, 0) | 1;
                            hasYou = true;
                        }
                    }
                }
                if (hasYou)
                {
                    for (var pullPosition = position - displacement; m_logicGameManager.InMap(pullPosition); pullPosition -= displacement)
                    {
                        var blocks = m_logicGameManager.Map[pullPosition.x, pullPosition.y];
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
                    for (var pushPosition = position + displacement; m_logicGameManager.InMap(pushPosition); pushPosition += displacement)
                    {
                        var blocks = m_logicGameManager.Map[pushPosition.x, pushPosition.y];
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
                    var blocks = m_logicGameManager.Map[stopPosition.x, stopPosition.y];
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
                    var blocks = m_logicGameManager.Map[position.x, position.y];
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
                        var blocks = m_logicGameManager.Map[stopPosition.x, stopPosition.y];
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
                    PerformMoveBlockCommand(block, moveDirection, 1, tickCommands);
                }
            }

            DictionaryPool<Block, int>.Release(impactBlocks);
        }

        public void HandleAttributeMove(Stack<Command> tickCommands)
        {
            var mapXLength = m_logicGameManager.Map.GetLength(0);
            var mapYLength = m_logicGameManager.Map.GetLength(1);

            var scanDirection = Direction.Up;
            for (var i = 0; i < mapXLength; i++)
            {
                HandleDirectionMove(scanDirection, new Vector2Int(i, -1), new Vector2Int(i, mapYLength), tickCommands, true);
                HandleDirectionMove(scanDirection, new Vector2Int(i, -1), new Vector2Int(i, mapYLength), tickCommands, false);
            }
            
            scanDirection = Direction.Right;
            for (var j = 0; j < mapYLength; j++)
            {
                HandleDirectionMove(scanDirection, new Vector2Int(-1, j), new Vector2Int(mapXLength, j), tickCommands, true);
                HandleDirectionMove(scanDirection, new Vector2Int(-1, j), new Vector2Int(mapXLength, j), tickCommands, false);
            }
        }

        private void HandleDirectionMove(Direction scanDirection, Vector2Int negativeEndPosition, Vector2Int positiveEndPosition, Stack<Command> tickCommands, bool canBounce)
        {
            var scanDisplacement = DirectionUtils.DirectionToDisplacement(scanDirection);

            var impactBlocks = DictionaryPool<Block, int>.Get();

            for (var position = negativeEndPosition + scanDisplacement; position != positiveEndPosition; position += scanDisplacement)
            {
                var blocks = m_logicGameManager.Map[position.x, position.y];
                foreach (var block in blocks)
                {
                    if (!HasAttribute(block, AttributeCategory.Move) || !DirectionUtils.IsParallel(scanDirection, block.direction))
                    {
                        continue;
                    }
                    var impactDirection = 1;
                    var impactDisplacement = scanDisplacement;
                    if (block.direction != scanDirection)
                    {
                        impactDirection = 2;
                        impactDisplacement = Vector2Int.zero - scanDisplacement;
                    }
                    impactBlocks[block] = impactBlocks.GetOrDefault(block, 0) | impactDirection;
                    if (HasAttribute(block, AttributeCategory.Push))
                    {
                        for (var pushPosition = position + impactDisplacement; m_logicGameManager.InMap(pushPosition); pushPosition += impactDisplacement)
                        {
                            var pushBlocks = m_logicGameManager.Map[pushPosition.x, pushPosition.y];
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
                        for (var pullPosition = position - impactDisplacement; m_logicGameManager.InMap(pullPosition); pullPosition -= impactDisplacement)
                        {
                            var pullBlocks = m_logicGameManager.Map[pullPosition.x, pullPosition.y];
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

            for (var position = positiveEndPosition - scanDisplacement; position != negativeEndPosition; position -= scanDisplacement)
            {
                var blocks = m_logicGameManager.Map[position.x, position.y];
                foreach (var block in blocks)
                {
                    if (impactBlocks.GetOrDefault(block, 0) == 3)
                    {
                        impactBlocks[block] = 0;
                    }
                }
            }
            {
                var stopPosition = positiveEndPosition - scanDisplacement;
                {
                    var blocks = m_logicGameManager.Map[stopPosition.x, stopPosition.y];
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
                var stopPosition = negativeEndPosition + scanDisplacement;
                {
                    var blocks = m_logicGameManager.Map[stopPosition.x, stopPosition.y];
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
            for (var position = positiveEndPosition - scanDisplacement; position != negativeEndPosition + scanDisplacement; position -= scanDisplacement)
            {
                var hasStop = false;
                {
                    var blocks = m_logicGameManager.Map[position.x, position.y];
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
                    var stopPosition = position - scanDisplacement;
                    {
                        var blocks = m_logicGameManager.Map[stopPosition.x, stopPosition.y];
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
            for (var position = negativeEndPosition + scanDisplacement; position != positiveEndPosition - scanDisplacement; position += scanDisplacement)
            {
                var hasStop = false;
                {
                    var blocks = m_logicGameManager.Map[position.x, position.y];
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
                    var stopPosition = position + scanDisplacement;
                    {
                        var blocks = m_logicGameManager.Map[stopPosition.x, stopPosition.y];
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

                if (canBounce)
                {
                    if (HasAttribute(block, AttributeCategory.Move) && impact == 0)
                    {
                        block.direction = DirectionUtils.GetOppositeDirection(block.direction);
                    }
                }
                else
                {
                    if ((impact & 1) != 0)
                    {
                        PerformMoveBlockCommand(block, scanDirection, 1, tickCommands);
                    }
                    else if ((impact & 2) != 0)
                    {
                        PerformMoveBlockCommand(block, DirectionUtils.GetOppositeDirection(scanDirection), 1, tickCommands);
                    }
                }
            }

            DictionaryPool<Block, int>.Release(impactBlocks);
        }

        public void HandleAttributeSink(Stack<Command> tickCommands)
        {
            m_logicGameManager.ForeachMapPosition(position =>
            {
                var hasSink = false;
                var hasNonSink = false;
                var blocks = m_logicGameManager.Map[position.x, position.y];
                foreach (var block in blocks)
                {
                    if (HasAttribute(block, AttributeCategory.Sink))
                    {
                        hasSink = true;
                    }
                    else
                    {
                        hasNonSink = true;
                    }
                }

                if (hasSink && hasNonSink)
                {
                    var toDestroyBlocks = ListPool<Block>.Get();
                    toDestroyBlocks.AddRange(blocks);
                    foreach (var block in toDestroyBlocks)
                    {
                        PerformDestroyBlockCommand(block, tickCommands);
                    }
                    ListPool<Block>.Release(toDestroyBlocks);
                }

                return true;
            });
        }

        public void HandleAttributeDefeat(Stack<Command> tickCommands)
        {
            m_logicGameManager.ForeachMapPosition(position =>
            {
                var hasDefeat = m_logicGameManager.HasAttribute(position, AttributeCategory.Defeat);

                if (hasDefeat)
                {
                    var toDestroyBlocks = ListPool<Block>.Get();
                    var blocks = m_logicGameManager.Map[position.x, position.y];
                    foreach (var block in blocks)
                    {
                        if (HasAttribute(block, AttributeCategory.You))
                        {
                            toDestroyBlocks.Add(block);
                        }
                    }
                    foreach (var block in toDestroyBlocks)
                    {
                        PerformDestroyBlockCommand(block, tickCommands);
                    }
                    ListPool<Block>.Release(toDestroyBlocks);
                }

                return true;
            });
        }

        public void HandleAttributeHot(Stack<Command> tickCommands)
        {
            m_logicGameManager.ForeachMapPosition(position =>
            {
                var hasHot = m_logicGameManager.HasAttribute(position, AttributeCategory.Hot);

                if (hasHot)
                {
                    var toDestroyBlocks = ListPool<Block>.Get();
                    var blocks = m_logicGameManager.Map[position.x, position.y];
                    foreach (var block in blocks)
                    {
                        if (HasAttribute(block, AttributeCategory.Melt))
                        {
                            toDestroyBlocks.Add(block);
                        }
                    }
                    foreach (var block in toDestroyBlocks)
                    {
                        PerformDestroyBlockCommand(block, tickCommands);
                    }
                    ListPool<Block>.Release(toDestroyBlocks);
                }

                return true;
            });
        }
    }
}
