// CameraFollow_Arandia.cs
// Responsable: Arandia
// Descripcion: Sigue al jugador con suavidad y un angulo optimo para ver la accion.

using UnityEngine;

namespace LastMachine.Arandia
{
    public class CameraFollow_Arandia : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(0, 15, -10);
        public float smoothSpeed = 5f;

        void LateUpdate()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
            transform.position = smoothedPosition;

            // Siempre mirar un poco hacia adelante del jugador
            transform.LookAt(target.position + Vector3.forward * 2f);
        }
    }
}
