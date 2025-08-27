
// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Array para armazenar todos os AudioListeners da cena
    private AudioListener[] audioListeners;

    void Start()
    {
        // Encontra todos os AudioListeners na cena, incluindo os em objetos inativos
        audioListeners = FindObjectsOfType<AudioListener>(true);
        
        if (audioListeners.Length == 0)
        {
            Debug.LogWarning("Nenhum AudioListener encontrado na cena!");
        }
        else if (audioListeners.Length > 1)
        {
            string listenerInfo = "";
            foreach (var listener in audioListeners)
            {
                listenerInfo += $"\n- {listener.gameObject.name} (Ativo: {listener.gameObject.activeInHierarchy})";
            }
            Debug.LogWarning($"Encontrados {audioListeners.Length} AudioListeners na cena:{listenerInfo}");
        }
    }

    // Método chamado pelo JavaScript para controlar o mute
    public void SetMute(int isMuted)
    {
        // Se isMuted for 1, desativa o áudio, se for 0, ativa
        bool shouldMute = isMuted == 1;
        
        // Define o volume global (afeta todos os AudioListeners)
        AudioListener.volume = shouldMute ? 0f : 1f;

        // Também desativa/ativa cada AudioListener individualmente, mas apenas se o objeto estiver ativo
        foreach (var listener in audioListeners)
        {
            if (listener != null && listener.gameObject.activeInHierarchy)
            {
                listener.enabled = !shouldMute;
            }
        }

        string listenerStatus = "";
        foreach (var listener in audioListeners)
        {
            if (listener != null)
            {
                listenerStatus += $"\n- {listener.gameObject.name} (Ativo: {listener.gameObject.activeInHierarchy}, Enabled: {listener.enabled})";
            }
        }
        Debug.Log($"Audio {(shouldMute ? "mutado" : "desmutado")} - Status dos AudioListeners:{listenerStatus}");
    }

    // Quando o objeto for destruído ou desativado, restaura o volume
    void OnDisable()
    {
        AudioListener.volume = 1f;
        foreach (var listener in audioListeners)
        {
            if (listener != null && listener.gameObject.activeInHierarchy)
            {
                listener.enabled = true;
            }
        }
    }
}
        