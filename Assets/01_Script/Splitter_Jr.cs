using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Splitter_Jr : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Sensor";

    public float distanciaAlObjetivo = 0.5f;
    private float anguloPersonal;
    public Animator animator;
    public GameObject PiezaCañonPrefab;
    public ParticleSystem efectoExplosion;
    public ParticleSystem efectoDestello;

    public float alturaSalto = 4f;
    public float duracionSalto = 0.4f;
    public float esperaEntreSaltos = 0.7f;
    private bool estaSaltando = false;
    public float vida = 45f;
    public float daño = 6f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = 7f;

        anguloPersonal = Random.Range(0, 360) * Mathf.Deg2Rad;

        efectoExplosion.Stop();
        efectoDestello.Stop();

        if (agente != null) agente.enabled = false;
        StartCoroutine(SecuenciaActivacionHijo());
    }

    IEnumerator SecuenciaActivacionHijo()
    {
        yield return new WaitForSeconds(0.8f);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = true;
            Destroy(rb);
        }

        yield return new WaitForFixedUpdate();

        if (agente != null)
        {
            agente.enabled = true;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
            {
                agente.Warp(hit.position);
            }
        }
    }

    void Update()
    {
        if (vida <= 0)
        {
            Morir();
            return;
        }

        if (agente == null || !agente.enabled || !agente.isOnNavMesh || estaSaltando)
            return;

        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null)
        {
            Vector3 offset = new Vector3(
                Mathf.Cos(anguloPersonal) * distanciaAlObjetivo,
                0,
                Mathf.Sin(anguloPersonal) * distanciaAlObjetivo
            );

            Vector3 puntoDestino = objetivoCercano.transform.position + offset;
            agente.SetDestination(puntoDestino);

            if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance + 0.1f)
            {
                StartCoroutine(SecuenciaDeSalto(objetivoCercano.transform.position));
            }

            if (animator != null)
            {
                animator.SetBool("IsWalk", agente.velocity.magnitude > 0.1f);
                animator.SetBool("IsAttack", false);
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
        if (PiezaCañonPrefab != null)
        {
            Instantiate(efectoDestello, transform.position, transform.rotation);
            Instantiate(efectoExplosion, transform.position, transform.rotation);
            Instantiate(PiezaCañonPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    IEnumerator SecuenciaDeSalto(Vector3 posObjetivo)
    {
        estaSaltando = true;
        Vector3 posInicial = transform.position;

        if (animator != null)
        {
            animator.SetBool("IsAttack", true);
            animator.SetBool("IsWalk", false);
        }

        yield return MoverEnArco(posInicial, posObjetivo);

        yield return MoverEnArco(posObjetivo, posInicial);

        if (animator != null) animator.SetBool("IsAttack", false);

        yield return new WaitForSeconds(esperaEntreSaltos);

        estaSaltando = false;
    }

    IEnumerator MoverEnArco(Vector3 inicio, Vector3 fin)
    {
        float tiempoPasado = 0;
        agente.enabled = false;

        while (tiempoPasado < duracionSalto)
        {
            tiempoPasado += Time.deltaTime;
            float progreso = tiempoPasado / duracionSalto;

            Vector3 posActual = Vector3.Lerp(inicio, fin, progreso);
            float arco = Mathf.Sin(progreso * Mathf.PI) * alturaSalto;
            posActual.y += arco;

            transform.position = posActual;
            yield return null;
        }

        transform.position = fin;
        agente.enabled = true;
        agente.Warp(transform.position);
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
