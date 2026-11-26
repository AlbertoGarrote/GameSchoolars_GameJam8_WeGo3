using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class BatallaInfinita : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "BatallaInfinita";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Tiempo y Puntuación")]
    [SerializeField] private float tiempoTotalPartida = 30f;
    [SerializeField] private int puntosPorClick = 1;
    [SerializeField] private int penalizacionAlFallarEsquiva = 30;

    [Header("Mecánica de Esquiva")]
    [SerializeField] private GameObject avisoAtaqueEnemigo;
    [SerializeField] private float intervaloAtaqueEnemigo = 3f;
    [SerializeField] private float tiempoVentanaEsquiva = 0.4f;
    [SerializeField] private Key teclaEsquiva = Key.Space;

    [Header("Referencias Visuales y de Animación")]
    [SerializeField] private Transform spriteEnemigo;
    [SerializeField] private Transform spriteJugador;
    [SerializeField] private float distanciaEmbestida = 1.5f;
    [SerializeField] private float distanciaEsquivaLateral = 1f;

    [Header("UI de Progreso")]
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoTiempo;

    private int puntuacionActual = 0;
    private float tiempoRestante = 0f;
    private bool juegoActivo = false;
    private bool esperandoEsquiva = false;
    private float temporizadorAtaque = 0f;
    private float temporizadorVentanaEsquiva = 0f;

    private Vector3 posicionOriginalEnemigo;
    private Vector3 posicionOriginalJugador;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        puntuacionActual = 0;
        tiempoRestante = tiempoTotalPartida;
        juegoActivo = false;
        esperandoEsquiva = false;

        if (spriteEnemigo != null) posicionOriginalEnemigo = spriteEnemigo.localPosition;
        if (spriteJugador != null) posicionOriginalJugador = spriteJugador.localPosition;

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
        StopAllCoroutines();
        ResetearPosicionesVisuales();

        puntuacionActual = 0;
        tiempoRestante = tiempoTotalPartida;
        juegoActivo = true;
        esperandoEsquiva = false;
        temporizadorAtaque = intervaloAtaqueEnemigo;
        temporizadorVentanaEsquiva = 0f;

        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        ActualizarUI();
    }

    void Update()
    {
        if (!juegoActivo) return;

        // 1. TIEMPO DE PARTIDA
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            FinalizarPartida();
            return;
        }

        // 2. CLICS PARA SUMAR PUNTUACIÓN
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SumarPuntosPorClick();
        }

        // 3. TEMPORIZADOR DE ATAQUE DEL ENEMIGO
        if (!esperandoEsquiva)
        {
            temporizadorAtaque -= Time.deltaTime;
            if (temporizadorAtaque <= 0f)
            {
                IniciarAvisoAtaque();
            }
        }
        else
        {
            // 4. VENTANA DE ESQUIVA
            temporizadorVentanaEsquiva -= Time.deltaTime;

            if (Keyboard.current != null && Keyboard.current[teclaEsquiva].wasPressedThisFrame)
            {
                EsquivaExitosa();
            }
            else if (temporizadorVentanaEsquiva <= 0f)
            {
                FalloEsquiva();
            }
        }

        ActualizarUI();
    }

    private void SumarPuntosPorClick()
    {
        puntuacionActual += puntosPorClick;
        ActualizarUI();
    }

    private void IniciarAvisoAtaque()
    {
        esperandoEsquiva = true;
        temporizadorVentanaEsquiva = tiempoVentanaEsquiva;

        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(true);
    }

    private void EsquivaExitosa()
    {
        esperandoEsquiva = false;
        temporizadorAtaque = intervaloAtaqueEnemigo;

        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        if (MusicSoundController.Instance != null)
            MusicSoundController.Instance.ReproducirSFX("Acierto");

        if (spriteJugador != null) StartCoroutine(RutinaEsquivaJugador());
        if (spriteEnemigo != null) StartCoroutine(RutinaPlacajeEnemigo());
    }

    private void FalloEsquiva()
    {
        esperandoEsquiva = false;
        temporizadorAtaque = intervaloAtaqueEnemigo;

        puntuacionActual = Mathf.Max(0, puntuacionActual - penalizacionAlFallarEsquiva);
        ActualizarUI();

        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        if (MusicSoundController.Instance != null)
            MusicSoundController.Instance.ReproducirSFX("Fallo");

        if (spriteEnemigo != null) StartCoroutine(RutinaPlacajeEnemigo());
    }

    private IEnumerator RutinaPlacajeEnemigo()
    {
        if (spriteEnemigo == null) yield break;

        Vector3 posInicio = posicionOriginalEnemigo;
        Vector3 posAtaque = posicionOriginalEnemigo + new Vector3(-distanciaEmbestida, 0, 0);

        float duracionIda = 0.2f;
        float tiempo = 0f;

        while (tiempo < duracionIda)
        {
            if (spriteEnemigo == null) yield break;
            tiempo += Time.deltaTime;
            float t = tiempo / duracionIda;
            spriteEnemigo.localPosition = Vector3.Lerp(posInicio, posAtaque, t * t);
            yield return null;
        }
        spriteEnemigo.localPosition = posAtaque;

        float duracionVuelta = 0.10f;
        tiempo = 0f;
        while (tiempo < duracionVuelta)
        {
            if (spriteEnemigo == null) yield break;
            tiempo += Time.deltaTime;
            spriteEnemigo.localPosition = Vector3.Lerp(posAtaque, posicionOriginalEnemigo, tiempo / duracionVuelta);
            yield return null;
        }
        spriteEnemigo.localPosition = posicionOriginalEnemigo;
    }

    private IEnumerator RutinaEsquivaJugador()
    {
        if (spriteJugador == null) yield break;

        Vector3 posCentro = posicionOriginalJugador;
        Vector3 posLateral = posicionOriginalJugador + new Vector3(0, -distanciaEsquivaLateral, 0);

        float t = 0f;
        while (t < 0.2f)
        {
            if (spriteJugador == null) yield break;
            t += Time.deltaTime;
            spriteJugador.localPosition = Vector3.Lerp(posCentro, posLateral, t / 0.2f);
            yield return null;
        }

        t = 0f;
        while (t < 0.12f)
        {
            if (spriteJugador == null) yield break;
            t += Time.deltaTime;
            spriteJugador.localPosition = Vector3.Lerp(posLateral, posCentro, t / 0.12f);
            yield return null;
        }
        spriteJugador.localPosition = posCentro;
    }

    private void ResetearPosicionesVisuales()
    {
        if (spriteEnemigo != null) spriteEnemigo.localPosition = posicionOriginalEnemigo;
        if (spriteJugador != null) spriteJugador.localPosition = posicionOriginalJugador;
    }

    private void ActualizarUI()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"Puntos: {puntuacionActual}";
        }

        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        esperandoEsquiva = false;
        StopAllCoroutines();
        ResetearPosicionesVisuales();

        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        bool esNuevoRecord = false;
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);
            esNuevoRecord = puntuacionActual > puntuacionAnterior;
            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(idMinijuego, puntuacionActual);
        }

        if (panelFin != null) panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text = $"Puntuacion final: {puntuacionActual}";
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