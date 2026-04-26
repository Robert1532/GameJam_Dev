using UnityEngine;

public class Efecto_Particles : MonoBehaviour
{
    private ParticleSystem _ps;

    void Start()
    {
        _ps = GetComponent<ParticleSystem>();
        _ps.Play();
    }

    void Update()
    {
        if (_ps != null && !_ps.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
