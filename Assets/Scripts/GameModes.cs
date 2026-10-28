using UnityEngine;

/// <summary>
/// Constantes partagées pour les modes de jeu.
///
/// Le menu principal (MainMenu.cs) écrit le mode choisi dans PlayerPrefs
/// sous la clé "GameMode". Les scripts de la scène Main le lisent ici,
/// ce qui évite de retaper la chaîne à plusieurs endroits et de se tromper
/// d'orthographe ("Combat" vs "CombatAerien", par exemple).
/// </summary>
public static class GameModes {
    // Clé PlayerPrefs utilisée par MainMenu.cs. Ne pas la changer sans
    // mettre le menu à jour en même temps.
    public const string Key = "GameMode";

    // Valeurs écrites par le menu. Elles doivent rester identiques aux
    // chaînes de MainMenu.StartVolLibre() et MainMenu.StartVoyage().
    public const string VolLibre = "VolLibre";
    public const string Voyage = "Voyage";

    // Pas encore écrit par le menu : le bouton "Combat Aerien" affiche
    // "Coming soon" pour l'instant. La personne qui rendra ce mode jouable
    // devra appeler GameModes.Set(GameModes.CombatAerien).
    public const string CombatAerien = "CombatAerien";

    /// <summary>
    /// Mode de jeu courant.
    ///
    /// PlayerPrefs est sauvegardé sur le disque, donc la valeur peut être
    /// absente (le menu n'a jamais été lancé sur cette machine) ou vide.
    /// Dans ces deux cas on retourne VolLibre, ce qui correspond à ce qu'on
    /// obtient en appuyant sur Play directement dans la scène Main.
    /// </summary>
    public static string Current {
        get {
            string mode = PlayerPrefs.GetString(Key, VolLibre);
            return string.IsNullOrEmpty(mode) ? VolLibre : mode;
        }
    }

    /// <summary>
    /// Enregistre le mode de jeu. À utiliser par le menu à la place de
    /// PlayerPrefs.SetString("GameMode", "...").
    /// </summary>
    public static void Set(string mode) {
        PlayerPrefs.SetString(Key, mode);
    }

    /// <summary>
    /// Vrai si la chaîne correspond à un mode connu. Utile pour afficher un
    /// avertissement clair quand PlayerPrefs contient une valeur inattendue.
    /// </summary>
    public static bool IsKnown(string mode) {
        return mode == VolLibre || mode == Voyage || mode == CombatAerien;
    }
}
