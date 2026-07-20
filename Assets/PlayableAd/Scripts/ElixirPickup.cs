using System;
using UnityEngine;

namespace PlayableAd
{
    public sealed class ElixirPickup : MonoBehaviour
    {
        [SerializeField, Range(1, PlayerSpeedSettings.RequiredLevelCount)] private int targetSpeedLevel = 4;
        [SerializeField] private Collider[] pickupColliders = Array.Empty<Collider>();
        [SerializeField] private bool hasCollected;

        private PlayerSpeedController speedController;

        public event Action<ElixirPickup> Collected;

        public bool HasCollected => hasCollected;

        public void Initialize(PlayerSpeedController controller, int targetLevel, Collider[] colliders)
        {
            speedController = controller;
            targetSpeedLevel = Mathf.Clamp(targetLevel, 1, controller != null ? controller.MaxLevel : PlayerSpeedSettings.RequiredLevelCount);
            pickupColliders = colliders ?? Array.Empty<Collider>();
            hasCollected = false;
        }

        public bool TryCollect()
        {
            if (hasCollected || speedController == null) return false;

            hasCollected = true;
            for (int i = 0; i < pickupColliders.Length; i++)
                if (pickupColliders[i] != null) pickupColliders[i].enabled = false;
            float targetSpeed = speedController.GetLevelStartSpeed(targetSpeedLevel);
            speedController.SetSpeed(Mathf.Max(speedController.CurrentSpeed, targetSpeed), SpeedChangeReason.PotionPickup, this);
            Collected?.Invoke(this);
            return true;
        }
    }
}
