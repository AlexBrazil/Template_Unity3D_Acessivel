
mergeInto(LibraryManager.library, {

StartToSpeak:function(msg) {
    StartToSpeak(UTF8ToString(msg));
},
CancelSpeech:function() {
    CancelSpeech();
},
AdjustRate:function(value) {
    AdjustRate(value);
},
AdjustVolume:function(value) {
    AdjustVolume(value);
},
AdjustPitch:function(value) {
    AdjustPitch(value);
},
ChangeVoice:function(value) {
    ChangeVoice(value);
},
SetGameObjectName:function(name) {
    SetGameObjectName(UTF8ToString(name));
}
});
        