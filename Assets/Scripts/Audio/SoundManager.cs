using UnityEngine;

namespace CosmicRunner.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;

        private AudioClip jumpClip;
        private AudioClip coinClip;
        private AudioClip laneChangeClip;
        private AudioClip crashClip;
        private AudioClip buttonClickClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            GenerateProceduralSounds();
        }

        public void PlayJumpSound()
        {
            if (jumpClip != null) sfxSource.PlayOneShot(jumpClip, 0.7f);
        }

        public void PlayCoinSound()
        {
            if (coinClip != null) sfxSource.PlayOneShot(coinClip, 0.8f);
        }

        public void PlayLaneChangeSound()
        {
            if (laneChangeClip != null) sfxSource.PlayOneShot(laneChangeClip, 0.4f);
        }

        public void PlayCrashSound()
        {
            if (crashClip != null) sfxSource.PlayOneShot(crashClip, 0.9f);
        }

        public void PlayButtonClick()
        {
            if (buttonClickClip != null) sfxSource.PlayOneShot(buttonClickClip, 0.6f);
        }

        private void GenerateProceduralSounds()
        {
            jumpClip = CreateToneSweep(0.18f, 300f, 750f);
            coinClip = CreateArpeggio(0.15f, new float[] { 880f, 1320f });
            laneChangeClip = CreateToneSweep(0.06f, 400f, 300f);
            crashClip = CreateNoiseBurst(0.35f);
            buttonClickClip = CreateTone(0.04f, 900f);
        }

        // Tạo âm thanh quét tần số (Frequency Sweep)
        private AudioClip CreateToneSweep(float duration, float startFreq, float endFreq)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float wave = Mathf.Sin(2 * Mathf.PI * currentFreq * (i / (float)sampleRate));
                float envelope = 1f - t; // Giảm dần âm lượng
                samples[i] = wave * envelope;
            }

            AudioClip clip = AudioClip.Create("ToneSweep", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Tạo chuỗi hợp âm ngắn (Arpeggio)
        private AudioClip CreateArpeggio(float duration, float[] freqs)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];
            int noteLength = sampleCount / freqs.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                int noteIndex = Mathf.Min(i / noteLength, freqs.Length - 1);
                float freq = freqs[noteIndex];
                float wave = Mathf.Sin(2 * Mathf.PI * freq * (i / (float)sampleRate));
                float decay = 1f - ((float)(i % noteLength) / noteLength);
                samples[i] = wave * decay;
            }

            AudioClip clip = AudioClip.Create("Arpeggio", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Tạo tiếng nổ va chạm (Noise Burst)
        private AudioClip CreateNoiseBurst(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float noise = (Random.value * 2f) - 1f;
                float envelope = Mathf.Exp(-5f * t); // Tiêu hao nhanh
                samples[i] = noise * envelope;
            }

            AudioClip clip = AudioClip.Create("NoiseBurst", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        // Tạo tiếng bip đơn
        private AudioClip CreateTone(float duration, float freq)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleCount;
                float wave = Mathf.Sin(2 * Mathf.PI * freq * (i / (float)sampleRate));
                samples[i] = wave * (1f - t);
            }

            AudioClip clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
