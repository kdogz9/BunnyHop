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
    [Tooltip("Number of beats before switching to a new scale key/mode for dynamic variety.")]
    [SerializeField] private int beatsPerScaleChange = 16;

    // Defined Musical Scales (Frequencies in Hz)
    private readonly float[][] musicalScales = new float[][]
    {
        // 0: C Minor Pentatonic (Classic Retro / Dark)
        new float[] { 261.63f, 311.13f, 349.23f, 392.00f, 466.16f, 523.25f },

        // 1: C Major Pentatonic (Upbeat / Happy Bunny Vibe)
        new float[] { 261.63f, 293.66f, 329.63f, 392.00f, 440.00f, 523.25f },

        // 2: C Dorian Scale (Funky / Adventure)
        new float[] { 261.63f, 293.66f, 311.13f, 349.23f, 392.00f, 440.00f, 466.16f },

        // 3: C Blues Scale (Playful / Groovy)
        new float[] { 261.63f, 311.13f, 349.23f, 370.00f, 392.00f, 466.16f }
    };

    private float[] currentScale;
    private float frequency = 261.63f;
    private double _phase;
    private int _sampleRate;

    private float beatInterval;
    private float beatTimer;
    private int totalBeatsTicked = 0;

    // Envelope / Note Control
    private bool isSoundingNote = false;
    private float noteDurationTimer = 0f;
    private float noteTotalDuration = 0.14f;

    public static event Action OnBeatTick;

    private void Awake()
    {
        _sampleRate = AudioSettings.outputSampleRate;
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

            // Periodically switch scales to keep the music fresh
            if (totalBeatsTicked % beatsPerScaleChange == 0)
            {
                SelectRandomScale();
            }

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

    private void SelectRandomScale()
    {
        int randomIndex = UnityEngine.Random.Range(0, musicalScales.Length);
        currentScale = musicalScales[randomIndex];
    }

    private void TriggerProceduralNote()
    {
        if (currentScale == null || currentScale.Length == 0) return;

        // Pick a random pitch from the currently selected scale
        int noteIndex = UnityEngine.Random.Range(0, currentScale.Length);
        float basePitch = currentScale[noteIndex];

        // 20% chance to jump an octave higher for melodic accents
        if (UnityEngine.Random.value > 0.80f)
        {
            basePitch *= 2.0f; // Octave up
        }

        frequency = basePitch;
        isSoundingNote = true;
        noteDurationTimer = noteTotalDuration;

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
            // Simple decay envelope calculation so notes fade out smoothly
            float envelope = Mathf.Clamp01(noteDurationTimer / noteTotalDuration);
            float currentAmplitude = amplitude * envelope;

            float value = 0f;

            switch (waveform)
            {
                case WaveformType.Sine:
                    value = Mathf.Sin((float)(_phase * 2.0 * Mathf.PI)) * currentAmplitude;
                    break;

                case WaveformType.Square:
                    value = (_phase < 0.5 ? 1.0f : -1.0f) * currentAmplitude;
                    break;

                case WaveformType.Triangle:
                    // Triangle wave math for a softer retro chime
                    value = (Mathf.PingPong((float)(_phase * 2.0), 1.0f) - 0.5f) * 2.0f * currentAmplitude;
                    break;
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