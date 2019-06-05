using System.Collections.Generic;
using Gfen.Game.Logic;
using Gfen.Game.Utility;
using UnityEngine;

namespace Gfen.Game.Presentation
{
    public class PresentationGameManager
    {
        private GameManager m_gameManager;

        private LogicGameManager m_logicGameManager;

        private Vector3 m_origin;

        private Transform m_mapRoot;

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

            m_origin = new Vector3(-mapXLength/2f + 0.5f, -mapYLength/2f + 0.5f, 0f);

            m_mapRoot = new GameObject("MapRoot").transform;
            m_mapRoot.transform.Reset();

            var blockRoot = new GameObject("BlockRoot").transform;
            blockRoot.SetParent(m_mapRoot, false);
            blockRoot.Reset();

            var backgroundRoot = new GameObject("BackgroundRoot").transform;
            backgroundRoot.SetParent(m_mapRoot, false);
            backgroundRoot.Reset();

            for (var i = 0; i < mapXLength; i++)
            {
                for (var j = 0; j < mapYLength; j++)
                {
                    CreateBackground(backgroundRoot.transform, i, j);
                    foreach (var block in map[i, j])
                    {
                        CreatePresentationBlock(blockRoot.transform, block);
                    }
                }
            }

            m_gameManager.gameCamera.orthographicSize = mapYLength/2f;
        }

        public void StopPresent()
        {
            if (m_mapRoot != null)
            {
                Object.Destroy(m_mapRoot.gameObject);
            }

            m_blockDict.Clear();
        }

        public void RefreshPresentation()
        {
            StopPresent();
            StartPresent();
        }

        private void CreateBackground(Transform backgroundRoot, int x, int y)
        {
            var backgroundGameObject = Object.Instantiate(m_gameManager.gameConfig.backgroundPrefab);

            backgroundGameObject.transform.SetParent(backgroundRoot, false);
            backgroundGameObject.transform.position = new Vector3(x, y, 0) + m_origin;
        }

        private void CreatePresentationBlock(Transform blockRoot, Block block)
        {
            var presentationBlock = new PresentationBlock();
            presentationBlock.block = block;
            
            var entityConfig = m_gameManager.gameConfig.GetEntityConfig(block.entityType);
            var blockGameObject = Object.Instantiate(entityConfig.prefab);
            presentationBlock.blockGameObject = blockGameObject;
            presentationBlock.blockTransform = blockGameObject.transform;

            presentationBlock.blockTransform.SetParent(blockRoot, false);
            presentationBlock.blockTransform.localPosition = new Vector3(block.position.x, block.position.y, 0) + m_origin;
            if (entityConfig.category != EntityCategory.Rule)
            {
                var displacement = DirectionUtils.DirectionToDisplacement(presentationBlock.block.direction);
                presentationBlock.blockTransform.localRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(displacement.x, displacement.y, 0f));
            }

            m_blockDict[block] = presentationBlock;
        }
    }
}
