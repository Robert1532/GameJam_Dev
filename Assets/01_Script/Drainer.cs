using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Drainer : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Motor";
    public float velocidad = 2.5f;
    public Animator animator;
    public GameObject PiezaCañonPrefab;
    public ParticleSystem efectoExplosion;
    public ParticleSystem efectoDestello;

    public float vida = 70f;
    public float daño = 18f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = velocidad;

        agente.stoppingDistance = 1f;
        agente.acceleration = 20f;

        efectoExplosion.Stop();
        efectoDestello.Stop();
    }

    void Update()
    {
        if (vida <= 0)
        {
            Morir();
            return;
        }

        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null)
        {
            agente.SetDestination(objetivoCercano.transform.position);

            float distancia = Vector3.Distance(transform.position, objetivoCercano.transform.position);

            if (distancia <= agente.stoppingDistance)
            {
                if (animator != null)
                {
                    animator.SetBool("IsAttack", true);
                    animator.SetBool("IsWalk", false);
                }
            }
            else
            {
                if (animator != null)
                {
                    animator.SetBool("IsWalk", agente.velocity.magnitude > 0.1f);
                    animator.SetBool("IsAttack", false);
                }
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsWalk", false);
                animator.SetBool("IsAttack", false);
            }
        }
    }

    void Morir()
    {
        if (PiezaCañonPrefab != null)
        {
            Instantiate(efectoDestello, transform.position, transform.rotation);
            Instantiate(efectoExplosion, transform.position, transform.rotation);
            Instantiate(PiezaCañonPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    GameObject BuscarObjetivoMasCercano()
    {
        GameObject[] objetivos = GameObject.FindGameObjectsWithTag(tagObjetivo);
        GameObject masCercano = null;
        float distanciaMinima = Mathf.Infinity;
        Vector3 posicionActual = transform.position;

        foreach (GameObject obj in objetivos)
        {
            float distancia = Vector3.Distance(obj.transform.position, posicionActual);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                masCercano = obj;
            }
        }
        return masCercano;
    }
}
