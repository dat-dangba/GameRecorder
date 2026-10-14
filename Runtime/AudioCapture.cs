using System;
using System.IO;
using UnityEngine;

namespace GameRecorder
{
    /// <summary>
    /// Thu mix âm thanh của game (PCM float32, interleaved) ra file .raw.
    /// Phải nằm cùng GameObject với AudioListener (FFmpegRecorder tự add nếu chưa có).
    /// </summary>
    public class AudioCapture : MonoBehaviour
    {
        private readonly object _lock = new object();
        private FileStream _fs;
        private byte[] _bytes = new byte[0];
        private volatile int _channels;

        public int SampleRate { get; private set; }
        public int Channels => _channels;

        public void Begin(string rawPath)
        {
            SampleRate = AudioSettings.outputSampleRate;
            lock (_lock)
            {
                _channels = 0;
                _fs = new FileStream(rawPath, FileMode.Create, FileAccess.Write, FileShare.Read, 1 << 16);
            }
        }

        public void End()
        {
            lock (_lock)
            {
                if (_fs == null) return;
                _fs.Flush();
                _fs.Dispose();
                _fs = null;
            }
        }

        // Chạy trên audio thread
        private void OnAudioFilterRead(float[] data, int channels)
        {
            lock (_lock)
            {
                if (_fs == null) return;

                _channels = channels;
                int n = data.Length * sizeof(float);
                if (_bytes.Length < n) _bytes = new byte[n];
                Buffer.BlockCopy(data, 0, _bytes, 0, n);
                _fs.Write(_bytes, 0, n);
            }
        }

        private void OnDestroy()
        {
            End();
        }
    }
}