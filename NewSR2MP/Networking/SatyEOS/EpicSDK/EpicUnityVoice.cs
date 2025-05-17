/*using Epic.OnlineServices;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NewSR2MP.EpicSDK
{
    [RegisterTypeInIl2Cpp(false)]
    public class EpicUnityVoice : MonoBehaviour
    {
        public AudioSource VoiceSource;
        private short[] _currentVoiceFrame;
        private int _voiceFrameIndex;
        private const int AUDIO_SAMPLE_RATE = 48000;
        private ConcurrentQueue<short[]> _audioFrameQueue;
        private bool _catchUp;

        private void Awake()
        {
            _audioFrameQueue = new ConcurrentQueue<short[]>();
        }

        private void Start()
        {
            if (VoiceSource is null)
            {
                Debug.LogError("Voice source does not exist");
                return;
            }
            VoiceSource.clip = AudioClip.Create("voice", 480, 1, AUDIO_SAMPLE_RATE, true);
            VoiceSource.clip.
            VoiceSource.Play();
        }

        private void OnAudioRead(float[] data)
        {
            //ModEntry.Log(_audioFrameQueue?.Count);
            if (_audioFrameQueue?.Count > AUDIO_SAMPLE_RATE / 1000 || _catchUp)
            {
                _catchUp = true;
                _audioFrameQueue?.TryDequeue(out short[] _);
                _catchUp = _audioFrameQueue?.Count <= 20;
            }
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = 0;
                if (_audioFrameQueue is null) continue;
                if (_currentVoiceFrame is null || _voiceFrameIndex >= _currentVoiceFrame.Length)
                {
                    if (!_audioFrameQueue.TryDequeue(out short[] frame)) continue;
                    _voiceFrameIndex = 0;
                    _currentVoiceFrame = frame;
                }

                data[i] = _currentVoiceFrame[_voiceFrameIndex++] / (float)short.MaxValue;
            }
        }

        public void EnqueueAudioFrame(short[] frames)
        {
            //ModEntry.Log($"Adding {frames.Length} frames");
            if (_audioFrameQueue?.Count > AUDIO_SAMPLE_RATE / 500) // Clear frames if it's way over the queue
            {
                _audioFrameQueue = new ConcurrentQueue<short[]>();
            }
            _audioFrameQueue?.Enqueue(frames);
        }
    }
}
*/