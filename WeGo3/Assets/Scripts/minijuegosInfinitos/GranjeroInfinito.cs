using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GranjeroInfinito : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "GranjeroInfinito";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Configuración del Minijuego")]
    [SerializeField] private float tiempoTotalBatalla = 30f;
    [SerializeField] private float tiempoEnPantallaTopo = 1.2f;
    [SerializeField] private float intervaloEntreTopos = 0.5f;

    [Header("Agujeros y Prefabs")]
    [SerializeField] private Transform[] agujeros;
    [SerializeField] private GameObject prefabTopoBueno;
    [SerializeField] private GameObject prefabTopoMalo;
    [Range(0f, 1f)]
    [SerializeField] private float probabilidadTopoMalo = 0.3f;

    [Header("UI de Progreso")]
    [SerializeField] private TextMeshProUGUI textoPuntos;
    [SerializeField] private TextMeshProUGUI textoTiempo;

    private int puntosActuales = 0;
    private float tiempoRestante;
    private bool juegoActivo = false;

    private bool[] agujerosOcupados;
    private readonly List<GameObject> toposActivos = new List<GameObject>();

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        if (agujeros != null)
        {
            agujerosOcupados = new bool[agujeros.Length];
        }

        puntosActuales = 0;
        tiempoRestante = tiempoTotalBatalla;
        juegoActivo = false;

        ActualizarUI();
    }

    public void ComenzarBoton()
    {
        StartCoroutine(ComenzarJuegoCorrutina());
    }

    private IEnumerator ComenzarJuegoCorrutina()
    {
        if (panelInicio != null) panelInicio.SetActive(false);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(true);

        if (textoInicio != null) textoInicio.text = "LISTOS...?";
        yield return new WaitForSeconds(2f);

        if (textoInicio != null) textoInicio.text = "¡YA!";
        yield return new WaitForSeconds(1f);

        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);

        IniciarJuegoReal();
    }

    private void IniciarJuegoReal()
    {
        LimpiarToposActivos();
        if (agujeros != null)
        {
            agujerosOcupados = new bool[agujeros.Length];
        }

        juegoActivo = true;
        puntosActuales = 0;
        tiempoRestante = tiempoTotalBatalla;

        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        ActualizarUI();
        StartCoroutine(RutinaSpawnTopos());
    }

    void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;
        ActualizarUI();

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            FinalizarPartida();
        }
    }

    private IEnumerator RutinaSpawnTopos()
    {
        while (juegoActivo)
        {
            if (agujeros != null && agujeros.Length > 0)
            {
                int indiceAgujero = ObtenerAgujeroLibreAleatorio();

                if (indiceAgujero != -1)
                {
                    agujerosOcupados[indiceAgujero] = true;

                    bool esMalo = Random.value < probabilidadTopoMalo;
                    GameObject prefabAUsar = esMalo ? prefabTopoMalo : prefabTopoBueno;

                    if (prefabAUsar != null)
                    {
                        GameObject topoInstanciado = Instantiate(prefabAUsar, agujeros[indiceAgujero].position, Quaternion.identity, agujeros[indiceAgujero]);
                        toposActivos.Add(topoInstanciado);

                        ElementoTopoGranjeroInfinito scriptTopo = topoInstanciado.GetComponent<ElementoTopoGranjeroInfinito>();
                        if (scriptTopo == null) scriptTopo = topoInstanciado.AddComponent<ElementoTopoGranjeroInfinito>();

                        scriptTopo.Inicializar(esMalo, tiempoEnPantallaTopo, this, indiceAgujero);
                    }
                }
            }

            yield return new WaitForSeconds(intervaloEntreTopos);
        }
    }

    private int ObtenerAgujeroLibreAleatorio()
    {
        List<int> indicesLibres = new List<int>();
        for (int i = 0; i < agujeros.Length; i++)
        {
            if (!agujerosOcupados[i])
            {
                indicesLibres.Add(i);
            }
        }

        if (indicesLibres.Count == 0) return -1;

        int randomIndex = Random.Range(0, indicesLibres.Count);
        return indicesLibres[randomIndex];
    }

    public void LiberarAgujero(int indice)
    {
        if (agujerosOcupados != null && indice >= 0 && indice < agujerosOcupados.Length)
        {
            agujerosOcupados[indice] = false;
        }
    }

    public void RegistrarGolpe(bool esMalo)
    {
        if (!juegoActivo) return;

        if (!esMalo)
        {
            puntosActuales++;
            if (MusicSoundController.Instance != null)
                MusicSoundController.Instance.ReproducirSFX("Acierto");
        }
        else
        {
            puntosActuales -= 2;
            if (puntosActuales < 0) puntosActuales = 0;
            if (MusicSoundController.Instance != null)
                MusicSoundController.Instance.ReproducirSFX("Fallo");
        }

        ActualizarUI();
    }

    public void RegistrarTopoExpirado(bool esMalo)
    {
        if (!juegoActivo) return;

        if (!esMalo)
        {
            puntosActuales--;
            if (puntosActuales < 0) puntosActuales = 0;
            ActualizarUI();
        }
    }

    private void LimpiarToposActivos()
    {
        foreach (GameObject topo in toposActivos)
        {
            if (topo != null) Destroy(topo);
        }
        toposActivos.Clear();
    }

    private void ActualizarUI()
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = $"Ñemas: {puntosActuales}";
        }
        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarToposActivos();

        bool esNuevoRecord = false;
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);
            esNuevoRecord = puntosActuales > puntuacionAnterior;
            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(idMinijuego, puntosActuales);
        }

        if (panelFin != null) panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text = $"Puntuacion final: {puntosActuales}";
        }

        if (textoNuevoRecord != null)
        {
            textoNuevoRecord.gameObject.SetActive(esNuevoRecord);
            if (esNuevoRecord)
            {
                textoNuevoRecord.text = "¡Nueva puntuación más alta!";
            }
        }

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Ganar");
        }
    }

    public void VolverMenuInfinito()
    {
        if (!string.IsNullOrEmpty(nombreEscenaMenuInfinito) && TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(nombreEscenaMenuInfinito);
        }
    }
}