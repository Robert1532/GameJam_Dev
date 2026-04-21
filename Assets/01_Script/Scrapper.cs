using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AdaptivePerformance;
using UnityEngine.AI;

public class Scrapper : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Cañon";
    public Animator animator;

    public float vida = 100f;
    public float daño = 12f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = 5f;
    }

    void Update()
    {
        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null && agente.isActiveAndEnabled && agente.isOnNavMesh)
        {
            float distancia = Vector3.Distance(transform.position, objetivoCercano.transform.position);

            if (distancia > agente.stoppingDistance)
            {
                agente.SetDestination(objetivoCercano.transform.position);

                animator.SetBool("IsWalk", true);
                animator.SetBool("IsAttack", false);
            }
            else
            {
                animator.SetBool("IsAttack", true);
                animator.SetBool("IsWalk", false);

                Vector3 direccionLook = (objetivoCercano.transform.position - transform.position).normalized;
                direccionLook.y = 0;
                if (direccionLook != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direccionLook), Time.deltaTime * 10f);
                }
            }
        }
        else if (objetivoCercano == null)
        {
            animator.SetBool("IsWalk", false);
            animator.SetBool("IsAttack", false);
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
