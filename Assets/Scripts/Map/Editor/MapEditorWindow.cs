using UnityEditor;
using UnityEngine;
using Gfen.Game.Logic;
using System.IO;
using System.Collections.Generic;
using Gfen.Game.Config;

namespace Gfen.Game.Map
{
    public class MapEditorWindow : EditorWindow 
    {
        private const string mapDirectoryKey = "MapDirectory";

        private const string configPathKey = "ConfigPath";

        private string m_mapDirectory;

        private string m_configPath;

        private string m_mapName;

        private Config.MapConfig m_map;

        private GameConfig m_config;

        [MenuItem("babaisyou/MapWindow")]
        private static void ShowWindow() 
        {
            var window = GetWindow<MapEditorWindow>();
            window.titleContent = new GUIContent("MapWindow");

            if (EditorPrefs.HasKey(mapDirectoryKey))
            {
                window.m_mapDirectory = EditorPrefs.GetString(mapDirectoryKey, "");
            }
            if (EditorPrefs.HasKey(configPathKey))
            {
                window.m_configPath = EditorPrefs.GetString(configPathKey, "");
                window.m_config = AssetDatabase.LoadAssetAtPath<GameConfig>(window.m_configPath);
            }

            window.Show();
        }
    
        private void OnGUI() 
        {
            GUILayout.BeginHorizontal();
            m_mapDirectory = EditorGUILayout.TextField(m_mapDirectory);
            if (GUILayout.Button("保存路径", GUILayout.Width(100)))
            {
                EditorPrefs.SetString(mapDirectoryKey, m_mapDirectory);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            m_configPath = EditorGUILayout.TextField(m_configPath);
            if (GUILayout.Button("加载配置", GUILayout.Width(100)))
            {
                m_config = AssetDatabase.LoadAssetAtPath<GameConfig>(m_configPath);
                EditorPrefs.SetString(configPathKey, m_configPath);
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Separator();

            GUI.enabled = m_config != null;

            m_mapName = EditorGUILayout.TextField(m_mapName);
            if (GUILayout.Button("创建新地图"))
            {
                CreateNewMap();
            }

            GUI.enabled = true;

            EditorGUILayout.Separator();

            m_map = EditorGUILayout.ObjectField(m_map, typeof(Config.MapConfig), false) as Config.MapConfig;

            GUI.enabled = m_map != null && m_config != null;

            if (GUILayout.Button("导入地图"))
            {
                ImportMap();
            }

            GUI.enabled = m_map != null;
            
            if (GUILayout.Button("导出地图"))
            {
                ExportMap();
            }

            GUI.enabled = m_config != null;

            EditorGUILayout.Separator();

            if (GUILayout.Button("生成MapBlockIdentifier"))
            {
                AttachMapBlockIdentifier();
            }

            GUI.enabled = true;
        }

        private void CreateNewMap()
        {
            if (string.IsNullOrEmpty(m_mapName))
            {
                ShowTip("文件名不能为空");
                return;
            }

            var path = Path.Combine(m_mapDirectory, m_mapName + ".asset");
            if (File.Exists(path))
            {
                ShowTip("文件已存在，请换个名字继续");
                return;
            }

            if (!Directory.Exists(m_mapDirectory))
            {
                Directory.CreateDirectory(m_mapDirectory);
            }

            m_map = ScriptableObject.CreateInstance<Config.MapConfig>();
            AssetDatabase.CreateAsset(m_map, path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var mapRoot = GetMapRoot();

            ClearMap(mapRoot);
        }

        private void ImportMap()
        {
            var mapRoot = GetMapRoot();

            ClearMap(mapRoot);

            m_config.Init();

            mapRoot.size = m_map.size;

            if (m_map.blocks != null)
            {
                foreach (var mapBlock in m_map.blocks)
                {
                    var entityConfig = m_config.GetEntityConfig(mapBlock.entityType);
                    var instantiatedBlock = Object.Instantiate(entityConfig.prefab);
                    instantiatedBlock.transform.SetParent(mapRoot.transform, false);
                    instantiatedBlock.transform.localPosition = new Vector3(mapBlock.position.x, mapBlock.position.y, 0);
                    var displacement = DirectionUtils.DirectionToDisplacement(mapBlock.direction);
                    instantiatedBlock.transform.localRotation = Quaternion.LookRotation(Vector3.forward, new Vector3(displacement.x, displacement.y, 0f));
                }
            }
        }

        private void ExportMap()
        {
            var mapRoot = GetMapRoot();

            m_map.size = mapRoot.size;

            var mapBlocks = new List<MapBlock>();
            for (var i = 0; i < mapRoot.transform.childCount; i++)
            {
                var mapBlockTransform = mapRoot.transform.GetChild(i);
                var mapBlockIdentifier = mapBlockTransform.GetComponent<MapBlockIdentifier>();
                if (mapBlockIdentifier != null)
                {
                    mapBlocks.Add(new MapBlock 
                    { 
                        entityType = mapBlockIdentifier.entityType, 
                        position = new Vector2Int(Mathf.RoundToInt(mapBlockTransform.localPosition.x), Mathf.RoundToInt(mapBlockTransform.localPosition.y)),
                        direction = DirectionUtils.DisplacementToDirection(new Vector2(mapBlockTransform.up.x, mapBlockTransform.up.y)),
                    });
                }
            }
            m_map.blocks = mapBlocks.ToArray();

            EditorUtility.SetDirty(m_map);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private MapRoot GetMapRoot()
        {
            var mapRoot = Object.FindObjectOfType<MapRoot>();
            if (mapRoot == null)
            {
                var mapRootGameObject = new GameObject("MapRoot");
                mapRootGameObject.transform.localPosition = Vector3.zero;
                mapRootGameObject.transform.localRotation = Quaternion.identity;
                mapRootGameObject.transform.localScale = Vector3.one;

                mapRoot = mapRootGameObject.AddComponent<MapRoot>();
            }

            return mapRoot;
        }

        private void ClearMap(MapRoot mapRoot)
        {
            mapRoot.size = Vector2Int.zero;
            for (var i = mapRoot.transform.childCount - 1; i >= 0; i--)
            {
                var mapBlockTransform = mapRoot.transform.GetChild(i);
                Object.DestroyImmediate(mapBlockTransform.gameObject);
            }
        }

        private void AttachMapBlockIdentifier()
        {
            m_config.Init();

            foreach (var entityConfig in m_config.entityConfigs)
            {
                var mapBlockIdentifier = entityConfig.prefab.GetComponent<MapBlockIdentifier>();
                if (mapBlockIdentifier == null)
                {
                    mapBlockIdentifier = entityConfig.prefab.AddComponent<MapBlockIdentifier>();
                }
                mapBlockIdentifier.entityType = entityConfig.type;
                EditorUtility.SetDirty(entityConfig.prefab);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        // 错误处理
        private void ShowTip(string tip)
        {
            ShowNotification(new GUIContent(tip));
        }
    }
}
