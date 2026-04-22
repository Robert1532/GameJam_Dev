// PlayerMovement_Arandia.cs
// Responsable: Arandia
// Descripcion: Movimiento basico del jugador con WASD/flechas para LAST MACHINE.
//              Coloca en el mismo GameObject que el Player.

using UnityEngine;

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
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
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
