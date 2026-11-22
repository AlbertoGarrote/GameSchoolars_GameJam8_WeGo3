using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CristalMinijuego : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Objetos de la Secuencia")]
    [SerializeField] private List<ElementoSecuenciaSimon> objetosSecuencia = new List<ElementoSecuenciaSimon>();
    [SerializeField] private int longitudInicialSecuencia = 3;
    [SerializeField] private bool incrementarLongitudCadaRonda = true;
    [SerializeField] private float tiempoIluminado = 0.55f;
    [SerializeField] private float pausaEntreElementos = 0.25f;
    [SerializeField] private float pausaAntesDeNuevaRonda = 0.8f;

    [Header("Reglas de Rondas y Vidas")]
    [SerializeField] private int rondasObjetivo = 5;
    [SerializeField] private int vidasMaximas = 3;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderVidas;
    [SerializeField] private Slider sliderPuntuacion;
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
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

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

        ApagarTodosLosObjetos();
        ActualizarUI();

        IniciarSiguienteRonda();
    }

    private void IniciarSiguienteRonda()
    {
        if (!juegoActivo) return;

        turnoJugador = false;
        indicePasoActual = 0;

        int cantidadPasos = incrementarLongitudCadaRonda
            ? longitudInicialSecuencia + rondasCompletadas
            : longitudInicialSecuencia;

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
            ElementoSecuenciaSimon elem = (indiceElemento >= 0 && indiceElemento < objetosSecuencia.Count)
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

    public void RegistrarPulsacionJugador(int indicePulsado, ElementoSecuenciaSimon elementoPulsado)
    {
        if (!juegoActivo || !turnoJugador) return;
        if (secuenciaActual.Count == 0 || indicePasoActual >= secuenciaActual.Count) return;

        int indiceEsperado = secuenciaActual[indicePasoActual];

        if (indicePulsado == indiceEsperado)
        {
            // Acierto en este paso
            if (elementoPulsado != null)
            {
                elementoPulsado.Parpadear(0.2f);
            }

            if (MusicSoundController.Instance != null)
            {
                MusicSoundController.Instance.ReproducirSFX("Acierto");
            }

            indicePasoActual++;

            // Si completó toda la secuencia de la ronda
            if (indicePasoActual >= secuenciaActual.Count)
            {
                turnoJugador = false;
                rondasCompletadas++;
                ActualizarUI();

                if (rondasCompletadas >= rondasObjetivo)
                {
                    GanarBatalla();
                }
                else
                {
                    StartCoroutine(PrepararNuevaRondaTrasAcierto());
                }
            }
        }
        else
        {
            // Fallo en la secuencia
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
                PerderBatalla();
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

        if (sliderPuntuacion != null)
        {
            sliderPuntuacion.minValue = 0f;
            sliderPuntuacion.maxValue = rondasObjetivo;
            sliderPuntuacion.value = rondasCompletadas;
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"Vidas: {vidasActuales} / {vidasMaximas}";
        }

        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"Puntos: {rondasCompletadas} / {rondasObjetivo}";
        }
    }

    private void ActualizarTextoEstado(string mensaje)
    {
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }

    private void GanarBatalla()
    {
        juegoActivo = false;
        turnoJugador = false;
        StopAllCoroutines();
        ApagarTodosLosObjetos();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Ganar");
    }

    private void PerderBatalla()
    {
        juegoActivo = false;
        turnoJugador = false;
        StopAllCoroutines();
        ApagarTodosLosObjetos();

        if (panelDerrota != null) panelDerrota.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Perder");
    }

    public void volverGym(bool victoriaConseguida)
    {
        if (victoriaConseguida)
        {
            if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(flagVictoria))
            {
                EstadoJuegoManager.Instance.ActivarFlag(flagVictoria);
            }
        }

        if (!string.IsNullOrEmpty(nombreEscenaGym))
        {
            if (memoriaPosicion != null)
            {
                memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
                memoriaPosicion.usarPosicionGuardada = true;
            }

            if (TransicionController.Instance != null)
            {
                TransicionController.Instance.CargarEscena(nombreEscenaGym);
            }
        }
    }
}