using UnityEngine;

/// <summary>
/// Interface para qualquer elemento da UI que precise ser exposto para a camada de acessibilidade.
/// </summary>
public interface IAccessible
{
    string AccessibilityId { get; }
    string AccessibilityLabel { get; }
    RectTransform RectTransform { get; }
    bool IsInteractable();
}