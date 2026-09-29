using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
    [SerializeField]
    new Camera camera;
    [SerializeField]
    Plane plane;
    [SerializeField]
    PlaneHUD planeHUD;
    [SerializeField]
    float mouseSensitivity = 0.025f;

    Vector3 controlInput;
    PlaneCamera planeCamera;
    Vector2 mouseLook;

    void Awake() {
        planeCamera = GetComponent<PlaneCamera>();
    }

    void Start() {
        if (plane != null) {
            SetPlane(plane);
        }
    }

    public void SetPlane(Plane plane) {
        this.plane = plane;

        if (plane != null && planeHUD != null) {
            planeHUD.SetPlane(plane);
            planeHUD.SetCamera(camera);
        }

        planeCamera.SetPlane(plane);
    }

    public void OnToggleHelp(InputAction.CallbackContext context) {
        if (plane == null) return;

        if (context.phase == InputActionPhase.Performed) {
            planeHUD.ToggleHelpDialogs();
        }
    }

    public void SetThrottleInput(InputAction.CallbackContext context) {
        if (plane == null) return;
        //if (aiController.enabled) return;

        plane.SetThrottleInput(context.ReadValue<float>());
    }

    public void OnRollPitchInput(InputAction.CallbackContext context) {
        if (plane == null) return;

        var input = context.ReadValue<Vector2>();
        controlInput = new Vector3(input.y, controlInput.y, -input.x);
    }

    public void OnYawInput(InputAction.CallbackContext context) {
        if (plane == null) return;

        var input = context.ReadValue<float>();
        controlInput = new Vector3(controlInput.x, input, controlInput.z);
    }

    public void OnCameraInput(InputAction.CallbackContext context) {
        if (plane == null) return;

        var input = context.ReadValue<Vector2>();

        if (context.control != null && context.control.device is Mouse) {
            mouseLook.x = Mathf.Clamp(mouseLook.x + input.x * mouseSensitivity, -1f, 1f);
            mouseLook.y = Mathf.Clamp(mouseLook.y + input.y * mouseSensitivity, -1f, 1f);
            planeCamera.SetInput(mouseLook);
        } else {
            planeCamera.SetInput(input);
        }
    }

    public void OnToggleFlaps(InputAction.CallbackContext context) {
        if (plane == null) return;

        if (context.phase == InputActionPhase.Performed) {
            plane.ToggleFlaps();
        }
    }

    public void OnAirbrakeInput(InputAction.CallbackContext context) {
        if (plane == null) return;

        plane.SetAirbrakeInput(context.ReadValueAsButton());
    }

    void Update() {
        if (plane == null) return;

        plane.SetControlInput(controlInput);
    }
}
