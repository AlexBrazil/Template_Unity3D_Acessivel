//Adicione este componente a cada Button que você quer tornar acessível.
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]

//Implementa interface IAccessible para padronizar métodos de acessibilidade
public class AccessibleButton : MonoBehaviour, IAccessible
{
    [Tooltip("ID único para este elemento. Ex: 'start-game-button'.")]
    [SerializeField] private string accessibilityId;

    [Tooltip("Texto que será lido pelo leitor de tela. Ex: 'Iniciar Jogo'.")]
    [SerializeField] private string accessibilityLabel;

    private Button _button;
    private RectTransform _rectTransform;

    // Implementação da Interface contratada na interface IAccessible
    public string AccessibilityId => accessibilityId;
    public string AccessibilityLabel => accessibilityLabel;
    public RectTransform RectTransform => _rectTransform;
    public bool IsInteractable() => _button != null && _button.interactable;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _rectTransform = GetComponent<RectTransform>();

        if (string.IsNullOrEmpty(accessibilityId))
        {
            Debug.LogWarning($"Elemento acessível '{gameObject.name}' não possui um ID definido.", this);
        }
    }
}