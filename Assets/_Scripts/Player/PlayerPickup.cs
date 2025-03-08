using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class WeaponPickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputActionReference _pickupAction;  // Touche F pour ramasser/lâcher
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Transform _weaponsContainer;
    [SerializeField] private List<string> _takeableTags = new List<string> { "Weapon", "Shield", "Bow" };
    [SerializeField] private Animator _animator;

    [Header("Settings")]
    [SerializeField] private float _pickupRange = 2f;
    [SerializeField] private float _raycastLength = 100f;

    [Header("Debug")]
    [SerializeField] private bool _showDebugRay = true;
    [SerializeField] private Color _rayColorNoHit = Color.red;
    [SerializeField] private Color _rayColorInvalidHit = Color.yellow;
    [SerializeField] private Color _rayColorValidHit = Color.green;

    private Camera _mainCamera;
    private GameObject _currentHighlightedObject;
    private Material[] _originalMaterials;
    private bool GotSword = false;
    private GameObject _currentlyHeldWeapon;
    private bool isHoldingWeapon = false; 

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (_pickupAction != null)
            _pickupAction.action.Enable();
    }

    private void OnDisable()
    {
        if (_pickupAction != null)
            _pickupAction.action.Disable();

        if (_currentHighlightedObject != null)
            RestoreOriginalMaterial();
    }

    private void Update()
    {
        HandleObjectHighlight();
        HandleObjectPickupOrDrop(); 
    }

    private void HandleObjectHighlight()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, _raycastLength))
        {
            GameObject hitObject = hit.collider.gameObject;
            bool isValidObject = IsValidObject(hitObject);
            bool isInRange = IsInRange(hit.point);

            if (_showDebugRay)
            {
                Color rayColor = isValidObject && isInRange ? _rayColorValidHit :
                               isValidObject ? _rayColorInvalidHit :
                               _rayColorNoHit;

                Debug.DrawLine(ray.origin, hit.point, rayColor);

                if (isValidObject)
                {
                    DrawPickupRangeCircle(rayColor);
                    Debug.DrawLine(transform.position, hit.point,
                        isInRange ? _rayColorValidHit : _rayColorInvalidHit);
                }
            }

            if (isValidObject && isInRange)
            {
                if (_currentHighlightedObject != hitObject)
                {
                    if (_currentHighlightedObject != null)
                    {
                        RestoreOriginalMaterial();
                    }

                    _currentHighlightedObject = hitObject;
                    ApplyHighlightMaterial();
                }
            }
            else if (_currentHighlightedObject != null)
            {
                RestoreOriginalMaterial();
                _currentHighlightedObject = null;
            }
        }
        else
        {
            if (_showDebugRay)
            {
                Debug.DrawRay(ray.origin, ray.direction * _raycastLength, _rayColorNoHit);
                DrawPickupRangeCircle(_rayColorNoHit);
            }

            if (_currentHighlightedObject != null)
            {
                RestoreOriginalMaterial();
                _currentHighlightedObject = null;
            }
        }
    }

    private void DrawPickupRangeCircle(Color color)
    {
        if (!_showDebugRay) return;

        int segments = 32;
        float angleStep = 360f / segments;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        for (int i = 0; i < segments; i++)
        {
            float angleA = angleStep * i * Mathf.Deg2Rad;
            float angleB = angleStep * (i + 1) * Mathf.Deg2Rad;

            Vector3 pointA = transform.position + (forward * Mathf.Sin(angleA) + right * Mathf.Cos(angleA)) * _pickupRange;
            Vector3 pointB = transform.position + (forward * Mathf.Sin(angleB) + right * Mathf.Cos(angleB)) * _pickupRange;

            Debug.DrawLine(pointA, pointB, color);
        }
    }

    private void HandleObjectPickupOrDrop()
    {
        if (_pickupAction.action.WasPressedThisFrame())
        {
            if (isHoldingWeapon)
            {
                DropWeapon();  // Si on tient une arme, on la lâche
            }
            else
            {
                PickupWeapon();  // Si on ne tient pas d'arme, on en prend une
            }
        }
    }

    private void PickupWeapon()
    {
        if (_currentHighlightedObject != null && IsValidObject(_currentHighlightedObject) && IsInRange(_currentHighlightedObject.transform.position))
        {
            Rigidbody rb = _currentHighlightedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            Collider objectCollider = _currentHighlightedObject.GetComponent<Collider>();
            if (objectCollider != null)
            {
                objectCollider.isTrigger = true;
            }

            RestoreOriginalMaterial();

            _currentHighlightedObject.transform.SetParent(_weaponsContainer);
            _currentHighlightedObject.transform.localPosition = Vector3.zero;
            _currentHighlightedObject.transform.localRotation = Quaternion.identity;

            if (_currentHighlightedObject.CompareTag("Weapon"))
            {
                GotSword = true;
                _currentlyHeldWeapon = _currentHighlightedObject; 
                isHoldingWeapon = true; 
            }
            else
            {
                GotSword = false;
            }

            _currentHighlightedObject = null;

            UpdateAnimator();
        }
    }

    private void DropWeapon()
    {
        if (_currentlyHeldWeapon != null)
        {
            Rigidbody rb = _currentlyHeldWeapon.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            // Réactive le collider
            Collider objectCollider = _currentlyHeldWeapon.GetComponent<Collider>();
            if (objectCollider != null)
            {
                objectCollider.isTrigger = false;
            }

            // Lâche l'arme (définit son parent à null)
            _currentlyHeldWeapon.transform.SetParent(null);

            // Réinitialise l'arme
            _currentlyHeldWeapon = null;
            GotSword = false;
            isHoldingWeapon = false;  // L'arme est lâchée

            UpdateAnimator();
        }
    }

    private bool IsValidObject(GameObject obj)
    {
        return _takeableTags.Contains(obj.tag);
    }

    private bool IsInRange(Vector3 objectPosition)
    {
        return Vector3.Distance(transform.position, objectPosition) <= _pickupRange;
    }

    private void ApplyHighlightMaterial()
    {
        if (_currentHighlightedObject == null) return;

        Renderer[] renderers = _currentHighlightedObject.GetComponentsInChildren<Renderer>();
        _originalMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            _originalMaterials[i] = renderers[i].material;
            renderers[i].material = _highlightMaterial;
        }
    }

    private void RestoreOriginalMaterial()
    {
        if (_currentHighlightedObject == null || _originalMaterials == null) return;

        Renderer[] renderers = _currentHighlightedObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            if (i < _originalMaterials.Length)
            {
                renderers[i].material = _originalMaterials[i];
            }
        }
    }

    private void UpdateAnimator()
    {
        if (_animator != null)
        {
            _animator.SetBool("GotSword", GotSword);
        }
    }
}
