using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProceduralAudioGenerator : MonoBehaviour
{
    public enum WaveformType { Sine, Square }

    [Header("Audio Settings")]
    [SerializeField] private WaveformType waveform = WaveformType.Square;
    [SerializeField, Range(0, 0.4f)] private float amplitude = 0.2f;
    [SerializeField] private float bpm = 120f;

    // C Minor Pentatonic Scale Frequencies (Hz)
    private readonly float[] scaleFrequencies = { 261.63f, 277.2f, 293.7f, 311.13f, 329.6f, 349.23f, 370.0f, 392.00f, };

    private float frequency = 261.63f;
    private double _phase;
    private int _sampleRate;

    private float beatInterval;
    private float beatTimer;
    private bool isSoundingNote = false;
    private float noteDurationTimer = 0f;

    public static event Action OnBeatTick;

    private void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
        beatInterval = 60f / bpm;
    }

    private void Update()
    {
        beatTimer += Time.deltaTime;
        if (beatTimer >= beatInterval)
        {
            beatTimer -= beatInterval;
            TriggerProceduralNote();
        }

        if (isSoundingNote)
        {
            noteDurationTimer -= Time.deltaTime;
            if (noteDurationTimer <= 0)
            {
                isSoundingNote = false;
            }
        }
    }

    private void TriggerProceduralNote()
    {
        int randomIndex = UnityEngine.Random.Range(0, scaleFrequencies.Length);
        frequency = scaleFrequencies[randomIndex];

        isSoundingNote = true;
        noteDurationTimer = 0.5f; // 500ms burst

        OnBeatTick?.Invoke();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isSoundingNote)
        {
            Array.Clear(data, 0, data.Length);
            return;
        }

        double phaseIncrement = frequency / _sampleRate;

        for (int sample = 0; sample < data.Length; sample += channels)
        {
            float value = 0f;

            if (waveform == WaveformType.Sine)
            {
                value = Mathf.Sin((float)(_phase * 2.0 * Mathf.PI)) * amplitude;
            }
            else if (waveform == WaveformType.Square)
            {
                // Square wave math: 1.0 when phase < 0.5, -1.0 when phase >= 0.5
                value = (_phase < 0.5 ? 1.0f : -1.0f) * amplitude;
            }

            _phase = (_phase + phaseIncrement) % 1.0;

            for (int channel = 0; channel < channels; channel++)
            {
                data[sample + channel] = value;
            }
        }
    }

    public float GetBeatInterval() => beatInterval;
}