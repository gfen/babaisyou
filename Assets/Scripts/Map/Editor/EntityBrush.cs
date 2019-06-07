using System.Linq;
using Gfen.Game.Config;
using UnityEditor;
using UnityEngine;

namespace Gfen.Game.Map
{
    [CreateAssetMenu]
	[CustomGridBrush(false, true, false, "Entity Brush")]
    public class EntityBrush : GridBrushBase
    {
		public GameConfig gameConfig;

        public int selectedIndex;

		public override void Paint(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			// Do not allow editing palettes
			if (brushTarget.layer == 31)
            {
                return;
            }
            
            if (gameConfig == null)
            {
                return;
            }

			var entityConfig = gameConfig.entityConfigs[selectedIndex];
			GameObject instance = (GameObject) PrefabUtility.InstantiatePrefab(entityConfig.prefab);
			if (instance != null)
			{
				Undo.MoveGameObjectToScene(instance, brushTarget.scene, "Paint Entity");
				Undo.RegisterCreatedObjectUndo((Object)instance, "Paint Entity");
				instance.transform.SetParent(brushTarget.transform);
				instance.transform.position = grid.LocalToWorld(grid.CellToLocalInterpolated(new Vector3Int(position.x, position.y, 0) + new Vector3(.5f, .5f, 0f)));
			}
		}

		public override void Erase(GridLayout grid, GameObject brushTarget, Vector3Int position)
		{
			// Do not allow editing palettes
			if (brushTarget.layer == 31)
            {
                return;
            }

			var erased = GetObjectInCell(grid, brushTarget.transform, new Vector3Int(position.x, position.y, 0));
			if (erased != null)
            {
                Undo.DestroyObjectImmediate(erased.gameObject);
            }
		}

		private static Transform GetObjectInCell(GridLayout grid, Transform parent, Vector3Int position)
		{
			var childCount = parent.childCount;
			var min = grid.LocalToWorld(grid.CellToLocalInterpolated(position));
			var max = grid.LocalToWorld(grid.CellToLocalInterpolated(position + Vector3Int.one));
			var bounds = new Bounds((max + min)*.5f, max - min);

			for (var i = 0; i < childCount; i++)
			{
				var child = parent.GetChild(i);
				if (bounds.Contains(child.position))
                {
                    return child;
                }
			}
			return null;
		}
    }

    [CustomEditor(typeof(EntityBrush))]
	public class PrefabBrushEditor : GridBrushEditorBase
	{
		private EntityBrush entityBrush { get { return target as EntityBrush; } }

		private SerializedObject m_serializedObject;

        private string[] m_displayOptions;

		protected void OnEnable()
		{
			m_serializedObject = new SerializedObject(target);
            m_displayOptions = CreateDisplayOptions();
		}

		public override void OnPaintInspectorGUI()
		{
			m_serializedObject.UpdateIfRequiredOrScript();

            EditorGUI.BeginChangeCheck();
			entityBrush.gameConfig = EditorGUILayout.ObjectField("Game Config", entityBrush.gameConfig, typeof(GameConfig), false) as GameConfig;
            if (EditorGUI.EndChangeCheck())
            {
                m_displayOptions = CreateDisplayOptions();
            }
            if (m_displayOptions != null)
            {
                entityBrush.selectedIndex = EditorGUILayout.Popup("Select", entityBrush.selectedIndex, m_displayOptions);
            }

			m_serializedObject.ApplyModifiedPropertiesWithoutUndo();
		}

        private string[] CreateDisplayOptions()
        {
            if (entityBrush.gameConfig == null)
            {
                return null;
            }

            return (from entityConfig in entityBrush.gameConfig.entityConfigs select entityConfig.name).ToArray();
        }
	}
}
