using UnityEngine;

/// <summary>
/// Orbit/zoom camera controller for 3D model viewing (no pan).
/// Attach to your main camera and assign the target (model root).
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = Vector3.zero;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    public float minYAngle = -80f;
    public float maxYAngle = 80f;
    private float yaw = 0f;
    private float pitch = 30f;
    private float targetYaw = 0f;
    private float targetPitch = 30f;
    public float rotationSmoothTime = 0.15f;
    private float yawVelocity = 0f;
    private float pitchVelocity = 0f;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minDistance = 2f;
    public float maxDistance = 10f;
    private float distance = 5f;
    private float targetDistance = 5f;
    public float zoomSmoothTime = 0.15f;
    private float zoomVelocity = 0f;

    [Header("Initial View")]
    public float initialYaw = 0f;
    public float initialPitch = 30f;

    private Vector3 lastMousePos;
    private bool isRotating = false;

    void Start()
    {
        if (target != null)
        {
            distance = Vector3.Distance(transform.position, target.position + targetOffset);
            targetDistance = distance;
            yaw = targetYaw = initialYaw;
            pitch = targetPitch = initialPitch;
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Touch input for rotation and zoom
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                Vector2 delta = touch.deltaPosition;
                targetYaw += delta.x * rotationSpeed * 0.02f;
                targetPitch -= delta.y * rotationSpeed * 0.02f;
                targetPitch = Mathf.Clamp(targetPitch, minYAngle, maxYAngle);
            }
        }
        else if (Input.touchCount == 2)
        {
            // Pinch to zoom
            Touch touch0 = Input.GetTouch(0);
            Touch touch1 = Input.GetTouch(1);
            Vector2 prevTouch0 = touch0.position - touch0.deltaPosition;
            Vector2 prevTouch1 = touch1.position - touch1.deltaPosition;
            float prevDist = (prevTouch0 - prevTouch1).magnitude;
            float currDist = (touch0.position - touch1.position).magnitude;
            float deltaDist = currDist - prevDist;
            targetDistance -= deltaDist * zoomSpeed * 0.005f;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
        else
        {
            // Mouse input
            if (Input.GetMouseButtonDown(0))
            {
                isRotating = true;
                lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
                isRotating = false;

            // Rotate
            if (isRotating)
            {
                Vector3 delta = Input.mousePosition - lastMousePos;
                targetYaw += delta.x * rotationSpeed * 0.02f;
                targetPitch -= delta.y * rotationSpeed * 0.02f;
                targetPitch = Mathf.Clamp(targetPitch, minYAngle, maxYAngle);
                lastMousePos = Input.mousePosition;
            }

            // Zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                targetDistance -= scroll * zoomSpeed;
                targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
            }
        }

        // Smooth rotation
        yaw = Mathf.SmoothDampAngle(yaw, targetYaw, ref yawVelocity, rotationSmoothTime);
        pitch = Mathf.SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, rotationSmoothTime);

        // Smooth zoom
        distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, zoomSmoothTime);
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // Calculate camera position
        Quaternion rot = Quaternion.Euler(pitch, yaw, 0);
        Vector3 dir = rot * Vector3.forward;
        transform.position = (target.position + targetOffset) - dir * distance;
        transform.LookAt(target.position + targetOffset);
    }
} 