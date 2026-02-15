// Wordle extras: stats, confetti, share
window.wordle = {
  statsKey: 'wordle-1940-stats',
  dailyKey: 'wordle-1940-daily',
  themeKey: 'wordle-theme',

  getTheme: function () {
    var t = localStorage.getItem(this.themeKey);
    return t === 'dark' || t === 'light' ? t : 'light';
  },

  setTheme: function (theme) {
    if (theme !== 'dark' && theme !== 'light') return;
    localStorage.setItem(this.themeKey, theme);
    document.documentElement.setAttribute('data-theme', theme);
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

  showConfetti: function () {
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
window.getWordleTheme = function () { return window.wordle.getTheme(); };
window.setWordleTheme = function (theme) { return window.wordle.setTheme(theme); };
window.wordleCopyShare = function (text) { return window.wordle.copyShareToClipboard(text); };
window.wordleConfetti = function () { return window.wordle.showConfetti(); };
