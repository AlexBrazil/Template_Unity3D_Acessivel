
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TextToSpeech : MonoBehaviour
{
    private Button currentButton;
        
    private void Start()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        TTSHandler.SetGameObjectName(transform.gameObject.name);
        #endif
    }

    
    public void SpeakFromInputField(string textToSpeak)
    {
        if (textToSpeak == null || string.IsNullOrEmpty(textToSpeak))
        {
            Debug.LogWarning("InputField não está atribuído ou está vazio. Usando texto padrão.");
            return;
        }

        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("Não foi possível encontrar o GameObject que disparou o evento.");
            return;
        }

        currentButton = selectedObject.GetComponent<Button>();
        if (currentButton == null)
        {
            Debug.LogError($"O GameObject '{selectedObject.name}' não possui um componente Button.");
            return;
        }
        
        Speak(textToSpeak, currentButton);
    }
    
    void Speak(string textToSpeak, Button speakButton)
    {
        if (string.IsNullOrEmpty(textToSpeak))
        {
            Debug.LogError("Tentativa de falar um texto vazio ou nulo.");
            return;
        }

        if (speakButton != null)
        {
            speakButton.interactable = false;
        }
        
        #if !UNITY_EDITOR && UNITY_WEBGL
        TTSHandler.StartToSpeak(textToSpeak);
        #else
        Debug.Log($"MODO EDITOR: Falando '{textToSpeak}'");
        OnSpeechEnded(0); 
        #endif
    }

    public void CancelSpeech()
    {
        #if !UNITY_EDITOR && UNITY_WEBGL
        TTSHandler.CancelSpeech();
        #endif
        
        GameObject selectedObject = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null)
        {
            Debug.LogError("Não foi possível encontrar o GameObject que disparou o evento.");
            return;
        }

        Button speakButton = selectedObject.GetComponent<Button>();
        if (speakButton == null)
        {
            Debug.LogError($"O GameObject '{selectedObject.name}' não possui um componente Button.");
            return;
        }
        
        speakButton.interactable = true;
        Debug.Log("Pedido de cancelamento de fala enviado.");
    }

    private void OnSpeechEnded(float elapsedTime)
    {
        Debug.Log($"A fala terminou após {elapsedTime} segundos.");
        
        if (currentButton != null)
        {
            currentButton.interactable = true;
            currentButton = null; 
        }
    }
}
        
