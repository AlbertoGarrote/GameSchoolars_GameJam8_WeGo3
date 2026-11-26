
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VolleyInifnito : MonoBehaviour
{
    [Header("Identificador")]
    [SerializeField] private string idMinijuego = "VolleyInifnito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;

    [Header("UI de Partida")]
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private TMP_Text textoEstadoDificultad;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;
    [SerializeField] private Slider sliderVidas;

    [Header("Vidas")]
    [SerializeField] private int vidasMaximas = 3;

    [Header("Jugador (Campo Propio)")]
    [SerializeField] private Transform jugador;
    [SerializeField] private float velocidadJugador = 8f;
    [SerializeField] private float minXJugador = -8f;
    [SerializeField] private float maxXJugador = -0.8f;
    [SerializeField] private float distanciaGolpeJugador = 1.3f;

    [Header("Rival (Campo Rival)")]
    [SerializeField] private Transform rival;
    [SerializeField] private float velocidadRival = 6.5f;
    [SerializeField] private float minXRival = 0.8f;
    [SerializeField] private float maxXRival = 8f;
    [SerializeField] private float distanciaGolpeRival = 1.3f;

    [Header("Pelota y Trayectoria")]
    [SerializeField] private Transform pelota;
    [SerializeField] private float alturaSuelo = -3f;
    [SerializeField] private float alturaArco = 3.5f;
    [SerializeField] private float duracionVuelo = 1.1f;

    [Header("Dificultad Progresiva")]
    [Tooltip("Cada cuántos segundos aumenta la dificultad.")]
    [SerializeField] private float intervaloAumentoDificultad = 8f;

    [Tooltip("Reducción de la duración del vuelo por nivel.")]
    [SerializeField] private float reduccionDuracionVuelo = 0.08f;

    [Tooltip("Duración mínima del vuelo.")]
    [SerializeField] private float duracionVueloMinima = 0.35f;

    [Tooltip("Aumento de la velocidad del rival por nivel.")]
    [SerializeField] private float aumentoVelocidadRival = 0.5f;

    [Tooltip("Desviación lateral máxima inicial.")]
    [SerializeField] private float desviacionInicial = 0.15f;

    [Tooltip("Aumento de la desviación por nivel.")]
    [SerializeField] private float aumentoDesviacion = 0.12f;

    [Tooltip("Desviación lateral máxima permitida.")]
    [SerializeField] private float desviacionMaxima = 1.5f;

    [Tooltip("Variación máxima del arco por nivel.")]
    [SerializeField] private float variacionArcoPorNivel = 0.2f;

    private int vidasActuales;
    private int puntuacionActual;
    private int nivelDificultad;
    private int intercambiosPelota;

    // Este tiempo se utiliza únicamente para aumentar la dificultad.
    private float tiempoSobrevivido;
    private float tiempoSiguienteDificultad;

    private bool juegoActivo;
    private bool cuentaAtrasIniciada;

    private bool pelotaHaciaJugador;
    private bool turnoMovimientoRival;

    private Vector2 puntoInicioVuelo;
    private Vector2 puntoDestinoAterrizaje;

    private float progresoVuelo;
    private float duracionVueloActual;
    private float alturaArcoActual;
    private float desviacionLateralActual;
    private float faseDesviacion;

    private void Start()
    {
        if (panelInicio != null)
            panelInicio.SetActive(true);

        if (temporizadorInicial != null)
            temporizadorInicial.SetActive(false);

        if (panelFin != null)
            panelFin.SetActive(false);

        vidasActuales = vidasMaximas;
        puntuacionActual = 0;
        tiempoSobrevivido = 0f;
        nivelDificultad = 0;
        intercambiosPelota = 0;

        juegoActivo = false;
        cuentaAtrasIniciada = false;
        turnoMovimientoRival = false;

        ActualizarUI();
    }

    public void ComenzarBoton()
    {
        if (cuentaAtrasIniciada || juegoActivo)
            return;

        cuentaAtrasIniciada = true;
        StartCoroutine(ComenzarJuegoCorrutina());
    }

    private IEnumerator ComenzarJuegoCorrutina()
    {
        if (panelInicio != null)
            panelInicio.SetActive(false);

        if (temporizadorInicial != null)
            temporizadorInicial.SetActive(true);

        if (textoInicio != null)
            textoInicio.text = "LISTOS...";

        yield return new WaitForSeconds(2f);

        if (textoInicio != null)
            textoInicio.text = "¡YA!";

        yield return new WaitForSeconds(1f);

        if (temporizadorInicial != null)
            temporizadorInicial.SetActive(false);

        IniciarJuegoReal();
    }

    private void IniciarJuegoReal()
    {
        vidasActuales = vidasMaximas;
        puntuacionActual = 0;
        tiempoSobrevivido = 0f;
        nivelDificultad = 0;
        intercambiosPelota = 0;

        tiempoSiguienteDificultad = intervaloAumentoDificultad;

        juegoActivo = true;
        turnoMovimientoRival = false;

        if (jugador != null)
        {
            Vector3 pos = jugador.position;
            pos.x = Mathf.Clamp(pos.x, minXJugador, maxXJugador);
            jugador.position = pos;
        }

        if (rival != null)
        {
            Vector3 pos = rival.position;
            pos.x = Mathf.Clamp(pos.x, minXRival, maxXRival);
            rival.position = pos;
        }

        ActualizarUI();
        LanzarPelotaHacia(true);
    }

    private void Update()
    {
        if (!juegoActivo)
            return;

        tiempoSobrevivido += Time.deltaTime;

        ComprobarAumentoDificultad();

        MoverJugador();

        if (turnoMovimientoRival && !pelotaHaciaJugador)
        {
            MoverRivalHaciaObjetivo();
        }

        ActualizarPelota();
    }

    private void ComprobarAumentoDificultad()
    {
        if (tiempoSobrevivido >= tiempoSiguienteDificultad)
        {
            nivelDificultad++;

            tiempoSiguienteDificultad += intervaloAumentoDificultad;

            if (MusicSoundController.Instance != null)
            {
                MusicSoundController.Instance.ReproducirSFX("Interactuar");
            }

            ActualizarUI();
        }
    }

    private void MoverJugador()
    {
        if (jugador == null || Keyboard.current == null)
            return;

        float inputX = 0f;

        if (Keyboard.current.aKey.isPressed)
            inputX -= 1f;

        if (Keyboard.current.dKey.isPressed)
            inputX += 1f;

        Vector3 pos = jugador.position;

        pos.x = Mathf.Clamp(
            pos.x + inputX * velocidadJugador * Time.deltaTime,
            minXJugador,
            maxXJugador
        );

        jugador.position = pos;
    }

    private void MoverRivalHaciaObjetivo()
    {
        if (rival == null)
            return;

        float velocidadActual =
            velocidadRival + nivelDificultad * aumentoVelocidadRival;

        float objetivoX = Mathf.Clamp(
            puntoDestinoAterrizaje.x,
            minXRival,
            maxXRival
        );

        Vector3 pos = rival.position;

        pos.x = Mathf.MoveTowards(
            pos.x,
            objetivoX,
            velocidadActual * Time.deltaTime
        );

        rival.position = pos;
    }

    private void LanzarPelotaHacia(bool haciaJugador)
    {
        pelotaHaciaJugador = haciaJugador;
        turnoMovimientoRival = !haciaJugador;

        progresoVuelo = 0f;

        Vector2 origen = pelota != null
            ? (Vector2)pelota.position
            : Vector2.zero;

        if (origen == Vector2.zero && !haciaJugador && jugador != null)
        {
            origen = jugador.position;
        }
        else if (origen == Vector2.zero && haciaJugador && rival != null)
        {
            origen = rival.position;
        }

        puntoInicioVuelo = origen;

        float destinoX = haciaJugador
            ? Random.Range(minXJugador, maxXJugador)
            : Random.Range(minXRival, maxXRival);

        puntoDestinoAterrizaje = new Vector2(destinoX, alturaSuelo);

        // La pelota se vuelve más rápida con el tiempo.
        duracionVueloActual = Mathf.Max(
            duracionVueloMinima,
            duracionVuelo - nivelDificultad * reduccionDuracionVuelo
        );

        // La trayectoria gana variaciones laterales.
        desviacionLateralActual = Mathf.Min(
            desviacionMaxima,
            desviacionInicial + nivelDificultad * aumentoDesviacion
        );

        // Cada lanzamiento tiene una desviación y un arco diferentes.
        faseDesviacion = Random.Range(0f, Mathf.PI * 2f);

        float variacionArco = nivelDificultad * variacionArcoPorNivel;

        alturaArcoActual = Random.Range(
            Mathf.Max(1f, alturaArco - variacionArco),
            alturaArco + variacionArco
        );
    }

    private void ActualizarPelota()
    {
        if (pelota == null)
            return;

        float duracion = Mathf.Max(0.15f, duracionVueloActual);

        progresoVuelo += Time.deltaTime / duracion;

        float t = Mathf.Clamp01(progresoVuelo);

        float x = Mathf.Lerp(
            puntoInicioVuelo.x,
            puntoDestinoAterrizaje.x,
            t
        );

        float baseY = Mathf.Lerp(
            puntoInicioVuelo.y,
            puntoDestinoAterrizaje.y,
            t
        );

        float arcoY = 4f * alturaArcoActual * t * (1f - t);

        // Desviación lateral que aumenta con la dificultad.
        float desviacionX =
            Mathf.Sin(t * Mathf.PI + faseDesviacion)
            * desviacionLateralActual
            * Mathf.Sin(t * Mathf.PI);

        x += desviacionX;

        pelota.position = new Vector3(
            x,
            baseY + arcoY,
            pelota.position.z
        );

        // Comprobar si el jugador alcanza la pelota.
        if (pelotaHaciaJugador && jugador != null)
        {
            float distanciaJugador = Vector2.Distance(
                new Vector2(pelota.position.x, pelota.position.y),
                new Vector2(jugador.position.x, jugador.position.y)
            );

            if (distanciaJugador <= distanciaGolpeJugador)
            {
                AlTocarPelotaJugador();
                return;
            }
        }

        if (t >= 1f)
        {
            if (pelotaHaciaJugador)
            {
                if (
                    jugador != null &&
                    Mathf.Abs(pelota.position.x - jugador.position.x)
                    <= distanciaGolpeJugador
                )
                {
                    AlTocarPelotaJugador();
                }
                else
                {
                    AlFallarJugador();
                }
            }
            else
            {
                // Termina el turno del rival.
                turnoMovimientoRival = false;

                bool rivalLlega =
                    rival != null &&
                    Mathf.Abs(pelota.position.x - rival.position.x)
                    <= distanciaGolpeRival;

                if (
                    rivalLlega &&
                    MusicSoundController.Instance != null
                )
                {
                    MusicSoundController.Instance.ReproducirSFX("Interactuar");
                }

                LanzarPelotaHacia(true);
            }
        }
    }

    private void AlTocarPelotaJugador()
    {
        // Cada golpe conseguido suma un punto.
        puntuacionActual++;
        intercambiosPelota++;

        ActualizarUI();

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Acierto");
        }

        LanzarPelotaHacia(false);
    }

    private void AlFallarJugador()
    {
        turnoMovimientoRival = false;

        vidasActuales = Mathf.Max(0, vidasActuales - 1);

        ActualizarUI();

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Fallo");
        }

        if (vidasActuales <= 0)
        {
            FinalizarPartida();
            return;
        }

        LanzarPelotaHacia(true);
    }

    private void ActualizarUI()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"Puntuación: {puntuacionActual}";
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"Vidas: {vidasActuales}/{vidasMaximas}";
        }

        if (sliderVidas != null)
        {
            sliderVidas.minValue = 0f;
            sliderVidas.maxValue = vidasMaximas;
            sliderVidas.value = vidasActuales;
        }

        if (textoEstadoDificultad != null)
        {
            textoEstadoDificultad.text =
                $"Dificultad: Nivel {nivelDificultad + 1}";
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        turnoMovimientoRival = false;

        StopAllCoroutines();

        int puntuacionFinal = puntuacionActual;

        int recordAnterior = 0;

        if (EstadoJuegoManager.Instance != null)
        {
            recordAnterior =
                EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(
                    idMinijuego
                );

            if (puntuacionFinal > recordAnterior)
            {
                EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(
                    idMinijuego,
                    puntuacionFinal
                );

                if (textoNuevoRecord != null)
                {
                    textoNuevoRecord.text = "¡NUEVO RÉCORD!";
                    textoNuevoRecord.gameObject.SetActive(true);
                }
            }
            else
            {
                if (textoNuevoRecord != null)
                {
                    textoNuevoRecord.text = "";
                    textoNuevoRecord.gameObject.SetActive(false);
                }
            }
        }

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text =
                $"Puntuación final: {puntuacionFinal}";
        }

        if (panelFin != null)
        {
            panelFin.SetActive(true);
        }

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Perder");
        }
    }

    public void VolverMenuInfinito()
    {
        if (TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena("MenuInfinito");
        }
    }
}
