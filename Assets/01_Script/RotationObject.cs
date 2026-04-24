using UnityEngine;

public class RotationObject : MonoBehaviour
{
    // 50 a 100 suele ser una velocidad media agradable
    public float velocidadRotacion = 60f;

    public bool rotarEnX = false;
    public bool rotarEnY = true;
    public bool rotarEnZ = false;

    void Update()
    {
        float paso = velocidadRotacion * Time.deltaTime;

        transform.Rotate(
            rotarEnX ? paso : 0,
            rotarEnY ? paso : 0,
            rotarEnZ ? paso : 0
        );
    }
}
