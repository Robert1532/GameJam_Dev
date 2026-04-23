using UnityEngine;

public class MisilCentral : MonoBehaviour
{
    private Transform objetivoFinal;
    public float velocidad = 15f;
    public float daño = 30f;
    public float radioExplosion = 3f;
    //public GameObject efectoExplosion;

    [Header("Lanzamiento")]
    public float fuerzaImpulsoInicial = 20f;
    public float tiempoDePropulsionInicial = 0.5f;
    private float cronometroLanzamiento = 0f;
    private Vector3 direccionInicial;

    private string[] subPartes = { "Sensor", "Motor", "Cañon", "Torreta" };

    public void Inicializar(int nivelOleada)
    {
        daño += (nivelOleada * 2);
        direccionInicial = transform.forward;

        BuscarObjetivoAleatorio();
    }

    void BuscarObjetivoAleatorio()
    {
        string tagElegido = subPartes[Random.Range(0, subPartes.Length)];
        GameObject[] posiblesObjetivos = GameObject.FindGameObjectsWithTag(tagElegido);

        if (posiblesObjetivos.Length > 0)
        {
            objetivoFinal = posiblesObjetivos[Random.Range(0, posiblesObjetivos.Length)].transform;
        }
        else
        {
            GameObject respaldo = GameObject.FindGameObjectWithTag("Torreta");
            if (respaldo) objetivoFinal = respaldo.transform;
        }
    }

    void Update()
    {
        cronometroLanzamiento += Time.deltaTime;

        if (cronometroLanzamiento < tiempoDePropulsionInicial)
        {
            transform.position += direccionInicial * fuerzaImpulsoInicial * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direccionInicial);
        }
        else
        {
            if (objetivoFinal == null)
            {
                transform.position += transform.forward * velocidad * Time.deltaTime;
                Destroy(gameObject, 3f);
                return;
            }

            Vector3 direccionObjetivo = (objetivoFinal.position - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, direccionObjetivo, Time.deltaTime * 5f);

            transform.position += transform.forward * velocidad * Time.deltaTime;

            if (Vector3.Distance(transform.position, objetivoFinal.position) < 0.7f)
            {
                Explotar();
            }
        }
    }

    void Explotar()
    {
        /*if (efectoExplosion != null)
            Instantiate(efectoExplosion, transform.position, Quaternion.identity);*/

        // Aquí aplicar el daño real de vida del objetivo
        Debug.Log($"Misil impactó en {objetivoFinal.name} causando {daño} de daño.");

        Destroy(gameObject);
    }
}
