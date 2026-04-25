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
    public GameObject PiezaSensorPrefab;
    public float vida = 100f;
    public float daño = 7f;

    private bool estaMuerto = false;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = 7f;
    }

    void Update()
    {
        if (estaMuerto) return;

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

            if (distancia <= 10f)
            {
                ActualizarRayo(objetivoCercano.transform.position);
                animator.SetBool("IsAttack", true);
                animator.SetBool("IsWalk", false);
                // EnemyTurretAttack maneja el daño real, Jammer solo mueve el rayo visual
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

    public void RecibirDaño(float cantidad)
    {
        if (estaMuerto) return;
        vida -= cantidad;
        if (vida <= 0) Morir();
    }

    void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        DesactivarRayo(); // el rayo desaparece siempre

        if (PiezaSensorPrefab != null)
            Instantiate(PiezaSensorPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void ActualizarRayo(Vector3 posicionObjetivo)
    {
        if (rayoInstanciado == null)
            rayoInstanciado = Instantiate(tunderPrefab);

        Vector3 origen = tunderPoint.position;
        Vector3 direccion = posicionObjetivo - origen;
        float distanciaActual = direccion.magnitude;

        if (distanciaActual < 0.1f) return;

        rayoInstanciado.transform.position = origen + (direccion / 2f);
        rayoInstanciado.transform.rotation = Quaternion.LookRotation(direccion) * Quaternion.Euler(90f, 0f, 0f);

        Vector3 escala = rayoInstanciado.transform.localScale;
        escala.y = distanciaActual / 2f;
        rayoInstanciado.transform.localScale = escala;
    }

    void DesactivarRayo()
    {
        if (rayoInstanciado != null)
        {
            Destroy(rayoInstanciado);
            rayoInstanciado = null;
        }
    }

    void OnDestroy()
    {
        DesactivarRayo();
    }

    GameObject BuscarObjetivoMasCercano()
    {
        GameObject[] objetivos = GameObject.FindGameObjectsWithTag(tagObjetivo);
        GameObject masCercano = null;
        float distanciaMinima = Mathf.Infinity;

        foreach (GameObject obj in objetivos)
        {
            float d = Vector3.Distance(obj.transform.position, transform.position);
            if (d < distanciaMinima)
            {
                distanciaMinima = d;
                masCercano = obj;
            }
        }
        return masCercano;
    }
}