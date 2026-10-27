using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralAudioGenerator : MonoBehaviour
{
    public enum WaveformType { Square, Sine, Triangle }

    [Header("Audio Settings")]
    [SerializeField] private WaveformType waveform = WaveformType.Square;
    [SerializeField, Range(0, 0.4f)] private float amplitude = 0.15f;
    [SerializeField] private float bpm = 120f;

    [Header("Melodic Variety")]
    [SerializeField] private int beatsPerScaleChange = 16;

    private readonly float[][] musicalScales = new float[][]
    {
        new float[] { 261.63f, 311.13f, 349.23f, 392.00f, 466.16f, 523.25f }, // C Minor Pentatonic
        new float[] { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f }, // C Major Pentatonic
        new float[] { 261.63f, 293.66f, 311.13f, 349.23f, 392.00f, 440.00f, 466.16f }, // C Dorian
        new float[] { 261.63f, 311.13f, 349.23f, 370.00f, 392.00f, 466.16f }  // C Blues
    };

    private float[] currentScale;
    private AudioSource audioSource;
    private float beatInterval;
    private float beatTimer;
    private int totalBeatsTicked = 0;
    private const int sampleRate = 44100;

    public static event Action OnBeatTick;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        beatInterval = 60f / bpm;
        SelectRandomScale();
    }

    private void Update()
    {
        beatTimer += Time.deltaTime;
        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval;
            totalBeatsTicked++;

            if (totalBeatsTicked % beatsPerScaleChange == 0)
            {
                SelectRandomScale();
            }

            PlayProceduralNote();
        }
    }

    private void SelectRandomScale()
    {
        int randomIndex = UnityEngine.Random.Range(0, musicalScales.Length);
        currentScale = musicalScales[randomIndex];
    }

    private void PlayProceduralNote()
    {
        if (currentScale == null || currentScale.Length == 0) return;

        // Wake up browser audio context if paused
        if (AudioListener.pause) AudioListener.pause = false;

        // Pick pitch from current scale
        int noteIndex = UnityEngine.Random.Range(0, currentScale.Length);
        float frequency = currentScale[noteIndex];

        if (UnityEngine.Random.value > 0.80f)
        {
            frequency *= 2.0f; // Octave jump accent
        }

        // Procedurally generate tone in RAM
        float duration = 0.14f;
        int sampleCount = (int)(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            double phase = (double)i * frequency / sampleRate;
            float envelope = 1.0f - ((float)i / sampleCount); // Smooth decay
            float value = 0f;

            switch (waveform)
            {
                case WaveformType.Sine:
                    value = Mathf.Sin((float)(phase * 2.0 * Mathf.PI)) * amplitude * envelope;
                    break;
                case WaveformType.Square:
                    value = ((phase % 1.0) < 0.5 ? 1.0f : -1.0f) * amplitude * envelope;
                    break;
                case WaveformType.Triangle:
                    value = (Mathf.PingPong((float)(phase * 2.0), 1.0f) - 0.5f) * 2.0f * amplitude * envelope;
                    break;
            }

            samples[i] = value;
        }

        // Create temporary procedural clip and play
        AudioClip toneClip = AudioClip.Create("ProceduralNote", sampleCount, 1, sampleRate, false);
        toneClip.SetData(samples, 0);

        audioSource.PlayOneShot(toneClip);
        OnBeatTick?.Invoke();
    }

    public float GetBeatInterval() => beatInterval;
}