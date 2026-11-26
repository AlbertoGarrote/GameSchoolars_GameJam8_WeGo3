using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class cigarrosInfinito : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "Cigarros";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Configuración del Jugador y Vidas")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Rigidbody2D rbJugador;
    [SerializeField] private SpriteRenderer spriteJugadorRenderer;
    [SerializeField] private float velocidadJugador = 5f;
    [SerializeField] private int vidasMaximas = 3;
    [SerializeField] private float duracionParpadeo = 0.5f;
    [SerializeField] private int vecesQueParpadea = 5;

    private int vidasActuales;
    private float tiempoSobrevivido = 0f;
    private bool juegoActivo = false;

    [Header("Prefabs de Ataques")]
    [SerializeField] private GameObject prefabPelota;
    [SerializeField] private GameObject prefabRectangulo;
    [SerializeField] private GameObject prefabBalaRapida;

    [Header("Velocidades de Proyectiles")]
    [SerializeField] private float velocidadPelota = 5f;
    [SerializeField] private float velocidadRectangulo = 4.5f;
    [SerializeField] private float velocidadBalaRapida = 10f;

    [Header("Frecuencia de Spawn")]
    [SerializeField] private float intervaloSpawn = 0.7f;

    [Header("Puntos de Spawn")]
    [SerializeField] private Transform[] puntosSpawnPelota;
    [SerializeField] private Transform[] puntosSpawnRectangulo;
    [SerializeField] private Transform[] puntosSpawnBalaRapida;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderVida;
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private TextMeshProUGUI textoTiempo;

    private readonly List<GameObject> proyectilesActivos = new List<GameObject>();
    private Coroutine rutinaAtaques;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        vidasActuales = vidasMaximas;
        tiempoSobrevivido = 0f;
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
        LimpiarProyectiles();
        if (rutinaAtaques != null) StopCoroutine(rutinaAtaques);

        vidasActuales = vidasMaximas;
        tiempoSobrevivido = 0f;
        juegoActivo = true;

        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        ActualizarUI();
        rutinaAtaques = StartCoroutine(RutinaAtaquesCombinados());
    }

    void Update()
    {
        if (!juegoActivo) return;

        MoverJugador();

        tiempoSobrevivido += Time.deltaTime;
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

    private IEnumerator RutinaAtaquesCombinados()
    {
        while (juegoActivo)
        {
            int tipoAtaque = Random.Range(0, 3);

            if (tipoAtaque == 0 && prefabPelota != null && puntosSpawnPelota.Length > 0 && jugador != null)
            {
                Transform punto = puntosSpawnPelota[Random.Range(0, puntosSpawnPelota.Length)];
                GameObject bola = Instantiate(prefabPelota, punto.position, Quaternion.identity);
                proyectilesActivos.Add(bola);

                Rigidbody2D rb = bola.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = ((Vector2)jugador.position - (Vector2)bola.transform.position).normalized;
                    rb.linearVelocity = dir * velocidadPelota;
                }
            }
            else if (tipoAtaque == 1 && prefabRectangulo != null && puntosSpawnRectangulo.Length > 0)
            {
                Transform punto = puntosSpawnRectangulo[Random.Range(0, puntosSpawnRectangulo.Length)];
                GameObject rect = Instantiate(prefabRectangulo, punto.position, Quaternion.identity);
                proyectilesActivos.Add(rect);

                Rigidbody2D rb = rect.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.down * velocidadRectangulo;
                }
            }
            else if (tipoAtaque == 2 && prefabBalaRapida != null && puntosSpawnBalaRapida.Length > 0 && jugador != null)
            {
                Transform punto = puntosSpawnBalaRapida[Random.Range(0, puntosSpawnBalaRapida.Length)];
                GameObject bala = Instantiate(prefabBalaRapida, punto.position, Quaternion.identity);
                proyectilesActivos.Add(bala);

                Rigidbody2D rb = bala.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = ((Vector2)jugador.position - (Vector2)bala.transform.position).normalized;
                    rb.linearVelocity = dir * velocidadBalaRapida;
                }
            }

            yield return new WaitForSeconds(intervaloSpawn);
        }
    }

    public void RecibirDanio()
    {
        if (!juegoActivo) return;

        vidasActuales = Mathf.Max(0, vidasActuales - 1);
        if (MusicSoundController.Instance != null)
            MusicSoundController.Instance.ReproducirSFX("Fallo");

        ActualizarUI();

        if (spriteJugadorRenderer != null)
        {
            StartCoroutine(RutinaParpadeoJugador());
        }

        if (vidasActuales <= 0)
        {
            FinalizarPartida();
        }
    }

    private IEnumerator RutinaParpadeoJugador()
    {
        float tiempoPorParpadeo = duracionParpadeo / (vecesQueParpadea * 2);

        for (int i = 0; i < vecesQueParpadea; i++)
        {
            if (spriteJugadorRenderer == null) yield break;
            Color colorTransparente = spriteJugadorRenderer.color;
            colorTransparente.a = 0.2f;
            spriteJugadorRenderer.color = colorTransparente;

            yield return new WaitForSeconds(tiempoPorParpadeo);

            if (spriteJugadorRenderer == null) yield break;
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
        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.FloorToInt(tiempoSobrevivido)}s";
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        if (rbJugador != null) rbJugador.linearVelocity = Vector2.zero;
        StopAllCoroutines();
        LimpiarProyectiles();

        int puntuacionFinal = Mathf.FloorToInt(tiempoSobrevivido);
        bool esNuevoRecord = false;

        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);
            esNuevoRecord = puntuacionFinal > puntuacionAnterior;
            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(idMinijuego, puntuacionFinal);
        }

        if (panelFin != null) panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text = $"Puntuacion final: {puntuacionFinal}";
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