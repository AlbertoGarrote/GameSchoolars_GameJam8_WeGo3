using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinijuegoSupervivencia : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria = "Torneo_ganado";
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Cementerio";

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;
    [SerializeField] private TMP_Text textoAnuncioFase;

    [Header("Configuración del Jugador y Área")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Rigidbody2D rbJugador;
    [SerializeField] private SpriteRenderer spriteJugadorRenderer; // Para el parpadeo al recibir daño
    [SerializeField] private float velocidadJugador = 5f;
    [SerializeField] private int vidasMaximas = 5;
    [SerializeField] private float duracionParpadeo = 0.5f;
    [SerializeField] private int vecesQueParpadea = 5;
    private int vidasActuales;

    [Header("Configuración de Fases (4 Fases)")]
    [SerializeField] private int totalFases = 4;
    [SerializeField] private float duracionCadaFase = 15f;
    private int faseActual = 0;
    private float tiempoFaseRestante = 0f;
    private bool juegoActivo = false;
    private bool enPausaCambioFase = false;

    [Header("Prefabs de Ataques")]
    [SerializeField] private GameObject prefabPelotaFase1;
    [SerializeField] private GameObject prefabRectanguloFase2;
    [SerializeField] private GameObject prefabObstaculoFase3;

    [Header("Velocidades de Proyectiles (Editables)")]
    [SerializeField] private float velocidadPelotaFase1 = 5f;
    [SerializeField] private float velocidadRectanguloFase2 = 3.5f;
    [SerializeField] private float velocidadBalaFase3 = 9f;
    [SerializeField] private float velocidadRectanguloFase4 = 4.5f;
    [SerializeField] private float velocidadBalaFase4 = 10f;

    [Header("Tiempos de Spawn / Frecuencia (Editables)")]
    [SerializeField] private float intervaloSpawnFase1 = 0.8f;
    [SerializeField] private float intervaloSpawnFase2 = 2.0f;
    [SerializeField] private float intervaloSpawnFase3 = 0.6f;
    [SerializeField] private float intervaloSpawnFase4 = 0.7f;

    [Header("Puntos de Spawn o Referencias")]
    [SerializeField] private Transform[] puntosSpawnFase1;
    [SerializeField] private Transform[] puntosSpawnFase2;
    [SerializeField] private Transform[] puntosSpawnFase3;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderVida;
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private Slider sliderFases;
    [SerializeField] private TextMeshProUGUI textoFases;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoDanio;
    [SerializeField] private AudioClip sonidoCambioFase;

    private List<GameObject> proyectilesActivos = new List<GameObject>();

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);
        if (textoAnuncioFase != null) textoAnuncioFase.gameObject.SetActive(false);

        vidasActuales = vidasMaximas;
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
        juegoActivo = true;
        vidasActuales = vidasMaximas;
        faseActual = 1;
        tiempoFaseRestante = duracionCadaFase;

        ActualizarUI();
        StartCoroutine(RutinaGestionFases());
    }

    void Update()
    {
        if (!juegoActivo || enPausaCambioFase) return;

        MoverJugador();

        tiempoFaseRestante -= Time.deltaTime;
        if (tiempoFaseRestante <= 0f)
        {
            StartCoroutine(RutinaCambioDeFasePausado());
        }

        ActualizarUI();
    }

    private void MoverJugador()
    {
        if (Keyboard.current == null) return;

        Vector2 direccion = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) direccion.y += 1f;
        if (Keyboard.current.sKey.isPressed) direccion.y -= 1f;
        if (Keyboard.current.aKey.isPressed) direccion.x -= 1f;
        if (Keyboard.current.dKey.isPressed) direccion.x += 1f;

        if (rbJugador != null)
        {
            rbJugador.linearVelocity = direccion.normalized * velocidadJugador;
        }
    }

    private IEnumerator RutinaGestionFases()
    {
        while (juegoActivo && faseActual <= totalFases)
        {
            tiempoFaseRestante = duracionCadaFase;

            Coroutine patronActual = null;
            if (faseActual == 1) patronActual = StartCoroutine(PatronFase1Pelotas());
            else if (faseActual == 2) patronActual = StartCoroutine(PatronFase2Rectangulos());
            else if (faseActual == 3) patronActual = StartCoroutine(PatronFase3DisparosRapidos());
            else if (faseActual == 4) patronActual = StartCoroutine(PatronFase4Combinado());

            while (tiempoFaseRestante > 0f && juegoActivo && !enPausaCambioFase)
            {
                tiempoFaseRestante -= Time.deltaTime;
                yield return null;
            }

            if (patronActual != null) StopCoroutine(patronActual);
            LimpiarProyectiles();

            while (enPausaCambioFase && juegoActivo)
            {
                yield return null;
            }
        }
    }

    private IEnumerator RutinaCambioDeFasePausado()
    {
        enPausaCambioFase = true;
        if (rbJugador != null) rbJugador.linearVelocity = Vector2.zero;

        faseActual++;

        if (faseActual > totalFases)
        {
            GanarPartida();
            yield break;
        }

        ReproducirSonido(sonidoCambioFase);

        if (textoAnuncioFase != null)
        {
            textoAnuncioFase.text = $"¡FASE {faseActual}!";
            textoAnuncioFase.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(3f);

        if (textoAnuncioFase != null)
        {
            textoAnuncioFase.gameObject.SetActive(false);
        }

        tiempoFaseRestante = duracionCadaFase;
        enPausaCambioFase = false;
    }

    // --- PATRONES DE ATAQUE ---

    private IEnumerator PatronFase1Pelotas()
    {
        while (juegoActivo && faseActual == 1 && !enPausaCambioFase)
        {
            if (prefabPelotaFase1 != null && puntosSpawnFase1.Length > 0 && jugador != null)
            {
                int index = Random.Range(0, puntosSpawnFase1.Length);
                GameObject bola = Instantiate(prefabPelotaFase1, puntosSpawnFase1[index].position, Quaternion.identity);
                proyectilesActivos.Add(bola);

                Vector2 posicionObjetivo = jugador.position;
                Rigidbody2D rb = bola.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (posicionObjetivo - (Vector2)bola.transform.position).normalized;
                    rb.linearVelocity = dir * velocidadPelotaFase1;
                }
            }
            yield return new WaitForSeconds(intervaloSpawnFase1);
        }
    }

    private IEnumerator PatronFase2Rectangulos()
    {
        while (juegoActivo && faseActual == 2 && !enPausaCambioFase)
        {
            if (prefabRectanguloFase2 != null && puntosSpawnFase2.Length > 0)
            {
                int index = Random.Range(0, puntosSpawnFase2.Length);
                GameObject rect = Instantiate(prefabRectanguloFase2, puntosSpawnFase2[index].position, Quaternion.identity);
                proyectilesActivos.Add(rect);

                Rigidbody2D rb = rect.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.down * velocidadRectanguloFase2;
                }
            }
            yield return new WaitForSeconds(intervaloSpawnFase2);
        }
    }

    private IEnumerator PatronFase3DisparosRapidos()
    {
        while (juegoActivo && faseActual == 3 && !enPausaCambioFase)
        {
            if (prefabObstaculoFase3 != null && puntosSpawnFase3.Length > 0 && jugador != null)
            {
                int index = Random.Range(0, puntosSpawnFase3.Length);
                GameObject balaRapida = Instantiate(prefabObstaculoFase3, puntosSpawnFase3[index].position, Quaternion.identity);
                proyectilesActivos.Add(balaRapida);

                Vector2 posicionObjetivo = jugador.position;
                Rigidbody2D rb = balaRapida.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = (posicionObjetivo - (Vector2)balaRapida.transform.position).normalized;
                    rb.linearVelocity = dir * velocidadBalaFase3;
                }
            }
            yield return new WaitForSeconds(intervaloSpawnFase3);
        }
    }

    private IEnumerator PatronFase4Combinado()
    {
        while (juegoActivo && faseActual == 4 && !enPausaCambioFase)
        {
            int tipoAtaque = Random.Range(0, 3);
            if (tipoAtaque == 0 && puntosSpawnFase1.Length > 0 && jugador != null)
            {
                GameObject bola = Instantiate(prefabPelotaFase1, puntosSpawnFase1[Random.Range(0, puntosSpawnFase1.Length)].position, Quaternion.identity);
                proyectilesActivos.Add(bola);
                Vector2 posObj = jugador.position;
                Rigidbody2D rb = bola.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = (posObj - (Vector2)bola.transform.position).normalized * velocidadPelotaFase1;
            }
            else if (tipoAtaque == 1 && puntosSpawnFase2.Length > 0)
            {
                int index = Random.Range(0, puntosSpawnFase2.Length);
                GameObject rect = Instantiate(prefabRectanguloFase2, puntosSpawnFase2[index].position, Quaternion.identity);
                proyectilesActivos.Add(rect);
                Rigidbody2D rb = rect.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.down * velocidadRectanguloFase4;
            }
            else if (tipoAtaque == 2 && puntosSpawnFase3.Length > 0 && jugador != null)
            {
                GameObject balaRapida = Instantiate(prefabObstaculoFase3, puntosSpawnFase3[Random.Range(0, puntosSpawnFase3.Length)].position, Quaternion.identity);
                proyectilesActivos.Add(balaRapida);
                Vector2 posObj = jugador.position;
                Rigidbody2D rb = balaRapida.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = (posObj - (Vector2)balaRapida.transform.position).normalized * velocidadBalaFase4;
            }

            yield return new WaitForSeconds(intervaloSpawnFase4);
        }
    }

    public void RecibirDanio()
    {
        if (!juegoActivo || enPausaCambioFase) return;

        vidasActuales--;
        ReproducirSonido(sonidoDanio);
        ActualizarUI();

        if (spriteJugadorRenderer != null)
        {
            StartCoroutine(RutinaParpadeoJugador());
        }

        if (vidasActuales <= 0)
        {
            PerderPartida();
        }
    }

    private IEnumerator RutinaParpadeoJugador()
    {
        float tiempoPorParpadeo = duracionParpadeo / (vecesQueParpadea * 2);

        for (int i = 0; i < vecesQueParpadea; i++)
        {
            Color colorTransparente = spriteJugadorRenderer.color;
            colorTransparente.a = 0.2f;
            spriteJugadorRenderer.color = colorTransparente;

            yield return new WaitForSeconds(tiempoPorParpadeo);

            Color colorNormal = spriteJugadorRenderer.color;
            colorNormal.a = 1f;
            spriteJugadorRenderer.color = colorNormal;

            yield return new WaitForSeconds(tiempoPorParpadeo);
        }

        if (spriteJugadorRenderer != null)
        {
            Color colorFinal = spriteJugadorRenderer.color;
            colorFinal.a = 1f;
            spriteJugadorRenderer.color = colorFinal;
        }
    }

    private void LimpiarProyectiles()
    {
        foreach (GameObject proj in proyectilesActivos)
        {
            if (proj != null) Destroy(proj);
        }
        proyectilesActivos.Clear();
    }

    private void ActualizarUI()
    {
        if (sliderVida != null)
        {
            sliderVida.maxValue = vidasMaximas;
            sliderVida.value = vidasActuales;
        }
        if (textoVida != null)
        {
            textoVida.text = $"Vida: {vidasActuales} / {vidasMaximas}";
        }

        if (sliderFases != null)
        {
            sliderFases.maxValue = totalFases;
            sliderFases.value = Mathf.Clamp(faseActual, 1, totalFases);
        }
        if (textoFases != null)
        {
            textoFases.text = $"Fase: {Mathf.Clamp(faseActual, 1, totalFases)} / {totalFases}";
        }
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void GanarPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarProyectiles();
        if (panelVictoria != null) panelVictoria.SetActive(true);
    }

    private void PerderPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarProyectiles();
        if (panelDerrota != null) panelDerrota.SetActive(true);
    }

    public void volverGym(bool victoriaConseguida)
    {
        if (victoriaConseguida)
        {
            if (EstadoJuegoManager.Instance != null)
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