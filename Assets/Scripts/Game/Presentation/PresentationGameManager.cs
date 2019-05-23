using System.Collections.Generic;
using Gfen.Game.Logic;
using UnityEngine;

namespace Gfen.Game.Presentation
{
    public class PresentationGameManager
    {
        private GameManager m_gameManager;

        private LogicGameManager m_logicGameManager;

        private Dictionary<Block, PresentationBlock> m_blockDict = new Dictionary<Block, PresentationBlock>();

        public PresentationGameManager(GameManager gameManager, LogicGameManager logicGameManager)
        {
            m_gameManager = gameManager;
            m_logicGameManager = logicGameManager;
        }

        public void StartPresent()
        {
            var map = m_logicGameManager.Map;

            var mapXLength = map.GetLength(0);
            var mapYLength = map.GetLength(1);

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    foreach (var block in map[i, j])
                    {
                        CreatePresentationBlock(block);
                    }
                }
            }
        }

        public void StopPresent()
        {
            foreach (var blockPair in m_blockDict)
            {
                Object.Destroy(blockPair.Value.blockGameObject);
            }

            m_blockDict.Clear();
        }

        public void RefreshPresentation()
        {
            StopPresent();
            StartPresent();
        }

        private void CreatePresentationBlock(Block block)
        {
            var presentationBlock = new PresentationBlock();
            presentationBlock.block = block;
            
            var entityConfig = m_gameManager.configSet.GetEntityConfig(block.entityType);
            var blockGameObject = Object.Instantiate(entityConfig.prefab);
            presentationBlock.blockGameObject = blockGameObject;
            presentationBlock.blockTransform = blockGameObject.transform;

            presentationBlock.blockTransform.position = new Vector3(block.position.x*m_gameManager.unit, block.position.y*m_gameManager.unit, 0);

            m_blockDict[block] = presentationBlock;
        }
    }
}
