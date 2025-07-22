using UnityEngine;

public class SimpleOrbitCamera : MonoBehaviour
{
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;
    public float distanceMin = 2f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;
    // Add target variables for smooth pan
    private float targetX;
    private float targetY;
    private float targetDistance;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        // Initialize targetX and targetY
        targetX = x;
        targetY = y;
        targetDistance = distance;

        if (GetComponent<Rigidbody>())
            GetComponent<Rigidbody>().freezeRotation = true;
    }

    void LateUpdate()
    {
        if (target)
        {
            // Mouse input
            if (Input.GetMouseButton(0))
            {
                targetX += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
                targetY -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }

            // Touch input for pan (exactly one finger)
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    targetX += touch.deltaPosition.x * xSpeed * 0.02f * 0.3f;
                    targetY -= touch.deltaPosition.y * ySpeed * 0.02f * 0.3f;
                }
            }

            // Clamp targetY
            targetY = Mathf.Clamp(targetY, yMinLimit, yMaxLimit);

            // Smoothly interpolate x and y
            x = Mathf.LerpAngle(x, targetX, Time.deltaTime * 10f);
            y = Mathf.LerpAngle(y, targetY, Time.deltaTime * 10f);

            Quaternion rotation = Quaternion.Euler(y, x, 0);
            // Mouse wheel zoom (reduced sensitivity)
            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 1.5f, distanceMin, distanceMax);

            // Pinch to zoom (mobile, exactly two fingers, further reduced sensitivity, inverted direction)
            if (Input.touchCount == 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                float prevTouchDeltaMag = (touchZero.position - touchZero.deltaPosition - (touchOne.position - touchOne.deltaPosition)).magnitude;
                float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;
                float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

                // Invert sign and further reduce sensitivity
                targetDistance = Mathf.Clamp(targetDistance - deltaMagnitudeDiff * 0.0005f, distanceMin, distanceMax);
                distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * 10f);
            }

            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }
}