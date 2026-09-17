using UnityEngine;

public interface ISaveService
{
    int GetInt(string key, int defaultValue = 0);
    void SetInt(string key, int value);
    bool GetBool(string key, bool defaultValue = false);
    void SetBool(string key, bool value);
    void Save();
    void DeleteAll();
}

public class SaveService : ISaveService
{
    public int GetInt(string key, int defaultValue = 0)
        => PlayerPrefs.GetInt(key, defaultValue);

    public void SetInt(string key, int value)
        => PlayerPrefs.SetInt(key, value);

    public bool GetBool(string key, bool defaultValue = false)
        => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

    public void SetBool(string key, bool value)
        => PlayerPrefs.SetInt(key, value ? 1 : 0);

    public void Save() => PlayerPrefs.Save();
    public void DeleteAll() => PlayerPrefs.DeleteAll();
}