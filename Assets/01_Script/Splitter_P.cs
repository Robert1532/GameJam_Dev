using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Splitter_P : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Sensor";
    public Animator animator;

    public float alturaSalto = 4f;
    public float duracionSalto = 0.5f;
    public float esperaEntreSaltos = 0.7f;
    private bool estaSaltando = false;

    public int vida = 75;
    public float daño = 8f;
    public float intervaloDano = 1.5f;

    public GameObject prefabHijoUno;
    public GameObject prefabHijoDos;
    public float fuerzaImpulso = 5f;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        agente.speed = 7f;
        //StartCoroutine(SistemaDeDegradacion());
    }

    void Update()
    {
        if (estaSaltando) return;//

        GameObject objetivoCercano = BuscarObjetivoMasCercano();

        if (objetivoCercano != null && agente.isOnNavMesh)
        {
            agente.SetDestination(objetivoCercano.transform.position);

            if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance)
            {
                StartCoroutine(SecuenciaDeSalto(objetivoCercano.transform.position));
            }//

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

    
    IEnumerator SecuenciaDeSalto(Vector3 posObjetivo)//
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

        if (animator != null)
        {
            animator.SetBool("IsAttack", false);
        }

        yield return new WaitForSeconds(esperaEntreSaltos);

        estaSaltando = false;
    }

    IEnumerator MoverEnArco(Vector3 inicio, Vector3 fin)//
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

    /*IEnumerator SistemaDeDegradacion()
    {
        while (vida > 0)
        {
            yield return new WaitForSeconds(intervaloDano);
            vida -= 1;
            if (vida <= 0) Morir();
        }
    }*/

    void Morir()
    {
        Vector3 atras = -transform.forward;
        Vector3 izquierda = -transform.right;
        Vector3 derecha = transform.right;

        Vector3 dirHijo1 = (atras + izquierda).normalized;
        Vector3 dirHijo2 = (atras + derecha).normalized;

        SpawnearHijo(prefabHijoUno, dirHijo1);
        SpawnearHijo(prefabHijoDos, dirHijo2);

        Destroy(gameObject);
    }

    void SpawnearHijo(GameObject prefab, Vector3 direccion)
    {
        if (prefab == null) return;

        Vector3 posicionSpawn = transform.position + Vector3.up * 0.5f;
        GameObject hijo = Instantiate(prefab, posicionSpawn, transform.rotation);

        Rigidbody rb = hijo.GetComponent<Rigidbody>();
        NavMeshAgent agenteHijo = hijo.GetComponent<NavMeshAgent>();

        if (agenteHijo != null)
        {
            agenteHijo.enabled = false;
        }

        if (rb != null)
        {
            rb.AddForce((direccion + Vector3.up * 0.7f) * fuerzaImpulso, ForceMode.VelocityChange);
        }

        StartCoroutine(RetrasarAgenteHijo(hijo, agenteHijo));
    }

    IEnumerator RetrasarAgenteHijo(GameObject hijo, NavMeshAgent navAgente)
    {
        yield return new WaitForSeconds(0.8f);

        if (hijo != null && navAgente != null)
        {
            navAgente.enabled = true;
            navAgente.Warp(hijo.transform.position);
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
