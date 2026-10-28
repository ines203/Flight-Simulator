using UnityEngine;

public class Bullet : MonoBehaviour {
    static Material sharedMaterial;

    Vector3 velocity;
    float damage;
    float timeLeft;
    Transform owner;

    public static Bullet Spawn(Vector3 position, Quaternion rotation, Vector3 velocity, float damage, float lifetime, Transform owner) {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Bullet";
        Destroy(go.GetComponent<Collider>());

        go.transform.SetPositionAndRotation(position, rotation);
        go.transform.localScale = new Vector3(0.4f, 0.4f, 8f);

        if (sharedMaterial == null) {
            sharedMaterial = new Material(Shader.Find("Sprites/Default"));
            sharedMaterial.color = new Color(1f, 0.95f, 0.3f);
        }

        var meshRenderer = go.GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = sharedMaterial;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        var bullet = go.AddComponent<Bullet>();
        bullet.velocity = velocity;
        bullet.damage = damage;
        bullet.timeLeft = lifetime;
        bullet.owner = owner;
        return bullet;
    }

    void Update() {
        float dt = Time.deltaTime;
        timeLeft -= dt;

        if (timeLeft <= 0) {
            Destroy(gameObject);
            return;
        }

        Vector3 step = velocity * dt;
        float distance = step.magnitude;

        if (distance > 0 && TryGetHit(step / distance, distance, out RaycastHit hit)) {
            var target = hit.collider.GetComponentInParent<IDamageable>();

            if (target != null) {
                target.ApplyDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        transform.position += step;
    }

    bool TryGetHit(Vector3 direction, float distance, out RaycastHit closest) {
        var hits = Physics.RaycastAll(transform.position, direction, distance, ~0, QueryTriggerInteraction.Ignore);
        bool found = false;
        closest = default;
        float best = float.MaxValue;

        foreach (var hit in hits) {
            if (owner != null && hit.collider.transform.IsChildOf(owner)) continue;

            if (hit.distance < best) {
                best = hit.distance;
                closest = hit;
                found = true;
            }
        }

        return found;
    }
}
