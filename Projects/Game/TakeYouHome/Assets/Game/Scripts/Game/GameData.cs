using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameStatus
{
    Uninitiate,
    Standby,
    Run,
    Pause,
    Over
}

/// <summary>
/// Store gaming data and settings in a ScriptableObject.
/// </summary>
public class GameData : ScriptableObject
{
    #region Playing Status

    public GameStatus GameStatus { get; set; } = GameStatus.Uninitiate;

    public float LoadingPercentage
    {
        get => loadingPercentage;
        set
        {
            Slider slider = FindObjectOfType<Slider>();
            if (slider)
            {
                slider.value = loadingPercentage;
                Debug.Log($"> LOADING {100 * loadingPercentage}%...");
            }
        }
    }

    private float loadingPercentage = 0;

    public int Level { get; set; }

    public string LevelName { get; set; } = "SumCity";

    #endregion

    #region  Global Setting

    #endregion

}
