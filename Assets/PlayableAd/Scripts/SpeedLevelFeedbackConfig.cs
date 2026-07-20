using System;
using UnityEngine;

namespace PlayableAd
{
    [Serializable]
    public sealed class SpeedLevelFeedbackData
    {
        [Range(1, PlayerSpeedSettings.RequiredLevelCount)] public int level = 1;
        public bool isMajorLevel;
        [Range(0f, 1.5f)] public float coreFlashIntensity = 0.45f;
        [Range(4, 48)] public int burstParticleCount = 10;
        [Range(0.5f, 3f)] public float shockwaveScale = 1.15f;
        [Range(0.2f, 0.35f)] public float shockwaveDuration = 0.25f;
        [Range(0.8f, 1.8f)] public float levelBadgeScale = 1f;
        [Range(0.6f, 1f)] public float levelBadgeDuration = 0.75f;
        [Range(0f, 7f)] public float cameraZoomStrength = 2.2f;
        [Range(0f, 0.5f)] public float cameraImpulseStrength = 0.08f;
        [Range(1f, 1.8f)] public float trailBoostMultiplier = 1.18f;
        [Range(1f, 1.8f)] public float airflowBoostMultiplier = 1.12f;
        [Range(0.2f, 0.7f)] public float feedbackDuration = 0.34f;
    }

    [CreateAssetMenu(fileName = "SpeedLevelFeedbackConfig", menuName = "Playable Ad/Speed Level Feedback Config")]
    public sealed class SpeedLevelFeedbackConfig : ScriptableObject
    {
        [Header("Global")]
        [Range(0.1f, 0.5f)] public float normalGainFeedbackStrength = 0.2f;
        [Range(0.5f, 1.5f)] public float levelUpFeedbackStrength = 1f;
        [Range(1f, 1.8f)] public float multiLevelMaxMultiplier = 1.45f;
        [Range(0f, 0.2f)] public float levelUpCooldown = 0.04f;
        [Range(0.15f, 0.4f)] public float uiAnimationDuration = 0.25f;
        public bool cameraFeedbackEnabled = true;
        public bool levelBadgeEnabled = true;
        public bool accessibilityReducedFlash;
        [Range(0, 2)] public int vfxQualityLevel = 2;

        [SerializeField] private SpeedLevelFeedbackData[] levels = CreateDefaults();

        public SpeedLevelFeedbackData Get(int level)
        {
            EnsureValid();
            return levels[Mathf.Clamp(level, 1, levels.Length) - 1];
        }

        private void OnEnable() => EnsureValid();
        private void OnValidate() => EnsureValid();

        private void EnsureValid()
        {
            if (levels == null || levels.Length != PlayerSpeedSettings.RequiredLevelCount)
                levels = CreateDefaults();
        }

        private static SpeedLevelFeedbackData[] CreateDefaults()
        {
            SpeedLevelFeedbackData[] data = new SpeedLevelFeedbackData[PlayerSpeedSettings.RequiredLevelCount];
            for (int i = 0; i < data.Length; i++)
            {
                int level = i + 1;
                float t = i / 9f;
                bool major = level == 4 || level == 7 || level == 9 || level == 10;
                float majorScale = major ? 1.22f : 1f;
                data[i] = new SpeedLevelFeedbackData
                {
                    level = level,
                    isMajorLevel = major,
                    coreFlashIntensity = Mathf.Lerp(0.34f, 0.92f, t) * majorScale,
                    burstParticleCount = Mathf.RoundToInt(Mathf.Lerp(7f, 30f, t) * majorScale),
                    shockwaveScale = Mathf.Lerp(0.9f, 2.15f, t) * majorScale,
                    shockwaveDuration = Mathf.Lerp(0.22f, 0.32f, t),
                    levelBadgeScale = Mathf.Lerp(0.92f, 1.35f, t) * (major ? 1.08f : 1f),
                    levelBadgeDuration = major ? 0.88f : 0.7f,
                    cameraZoomStrength = Mathf.Lerp(1.5f, 5.8f, t) * majorScale,
                    cameraImpulseStrength = Mathf.Lerp(0.04f, 0.18f, t) * majorScale,
                    trailBoostMultiplier = Mathf.Lerp(1.1f, 1.42f, t),
                    airflowBoostMultiplier = Mathf.Lerp(1.06f, 1.38f, t),
                    feedbackDuration = Mathf.Lerp(0.28f, 0.5f, t)
                };
            }
            return data;
        }
    }
}
