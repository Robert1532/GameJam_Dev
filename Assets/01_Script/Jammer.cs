using UnityEngine;
using UnityEngine.AI;

public class Jammer : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Sensor";

    public GameObject tunderPrefab;
    public Transform tunderPoint;
    private GameObject rayoInstanciado;
    public Animator animator;

    public float vida = 100f;
    public float daño = 7f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = 7f;
    }

    void Update()
    {
        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null)
        {
            agente.SetDestination(objetivoCercano.transform.position);

            float distancia = Vector3.Distance(transform.position, objetivoCercano.transform.position);

            if (distancia <= 10)
            {
                ActualizarRayo(objetivoCercano.transform.position);

                animator.SetBool("IsAttack", true);
                animator.SetBool("IsWalk", false);
            }
            else
            {
                DesactivarRayo();

                animator.SetBool("IsAttack", false);
                animator.SetBool("IsWalk", true);
            }
        }
        else
        {
            DesactivarRayo();

            animator.SetBool("IsAttack", false);
            animator.SetBool("IsWalk", false);
        }
    }

    void ActualizarRayo(Vector3 posicionObjetivo)
    {
        if (rayoInstanciado == null)
        {
            rayoInstanciado = Instantiate(tunderPrefab);
        }

        Vector3 origen = tunderPoint.position;
        Vector3 direccion = posicionObjetivo - origen;
        float distanciaActual = direccion.magnitude;

        if (distanciaActual < 0.1f) return;

        rayoInstanciado.transform.position = origen + (direccion / 2f);

        Quaternion mirarAlObjetivo = Quaternion.LookRotation(direccion);
        Quaternion rotacionAcostado = Quaternion.Euler(90f, 0f, 0f);

        rayoInstanciado.transform.rotation = mirarAlObjetivo * rotacionAcostado;

        Vector3 nuevaEscala = rayoInstanciado.transform.localScale;

        nuevaEscala.y = distanciaActual / 2f;

        rayoInstanciado.transform.localScale = nuevaEscala;
    }

    void DesactivarRayo()
    {
        if (rayoInstanciado != null)
        {
            Destroy(rayoInstanciado);
            rayoInstanciado = null;
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
