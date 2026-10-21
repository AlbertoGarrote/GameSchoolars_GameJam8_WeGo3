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

    [Header("Paneles UI")]
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
    [SerializeField] private Transform spriteEnemigo;          // Sprite del enemigo
    [SerializeField] private Transform spriteJugador;          // Sprite del jugador
    [SerializeField] private float distanciaEmbestida = 1.5f;   // Distancia del placaje hacia adelante
    [SerializeField] private float distanciaEsquivaLateral = 1f; // Desplazamiento lateral del jugador

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

    private bool enBatalla = true;
    private bool esperandoEsquiva = false;
    private float temporizadorAtaque = 0f;
    private float temporizadorVentanaEsquiva = 0f;

    private Vector3 posicionOriginalEnemigo;
    private Vector3 posicionOriginalJugador;

    void Start()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        vidaActualEnemigo = vidaMaximaEnemigo;
        vidasActualesJugador = vidasMaximasJugador;
        enBatalla = true;
        temporizadorAtaque = intervaloAtaqueEnemigo;

        if (spriteEnemigo != null) posicionOriginalEnemigo = spriteEnemigo.localPosition;
        if (spriteJugador != null) posicionOriginalJugador = spriteJugador.localPosition;

        ActualizarUI();
    }

    void Update()
    {
        if (!enBatalla) return;

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

        // Muestra únicamente el aviso en pantalla. El enemigo NO se mueve aún.
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(true);
    }

    private void EsquivaExitosa()
    {
        esperandoEsquiva = false;
        temporizadorAtaque = intervaloAtaqueEnemigo;

        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);

        ReproducirSonido(sonidoEsquivaExitosa);

        // El jugador se aparta a un lado y el enemigo ejecuta el placaje hacia donde estaba
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

        // El enemigo ejecuta el placaje impactando al jugador
        if (spriteEnemigo != null) StartCoroutine(RutinaPlacajeEnemigo());

        if (vidasActualesJugador <= 0)
        {
            PerderBatalla();
        }
    }

    // Placaje Pokémon seco: ida acelerada en 0.08s y vuelta limpia en 0.1s
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
            spriteJugador.localPosition = Vector3.Lerp(posLateral, posCentro, t / 0.2f);
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
        // 1. Actualizar barra y texto del enemigo
        if (sliderVidaEnemigo != null)
        {
            sliderVidaEnemigo.maxValue = vidaMaximaEnemigo;
            sliderVidaEnemigo.value = vidaActualEnemigo;
        }
        if (textoVidaEnemigo != null)
        {
            textoVidaEnemigo.text = $"{vidaActualEnemigo} / {vidaMaximaEnemigo}";
        }

        // 2. Actualizar barra y texto del jugador
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
        enBatalla = false;
        StopAllCoroutines();
        if (avisoAtaqueEnemigo != null) avisoAtaqueEnemigo.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    private void PerderBatalla()
    {
        enBatalla = false;
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