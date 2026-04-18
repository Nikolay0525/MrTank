using UnityEngine;

public class ScrollableObject : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Глобальная координата X, при пересечении которой объект деактивируется")]
    public float despawnX = -15f;

    private void Update()
    {
        float currentSpeed = TankController.CurrentGlobalSpeed;

        transform.Translate(Vector3.left * currentSpeed * Time.deltaTime);

        if (transform.position.x <= despawnX)
        {
            Deactivate();
        }
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }
}