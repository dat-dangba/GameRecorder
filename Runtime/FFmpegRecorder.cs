using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

namespace GameRecorder
{
    /// <summary>
    /// Record game ra mp4 qua FFmpeg pipe (ScreenCapture), có thể kèm audio.
    /// - Cửa sổ game giữ size nhỏ (vd 576x1024), video được upscale lên outWidth x outHeight bằng FFmpeg.
    /// - Có audio: chạy thời gian thực (targetFrameRate) + tự bù/bỏ frame để video không lệch audio.
    /// - Không audio: dùng Time.captureFramerate (mỗi frame game = 1 frame video).
    /// Cần ffmpeg.exe trong StreamingAssets.
    /// </summary>
    public class FFmpegRecorder : MonoBehaviour
    {
        [Header("Video")] [SerializeField] private int fps = 30;
        [SerializeField] private int outWidth = 1080;
        [SerializeField] private int outHeight = 1920;
        [SerializeField, Range(0, 30)] private int crf = 16;

        [Tooltip("Bật nếu video bị lật ngược trên/dưới.")] [SerializeField]
        private bool flipVertical = false;

        [Header("Audio")] [SerializeField] private bool recordAudio = true;

        private AudioCapture audioCapture;

        [Header("Control")] [SerializeField] private KeyCode toggleKey = KeyCode.F9;
        [SerializeField] private string outputFolder = ""; // rỗng = cạnh file exe

        private Process _proc;
        private Stream _stdin;
        private RenderTexture _rt;
        private byte[] _buffer;
        private int _pending; // số readback chưa xong
        private int _srcW, _srcH; // size thật của frame, cache lúc start

        private bool _recording;
        private bool _finishing;
        private bool _useAudio;
        private bool _realtime;

        private Stopwatch _sw;
        private int _written;

        private string _ffmpegPath;
        private string _videoPath, _audioPath, _finalPath;

        private int _prevTargetFps, _prevVSync;
        private bool _timingChanged;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        private UnityEngine.InputSystem.Key _key = UnityEngine.InputSystem.Key.F9;
#endif

        public bool IsRecording => _recording;

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            Application.runInBackground = true;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            if (Enum.TryParse(toggleKey.ToString(), out UnityEngine.InputSystem.Key k)) _key = k;
#endif
            audioCapture = FindAnyObjectByType<AudioCapture>();

            fps = PlayerPrefs.GetInt("record_fps", 30);
            flipVertical = PlayerPrefs.GetInt("record_vflip", 0) == 1;
            recordAudio = PlayerPrefs.GetInt("record_audio", 1) == 1;
        }

        private void Update()
        {
            if (!TogglePressed()) return;
            if (_recording) StopRecording();
            else StartRecording();
        }

        private bool TogglePressed()
        {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb[_key].wasPressedThisFrame;
#else
            return Input.GetKeyDown(toggleKey);
#endif
        }

        // ───────────────────────── Start ─────────────────────────

        public void StartRecording()
        {
            fps = PlayerPrefs.GetInt("record_fps", 30);
            flipVertical = PlayerPrefs.GetInt("record_vflip", 0) == 1;
            recordAudio = PlayerPrefs.GetInt("record_audio", 1) == 1;
            Debug.Log($"datdb - {fps} {flipVertical} {recordAudio}");
            if (_recording || _finishing) return;

            _ffmpegPath = Path.Combine(Application.streamingAssetsPath, "ffmpeg.exe");
            if (!File.Exists(_ffmpegPath))
            {
                Debug.LogError($"[Recorder] Không thấy ffmpeg: {_ffmpegPath}");
                return;
            }

            _srcW = Screen.width;
            _srcH = Screen.height;
            int ow = outWidth & ~1; // yuv420p cần số chẵn
            int oh = outHeight & ~1;

            string dir = string.IsNullOrEmpty(outputFolder)
                ? Path.GetDirectoryName(Application.dataPath)
                : outputFolder;
            Directory.CreateDirectory(dir);

            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _finalPath = Path.Combine(dir, $"capture_{stamp}.mp4");
            _audioPath = Path.Combine(dir, $"tmp_{stamp}_audio.raw");

            _useAudio = recordAudio && SetupAudio();
            _videoPath = _useAudio ? Path.Combine(dir, $"tmp_{stamp}_video.mp4") : _finalPath;

            string vf = (flipVertical ? "vflip," : "") + $"scale={ow}:{oh}:flags=lanczos";
            string args =
                $"-y -loglevel error -f rawvideo -pix_fmt rgba -s {_srcW}x{_srcH} -r {fps} -i - " +
                $"-vf {vf} -c:v libx264 -preset veryfast -crf {crf} -pix_fmt yuv420p \"{_videoPath}\"";

            try
            {
                _proc = StartFFmpeg(args, true);
                _stdin = _proc.StandardInput.BaseStream;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Recorder] Không start được ffmpeg: {e.Message}");
                _proc = null;
                _stdin = null;
                return;
            }

            _rt = new RenderTexture(_srcW, _srcH, 0, RenderTextureFormat.ARGB32);
            _buffer = new byte[_srcW * _srcH * 4];
            _pending = 0;
            _written = 0;

            // Có audio -> chạy realtime. Không audio -> captureFramerate cho mượt tuyệt đối.
            _realtime = _useAudio;
            ApplyTiming();

            if (_useAudio) audioCapture.Begin(_audioPath);
            _sw = Stopwatch.StartNew();

            _recording = true;
            StartCoroutine(CaptureLoop());
            Debug.Log($"[Recorder] Start {_srcW}x{_srcH} -> {ow}x{oh} @{fps}fps, audio={_useAudio}");
        }

        private bool SetupAudio()
        {
            if (audioCapture != null) return true;

#if UNITY_2022_2_OR_NEWER
            var listener = FindFirstObjectByType<AudioListener>();
#else
            var listener = FindObjectOfType<AudioListener>();
#endif
            if (listener == null)
            {
                Debug.LogWarning("[Recorder] Không có AudioListener, record không tiếng");
                return false;
            }

            audioCapture = listener.GetComponent<AudioCapture>();
            if (audioCapture == null) audioCapture = listener.gameObject.AddComponent<AudioCapture>();
            return true;
        }

        private void ApplyTiming()
        {
            if (_realtime)
            {
                _prevTargetFps = Application.targetFrameRate;
                _prevVSync = QualitySettings.vSyncCount;
                QualitySettings.vSyncCount = 0;
                Application.targetFrameRate = fps;
                _timingChanged = true;
            }
            else
            {
                Time.captureFramerate = fps;
            }
        }

        private void RestoreTiming()
        {
            Time.captureFramerate = 0;
            if (!_timingChanged) return;
            Application.targetFrameRate = _prevTargetFps;
            QualitySettings.vSyncCount = _prevVSync;
            _timingChanged = false;
        }

        // ───────────────────────── Capture ─────────────────────────

        private IEnumerator CaptureLoop()
        {
            while (_recording)
            {
                yield return new WaitForEndOfFrame();
                if (!_recording) break;

                ScreenCapture.CaptureScreenshotIntoRenderTexture(_rt);
                _pending++;
                AsyncGPUReadback.Request(_rt, 0, TextureFormat.RGBA32, OnReadback);
            }
        }

        private void OnReadback(AsyncGPUReadbackRequest req)
        {
            _pending--;
            if (req.hasError || _stdin == null) return;
            if (req.width != _srcW || req.height != _srcH) return; // bỏ frame lệch size

            var data = req.GetData<byte>();
            if (data.Length != _buffer.Length) return;
            data.CopyTo(_buffer);

            int count = 1;
            if (_realtime)
            {
                // Giữ video đúng thời lượng thật để khớp audio:
                // tụt fps -> ghi lặp frame, nhanh hơn fps -> bỏ frame.
                int expected = (int)(_sw.Elapsed.TotalSeconds * fps) + 1;
                count = Mathf.Clamp(expected - _written, 0, 10);
            }

            try
            {
                for (int i = 0; i < count; i++) _stdin.Write(_buffer, 0, _buffer.Length);
                _written += count;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Recorder] Write lỗi: {e.Message}");
            }
        }

        // ───────────────────────── Stop ─────────────────────────

        public void StopRecording()
        {
            if (!_recording) return;
            _recording = false;
            _sw?.Stop();
            if (_useAudio) audioCapture.End(); // cắt audio ngay lúc stop để khớp video
            StartCoroutine(FinishRoutine());
        }

        private IEnumerator FinishRoutine()
        {
            _finishing = true;

            while (_pending > 0) yield return null; // đợi readback cuối
            RestoreTiming();

            try
            {
                _stdin?.Flush();
                _stdin?.Close();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Recorder] {e.Message}");
            }

            yield return WaitExit(_proc, 60f);

            _stdin = null;
            _proc = null;
            if (_rt != null)
            {
                _rt.Release();
                Destroy(_rt);
                _rt = null;
            }

            if (_useAudio) yield return MuxRoutine();

            Debug.Log($"[Recorder] Done -> {_finalPath}");
            _finishing = false;
        }

        private IEnumerator MuxRoutine()
        {
            var info = new FileInfo(_audioPath);
            if (!info.Exists || info.Length == 0 || audioCapture.Channels <= 0)
            {
                Debug.LogWarning("[Recorder] Không có dữ liệu audio, giữ video không tiếng");
                SafeMove(_videoPath, _finalPath);
                SafeDelete(_audioPath);
                yield break;
            }

            string args =
                $"-y -loglevel error -i \"{_videoPath}\" " +
                $"-f f32le -ar {audioCapture.SampleRate} -ac {audioCapture.Channels} -i \"{_audioPath}\" " +
                $"-c:v copy -c:a aac -b:a 192k -shortest \"{_finalPath}\"";

            Process mux = null;
            try
            {
                mux = StartFFmpeg(args, false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Recorder] Mux lỗi: {e.Message}");
            }

            if (mux == null)
            {
                SafeMove(_videoPath, _finalPath);
                SafeDelete(_audioPath);
                yield break;
            }

            yield return WaitExit(mux, 120f);

            bool ok = mux.HasExited && mux.ExitCode == 0 && File.Exists(_finalPath);
            if (ok)
            {
                SafeDelete(_videoPath);
                SafeDelete(_audioPath);
            }
            else
            {
                Debug.LogWarning($"[Recorder] Mux thất bại, giữ file tạm: {_videoPath}, {_audioPath}");
            }
        }

        // ───────────────────────── Helpers ─────────────────────────

        private Process StartFFmpeg(string args, bool redirectStdin)
        {
            var p = new Process
            {
                StartInfo = new ProcessStartInfo(_ffmpegPath, args)
                {
                    UseShellExecute = false,
                    RedirectStandardInput = redirectStdin,
                    CreateNoWindow = true
                }
            };
            p.Start();
            return p;
        }

        private static IEnumerator WaitExit(Process p, float timeoutSeconds)
        {
            float t = 0f;
            while (p != null && !p.HasExited && t < timeoutSeconds)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private static void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Recorder] Không xóa được {path}: {e.Message}");
            }
        }

        private static void SafeMove(string from, string to)
        {
            try
            {
                if (from == to || !File.Exists(from)) return;
                if (File.Exists(to)) File.Delete(to);
                File.Move(from, to);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Recorder] Không move được file: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            _recording = false;
            RestoreTiming();

            try
            {
                if (audioCapture != null) audioCapture.End();
            }
            catch
            {
            }

            try
            {
                _stdin?.Close();
            }
            catch
            {
            }

            try
            {
                if (_proc != null && !_proc.HasExited) _proc.Kill();
            }
            catch
            {
            }

            if (_rt != null) _rt.Release();
        }
    }
}