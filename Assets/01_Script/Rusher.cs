using UnityEngine;
using UnityEngine.AI;

public class Rusher : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Torreta";
    public ParticleSystem efecto;
    public Animator animator;

    public float amplitud = 10f;
    public float frecuencia = 1.2f;
    public float distanciaParaDetenerZigZag = 5f;

    private int ladoActual = 1;
    private float tiempoSiguienteCambio;
    public float vida = 45f;
    public float daño = 22f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();

        agente.speed = 30f;
        agente.acceleration = 1000f;
        agente.angularSpeed = 0f;
        agente.updateRotation = false;
        agente.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;

        tiempoSiguienteCambio = Time.time + (1f / frecuencia);
    }

    void Update()
    {
        if (agente == null || !agente.isOnNavMesh) return;

        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null)
        {
            MirarSiempreObjetivo(objetivoCercano.transform.position);

            if (Time.time >= tiempoSiguienteCambio)
            {
                ladoActual *= -1;
                tiempoSiguienteCambio = Time.time + (1f / frecuencia);
            }

            Vector3 destinoFinal;
            float distanciaAlObjetivo = Vector3.Distance(transform.position, objetivoCercano.transform.position);

            if (distanciaAlObjetivo > distanciaParaDetenerZigZag + 0.2f)
            {
                if (animator != null)
                {
                    animator.SetBool("IsWalk", true);
                    animator.SetBool("IsAttack", false);
                }

                if (efecto != null && !efecto.isPlaying) efecto.Play();

                Vector3 direccionHaciaObjetivo = (objetivoCercano.transform.position - transform.position).normalized;
                Vector3 perpendicular = new Vector3(-direccionHaciaObjetivo.z, 0, direccionHaciaObjetivo.x);
                Vector3 desviacionPura = perpendicular * (amplitud * ladoActual);

                destinoFinal = transform.position + (direccionHaciaObjetivo * 5f) + desviacionPura;
            }
            else
            {
                if (animator != null)
                {
                    animator.SetBool("IsAttack", true);
                    animator.SetBool("IsWalk", false);
                }

                if(animator.GetBool("IsAttack") == true) efecto.Stop();

                destinoFinal = objetivoCercano.transform.position;

                if (distanciaAlObjetivo <= agente.stoppingDistance + 0.1f)
                {
                    agente.velocity = Vector3.zero;
                }
            }

            NavMeshHit hit;
            if (NavMesh.SamplePosition(destinoFinal, out hit, amplitud * 2f, NavMesh.AllAreas))
            {
                agente.SetDestination(hit.position);
            }
        }
        else
        {
            if (animator != null)
            {
                animator.SetBool("IsWalk", false);
                animator.SetBool("IsAttack", false);
            }
            if (efecto != null) efecto.Stop();
        }
    }

    void MirarSiempreObjetivo(Vector3 posicionObjetivo)
    {
        Vector3 direccion = (posicionObjetivo - transform.position).normalized;
        direccion.y = 0;
        if (direccion != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direccion);
        }
    }

    GameObject BuscarObjetivoMasCercano()
    {
        GameObject[] objetivos = GameObject.FindGameObjectsWithTag(tagObjetivo);
        GameObject masCercano = null;
        float distanciaMinima = Mathf.Infinity;
        foreach (GameObject obj in objetivos)
        {
            float d = Vector3.Distance(obj.transform.position, transform.position);
            if (d < distanciaMinima) { distanciaMinima = d; masCercano = obj; }
        }
        return masCercano;
    }
}
