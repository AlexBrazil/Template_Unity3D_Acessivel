mergeInto(LibraryManager.library, {
  /**
   * Envia os dados da UI em formato JSON para a função global no lado do browser.
   * A função 'UpdateAccessibilityLayer' deve estar disponível no escopo 'window'.
   */
  SendAccessibilityDataToJS: function(jsonString) {
    // UTF8ToString converte a string do heap C# para uma string JavaScript
    const jsString = UTF8ToString(jsonString);
    if (window.UpdateAccessibilityLayer) {
      window.UpdateAccessibilityLayer(jsString);
    } else {
      console.error("A função 'UpdateAccessibilityLayer' não foi encontrada no window.");
    }
  }
});