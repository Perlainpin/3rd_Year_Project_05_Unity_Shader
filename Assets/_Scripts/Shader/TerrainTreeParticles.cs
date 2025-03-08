using UnityEngine;
using System.Collections.Generic;

public class TerrainTreeParticles : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject particleSystemPrefab;
    [SerializeField] private GameObject particleContainer;
    [SerializeField] private float heightOffset = 3f;

    [Header("Optimisation")]
    [SerializeField] private float activationDistance = 50f;
    [SerializeField] private float updateInterval = 0.5f;

    private Camera mainCamera;
    private float nextUpdateTime;
    private Dictionary<GameObject, ParticleSystemInfo> particleSystems = new Dictionary<GameObject, ParticleSystemInfo>();

    private class ParticleSystemInfo
    {
        public Vector3 position;
        public ParticleSystem ps;
        public bool isActive;

        public ParticleSystemInfo(Vector3 pos, ParticleSystem particleSystem)
        {
            position = pos;
            ps = particleSystem;
            isActive = false;
        }
    }

    private void Start()
    {
        mainCamera = Camera.main;
        CreateParticleSystems();
    }

    private void Update()
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateActiveParticleSystems();
            nextUpdateTime = Time.time + updateInterval;
        }
    }

    public void CreateParticleSystems()
    {
        if (particleSystemPrefab == null)
        {
            Debug.LogError("Particule System Prefab non assigné!");
            return;
        }

        if (particleContainer == null)
        {
            particleContainer = new GameObject("Particle Systems Container");
        }

        Terrain terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("Aucun terrain actif trouvé dans la scène!");
            return;
        }

        TerrainData terrainData = terrain.terrainData;
        TreeInstance[] trees = terrainData.treeInstances;

        foreach (TreeInstance tree in trees)
        {
            Vector3 worldPosition = Vector3.Scale(tree.position, terrainData.size) + terrain.transform.position;
            worldPosition.y += heightOffset;

            // Utilise la rotation du prefab
            GameObject particleObj = Instantiate(particleSystemPrefab, worldPosition, particleSystemPrefab.transform.rotation, particleContainer.transform);
            ParticleSystem ps = particleObj.GetComponent<ParticleSystem>();

            if (ps == null)
            {
                ps = particleObj.GetComponentInChildren<ParticleSystem>();
            }

            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                particleSystems.Add(particleObj, new ParticleSystemInfo(worldPosition, ps));
            }
            else
            {
                Debug.LogWarning("Pas de ParticleSystem trouvé dans le prefab!");
                Destroy(particleObj);
            }
        }

        Debug.Log($"Systèmes de particules créés pour {trees.Length} arbres.");
    }

    private void UpdateActiveParticleSystems()
    {
        if (mainCamera == null) return;

        Vector3 cameraPosition = mainCamera.transform.position;
        float activationDistanceSqr = activationDistance * activationDistance;

        foreach (var kvp in particleSystems)
        {
            if (kvp.Key == null) continue;

            var info = kvp.Value;
            float distanceSqr = (cameraPosition - info.position).sqrMagnitude;
            bool shouldBeActive = distanceSqr <= activationDistanceSqr;

            if (shouldBeActive != info.isActive)
            {
                if (shouldBeActive)
                {
                    info.ps.Play(true);
                }
                else
                {
                    info.ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                info.isActive = shouldBeActive;
            }
        }
    }

    public void CleanupParticleSystems()
    {
        if (particleContainer != null)
        {
            if (Application.isPlaying)
            {
                Destroy(particleContainer);
            }
            else
            {
                DestroyImmediate(particleContainer);
            }
        }
        particleSystems.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (mainCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(mainCamera.transform.position, activationDistance);
        }
    }
}