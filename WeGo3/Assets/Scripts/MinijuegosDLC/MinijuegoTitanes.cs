using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinijuegoTitanes : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym;

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Jugador / Objeto Superior (Patrulla y Disparo)")]
    [Tooltip("Objeto que se mueve de izquierda a derecha arriba en pantalla.")]
    [SerializeField] private Transform objetoTirador;
    [SerializeField] private float velocidadPatrulla = 8f;
    [SerializeField] private float limiteXIzquierda = -6f;
    [SerializeField] private float limiteXDerecha = 6f;
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private float velocidadCaidaProyectil = 18f;
    [SerializeField] private float cooldownDisparo = 1f;

    [Header("Objetivos que Bajan y Límite de Derrota")]
    [SerializeField] private Transform[] puntosSpawnObjetivos;
    [SerializeField] private GameObject prefabObjetivo;
    [SerializeField] private float velocidadCaidaObjetivo = 3.5f;
    [SerializeField] private float intervaloSpawnObjetivos = 1.2f;
    [SerializeField] private float velocidadSubidaObjetivo = 3f;
    [SerializeField] private float alturaObjetivoSubida = 2f;
    [SerializeField] private float escalaMaximaBajada = 10f;
    [SerializeField] private float limiteYDerrota = -1f;

    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoTotalSupervivencia = 30f;

    [Header("UI de Progreso")]
    [SerializeField] private TextMeshProUGUI textoTiempo;
    [SerializeField] private TextMeshProUGUI textoEstadoCooldown;

    private float tiempoRestante;
    private float temporizadorCooldown = 0f;
    private int direccionPatrulla = 1; // 1 = derecha, -1 = izquierda
    private bool juegoActivo = false;

    private SpriteRenderer spriteTirador;
    private Image imagenTirador;
    private Color colorOriginalTirador = Color.white;

    private readonly List<GameObject> entidadesActivas = new List<GameObject>();

    void Awake()
    {
        if (objetoTirador != null)
        {
            spriteTirador = objetoTirador.GetComponentInChildren<SpriteRenderer>();
            imagenTirador = objetoTirador.GetComponentInChildren<Image>();

            if (spriteTirador != null) colorOriginalTirador = spriteTirador.color;
            else if (imagenTirador != null) colorOriginalTirador = imagenTirador.color;
        }
    }

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        tiempoRestante = tiempoTotalSupervivencia;
        temporizadorCooldown = 0f;
        juegoActivo = false;
        EstablecerColorTirador(colorOriginalTirador);

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
        LimpiarEntidadesActivas();

        juegoActivo = true;
        tiempoRestante = tiempoTotalSupervivencia;
        temporizadorCooldown = 0f;
        direccionPatrulla = 1;
        EstablecerColorTirador(colorOriginalTirador);

        ActualizarUI();
        StartCoroutine(RutinaSpawnObjetivos());
    }

    void Update()
    {
        if (!juegoActivo) return;

        // 1. Cuenta atrás para sobrevivir los 30 segundos
        tiempoRestante -= Time.deltaTime;
        ActualizarUI();

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            GanarBatalla();
            return;
        }

        // 2. Gestión del cooldown de 1s y color rojo
        if (temporizadorCooldown > 0f)
        {
            temporizadorCooldown -= Time.deltaTime;
            EstablecerColorTirador(Color.red);

            if (temporizadorCooldown <= 0f)
            {
                temporizadorCooldown = 0f;
                EstablecerColorTirador(colorOriginalTirador);
            }
        }

        // 3. Patrulla horizontal constante de derecha a izquierda
        MoverTiradorHorizontal();

        // 4. Disparo al pulsar Espacio
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && temporizadorCooldown <= 0f)
        {
            LanzarProyectil();
        }
    }

    private void MoverTiradorHorizontal()
    {
        if (objetoTirador == null) return;

        Vector3 pos = objetoTirador.position;
        pos.x += direccionPatrulla * velocidadPatrulla * Time.deltaTime;

        if (pos.x >= limiteXDerecha)
        {
            pos.x = limiteXDerecha;
            direccionPatrulla = -1;
        }
        else if (pos.x <= limiteXIzquierda)
        {
            pos.x = limiteXIzquierda;
            direccionPatrulla = 1;
        }

        objetoTirador.position = pos;
    }

    private void LanzarProyectil()
    {
        if (objetoTirador == null || prefabProyectil == null) return;

        temporizadorCooldown = cooldownDisparo;
        EstablecerColorTirador(Color.red);

        GameObject proyectilObj = Instantiate(prefabProyectil, objetoTirador.position, Quaternion.identity, transform);
        RegistrarEntidad(proyectilObj);

        ProyectilCaida scriptProyectil = proyectilObj.GetComponent<ProyectilCaida>();
        if (scriptProyectil == null) scriptProyectil = proyectilObj.AddComponent<ProyectilCaida>();

        scriptProyectil.Inicializar(velocidadCaidaProyectil, this);

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Disparo");
        }
    }

    private IEnumerator RutinaSpawnObjetivos()
    {
        while (juegoActivo)
        {
            if (puntosSpawnObjetivos != null && puntosSpawnObjetivos.Length > 0 && prefabObjetivo != null)
            {
                int indice = UnityEngine.Random.Range(0, puntosSpawnObjetivos.Length);
                Transform punto = puntosSpawnObjetivos[indice];

                if (punto != null)
                {
                    GameObject obj = Instantiate(prefabObjetivo, punto.position, Quaternion.identity, transform);
                    RegistrarEntidad(obj);

                    ObjetivoDescendente scriptObj = obj.GetComponent<ObjetivoDescendente>();
                    if (scriptObj == null) scriptObj = obj.AddComponent<ObjetivoDescendente>();

                    scriptObj.Inicializar(velocidadSubidaObjetivo, alturaObjetivoSubida, velocidadCaidaObjetivo, limiteYDerrota, escalaMaximaBajada, this);
                }
            }

            yield return new WaitForSeconds(intervaloSpawnObjetivos);
        }
    }

    public void RegistrarImpacto(GameObject proyectil, GameObject objetivo)
    {
        if (!juegoActivo) return;

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Acierto");
        }

        if (proyectil != null)
        {
            entidadesActivas.Remove(proyectil);
            Destroy(proyectil);
        }

        if (objetivo != null)
        {
            entidadesActivas.Remove(objetivo);
            Destroy(objetivo);
        }
    }

    public void NotificarObjetivoLlegoAlFondo()
    {
        if (!juegoActivo) return;
        PerderBatalla();
    }

    private void EstablecerColorTirador(Color color)
    {
        if (spriteTirador != null) spriteTirador.color = color;
        if (imagenTirador != null) imagenTirador.color = color;
    }

    private void RegistrarEntidad(GameObject entidad)
    {
        if (entidad != null && !entidadesActivas.Contains(entidad))
        {
            entidadesActivas.Add(entidad);
        }
    }

    private void LimpiarEntidadesActivas()
    {
        for (int i = entidadesActivas.Count - 1; i >= 0; i--)
        {
            if (entidadesActivas[i] != null)
            {
                Destroy(entidadesActivas[i]);
            }
        }
        entidadesActivas.Clear();
    }

    private void ActualizarUI()
    {
        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }
        if (textoEstadoCooldown != null)
        {
            textoEstadoCooldown.text = temporizadorCooldown > 0f
                ? $"Recargando: {temporizadorCooldown:0.1}s"
                : "Listo [ESPACIO]";
        }
    }

    private void GanarBatalla()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarEntidadesActivas();
        EstablecerColorTirador(colorOriginalTirador);

        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Ganar");
    }

    private void PerderBatalla()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarEntidadesActivas();
        EstablecerColorTirador(colorOriginalTirador);

        if (panelDerrota != null) panelDerrota.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Perder");
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