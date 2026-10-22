using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BatallaPokemon : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial; // El objeto contenedor del texto de cuenta atrás
    [SerializeField] private TMP_Text textoInicio;           // El componente de texto para "LISTOS...?" / "¡YA!"
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Configuración de Vida del Enemigo (Clicks)")]
    [SerializeField] private float vidaMaximaEnemigo = 50f;
    private float vidaActualEnemigo;

    [Header("Configuración de Vidas del Jugador")]
    [SerializeField] private int vidasMaximasJugador = 3;
    private int vidasActualesJugador;

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

    [Header("UI de Vida del Enemigo")]
    [SerializeField] private Slider sliderVidaEnemigo;
    [SerializeField] private TextMeshProUGUI textoVidaEnemigo;

    [Header("UI de Vidas del Jugador")]
    [SerializeField] private Slider sliderVidasJugador;
    [SerializeField] private TextMeshProUGUI textoVidasJugador;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoEsquivaExitosa;
    [SerializeField] private AudioClip sonidoEsquivaFallida;

    private bool juegoActivo = false; // Controla si la partida está en marcha (reemplaza o actúa junto a enBatalla)
    private bool esperandoEsquiva = false;
    private float temporizadorAtaque = 0f;
    private float temporizadorVentanaEsquiva = 0f;

    private Vector3 posicionOriginalEnemigo;
    private Vector3 posicionOriginalJugador;

    void Start()
    {
        // Estado inicial de la UI al abrir la escena
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        vidaActualEnemigo = vidaMaximaEnemigo;
        vidasActualesJugador = vidasMaximasJugador;
        juegoActivo = false;

        if (spriteEnemigo != null) posicionOriginalEnemigo = spriteEnemigo.localPosition;
        if (spriteJugador != null) posicionOriginalJugador = spriteJugador.localPosition;

        ActualizarUI();
    }

    // Este método se vincula al botón "Comenzar" o "Jugar" del panelInicio en el Inspector
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
        juegoActivo = true;
        temporizadorAtaque = intervaloAtaqueEnemigo;
    }

    void Update()
    {
        if (!juegoActivo) return;

        // 1. GESTIÓN DE CLICS (Atacar)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AtacarEnemigo();
        }

        // 2. GESTIÓN DE TIEMPO DE ATAQUE DEL ENEMIGO
        temporizadorAtaque -= Time.deltaTime;
        if (temporizadorAtaque <= 0f && !esperandoEsquiva)
        {
            IniciarAvisoAtaque();
        }

        // 3. VENTANA DE REACCIÓN / ESQUIVA
        if (esperandoEsquiva)
        {
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
    }

    private void AtacarEnemigo()
    {
        vidaActualEnemigo -= 1f;
        ActualizarUI();

        if (vidaActualEnemigo <= 0f)
        {
            vidaActualEnemigo = 0f;
            GanarBatalla();
        }
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

        ReproducirSonido(sonidoEsquivaExitosa);

        if (spriteJugador != null) StartCoroutine(RutinaEsquivaJugador());
        if (spriteEnemigo != null) StartCoroutine(RutinaPlacajeEnemigo());
    }

    private void FalloEsquiva()
    {
        esperandoEsquiva = false;
        temporizadorAtaque = intervaloAtaqueEnemigo;
        vidasActualesJugador--;

        ActualizarUI();

        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);
        ReproducirSonido(sonidoEsquivaFallida);

        if (spriteEnemigo != null) StartCoroutine(RutinaPlacajeEnemigo());

        if (vidasActualesJugador <= 0)
        {
            PerderBatalla();
        }
    }

    private IEnumerator RutinaPlacajeEnemigo()
    {
        Vector3 posInicio = posicionOriginalEnemigo;
        Vector3 posAtaque = posicionOriginalEnemigo + new Vector3(-distanciaEmbestida, 0, 0);

        float duracionIda = 0.2f;
        float tiempo = 0f;

        while (tiempo < duracionIda)
        {
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
            tiempo += Time.deltaTime;
            spriteEnemigo.localPosition = Vector3.Lerp(posAtaque, posicionOriginalEnemigo, tiempo / duracionVuelta);
            yield return null;
        }
        spriteEnemigo.localPosition = posicionOriginalEnemigo;
    }

    private IEnumerator RutinaEsquivaJugador()
    {
        Vector3 posCentro = posicionOriginalJugador;
        Vector3 posLateral = posicionOriginalJugador + new Vector3(0, -distanciaEsquivaLateral, 0);

        float t = 0f;
        while (t < 0.2f)
        {
            t += Time.deltaTime;
            spriteJugador.localPosition = Vector3.Lerp(posCentro, posLateral, t / 0.2f);
            yield return null;
        }

        t = 0f;
        while (t < 0.12f)
        {
            t += Time.deltaTime;
            spriteJugador.localPosition = Vector3.Lerp(posLateral, posCentro, t / 0.12f);
            yield return null;
        }
        spriteJugador.localPosition = posCentro;
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void ActualizarUI()
    {
        if (sliderVidaEnemigo != null)
        {
            sliderVidaEnemigo.maxValue = vidaMaximaEnemigo;
            sliderVidaEnemigo.value = vidaActualEnemigo;
        }
        if (textoVidaEnemigo != null)
        {
            textoVidaEnemigo.text = $"{vidaActualEnemigo} / {vidaMaximaEnemigo}";
        }

        if (sliderVidasJugador != null)
        {
            sliderVidasJugador.maxValue = vidasMaximasJugador;
            sliderVidasJugador.value = vidasActualesJugador;
        }
        if (textoVidasJugador != null)
        {
            textoVidasJugador.text = $"Vidas: {vidasActualesJugador} / {vidasMaximasJugador}";
        }
    }

    private void GanarBatalla()
    {
        juegoActivo = false;
        StopAllCoroutines();
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    private void PerderBatalla()
    {
        juegoActivo = false;
        StopAllCoroutines();
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(true);
    }

    public void volverGym(bool victoriaConseguida)
    {
        if (victoriaConseguida)
        {
            if (EstadoJuegoManager.Instance != null)
            {
                EstadoJuegoManager.Instance.ActivarFlag("Batalla_ganada");
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