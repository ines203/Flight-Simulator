
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    new Camera camera;
    [SerializeField]
    Plane plane;
    [SerializeField]
    PlaneHUD planeHUD;
    [SerializeField]
    float mouseSensitivity = 0.025f;
    [SerializeField]
    float fireRate = 15f;
    [SerializeField]
    float bulletSpeed = 400f;
    [SerializeField]
    float bulletDamage = 10f;
    [SerializeField]
    float bulletLifetime = 3f;
    [SerializeField]
    Vector3 muzzleOffset = new Vector3(0, 0, 6f);

    Vector3 controlInput;
    PlaneCamera planeCamera;
    [SerializeField] private launcher missileLauncher;
    Vector2 mouseLook;
    bool firing;
    float nextFireTime;

    void Awake()
    {
        planeCamera = GetComponent<PlaneCamera>();
    }

    void Start()
    {
        if (plane != null)
        {
            SetPlane(plane);
        }
    }

    public void SetPlane(Plane plane)
    {
        this.plane = plane;

        missileLauncher = plane != null
            ? plane.GetComponentInChildren<launcher>()
            : null;

        if (plane != null && planeHUD != null)
        {
            planeHUD.SetPlane(plane);
            planeHUD.SetCamera(camera);
        }

        planeCamera.SetPlane(plane);
    }

    public void OnToggleHelp(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        if (context.phase == InputActionPhase.Performed)
        {
            planeHUD.ToggleHelpDialogs();
        }
    }

    public void SetThrottleInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        plane.SetThrottleInput(context.ReadValue<float>());
    }

    public void OnRollPitchInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        var input = context.ReadValue<Vector2>();
        controlInput = new Vector3(input.y, controlInput.y, -input.x);
    }

    public void OnYawInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        var input = context.ReadValue<float>();
        controlInput = new Vector3(controlInput.x, input, controlInput.z);
    }

    public void OnCameraInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        var input = context.ReadValue<Vector2>();

        if (context.control != null && context.control.device is Mouse)
        {
            mouseLook.x = Mathf.Clamp(
                mouseLook.x + input.x * mouseSensitivity, -1f, 1f
            );
            mouseLook.y = Mathf.Clamp(
                mouseLook.y + input.y * mouseSensitivity, -1f, 1f
            );

            planeCamera.SetInput(mouseLook);
        }
        else
        {
            planeCamera.SetInput(input);
        }
    }

    public void OnToggleFlaps(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        if (context.phase == InputActionPhase.Performed)
        {
            plane.ToggleFlaps();
        }
    }

    public void OnAirbrakeInput(InputAction.CallbackContext context)
    {
        if (plane == null) return;

        plane.SetAirbrakeInput(context.ReadValueAsButton());
    }

    // Tir des missiles avec L3
    public void OnFireMissile(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("L3 détecté !");

            if (missileLauncher != null)
            {
                missileLauncher.FireMissile();
            }
            else
            {
                Debug.LogError("Missile Launcher non assigné !");
            }
        }
    }

    // Commande du canon
    public void OnFireCannon(InputAction.CallbackContext context)
    {
        firing = context.ReadValueAsButton();
    }

    // Création des projectiles du canon
    void FireCannon()
    {
        var planeTransform = plane.transform;
        Vector3 position = planeTransform.TransformPoint(muzzleOffset);
        Vector3 velocity =
            plane.Rigidbody.velocity + planeTransform.forward * bulletSpeed;

        Bullet.Spawn(
            position,
            planeTransform.rotation,
            velocity,
            bulletDamage,
            bulletLifetime,
            planeTransform
        );
    }

    void Update()
    {
        if (plane == null) return;

        plane.SetControlInput(controlInput);

        if (firing &&
            !plane.Dead &&
            plane.Rigidbody != null &&
            Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + 1f / fireRate;
            FireCannon();
        }
    }
}
