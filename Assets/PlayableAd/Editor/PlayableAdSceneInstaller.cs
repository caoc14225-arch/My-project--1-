using System.Linq;
using System.Reflection;
using PlayableAd;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PlayableAdEditor
{
    [InitializeOnLoad]
    internal static class PlayableAdSceneInstaller
    {
        static PlayableAdSceneInstaller()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        [MenuItem("Playable Ad/Install Demo Scene")]
        private static void EnsureScene()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || string.IsNullOrEmpty(scene.path))
            {
                return;
            }

            PlayableAdGame game = Object.FindObjectOfType<PlayableAdGame>();
            if (game == null)
            {
                GameObject root = new GameObject("PlayableAdRoot");
                root.AddComponent<PlayableAdGame>();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Playable Ad demo root installed in " + scene.path);
            }

            ConfigurePlayer(scene.path);
            EnsureUnityMcpConnection();
        }

        private static void ConfigurePlayer(string scenePath)
        {
            PlayerSettings.productName = "Shieldguard: Speed Rescue";
            PlayerSettings.defaultScreenWidth = 1080;
            PlayerSettings.defaultScreenHeight = 1920;
            PlayerSettings.defaultWebScreenWidth = 1080;
            PlayerSettings.defaultWebScreenHeight = 1920;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.resizableWindow = false;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.runInBackground = false;
            PlayerSettings.visibleInBackground = false;

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (!scenes.Any(item => item.path == scenePath))
            {
                EditorBuildSettings.scenes = scenes
                    .Concat(new[] { new EditorBuildSettingsScene(scenePath, true) })
                    .ToArray();
            }
        }

        private static void EnsureUnityMcpConnection()
        {
            EditorPrefs.SetBool("MCPForUnity.AutoStartOnLoad", true);
            EditorPrefs.SetBool("MCPForUnity.UseHttpTransport", true);

            System.Type reconnectType = System.Type.GetType(
                "MCPForUnity.Editor.Services.HttpAutoStartHandler, MCPForUnity.Editor");
            MethodInfo reconnect = reconnectType?.GetMethod(
                "TryBeginReconnect",
                BindingFlags.Static | BindingFlags.NonPublic);
            reconnect?.Invoke(null, null);
        }
    }
}
