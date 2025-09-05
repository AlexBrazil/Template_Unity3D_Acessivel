using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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

    // Método público para que outros scripts possam solicitar uma atualização.
    // Este será nosso principal gatilho para UI dinâmica.
    public void RequestAccessibilitySync()
    {
        // Apenas inicia a mesma corrotina que já usamos.
        // Isso garante que a UI seja atualizada após o layout da Unity ser calculado.
        StartCoroutine(SyncAfterDelay());
    }

    private IEnumerator SyncAfterDelay()
    {
        yield return new WaitForEndOfFrame();
        SyncAccessibilityLayer();
    }
    
    public void SyncAccessibilityLayer()
    {
        AccessibleUIList uiList = new AccessibleUIList();
        //IAccessible[] accessibleElements = FindObjectsByType<AccessibleButton>(FindObjectsSortMode.None);
        IAccessible[] accessibleElements = FindObjectsByType<AccessibleButton>(FindObjectsSortMode.None).OrderBy(element => element.NavigationOrder).ToArray();
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
        // Cria um array de 4 vetores para armazenar os cantos do elemento    
        Vector3[] corners = new Vector3[4];

        // Preenche o array com as coordenadas mundiais dos 4 cantos do elemento
        // Ordem: bottom-left, top-left, top-right, bottom-right
        rectTransform.GetWorldCorners(corners);

        //Encontra o Canvas pai do elemento
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();


        if (canvas == null)
        {
            Debug.LogError("Accessible element não está dentro de um Canvas.", rectTransform);
            return Rect.zero;
        }

        // Determina a câmera de renderização
        // Se o Canvas for em modo overlay, usa null (renderização de tela)
        // Senão, usa a câmera do mundo do Canvas
        Camera camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        // Converte os cantos bottom-left e top-right para coordenadas de tela
        // Usa a câmera definida anteriormente para conversão
        Vector2 screenBottomLeft = RectTransformUtility.WorldToScreenPoint(camera, corners[0]);
        Vector2 screenTopRight = RectTransformUtility.WorldToScreenPoint(camera, corners[2]);

        // Calcula as dimensões do retângulo na tela
        // x: coordenada x do canto inferior esquerdo
        float x = screenBottomLeft.x;

        // inverte a coordenada y do canto superior direito
        // Necessário porque o sistema de coordenadas da Unity é diferente do sistema de coordenadas da tela
        // Screen.height subtrai para inverter o eixo Y (de cima para baixo)
        float y = Screen.height - screenTopRight.y; // Inverte o eixo Y

        // width: diferença entre as coordenadas x dos cantos
        float width = screenTopRight.x - screenBottomLeft.x;

        // height: diferença entre as coordenadas y dos cantos
        float height = screenTopRight.y - screenBottomLeft.y;

        // Retorna um Rect com as coordenadas e dimensões calculadas
        return new Rect(x, y, width, height);
    }
}