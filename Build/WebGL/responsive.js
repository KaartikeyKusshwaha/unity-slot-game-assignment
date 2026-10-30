
(function () {
  function fitGame() {
    if (document.fullscreenElement) return;
    const width = Math.max(1, Math.min(1280, window.innerWidth, (window.innerHeight - 38) * 1.6));
    const container = document.getElementById('unity-container');
    const surface = document.getElementById('unity-fullscreen-container');
    container.className = 'unity-desktop';
    surface.style.width = width + 'px';
    surface.style.height = width / 1.6 + 'px';
  }
  window.addEventListener('resize', fitGame);
  document.addEventListener('fullscreenchange', fitGame);
  fitGame();
})();
