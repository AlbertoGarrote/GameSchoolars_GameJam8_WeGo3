using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CuandoLlueveMinigame : MonoBehaviour
{
    public static CuandoLlueveMinigame Instance { get; private set; }

    [Header("UI y Paneles")]
    public GameObject panelInicio;
    public GameObject temporizadorInicial;
    public TMP_Text textoInicio;
    public GameObject panelVictoria;
    public GameObject panelDerrota;
    public GameObject botonContinuar;

    [Header("Elementos del Minijuego")]
    public Transform caminante;
    public Transform puntoA;
    public Transform puntoB;
    public GameObject prefabGota;
    public Transform[] puntosDeSpawnGotas;

    [Header("Configuración de Dificultad")]
    public float cadenciaGotas = 0.5f; // Frecuencia de caída de las gotas (en segundos)
    public float velocidadMinCaminante = 1f;
    public float velocidadMaxCaminante = 6f;

    private bool juegoActivo = false;
    private float velocidadActualCaminante;
    private float tiempoCambioVelocidad;

    private List<GameObject> gotasInstanciadas = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        panelInicio.SetActive(true);
        temporizadorInicial.SetActive(false);

        if (caminante != null && puntoA != null)
        {
            caminante.position = puntoA.position;
        }
    }

    public void ComenzarBoton()
    {
        StartCoroutine(ComenzarJuego());
    }

    private IEnumerator ComenzarJuego()
    {
        panelInicio.SetActive(false);
        temporizadorInicial.SetActive(true);
        yield return new WaitForSeconds(2f);
        textoInicio.text = "LISTOS...?";
        yield return new WaitForSeconds(2f);
        textoInicio.text = "¡YA!";
        yield return new WaitForSeconds(1f);
        temporizadorInicial.SetActive(false);

        IniciarJuego();
    }

    public void IniciarJuego()
    {
        juegoActivo = true;

        CambiarVelocidadCaminante();
        StartCoroutine(GenerarGotasLoop());
    }

    private void Update()
    {
        if (!juegoActivo) return;

        // Movimiento del gato de A a B
        MoverCaminante();
    }

    private void MoverCaminante()
    {
        if (caminante == null || puntoB == null) return;

        // Cambiar la velocidad de forma aleatoria periódicamente
        tiempoCambioVelocidad -= Time.deltaTime;
        if (tiempoCambioVelocidad <= 0)
        {
            CambiarVelocidadCaminante();
        }

        caminante.position = Vector3.MoveTowards(caminante.position, puntoB.position, velocidadActualCaminante * Time.deltaTime);

        // Si el gato llega sano y salvo al punto B, ganas
        if (Vector3.Distance(caminante.position, puntoB.position) < 0.1f)
        {
            StartCoroutine(Ganar());
        }
    }

    private void CambiarVelocidadCaminante()
    {
        velocidadActualCaminante = Random.Range(velocidadMinCaminante, velocidadMaxCaminante);
        tiempoCambioVelocidad = Random.Range(0.4f, 1.2f); // Cambia de ritmo súbitamente
    }

    private IEnumerator GenerarGotasLoop()
    {
        while (juegoActivo)
        {
            if (prefabGota != null && puntosDeSpawnGotas.Length > 0)
            {
                int indiceSpawn = Random.Range(0, puntosDeSpawnGotas.Length);
                GameObject nuevaGota = Instantiate(prefabGota, puntosDeSpawnGotas[indiceSpawn].position, Quaternion.identity);
                gotasInstanciadas.Add(nuevaGota);
            }
            yield return new WaitForSeconds(cadenciaGotas);
        }
    }

    public void ReportarColisionProyectil()
    {
        if (juegoActivo)
        {
            StartCoroutine(Perder());
        }
    }

    private IEnumerator Ganar()
    {
        juegoActivo = false;
        LimpiarGotas();
        panelVictoria.SetActive(true);
        yield return new WaitForSeconds(2f);
        botonContinuar.SetActive(true);
    }

    private IEnumerator Perder()
    {
        juegoActivo = false;
        LimpiarGotas();
        panelDerrota.SetActive(true);
        yield return new WaitForSeconds(2f);
        botonContinuar.SetActive(true);
    }

    private void LimpiarGotas()
    {
        foreach (GameObject gota in gotasInstanciadas)
        {
            if (gota != null)
            {
                Destroy(gota);
            }
        }
        gotasInstanciadas.Clear();
    }
}