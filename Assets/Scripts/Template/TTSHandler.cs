
using System.Runtime.InteropServices;

public static class TTSHandler
{
    [DllImport("__Internal")]
    public static extern void SetGameObjectName(string Name);

    [DllImport("__Internal")]
    public static extern void StartToSpeak(string msg);

    [DllImport("__Internal")]
    public static extern void CancelSpeech();

    [DllImport("__Internal")]
    public static extern void AdjustRate(float value);

    [DllImport("__Internal")]
    public static extern void AdjustVolume(float value);

    [DllImport("__Internal")]
    public static extern void AdjustPitch(float value);

    [DllImport("__Internal")]
    public static extern void ChangeVoice(int index);
}
        