using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MinijuegoVolley : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Puntuación y Vidas")]
    [SerializeField] private int puntuacionObjetivo = 15;
    [SerializeField] private int vidasMaximas = 3;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private Slider sliderPuntuacion;
    [SerializeField] private Slider sliderVidas;

    [Header("Jugador (Campo Propio)")]
    [SerializeField] private Transform jugador;
    [SerializeField] private float velocidadJugador = 8f;
    [SerializeField] private float minXJugador = -8f;
    [SerializeField] private float maxXJugador = -0.8f;
    [SerializeField] private float distanciaGolpeJugador = 1.3f;

    [Header("Rival (Campo Rival - Turno al Golpear el Jugador)")]
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

    private int puntuacionActual = 0;
    private int vidasActuales = 3;
    private bool juegoActivo = false;

    private bool pelotaHaciaJugador = true;
    private bool turnoMovimientoRival = false;
    private Vector2 puntoInicioVuelo;
    private Vector2 puntoDestinoAterrizaje;
    private float progresoVuelo = 0f;

    private void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        puntuacionActual = 0;
        vidasActuales = vidasMaximas;
        juegoActivo = false;
        turnoMovimientoRival = false;

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
        puntuacionActual = 0;
        vidasActuales = vidasMaximas;
        juegoActivo = true;
        turnoMovimientoRival = false;

        ActualizarUI();
        LanzarPelotaHacia(true);
    }

    private void Update()
    {
        if (!juegoActivo) return;

        MoverJugador();

        // El rival solo se mueve cuando el jugador ya tocó la pelota y está yendo hacia su campo
        if (turnoMovimientoRival && !pelotaHaciaJugador)
        {
            MoverRivalHaciaObjetivo();
        }

        ActualizarPelota();
    }

    private void MoverJugador()
    {
        if (jugador == null || Keyboard.current == null) return;

        float inputX = 0f;
        if (Keyboard.current.aKey.isPressed) inputX -= 1f;
        if (Keyboard.current.dKey.isPressed) inputX += 1f;

        Vector3 pos = jugador.position;
        pos.x = Mathf.Clamp(pos.x + inputX * velocidadJugador * Time.deltaTime, minXJugador, maxXJugador);
        jugador.position = pos;
    }

    private void MoverRivalHaciaObjetivo()
    {
        if (rival == null) return;

        float objetivoX = Mathf.Clamp(puntoDestinoAterrizaje.x, minXRival, maxXRival);
        Vector3 pos = rival.position;
        pos.x = Mathf.MoveTowards(pos.x, objetivoX, velocidadRival * Time.deltaTime);
        rival.position = pos;
    }

    private void LanzarPelotaHacia(bool haciaJugador)
    {
        pelotaHaciaJugador = haciaJugador;
        turnoMovimientoRival = !haciaJugador;
        progresoVuelo = 0f;

        Vector2 origen = pelota != null ? (Vector2)pelota.position : Vector2.zero;
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
    }

    private void ActualizarPelota()
    {
        if (pelota == null) return;

        float duracion = Mathf.Max(0.15f, duracionVuelo);
        progresoVuelo += Time.deltaTime / duracion;
        float t = Mathf.Clamp01(progresoVuelo);

        float x = Mathf.Lerp(puntoInicioVuelo.x, puntoDestinoAterrizaje.x, t);
        float baseY = Mathf.Lerp(puntoInicioVuelo.y, puntoDestinoAterrizaje.y, t);
        float arcoY = 4f * alturaArco * t * (1f - t);

        pelota.position = new Vector3(x, baseY + arcoY, pelota.position.z);

        // Solo cuando va hacia el jugador puede golpearla
        if (pelotaHaciaJugador && jugador != null)
        {
            float distJugador = Vector2.Distance(
                new Vector2(pelota.position.x, pelota.position.y),
                new Vector2(jugador.position.x, jugador.position.y)
            );

            if (distJugador <= distanciaGolpeJugador)
            {
                AlTocarPelotaJugador();
                return;
            }
        }

        if (t >= 1f)
        {
            if (pelotaHaciaJugador)
            {
                if (jugador != null && Mathf.Abs(pelota.position.x - jugador.position.x) <= distanciaGolpeJugador)
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
                // Termina el turno del rival: devuelve la pelota hacia el jugador y deja de moverse hasta el próximo golpe
                turnoMovimientoRival = false;
                bool rivalLlega = rival != null && Mathf.Abs(pelota.position.x - rival.position.x) <= distanciaGolpeRival;
                if (rivalLlega && MusicSoundController.Instance != null)
                {
                    MusicSoundController.Instance.ReproducirSFX("Interactuar");
                }
                LanzarPelotaHacia(true);
            }
        }
    }

    private void AlTocarPelotaJugador()
    {
        puntuacionActual++;
        ActualizarUI();

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Acierto");
        }

        if (puntuacionActual >= puntuacionObjetivo)
        {
            GanarMinijuego();
            return;
        }

        // Activa el movimiento del rival y manda la pelota a su campo
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
            PerderMinijuego();
            return;
        }

        LanzarPelotaHacia(true);
    }

    private void ActualizarUI()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"{puntuacionActual} / {puntuacionObjetivo}";
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"{vidasActuales} / {vidasMaximas}";
        }

        if (sliderPuntuacion != null)
        {
            sliderPuntuacion.minValue = 0f;
            sliderPuntuacion.maxValue = puntuacionObjetivo;
            sliderPuntuacion.value = puntuacionActual;
        }

        if (sliderVidas != null)
        {
            sliderVidas.minValue = 0f;
            sliderVidas.maxValue = vidasMaximas;
            sliderVidas.value = vidasActuales;
        }
    }

    private void GanarMinijuego()
    {
        juegoActivo = false;
        turnoMovimientoRival = false;
        StopAllCoroutines();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Ganar");
    }

    private void PerderMinijuego()
    {
        juegoActivo = false;
        turnoMovimientoRival = false;
        StopAllCoroutines();

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