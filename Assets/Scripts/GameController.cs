using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Button btnIniciar;
    public Button btnAvancar;
    public Button btnVoltar;

    private Button focoAtual;

    public void ReceberComando(string comando)
    {
        Debug.Log("Comando recebido do HTML: " + comando);

        switch (comando)
        {
            case "FocoIniciar":
                SetarFoco(btnIniciar);
                break;
            case "FocoAvancar":
                SetarFoco(btnAvancar);
                break;
            case "FocoVoltar":
                SetarFoco(btnVoltar);
                break;
            case "Acionar":
                if (focoAtual != null)
                {
                    focoAtual.onClick.Invoke();
                }
                break;
        }
    }

    private void SetarFoco(Button novoFoco)
    {
        focoAtual = novoFoco;

        // Visualmente realça o botão focado (opcional)
        ColorBlock cb = novoFoco.colors;
        cb.normalColor = Color.yellow;
        novoFoco.colors = cb;

        // Remove destaque dos outros (opcional)
        foreach (var btn in new[] { btnIniciar, btnAvancar, btnVoltar })
        {
            if (btn != novoFoco)
            {
                ColorBlock c = btn.colors;
                c.normalColor = Color.white;
                btn.colors = c;
            }
        }
    }
}
