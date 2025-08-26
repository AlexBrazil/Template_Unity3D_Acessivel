using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);

        // Sempre que o estado de visibilidade de um grupo de botões muda,
        // é uma boa prática ressincronizar.
        if (AccessibilityManager.Instance != null)
        {
            AccessibilityManager.Instance.RequestAccessibilitySync();
        }
    }
    
}
