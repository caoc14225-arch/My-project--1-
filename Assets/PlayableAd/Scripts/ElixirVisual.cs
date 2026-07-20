using System;
using UnityEngine;

namespace PlayableAd
{
    [Serializable]
    public sealed class ElixirPresentationSettings
    {
        [Header("Idle Readability")]
        [Range(0.05f, 0.5f)] public float hoverHeight = 0.18f;
        [Range(20f, 180f)] public float rotationSpeed = 72f;
        [Range(0.5f, 5f)] public float breathSpeed = 2.2f;
        [Range(0f, 2f)] public float emissionIntensity = 0.8f;
        [Range(0.8f, 2.2f)] public float ringRadius = 1.1f;

        [Header("Pickup Sequence")]
        [Range(0.6f, 0.9f)] public float totalDuration = 0.8f;
        [Range(0.06f, 0.16f)] public float collapseDuration = 0.11f;
        [Range(0.1f, 0.3f)] public float upgradeMoment = 0.18f;
        [Range(0.05f, 0.15f)] public float slowMotionDuration = 0.09f;
        [Range(0.35f, 0.9f)] public float slowMotionScale = 0.62f;
        [Range(0f, 5f)] public float cameraPushIn = 1.8f;
        [Range(0f, 6f)] public float cameraRebound = 3.2f;
        [Range(0f, 1f)] public float pickupFlash = 0.32f;
        [Range(0.2f, 1.5f)] public float energyRingMaxRadius = 1.25f;
    }

    public sealed class ElixirVisual : MonoBehaviour
    {
        private ElixirPresentationSettings settings;
        private Renderer[] renderers;
        private LineRenderer groundRing;
        private MaterialPropertyBlock propertyBlock;
        private Vector3 basePosition;
        private bool consumed;
        private float seed;
        private Color primaryColor;
        private Color secondaryColor;
        private Material sharedLineMaterial;

        public void Initialize(ElixirPresentationSettings presentationSettings, Renderer[] visualRenderers, SpeedVisualProfile profile, int targetLevel)
        {
            settings = presentationSettings;
            renderers = visualRenderers;
            propertyBlock = new MaterialPropertyBlock();
            basePosition = transform.localPosition;
            seed = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            SpeedTierVisualData tier = profile.Get(targetLevel);
            primaryColor = tier.primaryColor;
            secondaryColor = tier.secondaryColor;
            sharedLineMaterial = profile.lineMaterial;
            BuildGroundRing();

            for (int i = 0; i < renderers.Length; i++)
            {
                Material material = renderers[i].sharedMaterial;
                if (material != null && material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                }
            }
        }

        public void BeginConsume()
        {
            consumed = true;
            if (groundRing != null)
            {
                groundRing.enabled = false;
            }
        }

        private void Update()
        {
            if (consumed || settings == null || renderers == null || propertyBlock == null)
            {
                return;
            }

            float wave = (Mathf.Sin(Time.time * settings.breathSpeed + seed) + 1f) * 0.5f;
            transform.localPosition = basePosition + Vector3.up * (wave * settings.hoverHeight);
            transform.Rotate(0f, settings.rotationSpeed * Time.deltaTime, 0f, Space.Self);

            Color emission = secondaryColor * Mathf.Lerp(0.25f, settings.emissionIntensity, wave);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_EmissionColor", emission);
                renderer.SetPropertyBlock(propertyBlock);
            }

            if (groundRing != null)
            {
                float radius = settings.ringRadius * Mathf.Lerp(0.9f, 1.08f, wave);
                groundRing.transform.localScale = new Vector3(radius, radius, radius);
                Color color = Color.Lerp(primaryColor, secondaryColor, wave);
                color.a = Mathf.Lerp(0.28f, 0.68f, wave);
                groundRing.startColor = color;
                groundRing.endColor = color;
            }
        }

        private void BuildGroundRing()
        {
            GameObject ringObject = new GameObject("PickupRing");
            ringObject.transform.SetParent(transform, false);
            ringObject.transform.localPosition = new Vector3(0f, -0.82f, 0f);
            groundRing = ringObject.AddComponent<LineRenderer>();
            groundRing.useWorldSpace = false;
            groundRing.loop = true;
            groundRing.positionCount = 32;
            groundRing.startWidth = 0.045f;
            groundRing.endWidth = 0.045f;
            groundRing.sharedMaterial = sharedLineMaterial != null
                ? sharedLineMaterial
                : RuntimeStyle.CreateMaterial(Color.white, 0f, 0.1f);
            for (int i = 0; i < 32; i++)
            {
                float angle = i / 32f * Mathf.PI * 2f;
                groundRing.SetPosition(i, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)));
            }
        }
    }
}
