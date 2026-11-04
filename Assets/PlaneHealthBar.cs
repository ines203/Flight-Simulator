using UnityEngine;
using UnityEngine.UI;

public class PlaneHealthBar : MonoBehaviour
{
    [SerializeField] private Bar healthBar;
    [SerializeField] private Text healthText;

    private Plane plane;

    public void SetPlane(Plane newPlane)
    {
        plane = newPlane;
    }

    void Update()
    {
        if (plane == null) return;

        float percentage = plane.MaxHealth > 0
            ? plane.Health / plane.MaxHealth
            : 0f;

        healthBar.SetValue(percentage);

        if (healthText != null)
        {
            healthText.text = $"{plane.Health:0} / {plane.MaxHealth:0} HP";
        }
    }
}