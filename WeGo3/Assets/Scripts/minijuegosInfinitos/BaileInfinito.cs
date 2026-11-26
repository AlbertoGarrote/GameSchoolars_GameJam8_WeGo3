using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class BaileInfinito : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "BaileInfinito";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Configuración del Modo Infinito")]
    [SerializeField] private int vidasMaximas = 5;
    [SerializeField] private float velocidadInicial = 4f;
    [SerializeField] private float incrementoVelocidadPorAcierto = 0.25f;
    [SerializeField] private float velocidadMaxima = 14f;
    [SerializeField] private float frecuenciaSpawn = 0.8f;

    private int vidasActuales;
    private int notasAcertadas;
    private float velocidadActual;
    private bool juegoActivo = false;

    [Header("Prefabs de Notas (Asignados a su columna fija)")]
    [SerializeField] private GameObject prefabNotaIzquierda; // Columna 0 (Izquierda)
    [SerializeField] private GameObject prefabNotaAbajo;     // Columna 1 (Abajo)
    [SerializeField] private GameObject prefabNotaArriba;    // Columna 2 (Arriba)
    [SerializeField] private GameObject prefabNotaDerecha;   // Columna 3 (Derecha)

    [Header("Puntos de Spawn (Abajo) y Zonas de Pulsación (Arriba)")]
    [SerializeField] private Transform[] puntosSpawnAbajo = new Transform[4];     // 0: Izq, 1: Abajo, 2: Arriba, 3: Der
    [SerializeField] private Transform[] zonasResaltadasArriba = new Transform[4]; // 0: Izq, 1: Abajo, 2: Arriba, 3: Der

    [Header("Tolerancia de Acierto (Distancia en Y)")]
    [SerializeField] private float toleranciaAcierto = 0.5f;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderVida;
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private TextMeshProUGUI textoPuntuacion;

    private readonly List<NotaBaileInfinito>[] notasActivasPorCarril = new List<NotaBaileInfinito>[4];
    private Coroutine rutinaSpawn;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            notasActivasPorCarril[i] = new List<NotaBaileInfinito>();
        }

        vidasActuales = vidasMaximas;
        notasAcertadas = 0;
        velocidadActual = velocidadInicial;
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
        LimpiarNotasActivas();
        if (rutinaSpawn != null) StopCoroutine(rutinaSpawn);

        vidasActuales = vidasMaximas;
        notasAcertadas = 0;
        velocidadActual = velocidadInicial;
        juegoActivo = true;

        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

        ActualizarUI();
        rutinaSpawn = StartCoroutine(RutinaSpawnNotas());
    }

    void Update()
    {
        if (!juegoActivo) return;

        ComprobarEntradaJugador();
        ActualizarUI();
    }

    private void ComprobarEntradaJugador()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(0);
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(1);
        }
        else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(2);
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(3);
        }
    }

    private void IntentarPulsarNota(int carril)
    {
        if (carril < 0 || carril >= 4 || notasActivasPorCarril[carril].Count == 0) return;

        NotaBaileInfinito notaAChequear = notasActivasPorCarril[carril][0];

        if (notaAChequear != null && zonasResaltadasArriba[carril] != null)
        {
            float distanciaY = Mathf.Abs(notaAChequear.transform.position.y - zonasResaltadasArriba[carril].position.y);

            if (distanciaY <= toleranciaAcierto)
            {
                notasAcertadas++;
                velocidadActual = Mathf.Min(velocidadMaxima, velocidadActual + incrementoVelocidadPorAcierto);
                ActualizarVelocidadNotasActivas();

                if (MusicSoundController.Instance != null)
                    MusicSoundController.Instance.ReproducirSFX("Acierto");

                notasActivasPorCarril[carril].RemoveAt(0);
                Destroy(notaAChequear.gameObject);
            }
            else
            {
                notasActivasPorCarril[carril].RemoveAt(0);
                Destroy(notaAChequear.gameObject);
                RestarVida();
            }
        }
    }

    private void ActualizarVelocidadNotasActivas()
    {
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < notasActivasPorCarril[i].Count; j++)
            {
                if (notasActivasPorCarril[i][j] != null)
                {
                    notasActivasPorCarril[i][j].EstablecerVelocidad(velocidadActual);
                }
            }
        }
    }

    public void RegistrarNotaFallida(NotaBaileInfinito nota, int carril)
    {
        if (!juegoActivo) return;

        if (carril >= 0 && carril < 4 && notasActivasPorCarril[carril].Contains(nota))
        {
            notasActivasPorCarril[carril].Remove(nota);
        }

        RestarVida();
    }

    private void RestarVida()
    {
        vidasActuales = Mathf.Max(0, vidasActuales - 1);
        if (MusicSoundController.Instance != null)
            MusicSoundController.Instance.ReproducirSFX("Fallo");

        ActualizarUI();

        if (vidasActuales <= 0)
        {
            FinalizarPartida();
        }
    }

    private IEnumerator RutinaSpawnNotas()
    {
        while (juegoActivo)
        {
            int columnaAleatoria = Random.Range(0, 4);
            GameObject prefabEspecificoColumna = null;

            if (columnaAleatoria == 0) prefabEspecificoColumna = prefabNotaIzquierda;
            else if (columnaAleatoria == 1) prefabEspecificoColumna = prefabNotaAbajo;
            else if (columnaAleatoria == 2) prefabEspecificoColumna = prefabNotaArriba;
            else if (columnaAleatoria == 3) prefabEspecificoColumna = prefabNotaDerecha;

            if (prefabEspecificoColumna != null && puntosSpawnAbajo[columnaAleatoria] != null)
            {
                GameObject nuevaNotaObj = Instantiate(prefabEspecificoColumna, puntosSpawnAbajo[columnaAleatoria].position, Quaternion.identity);
                NotaBaileInfinito componenteNota = nuevaNotaObj.GetComponent<NotaBaileInfinito>();

                if (componenteNota != null)
                {
                    componenteNota.Inicializar(columnaAleatoria, velocidadActual, this);
                    notasActivasPorCarril[columnaAleatoria].Add(componenteNota);
                }
            }

            yield return new WaitForSeconds(frecuenciaSpawn);
        }
    }

    private void LimpiarNotasActivas()
    {
        for (int i = 0; i < 4; i++)
        {
            foreach (NotaBaileInfinito nota in notasActivasPorCarril[i])
            {
                if (nota != null) Destroy(nota.gameObject);
            }
            notasActivasPorCarril[i].Clear();
        }
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
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"Aciertos: {notasAcertadas}";
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarNotasActivas();

        bool esNuevoRecord = false;
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);
            esNuevoRecord = notasAcertadas > puntuacionAnterior;
            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(idMinijuego, notasAcertadas);
        }

        if (panelFin != null) panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text = $"Puntuacion final: {notasAcertadas}";
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