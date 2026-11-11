using UnityEngine;
using UnityEngine.InputSystem;

public class DayNightCycle : MonoBehaviour {
    public enum Mode {
        Auto,
        Day,
        Night
    }

    [SerializeField]
    Light sun;
    [SerializeField]
    Mode mode = Mode.Auto;
    [SerializeField]
    float dayDuration = 240f;
    [SerializeField]
    float transitionDuration = 6f;
    [SerializeField]
    [Range(0f, 1f)]
    float startTime = 0.3f;
    [SerializeField]
    float sunYaw = -30f;
    [SerializeField]
    float maxSunIntensity = 1f;
    [SerializeField]
    [Range(0f, 1f)]
    float nightBrightness = 0.8f;
    [SerializeField]
    float dayExposure = 1.3f;
    [SerializeField]
    Color dayColor = new Color(1f, 0.957f, 0.839f);
    [SerializeField]
    Color duskColor = new Color(1f, 0.45f, 0.2f);
    [SerializeField]
    bool showInfo = true;
    [SerializeField]
    Color moonColor = new Color(0.45f, 0.55f, 0.85f);

    float timeOfDay;
    float nextEnvironmentUpdate;
    Material skybox;
    Light moon;

    public float TimeOfDay {
        get { return timeOfDay; }
    }

    public float DayAmount { get; private set; }

    public bool IsNight {
        get { return DayAmount < 0.1f; }
    }

    public Mode CurrentMode {
        get { return mode; }
    }

    void Awake() {
        if (sun == null) {
            sun = GetComponent<Light>();
        }

        var moonObject = new GameObject("Moon");
        moon = moonObject.AddComponent<Light>();
        moon.type = LightType.Directional;
        moon.shadows = LightShadows.None;
        moon.color = moonColor;
        moon.intensity = 0f;

        if (RenderSettings.skybox != null) {
            skybox = new Material(RenderSettings.skybox);
            RenderSettings.skybox = skybox;
        }

        RenderSettings.sun = sun;
        timeOfDay = startTime;
        Apply();
    }

    void OnDestroy() {
        if (moon != null) {
            Destroy(moon.gameObject);
        }
    }

    void Update() {
        ReadInput();
        AdvanceTime(Time.deltaTime);
        Apply();
    }

    void OnGUI() {
        if (!showInfo) return;

        string modeName = mode == Mode.Auto ? "Auto" : mode == Mode.Day ? "Jour" : "Nuit";
        string text = "Cycle : " + modeName + "   Luminosite nuit : " + Mathf.RoundToInt(nightBrightness * 100f) + "%   (N = mode, - et = luminosite)";
        GUI.Label(new Rect(10, 10, 600, 25), text);
    }

    void ReadInput() {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.nKey.wasPressedThisFrame) {
            mode = (Mode)(((int)mode + 1) % 3);
        }

        if (keyboard.equalsKey.wasPressedThisFrame) {
            nightBrightness = Mathf.Clamp01(nightBrightness + 0.2f);
        }

        if (keyboard.minusKey.wasPressedThisFrame) {
            nightBrightness = Mathf.Clamp01(nightBrightness - 0.2f);
        }
    }

    void AdvanceTime(float dt) {
        if (mode == Mode.Auto) {
            if (dayDuration > 0f) {
                timeOfDay = Mathf.Repeat(timeOfDay + dt / dayDuration, 1f);
            }
            return;
        }

        float target = mode == Mode.Day ? 0.5f : 0f;
        float speed = 360f / Mathf.Max(0.1f, transitionDuration);
        float degrees = Mathf.MoveTowardsAngle(timeOfDay * 360f, target * 360f, speed * dt);
        timeOfDay = Mathf.Repeat(degrees / 360f, 1f);
    }

    void Apply() {
        if (sun == null) return;

        float angle = (timeOfDay - 0.25f) * 360f;
        sun.transform.rotation = Quaternion.Euler(angle, sunYaw, 0f);
        moon.transform.rotation = Quaternion.LookRotation(-sun.transform.forward, sun.transform.up);

        float height = Mathf.Sin(angle * Mathf.Deg2Rad);
        DayAmount = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(-0.2f, 0.35f, height));
        float nightAmount = 1f - DayAmount;

        float warmth = Mathf.Clamp01(Mathf.InverseLerp(0f, 0.5f, height));
        sun.color = Color.Lerp(duskColor, dayColor, warmth);
        sun.intensity = maxSunIntensity * DayAmount;

        moon.color = moonColor;
        moon.intensity = Mathf.Lerp(0f, 0.8f, nightBrightness) * nightAmount;

        float nightAmbient = Mathf.Lerp(0.05f, 0.8f, nightBrightness);
        RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbient, 1f, DayAmount);

        if (skybox != null && skybox.HasProperty("_Exposure")) {
            float nightExposure = Mathf.Lerp(0.05f, 0.9f, nightBrightness);
            skybox.SetFloat("_Exposure", Mathf.Lerp(nightExposure, dayExposure, DayAmount));
        }

        if (Time.time >= nextEnvironmentUpdate) {
            nextEnvironmentUpdate = Time.time + 0.5f;
            DynamicGI.UpdateEnvironment();
        }
    }
}
