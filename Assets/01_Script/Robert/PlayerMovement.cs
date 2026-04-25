using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Animator animator;

    private Vector3 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float h = 0f;
        float v = 0f;
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) h = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) h = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) v = -1f;
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) v = 1f;
        }
#else
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
#endif

        movement = new Vector3(h, 0f, v).normalized;

        // Animación
        animator.SetFloat("Speed", movement.magnitude);
    }

    void FixedUpdate()
    {
        if (movement.magnitude > 0.1f)
        {
            // Movimiento
            Vector3 move = movement * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);

            // Rotación hacia donde camina
            Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
            Quaternion rotation = Quaternion.Slerp(rb.rotation, toRotation, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(rotation);
        }
    }
}