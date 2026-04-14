using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform character;

    [Header("Settings")]
    public float sensitivity = 2f;
    public float smoothing = 5f; 

    private Vector2 velocity;       
    private Vector2 frameVelocity;  

    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;
    public float horizontalLimit = 90f;

    private float initialYRotation;

    void Reset()
    {
        character = GetComponentInParent<FirstPersonMovement>()?.transform;
    }

    void Start()
    {
        Vector3 angles = transform.localEulerAngles;
        velocity.y = angles.x;

        initialYRotation = character != null ? character.eulerAngles.y : 0f;
        velocity.x = initialYRotation;

        transform.localRotation = Quaternion.Euler(-velocity.y, 0f, 0f);
        if (character != null)
            character.localRotation = Quaternion.Euler(0f, velocity.x, 0f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            Vector2 rawFrameVelocity = mouseDelta * sensitivity;
            frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1f / smoothing);

            velocity.y += frameVelocity.y;
            velocity.y = Mathf.Clamp(velocity.y, minVerticalAngle, maxVerticalAngle);

            velocity.x += frameVelocity.x;
            float minHorizontal = initialYRotation - horizontalLimit;
            float maxHorizontal = initialYRotation + horizontalLimit;
            velocity.x = Mathf.Clamp(velocity.x, minHorizontal, maxHorizontal);

            transform.localRotation = Quaternion.Euler(-velocity.y, 0f, 0f);
            if (character != null)
                character.localRotation = Quaternion.Euler(0f, velocity.x, 0f);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}