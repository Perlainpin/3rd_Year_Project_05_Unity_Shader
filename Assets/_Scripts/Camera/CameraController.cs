using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 80f;
    [SerializeField] private InputActionReference middleClickAction;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 300;
    [SerializeField] private float scrollSensitivity = 0.1f;
    [SerializeField] private float zoomSmoothTime = 0.2f;
    [SerializeField] private float minZoom = 5f;
    [SerializeField] private float maxZoom = 20f;
    [SerializeField] private InputActionReference scrollAction;

    private float currentZoom;
    private float targetZoom;
    private float zoomVelocity;
    private Vector3 offset;
    [Header("Collision")]
    [SerializeField] private float collisionOffset = 0.2f;
    [SerializeField] private LayerMask collisionLayers;
    private void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("No target assigned to CameraController!");
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        // Initialiser le zoom   la distance actuelle
        currentZoom = Vector3.Distance(transform.position, target.position);
        targetZoom = currentZoom;
        offset = transform.position - target.position;
    }
    private void OnEnable()
    {
        middleClickAction.action.Enable();
        scrollAction.action.Enable();
    }
    private void OnDisable()
    {
        middleClickAction.action.Disable();
        scrollAction.action.Disable();
    }
    private void FixedUpdate()
    {
        if (target == null) return;
        // Rotation avec le middle click
        if (middleClickAction.action.IsPressed())
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            float rotationX = mouseDelta.x * rotationSpeed * Time.deltaTime;
            // Rotation autour du joueur
            offset = Quaternion.Euler(0, rotationX, 0) * offset;
        }
        // Zoom avec la molette
        float scrollValue = scrollAction.action.ReadValue<float>();
        if (Mathf.Abs(scrollValue) > scrollSensitivity) // Ne prend en compte que les valeurs significatives
        {
            targetZoom -= scrollValue * zoomSpeed * Time.deltaTime;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);

        // Calculer la position d sir e
        Vector3 desiredPosition = target.position + offset.normalized * currentZoom;
        // V rifier les collisions
        Ray ray = new Ray(target.position, offset.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, currentZoom, collisionLayers))
        {
            // Placer la cam ra juste avant le point de collision
            desiredPosition = hit.point - (offset.normalized * collisionOffset);
        }
        // Appliquer la position finale
        transform.position = desiredPosition;
        transform.LookAt(target);
    }
}