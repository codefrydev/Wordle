// Wordle extras: stats, confetti, share, settings
window.wordle = {
  statsKey: 'wordle-1940-stats',
  dailyKey: 'wordle-1940-daily',
  themeKey: 'wordle-theme',
  paletteKey: 'wordle-palette',
  hardModeKey: 'wordle-hard-mode',
  soundKey: 'wordle-sound',
  hapticsKey: 'wordle-haptics',
  reduceMotionKey: 'wordle-reduce-motion',

  getTheme: function () {
    var t = localStorage.getItem(this.themeKey);
    return t === 'dark' || t === 'light' ? t : 'light';
  },

  setTheme: function (theme) {
    if (theme !== 'dark' && theme !== 'light') return;
    localStorage.setItem(this.themeKey, theme);
    document.documentElement.setAttribute('data-theme', theme);
  },

  getPalette: function () {
    var p = localStorage.getItem(this.paletteKey);
    return p === 'highcontrast' || p === 'retro' || p === 'neon' ? p : 'default';
  },

  setPalette: function (palette) {
    var valid = ['default', 'highcontrast', 'retro', 'neon'];
    if (valid.indexOf(palette) === -1) return;
    localStorage.setItem(this.paletteKey, palette);
    if (palette === 'default') document.documentElement.removeAttribute('data-palette');
    else document.documentElement.setAttribute('data-palette', palette);
  },

  getHardMode: function () {
    return localStorage.getItem(this.hardModeKey) === 'true';
  },

  setHardMode: function (value) {
    localStorage.setItem(this.hardModeKey, value === true ? 'true' : 'false');
  },

  getSound: function () {
    return localStorage.getItem(this.soundKey) === 'true';
  },

  setSound: function (value) {
    localStorage.setItem(this.soundKey, value === true ? 'true' : 'false');
  },

  getHaptics: function () {
    return localStorage.getItem(this.hapticsKey) === 'true';
  },

  setHaptics: function (value) {
    localStorage.setItem(this.hapticsKey, value === true ? 'true' : 'false');
  },

  getReduceMotion: function () {
    return localStorage.getItem(this.reduceMotionKey) === 'true';
  },

  setReduceMotion: function (value) {
    localStorage.setItem(this.reduceMotionKey, value === true ? 'true' : 'false');
    document.documentElement.setAttribute('data-reduce-motion', value ? 'true' : '');
  },

  getStats: function () {
    try {
      var raw = localStorage.getItem(this.statsKey);
      if (!raw) return { played: 0, wins: 0, streak: 0, maxStreak: 0, distribution: [0, 0, 0, 0, 0, 0], lastPlayedDate: null };
      var o = JSON.parse(raw);
      o.distribution = Array.isArray(o.distribution) && o.distribution.length === 6 ? o.distribution : [0, 0, 0, 0, 0, 0];
      return o;
    } catch (e) {
      return { played: 0, wins: 0, streak: 0, maxStreak: 0, distribution: [0, 0, 0, 0, 0, 0], lastPlayedDate: null };
    }
  },

  setStats: function (stats) {
    try {
      localStorage.setItem(this.statsKey, JSON.stringify(stats));
      return true;
    } catch (e) {
      return false;
    }
  },

  getDateKey: function () {
    var d = new Date();
    return d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
  },

  getNextWordIn: function () {
    var now = new Date();
    var tomorrow = new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1);
    var ms = tomorrow - now;
    if (ms <= 0) return '0h 0m';
    var h = Math.floor(ms / 3600000);
    var m = Math.floor((ms % 3600000) / 60000);
    return h + 'h ' + m + 'm';
  },

  copyShareToClipboard: function (text) {
    if (navigator.clipboard && navigator.clipboard.writeText)
      return navigator.clipboard.writeText(text);
    var ta = document.createElement('textarea');
    ta.value = text;
    ta.style.position = 'fixed';
    ta.style.opacity = '0';
    document.body.appendChild(ta);
    ta.select();
    try {
      document.execCommand('copy');
      document.body.removeChild(ta);
      return Promise.resolve();
    } catch (e) {
      document.body.removeChild(ta);
      return Promise.reject(e);
    }
  },

  playSound: function (type) {
    if (localStorage.getItem(this.soundKey) !== 'true') return;
    try {
      var ctx = window.wordleAudioContext || (window.wordleAudioContext = new (window.AudioContext || window.webkitAudioContext)());
      var osc = ctx.createOscillator();
      var gain = ctx.createGain();
      osc.connect(gain);
      gain.connect(ctx.destination);
      var f = 400, d = 0.05;
      if (type === 'submit') { f = 523; d = 0.08; }
      else if (type === 'invalid') { f = 200; d = 0.12; }
      else if (type === 'win') { f = 660; d = 0.15; }
      osc.frequency.setValueAtTime(f, ctx.currentTime);
      osc.frequency.setValueAtTime(f * 1.2, ctx.currentTime + d * 0.3);
      gain.gain.setValueAtTime(0.15, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.01, ctx.currentTime + d);
      osc.start(ctx.currentTime);
      osc.stop(ctx.currentTime + d);
    } catch (e) {}
  },

  _savedFocus: null,
  _trapKeydown: null,

  saveFocus: function () {
    this._savedFocus = document.activeElement;
  },

  restoreFocus: function () {
    if (this._savedFocus && typeof this._savedFocus.focus === 'function') {
      this._savedFocus.focus();
      this._savedFocus = null;
    }
    if (this._trapKeydown) {
      document.removeEventListener('keydown', this._trapKeydown);
      this._trapKeydown = null;
    }
  },

  trapFocus: function () {
    var panel = document.querySelector('.modal-panel');
    if (!panel) return;
    var focusable = panel.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
    var first = focusable[0];
    var last = focusable[focusable.length - 1];
    if (first) first.focus();
    var self = this;
    this._trapKeydown = function (e) {
      if (e.key !== 'Tab') return;
      if (e.shiftKey) {
        if (document.activeElement === first) { e.preventDefault(); last.focus(); }
      } else {
        if (document.activeElement === last) { e.preventDefault(); first.focus(); }
      }
    };
    document.addEventListener('keydown', self._trapKeydown);
  },

  vibrate: function () {
    if (localStorage.getItem(this.hapticsKey) !== 'true' || !navigator.vibrate) return;
    try { navigator.vibrate(8); } catch (e) {}
  },

  showConfetti: function () {
    if (document.documentElement.getAttribute('data-reduce-motion') === 'true') return;
    var colors = ['#4a7c59', '#c9a227', '#5c5346', '#2c2420', '#8B7355', '#A0522D'];
    var count = 80;
    var container = document.createElement('div');
    container.style.cssText = 'position:fixed;inset:0;pointer-events:none;z-index:9999;overflow:hidden;';
    document.body.appendChild(container);
    for (var i = 0; i < count; i++) {
      (function () {
        var el = document.createElement('div');
        el.style.cssText = 'position:absolute;width:10px;height:10px;background:' + colors[Math.floor(Math.random() * colors.length)] + ';border:2px solid #2c2420;border-radius:2px;left:' + (Math.random() * 100) + '%;top:-20px;animation:wordle-confetti-fall 2.5s ease-out forwards;opacity:0.9;';
        el.style.animationDelay = (Math.random() * 0.5) + 's';
        el.style.setProperty('--tx', (Math.random() - 0.5) * 200 + 'px');
        container.appendChild(el);
        setTimeout(function () {
          if (el.parentNode) el.parentNode.removeChild(el);
        }, 3000);
      })();
    }
    setTimeout(function () {
      if (container.parentNode) container.parentNode.removeChild(container);
    }, 3200);
  }
};

// Inject confetti keyframes once
(function () {
  if (document.getElementById('wordle-confetti-style')) return;
  var style = document.createElement('style');
  style.id = 'wordle-confetti-style';
  style.textContent = '@keyframes wordle-confetti-fall { 0% { transform: translateY(0) translateX(0) rotate(0deg); opacity: 1; } 100% { transform: translateY(100vh) translateX(var(--tx, 0)) rotate(720deg); opacity: 0; } }';
  document.head.appendChild(style);
})();

// Blazor-callable wrappers
window.getWordleStats = function () { return window.wordle.getStats(); };
window.setWordleStats = function (obj) { return window.wordle.setStats(obj); };
window.getWordleDateKey = function () { return window.wordle.getDateKey(); };
window.getWordleNextWordIn = function () { return window.wordle.getNextWordIn(); };
window.getWordleTheme = function () { return window.wordle.getTheme(); };
window.setWordleTheme = function (theme) { return window.wordle.setTheme(theme); };
window.getWordlePalette = function () { return window.wordle.getPalette(); };
window.setWordlePalette = function (p) { return window.wordle.setPalette(p); };
window.getWordleHardMode = function () { return window.wordle.getHardMode(); };
window.setWordleHardMode = function (v) { return window.wordle.setHardMode(v); };
window.getWordleSound = function () { return window.wordle.getSound(); };
window.setWordleSound = function (v) { return window.wordle.setSound(v); };
window.getWordleHaptics = function () { return window.wordle.getHaptics(); };
window.setWordleHaptics = function (v) { return window.wordle.setHaptics(v); };
window.getWordleReduceMotion = function () { return window.wordle.getReduceMotion(); };
window.setWordleReduceMotion = function (v) { return window.wordle.setReduceMotion(v); };
window.wordleCopyShare = function (text) { return window.wordle.copyShareToClipboard(text); };
window.wordleConfetti = function () { return window.wordle.showConfetti(); };
window.wordlePlaySound = function (type) { return window.wordle.playSound(type || 'key'); };
window.wordleVibrate = function () { return window.wordle.vibrate(); };
window.wordleSaveFocus = function () { return window.wordle.saveFocus(); };
window.wordleRestoreFocus = function () { return window.wordle.restoreFocus(); };
window.wordleTrapFocus = function () { return window.wordle.trapFocus(); };
