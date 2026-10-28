using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Place les montgolfières ennemies sur la carte au démarrage de la scène.
///
/// Fonctionnement :
///  1. Lit le mode de jeu (GameModes.Current) et vérifie qu'il autorise
///     les montgolfières (cases Spawn In ... de l'Inspector).
///  2. Tire des positions au hasard avec une graine fixe (System.Random),
///     soit dans un anneau autour de l'Exclusion Center, soit sur tout le
///     terrain. Les positions sont toujours bornées aux limites du terrain
///     moins Edge Margin, et jamais à moins de Min Radius du centre.
///  3. Calcule la hauteur : point le plus haut du relief autour de la
///     position (pour éviter les pentes) + y du terrain + hauteur réglable.
///  4. Instancie le prefab et s'abonne au Health.OnDeath de chaque
///     montgolfière pour tenir AliveCount à jour. La montgolfière, elle,
///     ne connaît pas le spawner.
///
/// On utilise une instance de System.Random plutôt que
/// UnityEngine.Random.InitState pour ne pas changer la graine globale
/// de tout le projet.
/// </summary>
public class BalloonSpawner : MonoBehaviour {
    [Header("Prefab")]
    [SerializeField]
    [Tooltip("Prefab de montgolfière (doit porter Balloon et Health).")]
    GameObject balloonPrefab;

    [Header("Modes de jeu")]
    [SerializeField]
    bool spawnInVolLibre = true;
    [SerializeField]
    bool spawnInCombat = true;
    [SerializeField]
    bool spawnInVoyage = false;

    [Header("Quantité et hasard")]
    [SerializeField]
    [Tooltip("Nombre de montgolfières à placer. 0 = aucune.")]
    int count = 30;
    [SerializeField]
    [Tooltip("Graine du générateur aléatoire. Même graine = mêmes positions.")]
    int seed = 12345;
    [SerializeField]
    [Tooltip("Si coché, une graine différente à chaque Play (affichée dans la Console).")]
    bool randomizeSeed = false;

    [Header("Terrain")]
    [SerializeField]
    [Tooltip("Terrain de la scène. Si vide, Terrain.activeTerrain est utilisé.")]
    Terrain terrain;
    [SerializeField]
    [Tooltip("Distance minimale aux bords du terrain, en mètres.")]
    float edgeMargin = 1000f;

    [Header("Zone de placement")]
    [SerializeField]
    [Tooltip("Si coché, tout le terrain est utilisé au lieu de l'anneau. Min Radius reste la zone d'exclusion.")]
    bool useWholeTerrain = false;
    [SerializeField]
    [Tooltip("Centre de l'anneau et de la zone d'exclusion (glisser SpawnGround). Si vide : centre du terrain.")]
    Transform exclusionCenter;
    [SerializeField]
    [Tooltip("Aucune montgolfière à moins de cette distance du centre, en mètres.")]
    float minRadius = 2000f;
    [SerializeField]
    [Tooltip("Rayon extérieur de l'anneau, en mètres (ignoré si Use Whole Terrain).")]
    float maxRadius = 12000f;
    [SerializeField]
    [Tooltip("Distance minimale entre deux montgolfières, en mètres.")]
    float minDistanceBetweenBalloons = 300f;
    [SerializeField]
    [Tooltip("Essais de position par montgolfière avant d'abandonner celle-ci.")]
    int maxAttemptsPerBalloon = 30;

    [Header("Hauteur")]
    [SerializeField]
    [Tooltip("Hauteur au-dessus du point le plus haut du relief autour, en mètres.")]
    float heightAboveGround = 150f;
    [SerializeField]
    [Tooltip("Hauteur supplémentaire aléatoire entre 0 et cette valeur, en mètres.")]
    float heightVariation = 100f;
    [SerializeField]
    [Tooltip("Rayon, en mètres, dans lequel on cherche le point le plus haut du relief (marge contre les pentes).")]
    float slopeSampleRadius = 60f;

    [Header("Débogage")]
    [SerializeField]
    [Tooltip("Si rempli, remplace la lecture de PlayerPrefs (ex. : Voyage). Laisser vide en temps normal.")]
    string forceGameMode = "";
    [SerializeField]
    [Tooltip("Affiche « Montgolfieres : vivantes / total » en haut à gauche, sous le texte du cycle jour/nuit.")]
    bool showInfo = true;

    bool spawnAllowed;

    [Header("Événements")]
    [SerializeField]
    [Tooltip("Appelé quand la dernière montgolfière vivante est détruite.")]
    UnityEvent onAllBalloonsDestroyed = new UnityEvent();

    readonly List<Vector3> placedPositions = new List<Vector3>();

    /// <summary>Nombre de montgolfières créées au démarrage.</summary>
    public int TotalSpawned { get; private set; }

    /// <summary>Nombre de montgolfières encore en vie.</summary>
    public int AliveCount { get; private set; }

    public UnityEvent OnAllBalloonsDestroyed { get { return onAllBalloonsDestroyed; } }

    // Garde-fous appliqués dans l'éditeur dès qu'une valeur change.
    void OnValidate() {
        count = Mathf.Max(0, count);
        edgeMargin = Mathf.Max(0f, edgeMargin);
        minRadius = Mathf.Max(0f, minRadius);
        maxRadius = Mathf.Max(minRadius, maxRadius);
        minDistanceBetweenBalloons = Mathf.Max(0f, minDistanceBetweenBalloons);
        maxAttemptsPerBalloon = Mathf.Max(1, maxAttemptsPerBalloon);
        heightVariation = Mathf.Max(0f, heightVariation);
        slopeSampleRadius = Mathf.Max(0f, slopeSampleRadius);
    }

    void Start() {
        spawnAllowed = IsSpawnAllowedForCurrentMode();
        if (!spawnAllowed) return;

        if (balloonPrefab == null) {
            Debug.LogWarning("[BalloonSpawner] Aucun prefab assigné : aucune montgolfière créée.", this);
            return;
        }

        if (terrain == null) {
            terrain = Terrain.activeTerrain;
        }

        if (terrain == null) {
            Debug.LogWarning("[BalloonSpawner] Aucun terrain trouvé : aucune montgolfière créée.", this);
            return;
        }

        if (count == 0) {
            Debug.Log("[BalloonSpawner] Count = 0 : aucune montgolfière créée.", this);
            return;
        }

        int usedSeed = randomizeSeed ? System.Environment.TickCount : seed;
        var random = new System.Random(usedSeed);

        SpawnAll(random);

        Debug.Log("[BalloonSpawner] " + TotalSpawned + " montgolfière(s) créée(s) sur " + count + " demandée(s), graine " + usedSeed + ".", this);
    }

    // Même style que DayNightCycle.OnGUI : un texte simple, sans Canvas.
    void OnGUI() {
        if (!showInfo) return;

        string text = spawnAllowed
            ? "Montgolfieres : " + AliveCount + " / " + TotalSpawned
            : "Montgolfieres : desactivees dans ce mode";

        GUI.Label(new Rect(10, 35, 400, 25), text);
    }

    // ----- Mode de jeu -------------------------------------------------

    bool IsSpawnAllowedForCurrentMode() {
        string mode = string.IsNullOrEmpty(forceGameMode) ? GameModes.Current : forceGameMode;

        if (!GameModes.IsKnown(mode)) {
            Debug.LogWarning("[BalloonSpawner] Mode de jeu inconnu « " + mode + " » : aucune montgolfière créée.", this);
            return false;
        }

        bool allowed =
            (mode == GameModes.VolLibre && spawnInVolLibre) ||
            (mode == GameModes.CombatAerien && spawnInCombat) ||
            (mode == GameModes.Voyage && spawnInVoyage);

        if (!allowed) {
            Debug.Log("[BalloonSpawner] Mode « " + mode + " » : montgolfières désactivées.", this);
        }

        return allowed;
    }

    // ----- Placement ---------------------------------------------------

    void SpawnAll(System.Random random) {
        placedPositions.Clear();
        TotalSpawned = 0;
        AliveCount = 0;

        for (int i = 0; i < count; i++) {
            Vector3 position;

            if (!TryFindPosition(random, out position)) {
                Debug.LogWarning("[BalloonSpawner] Montgolfière " + (i + 1) + " : aucune position valide après " + maxAttemptsPerBalloon + " essais (zone trop petite ou trop remplie).", this);
                continue;
            }

            SpawnOne(position, i);
        }
    }

    /// <summary>
    /// Tire des positions au hasard jusqu'à en trouver une qui respecte
    /// toutes les contraintes, ou abandonne après Max Attempts Per Balloon.
    /// </summary>
    bool TryFindPosition(System.Random random, out Vector3 position) {
        Rect bounds = GetUsableBounds(terrain);
        Vector2 center = GetCenterXZ(bounds);

        for (int attempt = 0; attempt < maxAttemptsPerBalloon; attempt++) {
            Vector2 xz = useWholeTerrain
                ? RandomPointInRect(random, bounds)
                : RandomPointInRing(random, center, minRadius, maxRadius);

            // Toujours à l'intérieur du terrain moins la marge.
            if (!bounds.Contains(xz)) continue;

            // Jamais dans la zone d'exclusion (utile surtout en mode terrain entier).
            if (Vector2.Distance(xz, center) < minRadius) continue;

            // Pas trop près d'une montgolfière déjà placée.
            if (IsTooCloseToOthers(xz)) continue;

            float y = ComputeHeight(xz, random);
            position = new Vector3(xz.x, y, xz.y);
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    void SpawnOne(Vector3 position, int index) {
        var instance = Instantiate(balloonPrefab, position, Quaternion.identity, transform);
        instance.name = "Balloon (" + (index + 1) + ")";

        placedPositions.Add(position);
        TotalSpawned++;
        AliveCount++;

        // C'est ici que le spawner se branche sur la montgolfière, et non
        // l'inverse : Balloon.cs n'a aucune référence vers ce script.
        var health = instance.GetComponent<Health>();
        if (health != null) {
            health.OnDeath.AddListener(OnBalloonDied);
        } else {
            Debug.LogWarning("[BalloonSpawner] Le prefab n'a pas de composant Health : le compteur ne sera pas mis à jour.", this);
        }
    }

    void OnBalloonDied() {
        AliveCount = Mathf.Max(0, AliveCount - 1);

        if (AliveCount == 0 && TotalSpawned > 0) {
            Debug.Log("[BalloonSpawner] Toutes les montgolfières sont détruites.", this);
            onAllBalloonsDestroyed.Invoke();
        }
    }

    // ----- Géométrie ---------------------------------------------------

    /// <summary>Rectangle (x, z) utilisable : terrain moins Edge Margin.</summary>
    Rect GetUsableBounds(Terrain t) {
        Vector3 origin = t.transform.position;
        Vector3 size = t.terrainData.size;

        float width = Mathf.Max(0f, size.x - 2f * edgeMargin);
        float depth = Mathf.Max(0f, size.z - 2f * edgeMargin);

        return new Rect(origin.x + edgeMargin, origin.z + edgeMargin, width, depth);
    }

    /// <summary>Centre (x, z) de l'anneau : l'Exclusion Center, sinon le centre du terrain.</summary>
    Vector2 GetCenterXZ(Rect bounds) {
        if (exclusionCenter != null) {
            return new Vector2(exclusionCenter.position.x, exclusionCenter.position.z);
        }

        return bounds.center;
    }

    static Vector2 RandomPointInRect(System.Random random, Rect rect) {
        float x = rect.xMin + (float)random.NextDouble() * rect.width;
        float z = rect.yMin + (float)random.NextDouble() * rect.height;
        return new Vector2(x, z);
    }

    /// <summary>
    /// Point au hasard dans un anneau. Le rayon est tiré avec une racine
    /// carrée pour que la répartition soit uniforme en surface : sinon les
    /// montgolfières s'entassent près du centre.
    /// </summary>
    static Vector2 RandomPointInRing(System.Random random, Vector2 center, float innerRadius, float outerRadius) {
        float angle = (float)random.NextDouble() * Mathf.PI * 2f;
        float inner2 = innerRadius * innerRadius;
        float outer2 = outerRadius * outerRadius;
        float radius = Mathf.Sqrt(Mathf.Lerp(inner2, outer2, (float)random.NextDouble()));

        return center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    bool IsTooCloseToOthers(Vector2 xz) {
        float min2 = minDistanceBetweenBalloons * minDistanceBetweenBalloons;

        foreach (var placed in placedPositions) {
            float dx = placed.x - xz.x;
            float dz = placed.z - xz.y;

            if (dx * dx + dz * dz < min2) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Hauteur finale = relief le plus haut autour + y du terrain + hauteur
    /// réglable + variation aléatoire.
    /// </summary>
    float ComputeHeight(Vector2 xz, System.Random random) {
        float ground = SampleHighestGround(xz);
        float variation = (float)random.NextDouble() * heightVariation;

        return ground + heightAboveGround + variation;
    }

    /// <summary>
    /// Terrain.SampleHeight renvoie la hauteur relative au terrain, d'où
    /// l'ajout de terrain.transform.position.y. On échantillonne le centre
    /// et quatre points autour pour prendre le plus haut : sur une pente,
    /// la montgolfière reste au-dessus du côté qui monte.
    /// </summary>
    float SampleHighestGround(Vector2 xz) {
        float terrainY = terrain.transform.position.y;
        float r = slopeSampleRadius;

        float highest = terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y));
        highest = Mathf.Max(highest, terrain.SampleHeight(new Vector3(xz.x + r, 0f, xz.y)));
        highest = Mathf.Max(highest, terrain.SampleHeight(new Vector3(xz.x - r, 0f, xz.y)));
        highest = Mathf.Max(highest, terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y + r)));
        highest = Mathf.Max(highest, terrain.SampleHeight(new Vector3(xz.x, 0f, xz.y - r)));

        return highest + terrainY;
    }

    // ----- Gizmos (vue Scene, objet sélectionné) -----------------------

    void OnDrawGizmosSelected() {
        Terrain t = terrain != null ? terrain : Terrain.activeTerrain;
        if (t == null) return;

        Rect bounds = GetUsableBounds(t);
        Vector2 center = GetCenterXZ(bounds);
        float y = t.transform.position.y + heightAboveGround;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(bounds.center.x, y, bounds.center.y), new Vector3(bounds.width, 1f, bounds.height));

        Gizmos.color = Color.red;
        DrawCircle(center, y, minRadius);

        if (!useWholeTerrain) {
            Gizmos.color = Color.yellow;
            DrawCircle(center, y, maxRadius);
        }
    }

    static void DrawCircle(Vector2 center, float y, float radius) {
        const int segments = 64;
        Vector3 previous = new Vector3(center.x + radius, y, center.y);

        for (int i = 1; i <= segments; i++) {
            float angle = i / (float)segments * Mathf.PI * 2f;
            Vector3 next = new Vector3(center.x + Mathf.Cos(angle) * radius, y, center.y + Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }
}
