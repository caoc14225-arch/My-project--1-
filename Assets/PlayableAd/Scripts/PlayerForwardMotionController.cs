using UnityEngine;

namespace PlayableAd
{
    [DisallowMultipleComponent]
    public sealed class PlayerForwardMotionController : MonoBehaviour
    {
        private PlayerSpeedController speedController;
        private PlayerSpeedSettings settings;
        private SpeedChangeReason lastChangeReason = SpeedChangeReason.InitialSetup;

        public float TargetForwardSpeed { get; private set; }
        public float CurrentForwardSpeed { get; private set; }
        public float NormalizedActualSpeed { get; private set; }

        public void Initialize(PlayerSpeedController controller, PlayerSpeedSettings speedSettings)
        {
            speedController = controller;
            settings = speedSettings;
            TargetForwardSpeed = speedController.GetForwardSpeed();
            CurrentForwardSpeed = TargetForwardSpeed;
            UpdateNormalizedSpeed();
            speedController.SpeedChanged += OnSpeedChanged;
        }

        public float Tick(float deltaTime, bool movementActive)
        {
            if (speedController == null || settings == null)
            {
                return 0f;
            }

            if (!movementActive)
            {
                TargetForwardSpeed = speedController.GetForwardSpeed();
                UpdateNormalizedSpeed();
                return 0f;
            }

            TargetForwardSpeed = speedController.GetForwardSpeed();
            float response = SelectResponse(TargetForwardSpeed >= CurrentForwardSpeed);
            CurrentForwardSpeed = Mathf.MoveTowards(CurrentForwardSpeed, TargetForwardSpeed, response * Mathf.Max(0f, deltaTime));
            UpdateNormalizedSpeed();
            return CurrentForwardSpeed;
        }

        public void SnapToTarget()
        {
            if (speedController == null) return;
            TargetForwardSpeed = speedController.GetForwardSpeed();
            CurrentForwardSpeed = TargetForwardSpeed;
            UpdateNormalizedSpeed();
        }

        private float SelectResponse(bool accelerating)
        {
            if (accelerating)
            {
                return lastChangeReason == SpeedChangeReason.TutorialElixir || lastChangeReason == SpeedChangeReason.PotionPickup
                    ? settings.specialUpgradeAcceleration
                    : settings.baseAcceleration;
            }

            return lastChangeReason == SpeedChangeReason.ObstaclePenalty || lastChangeReason == SpeedChangeReason.HighLevelCollisionPenalty
                ? settings.penaltyDeceleration
                : settings.naturalDeceleration;
        }

        private void UpdateNormalizedSpeed()
        {
            float minimum = settings != null && settings.forwardSpeeds != null && settings.forwardSpeeds.Length > 0
                ? settings.forwardSpeeds[0]
                : 0f;
            float maximum = settings != null && settings.forwardSpeeds != null && settings.forwardSpeeds.Length > 0
                ? settings.forwardSpeeds[settings.forwardSpeeds.Length - 1]
                : 1f;
            NormalizedActualSpeed = Mathf.Clamp01(Mathf.InverseLerp(minimum, maximum, CurrentForwardSpeed));
        }

        private void OnSpeedChanged(SpeedChangedEvent change)
        {
            lastChangeReason = change.Reason;
        }

        private void OnDestroy()
        {
            if (speedController != null) speedController.SpeedChanged -= OnSpeedChanged;
        }
    }
}
