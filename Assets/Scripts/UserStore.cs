using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class UserData {
    public string name;
    public int bestScore;
}

[Serializable]
public class UserList {
    public List<UserData> users = new List<UserData>();
}

public static class UserStore {
    public const int MaxNameLength = 20;
    public const string FileName = "users.json";

    static UserList data;

    public static string FilePath {
        get {
            return Path.Combine(Application.persistentDataPath, FileName);
        }
    }

    public static IReadOnlyList<UserData> Users {
        get {
            Load();
            return data.users.AsReadOnly();
        }
    }

    public static void ReloadFromDisk() {
        data = null;
    }

    public static string Normalize(string rawName) {
        if (string.IsNullOrEmpty(rawName)) {
            return "";
        }

        return Regex.Replace(rawName.Trim(), @"\s+", " ");
    }

    public static UserData Find(string name) {
        Load();
        string normalized = Normalize(name);

        foreach (var user in data.users) {
            if (string.Equals(user.name, normalized, StringComparison.OrdinalIgnoreCase)) {
                return user;
            }
        }

        return null;
    }

    public static bool TryCreate(string rawName, out string message) {
        string name = Normalize(rawName);

        if (name.Length == 0) {
            message = "Le nom est vide.";
            return false;
        }

        if (name.Length > MaxNameLength) {
            message = "Le nom est trop long (" + MaxNameLength + " caracteres au maximum).";
            return false;
        }

        foreach (char c in name) {
            if (!(char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')) {
                message = "Le nom peut contenir seulement des lettres, des chiffres, des espaces, - et _.";
                return false;
            }
        }

        if (Find(name) != null) {
            message = "Ce nom existe deja.";
            return false;
        }

        data.users.Add(new UserData { name = name, bestScore = 0 });

        string error;
        if (!Save(out error)) {
            data.users.RemoveAt(data.users.Count - 1);
            message = error;
            return false;
        }

        message = name;
        return true;
    }

    public static bool UpdateBestScore(string name, int score) {
        var user = Find(name);

        if (user == null || score <= user.bestScore) {
            return false;
        }

        user.bestScore = score;
        string error;
        return Save(out error);
    }

    static void Load() {
        if (data != null) {
            return;
        }

        data = new UserList();
        string path = FilePath;

        if (!File.Exists(path)) {
            return;
        }

        try {
            var loaded = JsonUtility.FromJson<UserList>(File.ReadAllText(path));

            if (loaded != null && loaded.users != null) {
                loaded.users.RemoveAll(u => u == null || string.IsNullOrWhiteSpace(u.name));
                data = loaded;
            }
        } catch (Exception) {
            try {
                File.Copy(path, path + ".bad", true);
            } catch (Exception) {
            }

            Debug.LogWarning("Fichier des utilisateurs illisible, une copie a ete gardee dans " + path + ".bad");
        }
    }

    static bool Save(out string error) {
        error = null;

        try {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonUtility.ToJson(data, true));
            return true;
        } catch (Exception e) {
            error = "Impossible d'enregistrer les utilisateurs : " + e.Message;
            return false;
        }
    }
}
