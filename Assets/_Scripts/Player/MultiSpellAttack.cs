using System.Collections;
using UnityEngine;

public class MultiSpellAttack : MonoBehaviour
{
    [Header("Spells Configuration")]
    public GameObject blackHolePrefab;
    public Transform blackHoleConstraint;
    public Vector3 blackHolePositionOffset;
    public Vector3 blackHoleRotationOffset;
    public AudioClip blackHoleSound;
    public float blackHoleLifetime = 4f;

    public GameObject sparkPrefab;
    public Transform sparkConstraint;
    public Vector3 sparkPositionOffset;
    public Vector3 sparkRotationOffset;
    public AudioClip sparkSound;
    public float sparkLifetime = 5f;

    public GameObject jinxPrefab;
    public Transform jinxConstraint;
    public Vector3 jinxPositionOffset;
    public Vector3 jinxRotationOffset;
    public AudioClip jinxSound;
    public float jinxLifetime = 6f;
    public float jinxRaycastDistance = 10f;
    public float fireDuration = 2f;
    public Vector3 jinxRaycastOffset;
    public GameObject jinxEffectPrefab;

    public GameObject waterfallPrefab;
    public Transform waterfallConstraint;
    public Vector3 waterfallPositionOffset;
    public Vector3 waterfallRotationOffset;
    public AudioClip waterfallSound;
    public float waterfallLifetime = 7f;
    public float waterfallRaycastDistance = 12f;
    public Vector3 waterfallRaycastOffset;
    public GameObject waterfallEffectPrefab;

    [Header("General Settings")]
    public Animator playerAnimator;
    public Camera playerCamera;
    public float throwForce = 15f;

    [Header("Audio Settings")]
    public AudioSource audioSource;

    private bool isCasting = false;

    void Update()
    {
        if (playerAnimator.GetBool("magic_attack") && playerAnimator.GetBool("bookmenu") && !isCasting)
        {
            int spells = playerAnimator.GetInteger("Spells");
            if (spells != 0)
            {
                isCasting = true;
                StartCoroutine(CastSpell(spells));
            }
        }
    }

    IEnumerator CastSpell(int spellId)
    {
        GameObject spellPrefab = null;
        Transform spellConstraint = null;
        Vector3 positionOffset = Vector3.zero;
        Vector3 rotationOffset = Vector3.zero;
        AudioClip spellAudio = null;
        bool isStationarySpell = false;
        float lifetime = 0f;

        switch (spellId)
        {
            case 1:
                spellPrefab = blackHolePrefab;
                spellConstraint = blackHoleConstraint;
                positionOffset = blackHolePositionOffset;
                rotationOffset = blackHoleRotationOffset;
                spellAudio = blackHoleSound;
                lifetime = blackHoleLifetime;
                break;
            case 2:
                spellPrefab = sparkPrefab;
                spellConstraint = sparkConstraint;
                positionOffset = sparkPositionOffset;
                rotationOffset = sparkRotationOffset;
                spellAudio = sparkSound;
                isStationarySpell = true;
                lifetime = sparkLifetime;
                break;
            case 3:
                spellPrefab = jinxPrefab;
                spellConstraint = jinxConstraint;
                positionOffset = jinxPositionOffset;
                rotationOffset = jinxRotationOffset;
                spellAudio = jinxSound;
                isStationarySpell = true;
                lifetime = jinxLifetime;
                StartCoroutine(HandleJinxRaycast());
                break;
            case 4:
                spellPrefab = waterfallPrefab;
                spellConstraint = waterfallConstraint;
                positionOffset = waterfallPositionOffset;
                rotationOffset = waterfallRotationOffset;
                spellAudio = waterfallSound;
                isStationarySpell = true;
                lifetime = waterfallLifetime;
                StartCoroutine(HandleWaterfallRaycast());
                break;
            default:
                isCasting = false;
                yield break;
        }

        if (spellPrefab != null && spellConstraint != null)
        {
            Vector3 spawnPosition = spellConstraint.position + spellConstraint.TransformDirection(positionOffset);
            Quaternion spawnRotation = spellConstraint.rotation * Quaternion.Euler(rotationOffset);
            GameObject spellInstance = Instantiate(spellPrefab, spawnPosition, spawnRotation);

            if (spellAudio != null && audioSource != null)
            {
                audioSource.clip = spellAudio;
                audioSource.Play();
            }

            if (isStationarySpell)
            {
                StartCoroutine(KeepAttachedToConstraint(spellInstance, spellConstraint, positionOffset, rotationOffset, lifetime));
            }
            else
            {
                yield return StartCoroutine(FollowHand(spellInstance, spellConstraint, positionOffset, rotationOffset, 100));
                float initialHeight = GetDistanceToGround(spellInstance.transform.position);
                Rigidbody rb = spellInstance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 targetDirection = GetPointerPosition() - spellConstraint.position;
                    rb.isKinematic = false;
                    rb.AddForce(targetDirection.normalized * throwForce, ForceMode.VelocityChange);
                }
                StartCoroutine(MaintainHeight(spellInstance, initialHeight));
                Destroy(spellInstance, lifetime);
            }
        }

        isCasting = false;
    }

    IEnumerator KeepAttachedToConstraint(GameObject spellInstance, Transform constraint, Vector3 positionOffset, Vector3 rotationOffset, float lifetime)
    {
        Rigidbody rb = spellInstance.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        float timer = 0f;
        while (spellInstance != null && timer < lifetime)
        {
            spellInstance.transform.position = constraint.position + constraint.TransformDirection(positionOffset);
            spellInstance.transform.rotation = constraint.rotation * Quaternion.Euler(rotationOffset);
            timer += Time.deltaTime;
            yield return null;
        }

        if (spellInstance != null)
        {
            Destroy(spellInstance);
        }
    }

    IEnumerator HandleJinxRaycast()
    {
        float timer = 0f;
        while (timer < jinxLifetime)
        {
            Vector3 raycastStart = jinxConstraint.position + jinxConstraint.TransformDirection(jinxRaycastOffset);
            Vector3 raycastDirection = jinxConstraint.forward;

            RaycastHit hit;
            if (Physics.Raycast(raycastStart, raycastDirection, out hit, jinxRaycastDistance))
            {
                ApplyEffectToTarget(hit.collider.gameObject, jinxEffectPrefab, fireDuration);
                StartCoroutine(TriggerFire());
                break;
            }
            Debug.DrawRay(raycastStart, raycastDirection * jinxRaycastDistance, Color.red, 0.1f);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator HandleWaterfallRaycast()
    {
        float timer = 0f;
        while (timer < waterfallLifetime)
        {
            Vector3 raycastStart = waterfallConstraint.position + waterfallConstraint.TransformDirection(waterfallRaycastOffset);
            Vector3 raycastDirection = waterfallConstraint.forward;

            RaycastHit hit;
            if (Physics.Raycast(raycastStart, raycastDirection, out hit, waterfallRaycastDistance))
            {
                ApplyEffectToTarget(hit.collider.gameObject, waterfallEffectPrefab, waterfallLifetime);
                break;
            }
            Debug.DrawRay(raycastStart, raycastDirection * waterfallRaycastDistance, Color.blue, 0.1f);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    void ApplyEffectToTarget(GameObject target, GameObject effectPrefab, float duration)
    {
        if (effectPrefab != null)
        {
            Vector3 effectPosition = target.transform.position;
            GameObject effectInstance = Instantiate(effectPrefab, effectPosition, Quaternion.identity);
            Destroy(effectInstance, duration);
        }
    }

    IEnumerator TriggerFire()
    {
        Shader.SetGlobalFloat("_Fire", 1);
        yield return new WaitForSeconds(fireDuration);
        Shader.SetGlobalFloat("_Fire", 0);
    }

    IEnumerator FollowHand(GameObject spellInstance, Transform constraint, Vector3 positionOffset, Vector3 rotationOffset, int framesToFollow)
    {
        Rigidbody rb = spellInstance.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        for (int i = 0; i < framesToFollow; i++)
        {
            if (spellInstance != null)
            {
                spellInstance.transform.position = constraint.position + constraint.TransformDirection(positionOffset);
                spellInstance.transform.rotation = constraint.rotation * Quaternion.Euler(rotationOffset);
            }
            yield return null;
        }
    }

    IEnumerator MaintainHeight(GameObject spellInstance, float targetHeight, float smoothSpeed = 5f)
    {
        while (spellInstance != null)
        {
            float currentHeight = GetDistanceToGround(spellInstance.transform.position);
            float newY = Mathf.Lerp(spellInstance.transform.position.y, spellInstance.transform.position.y + (targetHeight - currentHeight), Time.deltaTime * smoothSpeed);
            Vector3 newPosition = spellInstance.transform.position;
            newPosition.y = newY;
            spellInstance.transform.position = newPosition;
            yield return null;
        }
    }

    float GetDistanceToGround(Vector3 position)
    {
        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.distance;
        }
        return 0f;
    }

    Vector3 GetPointerPosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.point;
        }
        return ray.GetPoint(10);
    }
}
