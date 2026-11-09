using UnityEngine;

public static class UserSession {
    const string Key = "CurrentUser";

    public static string CurrentUser {
        get {
            return PlayerPrefs.GetString(Key, "");
        }
    }

    public static bool HasUser {
        get {
            return !string.IsNullOrEmpty(CurrentUser) && UserStore.Find(CurrentUser) != null;
        }
    }

    public static void Select(string name) {
        PlayerPrefs.SetString(Key, name);
        PlayerPrefs.Save();
    }

    public static void Clear() {
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();
    }
}
