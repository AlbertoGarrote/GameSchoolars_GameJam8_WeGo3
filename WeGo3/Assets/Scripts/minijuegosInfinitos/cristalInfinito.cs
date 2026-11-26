using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CristalInfinito : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "CristalInfinito";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Objetos de la Secuencia")]
    [SerializeField] private List<ElementoSecuenciaCristalInfinito> objetosSecuencia = new List<ElementoSecuenciaCristalInfinito>();
    [SerializeField] private int longitudInicialSecuencia = 3;
    [SerializeField] private float tiempoIluminado = 0.55f;
    [SerializeField] private float pausaEntreElementos = 0.25f;
    [SerializeField] private float pausaAntesDeNuevaRonda = 0.8f;

    [Header("Vidas")]
    [SerializeField] private int vidasMaximas = 3;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderVidas;
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoEstado;

    private int rondasCompletadas = 0;
    private int vidasActuales = 3;
    private int indicePasoActual = 0;

    private bool juegoActivo = false;
    private bool turnoJugador = false;

    private readonly List<int> secuenciaActual = new List<int>();
    private Coroutine rutinaRondaActual;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        ConfigurarElementos();

        rondasCompletadas = 0;
        vidasActuales = vidasMaximas;
        juegoActivo = false;
        turnoJugador = false;

        ActualizarUI();
    }

    private void ConfigurarElementos()
    {
        for (int i = 0; i < objetosSecuencia.Count; i++)
        {
            if (objetosSecuencia[i] != null)
            {
                objetosSecuencia[i].Configurar(i, this);
            }
        }
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
        rondasCompletadas = 0;
        vidasActuales = vidasMaximas;
        indicePasoActual = 0;
        juegoActivo = true;
        turnoJugador = false;

        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        ApagarTodosLosObjetos();
        ActualizarUI();

        IniciarSiguienteRonda();
    }

    private void IniciarSiguienteRonda()
    {
        if (!juegoActivo) return;

        turnoJugador = false;
        indicePasoActual = 0;

        int cantidadPasos = Mathf.Max(1, longitudInicialSecuencia + rondasCompletadas);
        GenerarSecuenciaAleatoria(cantidadPasos);

        if (rutinaRondaActual != null) StopCoroutine(rutinaRondaActual);
        rutinaRondaActual = StartCoroutine(ReproducirSecuenciaCorrutina());
    }

    private void GenerarSecuenciaAleatoria(int cantidad)
    {
        secuenciaActual.Clear();
        if (objetosSecuencia == null || objetosSecuencia.Count == 0) return;

        for (int i = 0; i < cantidad; i++)
        {
            int indiceAleatorio = Random.Range(0, objetosSecuencia.Count);
            secuenciaActual.Add(indiceAleatorio);
        }
    }

    private IEnumerator ReproducirSecuenciaCorrutina()
    {
        ApagarTodosLosObjetos();
        ActualizarTextoEstado("Observa el orden...");
        yield return new WaitForSeconds(0.4f);

        for (int i = 0; i < secuenciaActual.Count; i++)
        {
            if (!juegoActivo) yield break;

            int indiceElemento = secuenciaActual[i];
            ElementoSecuenciaCristalInfinito elem = (indiceElemento >= 0 && indiceElemento < objetosSecuencia.Count)
                ? objetosSecuencia[indiceElemento]
                : null;

            if (elem != null)
            {
                elem.EstablecerIluminado(true);
                if (MusicSoundController.Instance != null)
                {
                    MusicSoundController.Instance.ReproducirSFX("Pulso");
                }
            }

            yield return new WaitForSeconds(tiempoIluminado);

            if (elem != null)
            {
                elem.EstablecerIluminado(false);
            }

            yield return new WaitForSeconds(pausaEntreElementos);
        }

        if (!juegoActivo) yield break;

        turnoJugador = true;
        ActualizarTextoEstado("¡Tu turno! Repite la secuencia");
    }

    public void RegistrarPulsacionJugador(int indicePulsado, ElementoSecuenciaCristalInfinito elementoPulsado)
    {
        if (!juegoActivo || !turnoJugador) return;
        if (secuenciaActual.Count == 0 || indicePasoActual >= secuenciaActual.Count) return;

        int indiceEsperado = secuenciaActual[indicePasoActual];

        if (indicePulsado == indiceEsperado)
        {
            if (elementoPulsado != null)
            {
                elementoPulsado.Parpadear(0.2f);
            }

            if (MusicSoundController.Instance != null)
            {
                MusicSoundController.Instance.ReproducirSFX("Acierto");
            }

            indicePasoActual++;

            if (indicePasoActual >= secuenciaActual.Count)
            {
                turnoJugador = false;
                rondasCompletadas++;
                ActualizarUI();
                StartCoroutine(PrepararNuevaRondaTrasAcierto());
            }
        }
        else
        {
            turnoJugador = false;
            vidasActuales = Mathf.Max(0, vidasActuales - 1);
            ActualizarUI();

            if (elementoPulsado != null)
            {
                elementoPulsado.Parpadear(0.25f);
            }

            if (MusicSoundController.Instance != null)
            {
                MusicSoundController.Instance.ReproducirSFX("Fallo");
            }

            if (vidasActuales <= 0)
            {
                FinalizarPartida();
            }
            else
            {
                StartCoroutine(RepetirMismaRondaTrasFallo());
            }
        }
    }

    private IEnumerator PrepararNuevaRondaTrasAcierto()
    {
        ActualizarTextoEstado($"¡Ronda {rondasCompletadas} superada!");
        yield return new WaitForSeconds(pausaAntesDeNuevaRonda);
        IniciarSiguienteRonda();
    }

    private IEnumerator RepetirMismaRondaTrasFallo()
    {
        ActualizarTextoEstado("¡Fallaste! Repitiendo ronda...");
        yield return new WaitForSeconds(pausaAntesDeNuevaRonda);

        if (!juegoActivo) yield break;

        indicePasoActual = 0;
        if (rutinaRondaActual != null) StopCoroutine(rutinaRondaActual);
        rutinaRondaActual = StartCoroutine(ReproducirSecuenciaCorrutina());
    }

    private void ApagarTodosLosObjetos()
    {
        foreach (var elem in objetosSecuencia)
        {
            if (elem != null)
            {
                elem.EstablecerIluminado(false);
            }
        }
    }

    private void ActualizarUI()
    {
        if (sliderVidas != null)
        {
            sliderVidas.minValue = 0f;
            sliderVidas.maxValue = vidasMaximas;
            sliderVidas.value = vidasActuales;
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"Vidas: {vidasActuales} / {vidasMaximas}";
        }

        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"Rondas: {rondasCompletadas}";
        }
    }

    private void ActualizarTextoEstado(string mensaje)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        turnoJugador = false;
        StopAllCoroutines();
        ApagarTodosLosObjetos();

        bool esNuevoRecord = false;
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);
            esNuevoRecord = rondasCompletadas > puntuacionAnterior;
            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(idMinijuego, rondasCompletadas);
        }

        if (panelFin != null) panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text = $"Puntuacion final: {rondasCompletadas}";
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
            MusicSoundController.Instance.ReproducirSFX("Perder");
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