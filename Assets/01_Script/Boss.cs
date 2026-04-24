using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    private NavMeshAgent agente;
    public string tagObjetivo = "Sensor";
    public string tagTorreta = "Torreta";

    [Header("Configuración de Ondas")]
    public GameObject tunderPrefab;
    public float rangoOndas = 12f;
    public float vidaActivacionSegundaOnda = 120f;
    public Transform origenIzquierda;
    public Transform origenDerecha;
    public Transform firePointCentral;
    private GameObject rayoIzquierda, rayoDerecha;
    public GameObject misilPrefab;
    public float tiempoEntreRafagas = 5f;
    private int cantidadMisiles = 3;
    public int oleadaActualParaMisiles = 6;

    [Header("Terremoto")]
    public GameObject objetivoPrioritario;
    public float radioTerremoto = 10f;
    public float tiempoEntreTerremotos = 5f;
    public float distanciaActivacionTerremoto = 5f;
    private bool puedeHacerTerremoto = true;
    private bool haciendoTerremoto = false;
    public int OleadaNmro = 3;

    [Header("Patrulla")]
    public float radioDePaseo = 15f;
    public float tiempoEsperaPaseo = 2f;
    private float cronometroPaseo;

    [Header("Estado")]
    public Animator animator;
    public float vida = 200f;
    public float dañoOndas = 18f;
    public float dañoEarth = 25f;

    public void ConfigurarDificultad(int numOleada)
    {
        oleadaActualParaMisiles = numOleada;
        float multiplicador = 1f + ((numOleada - 3) * 0.1f);
        vida *= multiplicador;
        dañoOndas *= multiplicador;
        dañoEarth *= multiplicador;
        cantidadMisiles = 2 + (numOleada / 3);

        if (agente != null) agente.speed = Mathf.Min(3.5f * multiplicador, 6f);
        vidaActivacionSegundaOnda = vida * 0.6f;
    }

    void Start()
    {
        if (agente == null) agente = GetComponent<NavMeshAgent>();

        ConfigurarDificultad(OleadaNmro);

        StartCoroutine(RutinaDisparoMisiles());
    }

    void Update()
    {
        if (haciendoTerremoto) return;

        ManejarPaseo();
        ManejarDeteccionDeOndas();
        VerificarProximidadParaTerremoto();
        ActualizarAnimaciones();
    }

    void ManejarPaseo()
    {
        if (objetivoPrioritario == null)
        {
            objetivoPrioritario = BuscarNuevoObjetivoPrioritario();
            if (objetivoPrioritario == null) return;
        }

        if (agente.isOnNavMesh)
        {
            agente.SetDestination(objetivoPrioritario.transform.position);

            float distancia = Vector3.Distance(transform.position, objetivoPrioritario.transform.position);
            if (distancia <= distanciaActivacionTerremoto)
            {
                if (puedeHacerTerremoto)
                {
                    StartCoroutine(SecuenciaTerremoto());
                }
            }
        }
    }

    GameObject BuscarNuevoObjetivoPrioritario()
    {
        objetivoPrioritario = null;
        GameObject[] objetivos = GameObject.FindGameObjectsWithTag(tagTorreta);

        if (objetivos.Length > 0)
        {
            int indiceAleatorio = Random.Range(0, objetivos.Length);
            return objetivos[indiceAleatorio];
        }

        return null;
    }

    IEnumerator SecuenciaTerremoto()
    {
        puedeHacerTerremoto = false;
        haciendoTerremoto = true;

        agente.isStopped = true;
        agente.velocity = Vector3.zero;

        if (animator != null)
        {
            animator.SetBool("IsEarth", true);
            animator.SetBool("IsWalk", false);
        }

        yield return new WaitForSeconds(0.7f);

        Collider[] afectados = Physics.OverlapSphere(transform.position, radioTerremoto);
        foreach (var hit in afectados)
        {
            if (hit.CompareTag(tagTorreta))
            {
                Debug.Log($"Impacto de Terremoto en: {hit.name}");
            }
        }

        yield return new WaitForSeconds(1.0f);

        if (animator != null) animator.SetBool("IsEarth", false);

        haciendoTerremoto = false;
        if (agente.isOnNavMesh) agente.isStopped = false;

        yield return new WaitForSeconds(tiempoEntreTerremotos);
        puedeHacerTerremoto = true;
    }

    IEnumerator RutinaDisparoMisiles()
    {
        while (true)
        {
            yield return new WaitForSeconds(tiempoEntreRafagas);

            if (!haciendoTerremoto)
            {
                for (int i = 0; i < cantidadMisiles; i++)
                {
                    LanzarMisil();
                    yield return new WaitForSeconds(0.3f);
                }
            }
        }
    }

    void LanzarMisil()
    {
        if (misilPrefab == null || firePointCentral == null) return;

        GameObject nuevoMisil = Instantiate(misilPrefab, firePointCentral.position, firePointCentral.rotation);
        MisilCentral scriptMisil = nuevoMisil.GetComponent<MisilCentral>();

        if (scriptMisil != null)
        {
            scriptMisil.Inicializar(oleadaActualParaMisiles);
        }
    }

    void ManejarDeteccionDeOndas()
    {
        GameObject[] todos = GameObject.FindGameObjectsWithTag(tagObjetivo);
        if (todos.Length == 0)
        {
            EliminarRayo(ref rayoIzquierda);
            EliminarRayo(ref rayoDerecha);
            return;
        }

        System.Array.Sort(todos, (a, b) => Vector3.Distance(transform.position, a.transform.position).CompareTo(Vector3.Distance(transform.position, b.transform.position)));

        ProcesarRayo(todos[0], origenIzquierda, ref rayoIzquierda);

        if (vida < vidaActivacionSegundaOnda && todos.Length > 1)
        {
            ProcesarRayo(todos[1], origenDerecha, ref rayoDerecha);
        }
        else
        {
            EliminarRayo(ref rayoDerecha);
        }
    }

    void ProcesarRayo(GameObject objetivo, Transform origen, ref GameObject rayo)
    {
        if (objetivo != null && Vector3.Distance(origen.position, objetivo.transform.position) <= rangoOndas)
        {
            if (rayo == null) rayo = Instantiate(tunderPrefab);
            Vector3 dir = objetivo.transform.position - origen.position;
            rayo.transform.position = origen.position + (dir / 2f);
            rayo.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0);
            Vector3 esc = rayo.transform.localScale;
            esc.y = dir.magnitude / 2f;
            rayo.transform.localScale = esc;

            // Aqui dañoOndas al objetivo
        }
        else
        {
            EliminarRayo(ref rayo);
        }
    }

    void VerificarProximidadParaTerremoto()
    {
        if (!puedeHacerTerremoto) return;

        Collider[] cercanos = Physics.OverlapSphere(transform.position, distanciaActivacionTerremoto);
        foreach (var col in cercanos)
        {
            if (col.CompareTag(tagObjetivo) || col.CompareTag(tagTorreta))
            {
                StartCoroutine(SecuenciaTerremoto());
                break;
            }
        }
    }

    void EliminarRayo(ref GameObject rayo)
    {
        if (rayo != null) { Destroy(rayo); rayo = null; }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;
        if (!haciendoTerremoto)
        {
            animator.SetBool("IsWalk", agente.velocity.magnitude > 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaActivacionTerremoto);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radioTerremoto);
    }
}
