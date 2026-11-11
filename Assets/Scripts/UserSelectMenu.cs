using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UserSelectMenu : MonoBehaviour {
    [SerializeField]
    string nextSceneName = "MainMenu";

    static readonly Color backgroundColor = new Color(0.07f, 0.09f, 0.13f, 1f);
    static readonly Color panelColor = new Color(0.12f, 0.15f, 0.21f, 1f);
    static readonly Color buttonColor = new Color(0.22f, 0.3f, 0.45f, 1f);
    static readonly Color selectedColor = new Color(0.2f, 0.55f, 0.35f, 1f);
    static readonly Color errorColor = new Color(1f, 0.45f, 0.4f, 1f);
    static readonly Color okColor = new Color(0.5f, 0.9f, 0.55f, 1f);
    static readonly Color textColor = new Color(0.95f, 0.95f, 0.95f, 1f);

    Font font;
    RectTransform listContent;
    InputField nameField;
    Text messageText;
    Text selectedText;
    Button continueButton;
    string selectedName;

    readonly List<Image> rowImages = new List<Image>();
    readonly List<string> rowNames = new List<string>();

    void Awake() {
        Time.timeScale = 1f;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        EnsureEventSystem();
        BuildUi();
        RefreshList();

        string last = UserSession.CurrentUser;

        if (!string.IsNullOrEmpty(last) && UserStore.Find(last) != null) {
            Select(UserStore.Find(last).name);
        } else {
            UpdateSelectionDisplay();
        }
    }

    void Update() {
        var keyboard = Keyboard.current;

        if (keyboard == null || nameField == null || !nameField.isFocused) {
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) {
            OnCreate();
        }
    }

    void EnsureEventSystem() {
        if (FindObjectOfType<EventSystem>() != null) {
            return;
        }

        var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_LEGACY_INPUT_MANAGER
        go.AddComponent<StandaloneInputModule>();
#else
        var module = go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        module.AssignDefaultActions();
#endif
    }

    RectTransform NewRect(string objectName, Transform parent) {
        var go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    void Place(RectTransform rect, float xMin, float yMin, float xMax, float yMax) {
        rect.anchorMin = new Vector2(xMin, yMin);
        rect.anchorMax = new Vector2(xMax, yMax);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    Image NewPanel(string objectName, Transform parent, Color color) {
        var rect = NewRect(objectName, parent);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    Text NewText(string objectName, Transform parent, string content, int size, TextAnchor anchor, Color color) {
        var rect = NewRect(objectName, parent);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = content;
        text.fontSize = size;
        text.alignment = anchor;
        text.color = color;
        text.supportRichText = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    Button NewButton(string objectName, Transform parent, string label, int size) {
        var image = NewPanel(objectName, parent, buttonColor);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var text = NewText("Label", image.transform, label, size, TextAnchor.MiddleCenter, textColor);
        Place(text.rectTransform, 0f, 0f, 1f, 1f);
        return button;
    }

    void BuildUi() {
        var canvasRect = NewRect("UserCanvas", transform);
        var canvas = canvasRect.gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasRect.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasRect.gameObject.AddComponent<GraphicRaycaster>();

        var background = NewPanel("Background", canvasRect, backgroundColor);
        Place(background.rectTransform, 0f, 0f, 1f, 1f);

        var title = NewText("Title", canvasRect, "Choisir un utilisateur", 64, TextAnchor.MiddleCenter, textColor);
        Place(title.rectTransform, 0.05f, 0.86f, 0.95f, 0.97f);

        BuildListPanel(canvasRect);
        BuildCreatePanel(canvasRect);

        selectedText = NewText("SelectedText", canvasRect, "", 36, TextAnchor.MiddleCenter, textColor);
        Place(selectedText.rectTransform, 0.05f, 0.11f, 0.95f, 0.18f);

        continueButton = NewButton("ContinueButton", canvasRect, "Continuer", 40);
        Place(continueButton.GetComponent<RectTransform>(), 0.35f, 0.03f, 0.65f, 0.1f);
        continueButton.onClick.AddListener(OnContinue);
    }

    void BuildListPanel(RectTransform parent) {
        var panel = NewPanel("ListPanel", parent, panelColor);
        Place(panel.rectTransform, 0.05f, 0.2f, 0.5f, 0.84f);

        var label = NewText("ListLabel", panel.transform, "Utilisateurs existants", 36, TextAnchor.MiddleLeft, textColor);
        Place(label.rectTransform, 0.04f, 0.88f, 0.96f, 0.98f);

        var viewport = NewPanel("Viewport", panel.transform, new Color(1f, 1f, 1f, 0f));
        Place(viewport.rectTransform, 0.03f, 0.03f, 0.97f, 0.86f);
        viewport.gameObject.AddComponent<RectMask2D>();

        listContent = NewRect("Content", viewport.transform);
        listContent.anchorMin = new Vector2(0f, 1f);
        listContent.anchorMax = new Vector2(1f, 1f);
        listContent.pivot = new Vector2(0.5f, 1f);
        listContent.offsetMin = Vector2.zero;
        listContent.offsetMax = Vector2.zero;

        var layout = listContent.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = listContent.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        var scroll = panel.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport.rectTransform;
        scroll.content = listContent;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 40f;
    }

    void BuildCreatePanel(RectTransform parent) {
        var panel = NewPanel("CreatePanel", parent, panelColor);
        Place(panel.rectTransform, 0.55f, 0.4f, 0.95f, 0.84f);

        var label = NewText("CreateLabel", panel.transform, "Nouvel utilisateur", 36, TextAnchor.MiddleLeft, textColor);
        Place(label.rectTransform, 0.05f, 0.82f, 0.95f, 0.96f);

        var fieldImage = NewPanel("NameField", panel.transform, new Color(0.04f, 0.05f, 0.08f, 1f));
        Place(fieldImage.rectTransform, 0.05f, 0.58f, 0.95f, 0.78f);

        var fieldText = NewText("Text", fieldImage.transform, "", 36, TextAnchor.MiddleLeft, textColor);
        Place(fieldText.rectTransform, 0.03f, 0f, 0.97f, 1f);
        var placeholder = NewText("Placeholder", fieldImage.transform, "Entre un nom...", 36, TextAnchor.MiddleLeft, new Color(0.6f, 0.6f, 0.65f, 1f));
        placeholder.fontStyle = FontStyle.Italic;
        Place(placeholder.rectTransform, 0.03f, 0f, 0.97f, 1f);

        nameField = fieldImage.gameObject.AddComponent<InputField>();
        nameField.targetGraphic = fieldImage;
        nameField.textComponent = fieldText;
        nameField.placeholder = placeholder;
        nameField.characterLimit = UserStore.MaxNameLength;

        var createButton = NewButton("CreateButton", panel.transform, "Creer", 36);
        Place(createButton.GetComponent<RectTransform>(), 0.05f, 0.36f, 0.5f, 0.52f);
        createButton.onClick.AddListener(OnCreate);

        messageText = NewText("Message", panel.transform, "", 30, TextAnchor.UpperLeft, textColor);
        Place(messageText.rectTransform, 0.05f, 0.03f, 0.95f, 0.32f);
    }

    void RefreshList() {
        foreach (Transform child in listContent) {
            Destroy(child.gameObject);
        }

        rowImages.Clear();
        rowNames.Clear();

        var names = new List<string>();

        foreach (var user in UserStore.Users) {
            names.Add(user.name);
        }

        names.Sort(System.StringComparer.OrdinalIgnoreCase);

        if (names.Count == 0) {
            var empty = NewText("Empty", listContent, "Aucun utilisateur pour l'instant. Cree-en un a droite.", 30, TextAnchor.UpperLeft, new Color(0.7f, 0.7f, 0.75f, 1f));
            var emptyElement = empty.gameObject.AddComponent<LayoutElement>();
            emptyElement.preferredHeight = 90f;
            return;
        }

        foreach (string userName in names) {
            string captured = userName;
            var button = NewButton("User_" + userName, listContent, userName, 34);
            var element = button.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 70f;
            button.onClick.AddListener(() => Select(captured));
            rowImages.Add(button.GetComponent<Image>());
            rowNames.Add(userName);
        }
    }

    void Select(string userName) {
        selectedName = userName;
        UpdateSelectionDisplay();
    }

    void UpdateSelectionDisplay() {
        for (int i = 0; i < rowImages.Count; i++) {
            rowImages[i].color = rowNames[i] == selectedName ? selectedColor : buttonColor;
        }

        bool hasSelection = !string.IsNullOrEmpty(selectedName);
        continueButton.interactable = hasSelection;
        selectedText.text = hasSelection ? "Utilisateur choisi : " + selectedName : "Aucun utilisateur choisi";
    }

    void ShowMessage(string content, bool ok) {
        messageText.text = content;
        messageText.color = ok ? okColor : errorColor;
    }

    void OnCreate() {
        string message;

        if (UserStore.TryCreate(nameField.text, out message)) {
            nameField.text = "";
            RefreshList();
            Select(message);
            ShowMessage("Utilisateur " + message + " cree.", true);
        } else {
            ShowMessage(message, false);
        }
    }

    void OnContinue() {
        if (string.IsNullOrEmpty(selectedName)) {
            ShowMessage("Choisis ou cree un utilisateur d'abord.", false);
            return;
        }

        UserSession.Select(selectedName);
        SceneManager.LoadScene(nextSceneName);
    }
}
