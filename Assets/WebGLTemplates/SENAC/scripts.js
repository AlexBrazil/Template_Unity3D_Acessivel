// scripts.js

let unityInstance = null;
let isSoundEnabled = true;
let currentPosition, controlsOffset, separationSpace;
let ttsConfig = null;
let isAccessibilityLayerActive = false;

const controlsContainer = document.querySelector(".controls-container");
const fullscreenBtn = document.getElementById("fullscreen-btn");
const soundBtn = document.getElementById("sound-btn");
const menuButton = document.getElementById("menu");
const menuPanel = document.getElementById("menu-panel");
const accessibilityOverlay = document.getElementById("accessibility-overlay");

let panelWidth, focusableEls, firstFocusableEl, lastFocusableEl;

document.addEventListener("DOMContentLoaded", async () => {
  // 1) Carrega config.json
  try {
    const res = await fetch("config.json");
    const cfg = await res.json();

    // VLibras
    if (cfg.enableVLibras) setupVLibras();

    // TTS (Text-to-Speech)
    if (cfg.tts?.enable) {
      ttsConfig = cfg.tts.defaults;
      setupTTS();
    }

    // Controles
    currentPosition = cfg.controls?.position ?? "SD";
    controlsOffset = cfg.controls?.offset ?? 16;
    separationSpace = cfg.controls?.separation_space_button ?? 12;

    // Cores via variáveis CSS
    const cols = cfg.controls?.colors || {};
    if (cols.fullscreen)
      document.documentElement.style.setProperty("--color-fullscreen", cols.fullscreen);
    if (cols.sound)
      document.documentElement.style.setProperty("--color-sound", cols.sound);
    if (cols.menu)
      document.documentElement.style.setProperty("--color-menu", cols.menu);
    if (cols.panel)
      document.documentElement.style.setProperty("--color-panel", cols.panel);
    
    // Configuração da Camada de Acessibilidade
    if (cfg.accessibilityLayer?.enable) {
      setupAccessibilityLayer();
    }
  } catch (err) {
    console.warn("Erro ao ler config.json:", err);
    // Define valores padrão em caso de falha
    currentPosition = "SD";
    controlsOffset = 16;
    separationSpace = 12;
  }

  // 2) Posiciona controles
  positionControls({
    position: currentPosition,
    offset: controlsOffset,
    separation_space_button: separationSpace,
  });

  // 3) Inicializa menu e filtros
  initMenu();
  initFilters();

  // 4) Pré-carrega ativos críticos
  preloadAssets();

  // 5) Som de clique em botões (exceto sound-btn)
  attachClickSound();

  // 6) Ícones iniciais
  updateSoundIcon();
  updateFullscreenIcon();

  // 7) Desabilita F11
  document.addEventListener("keydown", disableF11);

  // 8) Atualiza ícone fullscreen ao mudar
  document.addEventListener("fullscreenchange", updateFullscreenIcon);

  // 9) Resize do canvas com debounce
  window.addEventListener("resize", debounce(resizeCanvas, 100));

  // A "API" para a Unity é publicada no escopo global.
  window.UpdateAccessibilityLayer = UpdateAccessibilityLayer;
  
  // 10) Carrega Unity WebGL
  loadUnity();
});

// ——— Detecta Mobile e Ajusta VLibras ———
const detectMob = () => {
  const toMatch = [ /Android/i, /webOS/i, /iPhone/i, /iPad/i, /iPod/i, /BlackBerry/i, /Windows Phone/i ];
  return toMatch.some((toMatchItem) => navigator.userAgent.match(toMatchItem));
};

const vlibrasAjust = () => {
  if (detectMob()) {
    const vlibrasVW = document.getElementById("vlibras-container");
    if (vlibrasVW) {
      vlibrasVW.classList.add("mobile-ajust");
    }
  }
};
vlibrasAjust();
window.addEventListener("resize", vlibrasAjust);

// ——— VLibras ———
function setupVLibras() {
  const container = document.getElementById("vlibras-container");
  container.setAttribute("vw", "");
  container.classList.add("enabled");

  const accessBtn = document.createElement("div");
  accessBtn.setAttribute("vw-access-button", "");
  accessBtn.classList.add("active");
  container.appendChild(accessBtn);

  const pluginWrap = document.createElement("div");
  pluginWrap.setAttribute("vw-plugin-wrapper", "");
  const topWrap = document.createElement("div");
  topWrap.classList.add("vw-plugin-top-wrapper");
  pluginWrap.appendChild(topWrap);
  container.appendChild(pluginWrap);

  const vlScript = document.createElement("script");
  vlScript.src = "https://vlibras.gov.br/app/vlibras-plugin.js";
  vlScript.onload = () => new window.VLibras.Widget("https://vlibras.gov.br/app");
  document.body.appendChild(vlScript);
}

// ——— Text-to-Speech (TTS) ———
function setupTTS() {
  const ttsScript = document.createElement("script");
  ttsScript.src = "TextToSpeech.js";
  ttsScript.async = true;
  ttsScript.onload = () => {
    console.log("TTS script loaded.");
    if (ttsConfig) {
      if (typeof AdjustRate === 'function') AdjustRate(ttsConfig.rate || 1.0);
      if (typeof AdjustVolume === 'function') AdjustVolume(ttsConfig.volume || 1.0);
      if (typeof AdjustPitch === 'function') AdjustPitch(ttsConfig.pitch || 1.0);
    }
  };
  document.body.appendChild(ttsScript);
}

// ——— Função Global para Receber Textos da Unity (VLibras) ———
function registerVLibrasText(id, text) {
  const container = document.getElementById("vlibras-ui-texts");
  if (!container) return;

  let el = document.getElementById(`vlibras-text-${id}`);
  if (!el) {
    el = document.createElement("div");
    el.id = `vlibras-text-${id}`;
    container.appendChild(el);
  }
  el.textContent = text;
  
  setTimeout(() => el.dispatchEvent(new MouseEvent("click", { bubbles: true, cancelable: true })), 50);
}

// ——— Funções de UI e Controles ———
function preloadAssets() {
  ["image/rotate-device.gif", "image/fullscreen-enter.svg", "image/fullscreen-exit.svg", "image/sound-on.svg", "image/sound-off.svg"].forEach(src => {
    const img = new Image();
    img.src = src;
  });
}

function attachClickSound() {
  const clickSound = document.getElementById("click-sound");
  document.querySelectorAll("button:not(.sound-btn)").forEach(btn => {
    btn.addEventListener("click", () => {
      if (isSoundEnabled) {
        clickSound.currentTime = 0;
        clickSound.play();
      }
    });
  });
}

function positionControls({ position, offset, separation_space_button }) {
  const ctr = controlsContainer;
  ctr.style.position = "fixed";
  ctr.style.margin = "0";
  ctr.style.display = "flex";
  ctr.style.flexDirection = "column";
  ctr.style.gap = `${separation_space_button}px`;
  ctr.style.top = `${offset}px`;
  ctr.style[position === "SD" ? "right" : "left"] = `${offset}px`;
  ctr.style[position === "SD" ? "left" : "right"] = "auto";
}

function initMenu() {
  menuPanel.classList.remove("panel-right", "open-left", "open-right");
  menuButton.classList.remove("open-left", "open-right");
  if (currentPosition === "SD") {
    menuPanel.classList.add("panel-right");
  }
  menuPanel.setAttribute("aria-hidden", "true");
  menuButton.addEventListener("click", () => (isPanelOpen() ? closeMenu() : openMenu()));
}

function isPanelOpen() {
  return menuPanel.classList.contains(currentPosition === "SD" ? "open-right" : "open-left");
}

function openMenu() {
  menuPanel.classList.add(currentPosition === "SD" ? "open-right" : "open-left");
  menuButton.classList.add(currentPosition === "SD" ? "open-right" : "open-left");
  menuPanel.setAttribute("aria-hidden", "false");
  menuButton.setAttribute("aria-expanded", "true");
  trapFocus();
}

function closeMenu() {
  menuPanel.classList.remove("open-right", "open-left");
  menuButton.classList.remove("open-right", "open-left");
  menuPanel.setAttribute("aria-hidden", "true");
  menuButton.setAttribute("aria-expanded", "false");
  releaseFocus();
  menuButton.focus();
}

function trapFocus() {
  focusableEls = Array.from(menuPanel.querySelectorAll('button, [href], input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'));
  firstFocusableEl = focusableEls[0];
  lastFocusableEl = focusableEls[focusableEls.length - 1];
  firstFocusableEl.focus();
  document.addEventListener("keydown", handleMenuKeydown);
}

function releaseFocus() {
  document.removeEventListener("keydown", handleMenuKeydown);
}

function handleMenuKeydown(e) {
  if (e.key === "Escape") { e.preventDefault(); closeMenu(); }
  if (e.key === "Tab") {
    if (e.shiftKey && document.activeElement === firstFocusableEl) { e.preventDefault(); lastFocusableEl.focus(); }
    else if (!e.shiftKey && document.activeElement === lastFocusableEl) { e.preventDefault(); firstFocusableEl.focus(); }
  }
}

function initFilters() {
  const map = { acromatopsia: "grayscale", protanomalia: "protanopia", deuteranomalia: "deuteranopia", tritanomalia: "tritanopia", normal: "normal" };
  Object.keys(map).forEach(id => {
    const btn = document.getElementById(id);
    if (btn) {
      btn.addEventListener("click", () => {
        applyFilter(map[id]);
        closeMenu();
      });
    }
  });
}

function applyFilter(filterType) {
  document.body.className = document.body.className.replace(/\b(grayscale|deuteranopia|protanopia|tritanopia)\b/g, '').trim();
  if (filterType !== "normal") {
    document.body.classList.add(filterType);
  }
}

function toggleSound() {
  isSoundEnabled = !isSoundEnabled;
  updateSoundIcon();
  if (unityInstance) {
    unityInstance.SendMessage("AudioManager", "SetMute", isSoundEnabled ? 0 : 1);
  }
}

function updateSoundIcon() {
  const isMuted = !isSoundEnabled;
  soundBtn.style.maskImage = `url('image/sound-${isMuted ? 'on' : 'off'}.svg')`;
  soundBtn.style.webkitMaskImage = `url('image/sound-${isMuted ? 'on' : 'off'}.svg')`;
  soundBtn.setAttribute("aria-label", isMuted ? "Ativar som" : "Mutar som");
  soundBtn.setAttribute("aria-pressed", isMuted ? "false" : "true");
}

function toggleFullscreen() {
  if (!document.fullscreenElement) {
    document.documentElement.requestFullscreen?.() || document.documentElement.webkitRequestFullscreen?.();
  } else {
    document.exitFullscreen?.() || document.webkitExitFullscreen?.();
  }
}

function updateFullscreenIcon() {
  const isFullscreen = !!document.fullscreenElement;
  fullscreenBtn.style.maskImage = `url('image/fullscreen-${isFullscreen ? 'exit' : 'enter'}.svg')`;
  fullscreenBtn.style.webkitMaskImage = `url('image/fullscreen-${isFullscreen ? 'exit' : 'enter'}.svg')`;
  fullscreenBtn.setAttribute("aria-label", isFullscreen ? "Sair da tela cheia" : "Entrar em tela cheia");
  fullscreenBtn.setAttribute("aria-pressed", isFullscreen ? "true" : "false");
}

function disableF11(event) {
  if (event.key === "F11") {
    event.preventDefault();
    alert("Use o botão de ZOOM para zoom in e zoom out");
  }
}

function updateProgressBar(value) {
  const filler = document.getElementById("progressFiller");
  filler.style.width = `${Math.min(value * 100, 100)}%`;
  if (value >= 1) {
    filler.style.animation = "none";
    filler.style.backgroundColor = "#00ff00";
  }
}

// =================================================================
// LÓGICA DA CAMADA DE ACESSIBILIDADE - VERSÃO FINAL
// =================================================================

/**
 * Prepara um listener para a primeira interação do usuário,
 * que ativará a camada de acessibilidade.
 */
function setupAccessibilityLayer() {
  document.body.addEventListener('mousedown', activateAccessibilityLayer, { once: true, passive: true });
  document.body.addEventListener('touchstart', activateAccessibilityLayer, { once: true, passive: true });
}

/**
 * Ativa a camada de acessibilidade e solicita a primeira sincronização.
 * Chamada na primeira interação do usuário (clique ou toque).
 */
function activateAccessibilityLayer() {
  if (isAccessibilityLayerActive) return;
  
  console.log("Primeira interação detectada. Ativando camada de acessibilidade.");
  isAccessibilityLayerActive = true;
  
  const trap = document.getElementById('screen-reader-trap');
  if (trap) trap.remove();

  requestFullSyncFromUnity();
}

/**
 * Solicita que a Unity envie os dados completos da UI acessível.
 */
function requestFullSyncFromUnity() {
  if (!unityInstance) {
    console.log("Aguardando instância da Unity para sincronizar...");
    setTimeout(requestFullSyncFromUnity, 500); // Tenta novamente
    return;
  }
  
  if (isAccessibilityLayerActive) {
    console.log("Enviando mensagem 'RequestFullSyncFromJS' para a Unity.");
    unityInstance.SendMessage('AccessibilityManager', 'RequestFullSyncFromJS');
  }
}

/**
 * ATENÇÃO: Esta função é chamada PELA UNITY para atualizar a camada HTML.
 * jsonString - Uma string JSON contendo a lista de elementos acessíveis.
 */
function UpdateAccessibilityLayer(jsonString) {
  if (!isAccessibilityLayerActive) return;

  // Limpa a camada anterior
  accessibilityOverlay.innerHTML = '';

  try {
    const data = JSON.parse(jsonString);
    const elements = data.elements || [];

    // Pega o elemento canvas e suas coordenadas na página
    const canvas = document.getElementById('unityCanvas');
    if (!canvas) {
        console.error("Elemento canvas da Unity não encontrado!");
        return;
    }
    const canvasRect = canvas.getBoundingClientRect();

    elements.forEach(elementData => {
      const proxyEl = document.createElement('button');
      proxyEl.className = 'proxy-element';
      proxyEl.setAttribute('aria-label', elementData.label);
      
      // --- CÁLCULO DE POSIÇÃO DAQUELE MOMENTO ---
      // Corrigia o deslocamento do canvas, mas não a escala.
      proxyEl.style.left = `${canvasRect.left + elementData.x}px`;
      proxyEl.style.top = `${canvasRect.top + elementData.y}px`;

      // O tamanho era aplicado diretamente, sem correção de escala.
      proxyEl.style.width = `${elementData.width}px`;
      proxyEl.style.height = `${elementData.height}px`;

      // Adiciona o listener de clique que envia a informação de volta para a Unity
      proxyEl.addEventListener('click', () => {
        if (unityInstance) {
          unityInstance.SendMessage(
            'AccessibilityManager',
            'OnProxyElementClicked',
            elementData.id
          );
        }
      });

      accessibilityOverlay.appendChild(proxyEl);
    });

  } catch (e) {
    console.error("Erro ao processar dados da camada de acessibilidade:", e);
    console.error("JSON Recebido:", jsonString);
  }
}

// --- Redimensiona Canvas ---
function resizeCanvas() {
  const canvas = document.getElementById("unityCanvas");
  const container = document.getElementById("unityContainer");
  const cAR = container.clientWidth / container.clientHeight;
  const pAR = 16 / 9;

  if (cAR > pAR) {
    canvas.style.width = `${container.clientHeight * pAR}px`;
    canvas.style.height = `${container.clientHeight}px`;
  } else {
    canvas.style.width = `${container.clientWidth}px`;
    canvas.style.height = `${container.clientWidth / pAR}px`;
  }

  if (unityInstance && isAccessibilityLayerActive) {
    unityInstance.SendMessage('AccessibilityManager', 'OnCanvasResized', `${canvas.clientWidth},${canvas.clientHeight}`);
  }
}

// ——— Debounce ———
function debounce(func, wait = 100) {
  let timeout;
  return (...args) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func.apply(this, args), wait);
  };
}

// ——— Carrega Unity WebGL ———
function loadUnity() {
  const loaderScript = document.createElement("script");
  loaderScript.src = "Build/Build.loader.js";
  loaderScript.async = true;
  loaderScript.onload = () => {
    createUnityInstance(
      document.querySelector("#unityCanvas"),
      {
        dataUrl: "Build/Build.data.unityweb",
        frameworkUrl: "Build/Build.framework.js.unityweb",
        codeUrl: "Build/Build.wasm.unityweb",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "SENAC",
        productName: "Teste Acessibilidade",
        productVersion: "1.0",
      },
      (progress) => updateProgressBar(progress)
    )
    .then((instance) => {
      unityInstance = instance;
      window.unityInstance = instance; // Expõe globalmente se necessário

      if (!isSoundEnabled) {
        unityInstance.SendMessage("AudioManager", "SetMute", 1);
      }
      const loadingBar = document.getElementById("loadingBar");
      if (loadingBar) loadingBar.style.display = "none";
      
      resizeCanvas(); // Chama o resize inicial após o carregamento
    })
    .catch((error) => {
      console.error("Erro ao carregar a instância da Unity:", error);
      alert("Ocorreu um erro ao carregar a aplicação. Verifique o console para mais detalhes.");
    });
  };
  document.body.appendChild(loaderScript);
}