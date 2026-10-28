using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Composant de vie générique, réutilisable par n'importe quel objet
/// destructible (montgolfière, futur ennemi, bâtiment...).
///
/// Il implémente IDamageable : Bullet.cs cherche ce type avec
/// GetComponentInParent sur le collider touché et appelle ApplyDamage.
/// Le composant peut donc être sur le parent, et les colliders sur les
/// enfants (ballon, nacelle), sans rien changer aux balles.
///
/// Health ne détruit jamais le GameObject lui-même : il déclenche
/// l'événement On Death une seule fois, et c'est l'objet qui écoute
/// (Balloon.cs, par exemple) qui décide quoi faire.
/// </summary>
public class Health : MonoBehaviour, IDamageable {
    [SerializeField]
    [Tooltip("Vie maximale. Forcée à 1 au minimum par OnValidate.")]
    float maxHealth = 100f;

    [SerializeField]
    [Tooltip("Vie actuelle. Remise à Max Health au démarrage ; affichée ici pour le débogage.")]
    float currentHealth;

    [SerializeField]
    [Tooltip("Appelé une seule fois quand la vie tombe à 0.")]
    UnityEvent onDeath = new UnityEvent();

    public float MaxHealth { get { return maxHealth; } }
    public float CurrentHealth { get { return currentHealth; } }
    public bool IsDead { get; private set; }

    /// <summary>
    /// Permet à un autre script de s'abonner par code
    /// (ex. : health.OnDeath.AddListener(MaMethode)).
    /// </summary>
    public UnityEvent OnDeath { get { return onDeath; } }

    void Awake() {
        currentHealth = maxHealth;
        IsDead = false;
    }

    // Appelé par l'éditeur chaque fois qu'une valeur change dans l'Inspector.
    // On empêche ici une vie max à 0 ou négative, qui rendrait l'objet
    // mort dès sa création ou impossible à détruire proprement.
    void OnValidate() {
        if (maxHealth < 1f) {
            maxHealth = 1f;
        }
    }

    /// <summary>
    /// Retire des points de vie. Méthode de l'interface IDamageable.
    /// Ignorée si l'objet est déjà mort (tir continu sur une cible
    /// détruite) ou si la valeur n'est pas positive.
    /// </summary>
    public void ApplyDamage(float damage) {
        if (IsDead) return;
        if (damage <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (currentHealth <= 0f) {
            Die();
        }
    }

    /// <summary>
    /// Tue l'objet immédiatement, peu importe sa vie restante.
    /// Utilisé par la montgolfière quand l'avion la percute.
    /// </summary>
    public void Kill() {
        if (IsDead) return;

        currentHealth = 0f;
        Die();
    }

    void Die() {
        IsDead = true;
        onDeath.Invoke();
    }
}
