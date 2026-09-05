using UnityEngine;


namespace Chip8.Sound
{
    public class Audio : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _beepFrequency = 440f;

        private AudioClip _beepClip;

        private void Awake()
        {
            const int sampleRate = 44100;
            const float duration = 1f;

            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float time = (float)i / sampleRate;

                float wave = Mathf.Sin(2f * Mathf.PI * _beepFrequency * time);

                samples[i] = wave >= 0f ? 0.25f : -0.25f;
            }

            _beepClip = AudioClip.Create("Beep", sampleCount, 1, sampleRate, false);

            _beepClip.SetData(samples, 0);

            _audioSource.clip = _beepClip;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
        }

        public void Play()
        {
            if (!_audioSource.isPlaying)
            {
                _audioSource.Play();
            }
        }

        public void Stop()
        {
            if (_audioSource.isPlaying)
            {
                _audioSource.Stop();
            }
        }
    }
}