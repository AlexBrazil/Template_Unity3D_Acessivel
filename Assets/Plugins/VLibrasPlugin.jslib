
mergeInto(LibraryManager.library, {
  registerVLibrasText: function(idPtr, textPtr) {
    var id   = UTF8ToString(idPtr);
    var text = UTF8ToString(textPtr);
    if (typeof registerVLibrasText === 'function') {
      registerVLibrasText(id, text);
    }
  }
});
        