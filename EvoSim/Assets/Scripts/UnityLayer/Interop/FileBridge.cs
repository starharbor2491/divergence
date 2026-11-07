using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Evo.UnityLayer
{
    /// <summary>
    /// Downloads files and picks a file to load. In a WebGL build this calls FileBridge.jslib.
    /// In the Editor, downloads go to a folder under persistentDataPath and "pick" loads the newest saved Run there,
    /// so everything can be tested without a browser (PLAN 14.5).
    /// </summary>
    public sealed class FileBridge : MonoBehaviour
    {
        public const string ObjectName = "EvoFileBridge";
        public const string CallbackMethod = "OnFilePicked";

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void DownloadBytes(string name, string mime, byte[] data, int length);
        [DllImport("__Internal")] static extern float PixelRatio();
        [DllImport("__Internal")] static extern void ArmPicker(string accept);
        [DllImport("__Internal")] static extern void PickFile(string callbackObject, string callbackMethod, string accept);
#endif

        static FileBridge instance;
        Action<string> pending;

        public static string ExportFolder
        {
            get { return Path.Combine(Application.persistentDataPath, "exports"); }
        }

        public static FileBridge Create()
        {
            if (instance != null) return instance;
            var go = new GameObject(ObjectName);
            DontDestroyOnLoad(go);
            instance = go.AddComponent<FileBridge>();
            return instance;
        }

        /// <summary>Device pixels per CSS pixel. The window-size rule counts CSS pixels, as a person sees the window.</summary>
        public static float DevicePixelRatio
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return Mathf.Max(1f, PixelRatio());
#else
                return 1f;
#endif
            }
        }

        public static void Download(string fileName, string mime, byte[] data)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            DownloadBytes(fileName, mime, data, data.Length);
#else
            Directory.CreateDirectory(ExportFolder);
            File.WriteAllBytes(Path.Combine(ExportFolder, fileName), data);
#endif
        }

        /// <summary>Call on pointer down of a button that will call PickText, so the browser accepts the dialog (see FileBridge.jslib).</summary>
        public static void ArmPick(string accept)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ArmPicker(accept);
#endif
        }

        /// <summary>Opens a file chooser and returns the chosen file's text, or an empty string when cancelled.</summary>
        public static void PickText(string accept, Action<string> onText)
        {
            Create().pending = onText;
#if UNITY_WEBGL && !UNITY_EDITOR
            PickFile(ObjectName, CallbackMethod, accept);
#else
            string text = "";
            if (Directory.Exists(ExportFolder))
            {
                string[] files = Directory.GetFiles(ExportFolder, "evo-run-*.json");
                Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));
                if (files.Length > 0) text = File.ReadAllText(files[0]);
            }
            instance.OnFilePicked(text);
#endif
        }

        /// <summary>Called by the browser (SendMessage) with the file text.</summary>
        public void OnFilePicked(string text)
        {
            Action<string> callback = pending;
            pending = null;
            if (callback != null) callback(text ?? "");
        }
    }
}
