using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Player Ref")]
    [SerializeField] private GameObject player;

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Collision Effects")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private GameObject hitEffectPrefab;

    [Header("Fire Settings")]
    [SerializeField] private GameObject fireEffectPrefab;
    [SerializeField] private float fireEffectDuration = 2f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;

    private Rigidbody rb;
    private Rigidbody[] allChildrenRb;
    private bool fire = false;

    void Start()
    {
        currentHealth = maxHealth;

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        rb = GetComponent<Rigidbody>();
        allChildrenRb = GetComponentsInChildren<Rigidbody>();
    }

    private void Update()
    {
        if (fire)
        {
            Debug.Log("Fire effect activated");
            ActivateFireEffect();
            fire = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Weapon") &&
            other.gameObject.transform.parent.CompareTag("WeaponBelt") &&
            player.GetComponent<Animator>().GetCurrentAnimatorStateInfo(2).IsName("Attack_Sword_Mocap"))
        {
            Debug.Log("Weapon collision detected");
            ApplyKnockback(other.transform.position, other.gameObject.GetComponent<Sword>().GetKnockbackForce());
            TakeDamage(other.gameObject.GetComponent<Sword>().GetDamage());
            PlayHitEffects();
            fire = true;
        }
    }

    private void PlayHitEffects()
    {
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }

    private void ActivateFireEffect()
    {
        if (fireEffectPrefab != null)
        {
            GameObject fireEffectInstance = Instantiate(fireEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fireEffectInstance, fireEffectDuration);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log($"Enemy took damage: {damage}, Current Health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy died");
            Die();
        }
    }

    private void Die()
    {
        rb.constraints = RigidbodyConstraints.None;
        rb.mass = 5.0f;

        foreach (Rigidbody childRb in allChildrenRb)
        {
            childRb.constraints = RigidbodyConstraints.None;
            childRb.mass = 5.0f;
        }
    }

    private void ApplyKnockback(Vector3 sourcePosition, int knockbackForce)
    {
        Vector3 direction = (transform.position - sourcePosition).normalized;
        direction += Vector3.up * 0.5f;
        rb.AddForce(direction * knockbackForce, ForceMode.Impulse);

        Debug.Log($"Knockback applied with force: {knockbackForce}");
    }
}
