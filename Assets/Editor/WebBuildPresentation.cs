using System.IO;

/// <summary>Applies the same portable browser presentation after every WebGL build.</summary>
public static class WebBuildPresentation
{
    public static void Apply()
    {
        const string root = "Build/WebGL/";
        string path = root + "index.html";
        string html = File.ReadAllText(path);
        html = html.Replace("// config.autoSyncPersistentDataPath = true;", "config.autoSyncPersistentDataPath = true;");
        html = html.Replace("</head>", "<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\"><link rel=\"stylesheet\" href=\"responsive.css\"></head>");
        html = html.Replace("</body>", "<script src=\"responsive.js\"></script></body>");
        File.WriteAllText(path, html);
        File.WriteAllText(root + "responsive.css", @"
body { background: #100c25; color: #f9dfab; overflow: hidden; }
#unity-container { width: max-content; }
#unity-footer { height: 38px; background: #100c25; }
#unity-build-title { font-size: 14px; }
#unity-fullscreen-container:fullscreen { width: 100vw !important; height: 100vh !important; }
");
        File.WriteAllText(root + "responsive.js", @"
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
");
    }
}
