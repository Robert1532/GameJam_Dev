// PlayerMovement_Arandia.cs
// Responsable: Arandia
// Descripcion: Movimiento basico del jugador con WASD/flechas para LAST MACHINE.
//              Coloca en el mismo GameObject que el Player.

using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace LastMachine.Arandia
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement_Arandia : MonoBehaviour
    {
        [Header("Movimiento - Arandia")]
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f;

        private Rigidbody rb;
        private Vector3 moveDirection;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
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
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
#endif

            moveDirection = new Vector3(h, 0f, v).normalized;

            // Rotar hacia la dirección de movimiento
            if (moveDirection.magnitude > 0.1f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }

        void FixedUpdate()
        {
            rb.linearVelocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.linearVelocity.y,
                moveDirection.z * moveSpeed);
        }
    }
}
