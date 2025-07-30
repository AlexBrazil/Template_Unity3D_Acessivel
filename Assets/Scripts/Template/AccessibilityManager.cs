using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class AccessibilityManager : MonoBehaviour
{
    // --- Singleton Pattern ---
    public static AccessibilityManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- Ponte com o JavaScript ---
    [DllImport("__Internal")]
    private static extern void SendAccessibilityDataToJS(string jsonString);

    // --- Estruturas de Dados ---
    [System.Serializable]
    private struct AccessibleUIData { public string id, label; public float x, y, width, height; }

    [System.Serializable]
    private class AccessibleUIList { public List<AccessibleUIData> elements = new List<AccessibleUIData>(); }

    // --- Métodos Chamados pelo JavaScript ---
    public void OnCanvasResized(string dimensions)
    {
        StartCoroutine(SyncAfterDelay());
    }

    public void RequestFullSyncFromJS()
    {
        StartCoroutine(SyncAfterDelay());
    }

    public void OnProxyElementClicked(string id)
    {
        IAccessible[] accessibleElements = FindObjectsByType<AccessibleButton>(FindObjectsSortMode.None);
        foreach (var element in accessibleElements)
        {
            if (element.AccessibilityId == id && element.IsInteractable())
            {
                if (element.RectTransform.TryGetComponent<Button>(out var button))
                {
                    button.onClick.Invoke();
                    Debug.Log($"Proxy click on '{id}' invoked.");
                }
                return;
            }
        }
        Debug.LogWarning($"Proxy element with id '{id}' was clicked, but not found in scene.");
    }

    // --- Lógica Principal de Sincronização ---
    private IEnumerator SyncAfterDelay()
    {
        // Esta era a versão simples da corrotina naquele momento.
        yield return new WaitForEndOfFrame();
        SyncAccessibilityLayer();
    }
    
    public void SyncAccessibilityLayer()
    {
        AccessibleUIList uiList = new AccessibleUIList();
        IAccessible[] accessibleElements = FindObjectsByType<AccessibleButton>(FindObjectsSortMode.None);

        foreach (var element in accessibleElements)
        {
            if (element.RectTransform.gameObject.activeInHierarchy && element.IsInteractable())
            {
                Rect screenRect = GetScreenRect(element.RectTransform);
                uiList.elements.Add(new AccessibleUIData
                {
                    id = element.AccessibilityId,
                    label = element.AccessibilityLabel,
                    x = screenRect.x,
                    y = screenRect.y,
                    width = screenRect.width,
                    height = screenRect.height
                });
            }
        }

        string json = JsonUtility.ToJson(uiList);

        #if !UNITY_EDITOR && UNITY_WEBGL
            SendAccessibilityDataToJS(json);
        #else
            Debug.Log("Accessibility Sync (Editor): " + json);
        #endif
    }
    
    private Rect GetScreenRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);

        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Accessible element não está dentro de um Canvas.", rectTransform);
            return Rect.zero;
        }

        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);

        // A inversão do Y usava Screen.height, que funciona bem no desktop.
        float x = screenBottomLeft.x;
        float y = Screen.height - screenTopRight.y; // Inverte o eixo Y
        float width = screenTopRight.x - screenBottomLeft.x;
        float height = screenTopRight.y - screenBottomLeft.y;

        return new Rect(x, y, width, height);
    }
}