using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Собирает сцену InterviewUI: Canvas (Overlay, Scale With Screen Size 1440x1024),
/// EventSystem, фон и корневой UI с компонентом FoundationProbe.
/// Меню: Interview/Build UI Scene.
/// </summary>
public static class InterviewSceneBuilder
{
    [MenuItem("Interview/Build UI Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Canvas
        var canvasGO = new GameObject("Canvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(Theme.Reference.x, Theme.Reference.y);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Фон (белый, под UI)
        var bgGO = new GameObject("Background", typeof(RectTransform));
        bgGO.transform.SetParent(canvasGO.transform, false);
        var bgRT = (RectTransform)bgGO.transform;
        bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite = SpriteFactory.White; bgImg.type = Image.Type.Simple; bgImg.color = Theme.BgMain;
        bgImg.raycastTarget = false;

        // EventSystem
        new GameObject("EventSystem",
            typeof(UnityEngine.EventSystems.EventSystem),
            typeof(UnityEngine.EventSystems.StandaloneInputModule));

        // Корневой UI + демо-раскладка
        var uiGO = new GameObject("UI", typeof(RectTransform));
        uiGO.transform.SetParent(canvasGO.transform, false);
        var uiRT = (RectTransform)uiGO.transform;
        uiRT.anchorMin = Vector2.zero; uiRT.anchorMax = Vector2.one;
        uiRT.offsetMin = Vector2.zero; uiRT.offsetMax = Vector2.zero;
        uiGO.AddComponent<FoundationProbe>();

        System.IO.Directory.CreateDirectory("Assets/#Project/Scenes");
        const string path = "Assets/#Project/Scenes/InterviewUI.unity";
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        AssetDatabase.Refresh();
        Debug.Log("[InterviewSceneBuilder] Scene saved: " + path);
    }
}
