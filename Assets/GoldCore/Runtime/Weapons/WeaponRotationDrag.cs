using UnityEngine;

namespace GoldCore
{
public class WeaponRotationDrag : MonoBehaviour
{
    [Header("Position")]
    public float amount = 0.02f,
        maxAmount = 0.06f,
        smoothAmount = 6f;

    [Header("Rotation")]
    public float rotationAmount = 4f,
        maxRotationAmount = 5f,
        smoothRotation = 12f;

    [Space]
    public bool rotationX = true, rotationY = true, rotationZ = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float InputX, InputY;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    private void Update()
    {
        CalculateSway();
        MoveSway();
        TiltSway();
    }

    private void CalculateSway()
    {
        Vector2 look = GameInput.Look;
        InputX = look.x;
        InputY = look.y;
    }

    private void MoveSway()
    {
        float moveX = Mathf.Clamp(InputX * amount, -maxAmount, maxAmount);
        float moveY = Mathf.Clamp(InputY * amount, -maxAmount, maxAmount);

        Vector3 finalPosition = new Vector3(moveX, moveY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPosition, Time.deltaTime * smoothAmount);
    }

    private void TiltSway()
    {
        float tiltY = Mathf.Clamp(InputX * rotationAmount, -maxRotationAmount, maxRotationAmount);
        float tiltX = Mathf.Clamp(InputY * rotationAmount, -maxRotationAmount, maxRotationAmount);

        Quaternion finalRotation = Quaternion.Euler(new Vector3(rotationX ? -tiltX : 0f, rotationY ? tiltY : 0f, rotationZ ? tiltY : 0f));

        transform.localRotation = Quaternion.Slerp(transform.localRotation, finalRotation * initialRotation, smoothRotation * Time.deltaTime);
    }
}
}
