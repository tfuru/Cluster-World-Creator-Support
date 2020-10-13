using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

using ClusterVR.CreatorKit.Item;
using ClusterVR.CreatorKit.Gimmick;
using ClusterVR.CreatorKit.Gimmick.Implements;

public class ClusterWorldCreatorSupport : EditorWindow
{
    // プレハブディレクトリ名
    private static string PATH_DIST_REFAB_FOLDER_NAME = "Prefab";

    private static string PATH_SRC_DESPAWN_HEIGHTP_REFAB = "Assets/Editor/Cluster-World-Creator-Support/Assets/Prefab/DespawnHeight.prefab";
    private static string PATH_DIST_DESPAWN_HEIGHTP_REFAB = "Assets/Prefab/DespawnHeight.prefab";
    private static Vector3[] PATH_DIST_DESPAWN_HEIGHTP_REFAB_TRANSFORM = {new Vector3(0,-10,0), Vector3.zero, new Vector3(1,1,1)};

    private static string PATH_SRC_SPAWNPOINTS_REFAB = "Assets/Editor/Cluster-World-Creator-Support/Assets/Prefab/SpawnPoints.prefab";
    private static string PATH_DIST_SPAWNPOINTS_REFAB = "Assets/Prefab/SpawnPoints.prefab";
    private static Vector3[] PATH_DIST_SPAWNPOINTS_TRANSFORM = {new Vector3(0,2,-4), Vector3.zero, new Vector3(1,1,1)};

    private static string PATH_SRC_GROUND_REFAB = "Assets/Editor/Cluster-World-Creator-Support/Assets/Prefab/Ground.prefab";
    private static string PATH_DIST_GROUND_REFAB = "Assets/Prefab/Ground.prefab";
    private static Vector3[] PATH_DIST_GROUND_TRANSFORM = {new Vector3(0,0,0), Vector3.zero, new Vector3(1,1,1)};

    // ランダム配置するプレハブ
    private static GameObject selectRandomParent = null;
    private static GameObject selectRandomPrefab = null;
    private static int selectRandomPrefabCount = 5;

    // タイル配置するプレハブ
    private static GameObject selectTileParent = null;
    private static GameObject selectTilePrefab = null;
    private static int tilePrefabX = 15;
    private static int tilePrefabY = 0;
    private static int tilePrefabZ = 15;

    // RPG 向け Prefab/Rpg/
    private static string PATH_SRC_RPG_FOLDER_PATH = "Assets/Editor/Cluster-World-Creator-Support/Assets/Prefab/Rpg";
    private static string PATH_DIST_RPG_FOLDER_PATH = "Assets/Prefab/Rpg";
    
    // 乗り物向け Prefab/Racing/
    private static string PATH_SRC_RACING_FOLDER_PATH = "Assets/Editor/Cluster-World-Creator-Support/Assets/Prefab/Racing";
    private static string PATH_DIST_RACING_FOLDER_PATH = "Assets/Prefab/Racing";

    [MenuItem ("World-Creator-Support/ワールド初期設定")]
    private static void InitWorld()
    {
        // シーンに必須コンポーネントを配置
        InitScene();
    }
    
    [MenuItem ("World-Creator-Support/ユーテリティ")]
    private static void OpenUtility()
    {
        EditorWindow.GetWindow<ClusterWorldCreatorSupport>("Cluster-World-Creator-Support");
    }

    void OnGUI () {
        // using (new GUILayout.HorizontalScope())
        using (new GUILayout.VerticalScope())
        {
            // プレハブのランダム配置
            EditorGUILayout.LabelField("Prefabランダム配置", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {

                EditorGUILayout.LabelField("親GameObject",  GUILayout.Width (100));                
                selectRandomParent = EditorGUILayout.ObjectField(selectRandomParent, typeof(Object), true) as GameObject;
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("子Prefab",  GUILayout.Width (100));
                selectRandomPrefab = EditorGUILayout.ObjectField(selectRandomPrefab, typeof(Object), true) as GameObject;
                EditorGUILayout.LabelField("Count",  GUILayout.Width (100));
                selectRandomPrefabCount = EditorGUILayout.IntField(selectRandomPrefabCount);
            }
            if (GUILayout.Button("ランダム配置"))
            {
                RandomPrefab();
            }

            // セパレーター
            Separator(0);
            EditorGUILayout.LabelField("Prefabタイル配置", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {

                EditorGUILayout.LabelField("親GameObject",  GUILayout.Width (100));                
                selectTileParent = EditorGUILayout.ObjectField(selectTileParent, typeof(Object), true) as GameObject;
            }
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("子Prefab",  GUILayout.Width (100));
                selectTilePrefab = EditorGUILayout.ObjectField(selectTilePrefab, typeof(Object), true) as GameObject;
            }
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("X,Y,Z",  GUILayout.Width (100));
                tilePrefabX = EditorGUILayout.IntField(tilePrefabX);
                tilePrefabY = EditorGUILayout.IntField(tilePrefabY);
                tilePrefabZ = EditorGUILayout.IntField(tilePrefabZ);
            }            
            if (GUILayout.Button("タイル配置"))
            {
                TilePrefab();
            }

            // セパレーター
            Separator(0);
            EditorGUILayout.LabelField("RPG風ワールド向け", EditorStyles.boldLabel);
            if (GUILayout.Button("武器,モンスター等取り込み"))
            {
                CopyRpgPrefab();
            }    

            // セパレーター
            Separator(0);
            EditorGUILayout.LabelField("乗り物系ワールド向け", EditorStyles.boldLabel);
            if (GUILayout.Button("乗物, リセット取り込み"))
            {
                CopyRacingPrefab();
            }                   
        }
    }

    /// <summary>
    /// インデントレベルを設定する仕切り線.
    /// </summary>
    /// <param name="indentLevel">インデントレベル</param>
    public static void Separator(int indentLevel)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(indentLevel * 15);
        GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
        EditorGUILayout.EndHorizontal();
    }

    // シーンに必須コンポーネントを配置
    private static void InitScene() {

        // Prefab ディレクトリを作成
        Debug.Log(PATH_DIST_REFAB_FOLDER_NAME);
        if (AssetDatabase.IsValidFolder("Assets/" + PATH_DIST_REFAB_FOLDER_NAME) == false){
            Debug.Log("Create REFAB_FOLDER");
            AssetDatabase.CreateFolder("Assets", PATH_DIST_REFAB_FOLDER_NAME);
        }

        // Cluster 必須コンポーネントを Prefabから生成 する
        CreatePrefab(PATH_SRC_GROUND_REFAB, PATH_DIST_GROUND_REFAB, PATH_DIST_GROUND_TRANSFORM);
        CreatePrefab(PATH_SRC_SPAWNPOINTS_REFAB, PATH_DIST_SPAWNPOINTS_REFAB, PATH_DIST_SPAWNPOINTS_TRANSFORM);
        CreatePrefab(PATH_SRC_DESPAWN_HEIGHTP_REFAB, PATH_DIST_DESPAWN_HEIGHTP_REFAB, PATH_DIST_DESPAWN_HEIGHTP_REFAB_TRANSFORM);
        
        // シーンを保存する
        EditorSceneManager.SaveOpenScenes();
    }

    // Cluster 必須コンポーネントを Prefab から生成 する
    private static void CreatePrefab(string pathSrc, string pashDist, Vector3[] transform) {
        // すでにあった場合は処理をしない
        GameObject dist = AssetDatabase.LoadAssetAtPath<GameObject>(pashDist);
        if (dist == null) {
            // Prefabから生成 する
            var src = PrefabUtility.LoadPrefabContents(pathSrc);
            dist = PrefabUtility.SaveAsPrefabAsset(src, pashDist);
            PrefabUtility.UnloadPrefabContents(src);
        }

        // シーンに Prefab を配置する
        dist.transform.position = transform[0];
        dist.transform.rotation = Quaternion.Euler(transform[1]);
        dist.transform.localScale = transform[2];
        PrefabUtility.InstantiatePrefab(dist);
    }

    // プレハブをランダムに配置
    private static void RandomPrefab()
    {
        if (selectRandomParent == null) return;
        if (selectRandomPrefab == null) return;

        Debug.Log("RandomPrefab");
        Debug.Log("親GameObject " + selectRandomParent.name);
        Debug.Log("子Prefab " + selectRandomPrefab.name);
        Debug.Log("Count " + selectRandomPrefabCount.ToString());
        
        for (int i = 0; i < selectRandomPrefabCount; i++)
        {
            var gameObj = PrefabUtility.InstantiatePrefab(selectRandomPrefab) as GameObject;
            //シーンにランダムに配置する
            int x = Random.Range(-5,5);
            int y = 2;
            int z = Random.Range(-5,5);
            gameObj.transform.position = new Vector3(x,y,z);
            gameObj.transform.parent = selectRandomParent.transform;
        }

        // シーンを保存する
        EditorSceneManager.SaveOpenScenes();
    }

    private static void CopyRpgPrefab()
    {
        if (AssetDatabase.IsValidFolder("Assets/" + PATH_DIST_REFAB_FOLDER_NAME) == false){
            Debug.Log("Create REFAB_FOLDER");
            AssetDatabase.CreateFolder("Assets", PATH_DIST_REFAB_FOLDER_NAME);
        }
        FileUtil.CopyFileOrDirectory(PATH_SRC_RPG_FOLDER_PATH, PATH_DIST_RPG_FOLDER_PATH);
        // 更新
        AssetDatabase.Refresh();

        // ToDo シーンに配置する
        // ToDo 各 Spawn の CreateItemGimik 対象を Assets/Prefab/Rpg を参照するように修正が必要 
        
        // AssetDatabase.Refresh();
    }

    private static void CopyRacingPrefab()
    {
        if (AssetDatabase.IsValidFolder("Assets/" + PATH_DIST_REFAB_FOLDER_NAME) == false){
            Debug.Log("Create REFAB_FOLDER");
            AssetDatabase.CreateFolder("Assets", PATH_DIST_REFAB_FOLDER_NAME);
        }
        FileUtil.CopyFileOrDirectory(PATH_SRC_RACING_FOLDER_PATH, PATH_DIST_RACING_FOLDER_PATH);

        // 更新
        AssetDatabase.Refresh();

        // ToDo シーンに配置する

        // AssetDatabase.Refresh();
    }

    // プレハブをタイル状に配置する
    private static void TilePrefab()
    {
        if (selectTileParent == null) return;
        if (selectTilePrefab == null) return;
        
        Debug.Log("TilePrefab");
        Debug.Log("親GameObject " + selectTileParent.name);
        Debug.Log("子Prefab " + selectTilePrefab.name);
        Debug.Log("X,Y " + tilePrefabX + " " + tilePrefabY + " " + tilePrefabZ);

        Transform tf = selectTilePrefab.transform;
        Vector3 localScale = tf.localScale;
        
        Debug.Log("localScale " + localScale.x + " " + localScale.y + " " + localScale.z);
        float sizeX = localScale.x;
        float sizeY = localScale.y;
        float sizeZ = localScale.z;
        float marginX = localScale.x;
        float marginZ = localScale.z;

        // tilePrefabX x tilePrefabY にタイル配置
        for (int i = 0; i < tilePrefabX; i++)
        {
            for (float  j = 0; j < tilePrefabZ; j++)
            {
                var gameObj = PrefabUtility.InstantiatePrefab(selectTilePrefab) as GameObject;
                float  x = sizeX*(float)i + sizeX;
                float  y = (float)tilePrefabY;
                float  z = sizeZ*(float)j + sizeZ;
                gameObj.transform.position = new Vector3(x,y,z);
                gameObj.transform.parent = selectTileParent.transform;
            }
        }

        // シーンを保存する
        EditorSceneManager.SaveOpenScenes();
    }    
}
