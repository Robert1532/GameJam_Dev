using UnityEngine;
using UnityEngine.AI;

public class Armored : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Torreta";
    public Animator animator;
    public GameObject PiezaSensorPrefab;

    public float rangoAtaque = 1f;
    public float vida = 150f;
    public float daño = 8f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        if (agente != null)
        {
            agente.speed = 2.5f;
            agente.stoppingDistance = rangoAtaque;
        }
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
            // Evita el error: SetDestination solo funciona si el agente está activo y sobre NavMesh.
            if (agente != null && agente.enabled && agente.isOnNavMesh && gameObject.activeInHierarchy)
                agente.SetDestination(objetivoCercano.transform.position);

            float distancia = Vector3.Distance(transform.position, objetivoCercano.transform.position);

            if (agente != null && !agente.pathPending && agente.remainingDistance <= agente.stoppingDistance + 0.5f)
            {
                if (animator != null)
                {
                    animator.SetBool("IsAttack", true);
                    animator.SetBool("IsWalk", false);
                }
                GirarHaciaObjetivo(objetivoCercano.transform.position);
            }
            else
            {
                if (animator != null)
                {
                    animator.SetBool("IsAttack", false);
                    animator.SetBool("IsWalk", agente != null && agente.velocity.magnitude > 0.1f);
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

    public void RecibirDaño(float cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        if (PiezaSensorPrefab != null)
        {
            Instantiate(PiezaSensorPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void GirarHaciaObjetivo(Vector3 destino)
    {
        Vector3 direccion = (destino - transform.position).normalized;
        direccion.y = 0;
        if (direccion != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
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
