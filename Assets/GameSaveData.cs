using UnityEngine;

public static class GameSaveData
{
    private const string WoodKey = "TreeIdle.Wood";
    private const string GoldKey = "TreeIdle.Gold";

    public static int Wood => PlayerPrefs.GetInt(WoodKey, 0);
    public static int Gold => PlayerPrefs.GetInt(GoldKey, 0);

    public static void SaveWood(int amount)
    {
        PlayerPrefs.SetInt(WoodKey, Mathf.Max(0, amount));
        PlayerPrefs.Save();
    }

    public static void SaveGold(int amount)
    {
        PlayerPrefs.SetInt(GoldKey, Mathf.Max(0, amount));
        PlayerPrefs.Save();
    }

    public static void SaveResources(int wood, int gold)
    {
        PlayerPrefs.SetInt(WoodKey, Mathf.Max(0, wood));
        PlayerPrefs.SetInt(GoldKey, Mathf.Max(0, gold));
        PlayerPrefs.Save();
    }

    public static void ResetResources()
    {
        PlayerPrefs.DeleteKey(WoodKey);
        PlayerPrefs.DeleteKey(GoldKey);
        PlayerPrefs.Save();
    }

    public static void QuitApplication()
    {
        PlayerPrefs.Save();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
