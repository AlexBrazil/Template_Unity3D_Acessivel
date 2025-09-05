using UnityEngine;

/// <summary>
/// Interface para qualquer elemento da UI que precise ser exposto para a camada de acessibilidade.
/// </summary>
public interface IAccessible
{
    string AccessibilityId { get; }
    string AccessibilityLabel { get; }
    int NavigationOrder { get; }
    RectTransform RectTransform { get; }
    bool IsInteractable();
}