using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Посилання на Transform об'єкта танка")]
    public Transform target;
    [Tooltip("Вертикальне зміщення камери відносно цілі")]
    public float verticalOffset = 2f;

    [Header("Dynamics")]
    [Tooltip("Час затримки для згладжування (у секундах)")]
    public float smoothTime = 0.3f;

    private float currentVelocity;
    private float fixedX;
    private float fixedZ;

    private void Start()
    {
        fixedX = transform.position.x;
        fixedZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            float targetY = target.position.y + verticalOffset;

            float smoothedY = Mathf.SmoothDamp(transform.position.y, targetY, ref currentVelocity, smoothTime);

            transform.position = new Vector3(fixedX, smoothedY, fixedZ);
        }
    }
}