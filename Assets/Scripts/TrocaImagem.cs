using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TrocaImagem : MonoBehaviour
{
    [Header("Imagem externa que será trocada")]
    public Image imagemAlvo;

    [Header("Sprite para trocar")]
    public Sprite novaImagem;

    private Button botao;

    void Awake()
    {
        botao = GetComponent<Button>();
    }

    void Start()
    {
        if (botao != null)
        {
            botao.onClick.AddListener(TrocarImagem);
        }
    }

    public void TrocarImagem()
    {
        if (imagemAlvo != null && novaImagem != null)
        {
            imagemAlvo.sprite = novaImagem;
        }
        else
        {
            Debug.LogWarning("imagemAlvo ou novaImagem não foi atribuída.");
        }
    }
}