using UnityEngine;

/// <summary>
/// Comportement propre à une montgolfière ennemie.
///
/// Le composant Health (sur le même objet) gère les points de vie et
/// reçoit les dégâts des balles. Balloon s'occupe du reste :
///  - être détruite quand l'avion la percute ;
///  - jouer un effet de destruction et retirer l'objet de la scène.
///
/// Balloon ne connaît pas le spawner : une montgolfière glissée à la main
/// dans la scène fonctionne seule. C'est BalloonSpawner qui, de son côté,
/// s'abonne au On Death du Health pour compter les survivantes.
///
/// Un Rigidbody cinématique est requis sur la racine : sans lui, Unity
/// envoie OnCollisionEnter au GameObject enfant qui porte le collider
/// (l'enveloppe ou la nacelle), et ce script ne le recevrait jamais.
/// Avec lui, tous les colliders enfants forment un seul corps et le
/// message arrive ici. Cinématique = aucune physique appliquée : la
/// montgolfière reste parfaitement immobile.
/// </summary>
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody))]
public class Balloon : MonoBehaviour {
    [SerializeField]
    [Tooltip("Si vrai, la montgolfière est détruite quand l'avion la percute.")]
    bool destroyedByPlaneCollision = true;

    [SerializeField]
    [Tooltip("Prefab instancié à la position de la montgolfière à sa mort (ex. : Crash). Optionnel.")]
    GameObject deathEffectPrefab;

    [SerializeField]
    [Tooltip("Durée de vie de l'effet de destruction, en secondes.")]
    float deathEffectDuration = 4f;

    Health health;

    public Health Health { get { return health; } }

    // Appelé par l'éditeur quand le composant est ajouté, et via le menu
    // Reset de l'Inspector : règle le Rigidbody une fois pour toutes.
    void Reset() {
        ConfigureRigidbody();
    }

    void Awake() {
        ConfigureRigidbody();

        health = GetComponent<Health>();
        health.OnDeath.AddListener(OnDeath);
    }

    void OnDestroy() {
        // Se désabonner évite un appel vers un objet détruit si Health
        // survivait à Balloon (pas le cas ici, mais c'est la bonne habitude).
        if (health != null) {
            health.OnDeath.RemoveListener(OnDeath);
        }
    }

    void ConfigureRigidbody() {
        var rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void OnCollisionEnter(Collision collision) {
        if (!destroyedByPlaneCollision) return;
        if (health == null || health.IsDead) return;

        // collision.collider est le collider de l'AUTRE objet. L'avion a
        // ses colliders sur des enfants (fuselage, trains), donc on remonte
        // vers le parent pour trouver le script Plane.
        if (collision.collider.GetComponentInParent<Plane>() != null) {
            health.Kill();
        }
    }

    // Branché sur Health.OnDeath dans Awake. Peut aussi être déclenché par
    // les balles (vie à 0) : le chemin est le même dans les deux cas.
    void OnDeath() {
        if (deathEffectPrefab != null) {
            var effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, Mathf.Max(0f, deathEffectDuration));
        }

        // On cache et on désactive tout de suite, avant la suppression
        // réelle à la fin de la frame : plus aucune balle ni collision ne
        // peut toucher une montgolfière déjà morte.
        foreach (var r in GetComponentsInChildren<Renderer>()) {
            r.enabled = false;
        }

        foreach (var c in GetComponentsInChildren<Collider>()) {
            c.enabled = false;
        }

        Destroy(gameObject);
    }
}
