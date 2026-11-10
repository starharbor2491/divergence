using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

namespace Evo.UnityLayer
{
    public enum FilePickStatus { Success, Cancelled, Error }

    public struct FilePickResult
    {
        public FilePickStatus Status;
        public string Text;
    }

    /// <summary>Downloads and request-scoped file selection. Browser regions open the chooser in the original click gesture.</summary>
    public sealed class FileBridge : MonoBehaviour
    {
        public const string ObjectName = "EvoFileBridge";
        public const string CallbackMethod = "OnFilePicked";

        [Serializable]
        sealed class PickEnvelope { public int id; public string status; public string text; }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void DownloadBytes(string name, string mime, byte[] data, int length);
        [DllImport("__Internal")] static extern float PixelRatio();
        [DllImport("__Internal")] static extern void PickerRegion(int slot, float x, float y, float width, float height, int enabled);
        [DllImport("__Internal")] static extern void CancelPicker(int request);
        [DllImport("__Internal")] static extern void ProtectRunOnExit(int enabled);
#endif
        static FileBridge instance;
        static int nextSlot;
        static readonly Dictionary<int, Action> bindings = new Dictionary<int, Action>();
        Action<FilePickResult> pending;
        int pendingId, nativeId;

        public static string ExportFolder { get { return Path.Combine(Application.persistentDataPath, "exports"); } }

        public static FileBridge Create()
        {
            if (instance != null) return instance;
            var go = new GameObject(ObjectName);
            if (Application.isPlaying) DontDestroyOnLoad(go);
            instance = go.AddComponent<FileBridge>();
            return instance;
        }

        public static float ValidPixelRatio(float ratio)
        {
            return float.IsNaN(ratio) || float.IsInfinity(ratio) || ratio <= 0f ? 1f : ratio;
        }

        public static void ProtectRun(bool enabled)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ProtectRunOnExit(enabled ? 1 : 0);
#endif
        }

        public static float DevicePixelRatio
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                return ValidPixelRatio(PixelRatio());
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

        public static void BindPicker(EvoButton button, Action request, Func<bool> allowed)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int slot = ++nextSlot;
            Func<bool> available = () =>
            {
                if (!allowed() || button.panel == null) return false;
                for (VisualElement e = button; e != null; e = e.parent)
                    if (e.style.display == DisplayStyle.None || e.resolvedStyle.display == DisplayStyle.None ||
                        e.style.visibility == Visibility.Hidden || e.resolvedStyle.visibility == Visibility.Hidden) return false;
                return true;
            };
            Action invoke = () => { if (available()) request(); };
            bindings[slot] = invoke;
            Action update = () =>
            {
                bool enabled = available();
                button.Disabled = !allowed();
                Rect bounds = button.worldBound;
                Rect panel = button.panel != null ? button.panel.visualTree.worldBound : new Rect(0, 0, 1, 1);
                PickerRegion(slot, (bounds.x - panel.x) / panel.width, (bounds.y - panel.y) / panel.height,
                    bounds.width / panel.width, bounds.height / panel.height, enabled ? 1 : 0);
            };
            button.RegisterCallback<AttachToPanelEvent>(e => { bindings[slot] = invoke; update(); });
            button.RegisterCallback<DetachFromPanelEvent>(e => { bindings.Remove(slot); PickerRegion(slot, 0, 0, 0, 0, 0); });
            button.RegisterCallback<GeometryChangedEvent>(e => update());
            button.schedule.Execute(update).Every(100);
#else
            button.Clicked += () => { if (allowed()) request(); };
#endif
        }

        /// <summary>The browser sends this before opening the native chooser in that same user gesture.</summary>
        public void OnPickerRequested(string request)
        {
            string[] parts = request.Split(':');
            int slot, id;
            Action action;
            if (parts.Length != 2 || !int.TryParse(parts[0], out slot) || !int.TryParse(parts[1], out id)) return;
            if (pending != null || !bindings.TryGetValue(slot, out action))
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                CancelPicker(id);
#endif
                return;
            }
            nativeId = id;
            action();
            nativeId = 0;
            if (pending == null)
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                CancelPicker(id);
#endif
            }
        }

        public static void PickText(string accept, Action<FilePickResult> onResult)
        {
            FileBridge bridge = Create();
            if (bridge.pending != null) return;
            bridge.pending = onResult;
#if UNITY_WEBGL && !UNITY_EDITOR
            bridge.pendingId = bridge.nativeId;
            if (bridge.pendingId == 0)
            {
                bridge.pending = null;
                onResult(new FilePickResult { Status = FilePickStatus.Error });
            }
#else
            FilePickResult result = new FilePickResult { Status = FilePickStatus.Cancelled };
            try
            {
                if (Directory.Exists(ExportFolder))
                {
                    string[] files = Directory.GetFiles(ExportFolder, "evo-run-*.json");
                    Array.Sort(files, (a, b) => File.GetLastWriteTimeUtc(b).CompareTo(File.GetLastWriteTimeUtc(a)));
                    if (files.Length > 0)
                        result = new FileInfo(files[0]).Length > Evo.Core.SnapshotValidation.MaxFileBytes ?
                            new FilePickResult { Status = FilePickStatus.Error } :
                            new FilePickResult { Status = FilePickStatus.Success, Text = File.ReadAllText(files[0]) };
                }
            }
            catch (IOException) { result.Status = FilePickStatus.Error; }
            catch (UnauthorizedAccessException) { result.Status = FilePickStatus.Error; }
            bridge.pending = null;
            onResult(result);
#endif
        }

        public static void CancelPick()
        {
            if (instance == null) return;
#if UNITY_WEBGL && !UNITY_EDITOR
            if (instance.pendingId != 0) CancelPicker(instance.pendingId);
#endif
            instance.pending = null;
            instance.pendingId = 0;
        }

        public void OnFilePicked(string json)
        {
            PickEnvelope result;
            try { result = JsonUtility.FromJson<PickEnvelope>(json); }
            catch (ArgumentException) { return; }
            if (result == null || pending == null || result.id != pendingId) return;
            Action<FilePickResult> callback = pending;
            pending = null;
            pendingId = 0;
            callback(new FilePickResult
            {
                Status = result.status == "success" ? FilePickStatus.Success : result.status == "cancelled" ? FilePickStatus.Cancelled : FilePickStatus.Error,
                Text = result.text
            });
        }
    }
}
