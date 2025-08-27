
using UnityEngine;
using System.Runtime.InteropServices;

public class VLibrasBridge : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void registerVLibrasText(string id, string text);
#endif

    public void SendToVLibras(string text)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        registerVLibrasText("unityText", text);
#else
        Debug.Log($"[VLibrasBridge] text={text}");
#endif
    }
}
        