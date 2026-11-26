
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TitanesInfinito : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "TitanesInfinito";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;

    [Header("UI de Partida")]
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private TMP_Text textoEstadoCooldown;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;
    [SerializeField] private Slider sliderVidas;

    [Header("Configuración de Vidas")]
    [SerializeField] private int vidasMaximas = 5;

    [Header("Jugador / Objeto Superior")]
    [SerializeField] private Transform objetoTirador;
    [SerializeField] private float velocidadPatrulla = 8f;
    [SerializeField] private float limiteXIzquierda = -6f;
    [SerializeField] private float limiteXDerecha = 6f;
    [SerializeField] private GameObject prefabProyectil;
    [SerializeField] private float velocidadCaidaProyectil = 18f;
    [SerializeField] private float cooldownDisparo = 1f;

    [Header("Objetivos que Bajan")]
    [SerializeField] private Transform[] puntosSpawnObjetivos;
    [SerializeField] private GameObject prefabObjetivo;
    [SerializeField] private float velocidadCaidaObjetivo = 3.5f;
    [SerializeField] private float intervaloSpawnObjetivos = 1.2f;
    [SerializeField] private float velocidadSubidaObjetivo = 3f;
    [SerializeField] private float alturaObjetivoSubida = 2f;
    [SerializeField] private float escalaMaximaBajada = 10f;
    [SerializeField] private float limiteYDerrota = -1f;

    private float tiempoSobrevivido;
    private float temporizadorCooldown;

    private int direccionPatrulla = 1;
    private int vidasActuales;

    private bool juegoActivo;
    private bool partidaFinalizada;
    private bool cuentaAtrasIniciada;

    private SpriteRenderer spriteTirador;
    private Image imagenTirador;
    private Color colorOriginalTirador = Color.white;

    private readonly List<GameObject> entidadesActivas =
        new List<GameObject>();

    private void Awake()
    {
        if (objetoTirador != null)
        {
            spriteTirador =
                objetoTirador.GetComponentInChildren<SpriteRenderer>();

            imagenTirador =
                objetoTirador.GetComponentInChildren<Image>();

            if (spriteTirador != null)
                colorOriginalTirador = spriteTirador.color;
            else if (imagenTirador != null)
                colorOriginalTirador = imagenTirador.color;
        }
    }

    private void Start()
    {
        if (panelInicio != null)
            panelInicio.SetActive(true);

        if (temporizadorInicial != null)
            temporizadorInicial.SetActive(false);

        if (panelFin != null)
            panelFin.SetActive(false);

        tiempoSobrevivido = 0f;
        vidasActuales = vidasMaximas;
        temporizadorCooldown = 0f;

        juegoActivo = false;
        partidaFinalizada = false;
        cuentaAtrasIniciada = false;

        ConfigurarSliderVidas();
        EstablecerColorTirador(colorOriginalTirador);
        ActualizarUI();
    }

    private void ConfigurarSliderVidas()
    {
        if (sliderVidas == null)
            return;

        sliderVidas.minValue = 0;
        sliderVidas.maxValue = vidasMaximas;
        sliderVidas.value = vidasActuales;
    }

    public void ComenzarBoton()
    {
        if (juegoActivo || partidaFinalizada || cuentaAtrasIniciada)
            return;

        StartCoroutine(ComenzarJuegoCorrutina());
    }

    private IEnumerator ComenzarJuegoCorrutina()
    {
        cuentaAtrasIniciada = true;

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
        LimpiarEntidadesActivas();

        juegoActivo = true;
        partidaFinalizada = false;

        tiempoSobrevivido = 0f;
        vidasActuales = vidasMaximas;
        temporizadorCooldown = 0f;
        direccionPatrulla = 1;

        ConfigurarSliderVidas();
        EstablecerColorTirador(colorOriginalTirador);
        ActualizarUI();

        StartCoroutine(RutinaSpawnObjetivos());
    }

    private void Update()
    {
        if (!juegoActivo)
            return;

        // El tiempo aumenta mientras el jugador siga vivo.
        tiempoSobrevivido += Time.deltaTime;

        ActualizarUI();

        // Gestión del cooldown y color del tirador.
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

        MoverTiradorHorizontal();

        // Disparo con espacio.
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            temporizadorCooldown <= 0f)
        {
            LanzarProyectil();
        }
    }

    private void MoverTiradorHorizontal()
    {
        if (objetoTirador == null)
            return;

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
        if (objetoTirador == null || prefabProyectil == null)
            return;

        temporizadorCooldown = cooldownDisparo;
        EstablecerColorTirador(Color.red);

        GameObject proyectilObj = Instantiate(
            prefabProyectil,
            objetoTirador.position,
            Quaternion.identity,
            transform
        );

        RegistrarEntidad(proyectilObj);

        ProyectilInfinito scriptProyectil =
            proyectilObj.GetComponent<ProyectilInfinito>();

        if (scriptProyectil == null)
        {
            scriptProyectil =
                proyectilObj.AddComponent<ProyectilInfinito>();
        }

        scriptProyectil.Inicializar(
            velocidadCaidaProyectil,
            this
        );

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Disparo");
        }
    }

    private IEnumerator RutinaSpawnObjetivos()
    {
        while (juegoActivo)
        {
            if (puntosSpawnObjetivos != null &&
                puntosSpawnObjetivos.Length > 0 &&
                prefabObjetivo != null)
            {
                int indice = Random.Range(
                    0,
                    puntosSpawnObjetivos.Length
                );

                Transform punto = puntosSpawnObjetivos[indice];

                if (punto != null)
                {
                    GameObject obj = Instantiate(
                        prefabObjetivo,
                        punto.position,
                        Quaternion.identity,
                        transform
                    );

                    RegistrarEntidad(obj);

                    ObjetoDescendenteInifnito scriptObj =
                        obj.GetComponent<ObjetoDescendenteInifnito>();

                    if (scriptObj == null)
                    {
                        scriptObj =
                            obj.AddComponent<ObjetoDescendenteInifnito>();
                    }

                    scriptObj.Inicializar(
                        velocidadSubidaObjetivo,
                        alturaObjetivoSubida,
                        velocidadCaidaObjetivo,
                        limiteYDerrota,
                        escalaMaximaBajada,
                        this
                    );
                }
            }

            yield return new WaitForSeconds(intervaloSpawnObjetivos);
        }
    }

    public void RegistrarImpacto(
        GameObject proyectil,
        GameObject objetivo)
    {
        if (!juegoActivo)
            return;

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

    // Se llama cuando un objetivo alcanza el límite inferior.
    public void NotificarObjetivoLlegoAlFondo(GameObject objetivo)
    {
        if (!juegoActivo)
            return;

        if (objetivo != null)
        {
            entidadesActivas.Remove(objetivo);
            Destroy(objetivo);
        }

        vidasActuales = Mathf.Max(0, vidasActuales - 1);
        MusicSoundController.Instance.ReproducirSFX("Fallo");

        ActualizarUI();

        if (vidasActuales <= 0)
        {
            FinalizarPartida();
        }
    }

    private void EstablecerColorTirador(Color color)
    {
        if (spriteTirador != null)
            spriteTirador.color = color;

        if (imagenTirador != null)
            imagenTirador.color = color;
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
            textoTiempo.text =
                $"Tiempo: {Mathf.FloorToInt(tiempoSobrevivido)}s";
        }

        if (textoVidas != null)
        {
            textoVidas.text =
                $"Vidas: {vidasActuales}/{vidasMaximas}";
        }

        if (textoEstadoCooldown != null)
        {
            textoEstadoCooldown.text = temporizadorCooldown > 0f
                ? $"Recargando: {temporizadorCooldown:0.1}s"
                : "Listo [ESPACIO]";
        }

        if (sliderVidas != null)
        {
            sliderVidas.value = vidasActuales;
        }
    }

    private void FinalizarPartida()
    {
        if (partidaFinalizada)
            return;

        juegoActivo = false;
        partidaFinalizada = true;

        StopAllCoroutines();

        LimpiarEntidadesActivas();
        EstablecerColorTirador(colorOriginalTirador);

        // La puntuación corresponde al tiempo sobrevivido.
        int puntuacionFinal =
            Mathf.FloorToInt(tiempoSobrevivido);

        int recordAnterior = 0;

        if (EstadoJuegoManager.Instance != null)
        {
            recordAnterior =
                EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(
                    idMinijuego
                );

            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(
                idMinijuego,
                puntuacionFinal
            );
        }

        bool nuevoRecord = puntuacionFinal > recordAnterior;

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text =
                $"Tiempo aguantado: {puntuacionFinal}s";
        }

        if (textoNuevoRecord != null)
        {
            textoNuevoRecord.gameObject.SetActive(nuevoRecord);

            if (nuevoRecord)
            {
                textoNuevoRecord.text = "¡NUEVO RÉCORD!";
            }
        }

        if (panelFin != null)
            panelFin.SetActive(true);

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Perder");
        }
    }

    public void VolverMenuInfinito()
    {
        if (!string.IsNullOrEmpty(nombreEscenaMenuInfinito) &&
            TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(
                nombreEscenaMenuInfinito
            );
        }
    }
}
