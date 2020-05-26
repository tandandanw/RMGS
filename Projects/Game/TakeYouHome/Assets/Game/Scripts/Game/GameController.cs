using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameController : MonoBehaviour
{
    #region Event Functions

    private void Awake()
    {
        gameData = ScriptableObject.CreateInstance<GameData>();
        gameData.GameStatus = GameStatus.Uninitiate;
        taskPlanner = GetComponent<TaskPlanner>();
        DontDestroyOnLoad(this);
    }

    private void OnGUI()
    {
        /*
        if (Debugger.isDebug)
        {
            int offset = 1;
            int sizeX = 500;
            int sizeY = 20;
            GUI.TextArea(new Rect(10, sizeY * offset++, sizeX, sizeY), (1.0f / Time.deltaTime).ToString());
            foreach (string s in Debug.LogInfo)
            {
                GUI.TextArea(new Rect(10, sizeY * offset, sizeX, sizeY), s);
                ++offset;
            }
            if (Debug.LogInfo.Count > 48) Debugger.ClearLogger();
        }
        */
    }

    #endregion

    #region Scenes Management

    private IEnumerator LoadGameScene()
    {
        asyncLoadLevel = SceneManager.LoadSceneAsync("TakeYouHomeGameScene");
        while (!asyncLoadLevel.isDone)
        {
            Debug.Log("> LOADING SCENECE...");
            yield return null;
        }

        GameObject map = GameObject.FindGameObjectWithTag("Map");
        if (map)
        {
            mapGenerator = map.GetComponent<MapGenerator>();
            if (mapGenerator)
            {
                Debug.Log("> GENERATE IS STARTING...");
                mapGenerator.Result = mapPrepare.Result;
                mapGenerator.Generate(roadAssetBundle, fillerAssetBundles.ToArray());
                Debug.Log("> GENERATE DONE.");

                // Start game.
                player = GameObject.FindGameObjectWithTag("Player");
                uiController = GameObject.FindGameObjectWithTag("UI").GetComponent<UIController>();
                taskPlanner.StartNewRound(uiController, player, ScriptableObject.CreateInstance<LevelData>(), mapGenerator.Map, mapGenerator.MapInfo);
            }
            else
            {
                Debug.Log("> MAPGENERATOR NOT FOUND.");
            }

        }
        else
        {
            Debug.Log("> GAME MAP NOT FOUND.");
        }
    }

    public void StartGame()
    {
        gameData.LoadingPercentage = 0.1f;
        if (gameData.GameStatus == GameStatus.Uninitiate)
            StartCoroutine(StartLevel());
    }

    private IEnumerator StartLevel()
    {

        // Set game info.
        gameData.Level = 4;
        gameData.LevelName = "SunCity";
        gameData.GameStatus = GameStatus.Standby;

        gameData.LoadingPercentage = 0.2f;

        // Load asset bundles.
        Debug.Log("> ASSETS LOADING...");
        LoadAllAssets();
        while (constraintsAssetBundle == null || roadAssetBundle == null || fillerAssetBundles.Count < 4)
            yield return new WaitForSeconds(5);
        Debug.Log("> ASSETS LOADING DONE.");
        gameData.LoadingPercentage = Random.Range(0.2f, 0.5f);

        // Run WFC to get results.
        mapPrepare = new MapPrepare(gameData.Level,
            constraintsAssetBundle.LoadAsset<TextAsset>($"Constraints{gameData.LevelName}"));
        if (mapPrepare == null)
        {
            Debug.Log("> MAPPREPARE FAILED.");
            yield break;
        }
        if (0 != mapPrepare.Start())
        {
            Debug.Log("> MAPPREPARE STARTING FAILED.");
            yield break;
        }
        gameData.LoadingPercentage = Random.Range(0.5f, 0.8f);

        // Load game scene.
        yield return StartCoroutine(LoadGameScene());
        gameData.GameStatus = GameStatus.Run;
        gameData.LoadingPercentage = 1;
    }

    public void EndGame()
    {
        Application.Quit();
    }

    #endregion

    #region Assets Management

    private int LoadAllAssets()
    {
        fillerAssetBundles = new List<AssetBundle>();
        string bundleBaseName = gameData.LevelName.ToLower();

#if UNITY_EDITOR || UNITY_STANDALONE

        string basePath = $"{Application.streamingAssetsPath}/{bundleBaseName}";
        Debug.Log(basePath);

        constraintsAssetBundle = AssetBundle.LoadFromFile($"{basePath}/constraints");
        roadAssetBundle = AssetBundle.LoadFromFile($"{basePath}/roads");
        for (int i = 8; i > 0; i /= 2)
            fillerAssetBundles.Add(AssetBundle.LoadFromFile($"{basePath}/unit{i}"));

#endif

#if UNITY_WEBGL

        string baseURL = $"{Application.streamingAssetsPath}/{bundleBaseName}";
        Debug.Log(baseURL);

        StartCoroutine(LoadAssetBundleforWeb($"{baseURL}/constraints", 0));
        StartCoroutine(LoadAssetBundleforWeb($"{baseURL}/roads", 1));
        for (int i = 8; i > 0; i /= 2)
            StartCoroutine(LoadAssetBundleforWeb($"{baseURL}/unit{i}", 2));

#endif

#if UNITY_ANDROID

        string abasePath = $"{Application.dataPath}!assets/{bundleBaseName}";
        Debug.Log(abasePath);

        constraintsAssetBundle = AssetBundle.LoadFromFile($"{abasePath}/constraints");
        roadAssetBundle = AssetBundle.LoadFromFile($"{abasePath}/roads");
        for (int i = 8; i > 0; i /= 2)
            fillerAssetBundles.Add(AssetBundle.LoadFromFile($"{abasePath}/unit{i}"));

#endif
        return 0;
    }

    IEnumerator LoadAssetBundleforWeb(string name, int id)
    {
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(name))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                Debug.Log($"{id} bundle Done");
                if (id == 0) constraintsAssetBundle = bundle;
                else if (id == 1) roadAssetBundle = bundle;
                else fillerAssetBundles.Add(bundle);
            }
        }
    }

    #endregion

    private GameData gameData;
    private AsyncOperation asyncLoadLevel;

    private MapPrepare mapPrepare;
    private MapGenerator mapGenerator;

    private TaskPlanner taskPlanner;
    private GameObject player;

    private UIController uiController;

    private AssetBundle constraintsAssetBundle;
    private AssetBundle roadAssetBundle;
    private List<AssetBundle> fillerAssetBundles;
}
