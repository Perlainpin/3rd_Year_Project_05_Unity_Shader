using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ObjectFader : MonoBehaviour
{
    [SerializeField] private Transform target; // Le joueur
    [SerializeField] private Material transparentMaterial; // Material à appliquer
    [SerializeField] private LayerMask excludeLayers; // Layers à ignorer

    private Camera mainCamera;
    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();
    private GameObject lastHitObject;

    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned to ObjectFader!");
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("ObjectFader must be attached to a Camera!");
        }
    }

    private void FixedUpdate()
    {
        if (target == null || mainCamera == null) return;

        Vector3 direction = target.position - transform.position;
        float distance = direction.magnitude;
        Ray ray = new Ray(transform.position, direction.normalized);

        // Vérifie si un objet est entre la caméra et le joueur
        if (Physics.Raycast(ray, out RaycastHit hit, distance, ~excludeLayers))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Si on touche un nouvel objet
            if (hitObject != lastHitObject)
            {
                // Restaure le material précédent si nécessaire
                RestoreOriginalMaterial();

                // Sauvegarde et change le material du nouvel objet
                if (hitObject != target.gameObject)
                {
                    Renderer[] renderers = hitObject.GetComponentsInChildren<Renderer>();
                    if (renderers.Length > 0)
                    {
                        // Sauvegarde les materials originaux
                        Material[][] originalMatsArray = new Material[renderers.Length][];
                        for (int i = 0; i < renderers.Length; i++)
                        {
                            originalMatsArray[i] = renderers[i].materials;
                        }
                        originalMaterials[hitObject] = originalMatsArray.SelectMany(x => x).ToArray();

                        // Applique le material transparent
                        foreach (Renderer renderer in renderers)
                        {
                            Material[] newMaterials = new Material[renderer.materials.Length];
                            for (int i = 0; i < newMaterials.Length; i++)
                            {
                                newMaterials[i] = transparentMaterial;
                            }
                            renderer.materials = newMaterials;
                        }

                        lastHitObject = hitObject;
                    }
                }
            }
        }
        else
        {
            // Si rien n'est touché, restaure le material précédent
            RestoreOriginalMaterial();
        }
    }

    private void RestoreOriginalMaterial()
    {
        if (lastHitObject != null && originalMaterials.ContainsKey(lastHitObject))
        {
            Renderer[] renderers = lastHitObject.GetComponentsInChildren<Renderer>();
            int materialIndex = 0;

            foreach (Renderer renderer in renderers)
            {
                Material[] originalMats = new Material[renderer.materials.Length];
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    originalMats[i] = originalMaterials[lastHitObject][materialIndex++];
                }
                renderer.materials = originalMats;
            }

            originalMaterials.Remove(lastHitObject);
            lastHitObject = null;
        }
    }

    private void OnDisable()
    {
        // Restaure tous les materials originaux quand le script est désactivé
        RestoreOriginalMaterial();
    }
}