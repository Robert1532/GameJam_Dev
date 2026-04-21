using UnityEngine;
using UnityEngine.AI;

public class Armored : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Torreta";
    public Animator animator;

    public float rangoAtaque = 1f;
    public float vida = 150f;
    public float daño = 8f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = 2.5f;
        agente.stoppingDistance = rangoAtaque;
    }

    void Update()
    {
        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null)
        {
            agente.SetDestination(objetivoCercano.transform.position);

            float distancia = Vector3.Distance(transform.position, objetivoCercano.transform.position);

            if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance + 0.5f)
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
                    animator.SetBool("IsWalk", agente.velocity.magnitude > 0.1f);
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
