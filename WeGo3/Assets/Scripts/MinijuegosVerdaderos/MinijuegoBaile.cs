using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinijuegoBaile : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria = "Baile_Completado";
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Configuración del Minijuego (Basado en Tiempo)")]
    [SerializeField] private float tiempoTotalPartida = 30f;
    [SerializeField] private int vidasMaximas = 5;
    private int vidasActuales;
    private float tiempoRestante;
    private bool juegoActivo = false;

    [Header("Prefabs de Notas (Asignados a su columna fija)")]
    [SerializeField] private GameObject prefabNotaIzquierda; // Columna 0 (Izquierda)
    [SerializeField] private GameObject prefabNotaAbajo;     // Columna 1 (Abajo)
    [SerializeField] private GameObject prefabNotaArriba;    // Columna 2 (Arriba)
    [SerializeField] private GameObject prefabNotaDerecha;   // Columna 3 (Derecha)

    [Header("Puntos de Spawn (Abajo) y Zonas de Pulsación (Arriba)")]
    [SerializeField] private Transform[] puntosSpawnAbajo = new Transform[4];     // 0: Izq, 1: Abajo, 2: Arriba, 3: Der
    [SerializeField] private Transform[] zonasResaltadasArriba = new Transform[4]; // 0: Izq, 1: Abajo, 2: Arriba, 3: Der
    [SerializeField] private float velocidadAscenso = 4f;
    [SerializeField] private float frecuenciaSpawn = 0.8f;

    [Header("Tolerancia de Acierto (Distancia en Y)")]
    [SerializeField] private float toleranciaAcierto = 0.5f;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderVida;
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private TextMeshProUGUI textoTiempo; // Solo texto (sin slider)

    // Listas para rastrear las notas activas en cada carril/columna
    private List<NotaBaile>[] notasActivasPorCarril = new List<NotaBaile>[4];

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            notasActivasPorCarril[i] = new List<NotaBaile>();
        }

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
        tiempoRestante = tiempoTotalPartida;

        ActualizarUI();
        StartCoroutine(RutinaSpawnNotas());
    }

    void Update()
    {
        if (!juegoActivo) return;

        // 1. GESTIÓN DEL TIEMPO
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            GanarPartida();
            return;
        }

        // 2. ENTRADA DE TECLADO (WASD / Flechas)
        ComprobarEntradaJugador();

        ActualizarUI();
    }

    private void ComprobarEntradaJugador()
    {
        if (Keyboard.current == null) return;

        // Columna 0: Izquierda (A o Flecha Izquierda)
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(0);
        }
        // Columna 1: Abajo (S o Flecha Abajo)
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(1);
        }
        // Columna 2: Arriba (W o Flecha Arriba)
        else if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(2);
        }
        // Columna 3: Derecha (D o Flecha Derecha)
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            IntentarPulsarNota(3);
        }
    }

    private void IntentarPulsarNota(int carril)
    {
        if (carril < 0 || carril >= 4 || notasActivasPorCarril[carril].Count == 0) return;

        NotaBaile notaAChequear = notasActivasPorCarril[carril][0];

        if (notaAChequear != null && zonasResaltadasArriba[carril] != null)
        {
            float distanciaY = Mathf.Abs(notaAChequear.transform.position.y - zonasResaltadasArriba[carril].position.y);

            if (distanciaY <= toleranciaAcierto)
            {
                MusicSoundController.Instance.ReproducirSFX("Acierto");
                notasActivasPorCarril[carril].RemoveAt(0);
                Destroy(notaAChequear.gameObject);
            }
            else
            {
                RestarVida();
            }
        }
    }

    public void RegistrarNotaFallida(NotaBaile nota, int carril)
    {
        if (!juegoActivo) return;

        if (notasActivasPorCarril[carril].Contains(nota))
        {
            notasActivasPorCarril[carril].Remove(nota);
        }
        RestarVida();
    }

    private void RestarVida()
    {
        vidasActuales--;
        MusicSoundController.Instance.ReproducirSFX("Fallo");
        ActualizarUI();

        if (vidasActuales <= 0)
        {
            PerderPartida();
        }
    }

    private IEnumerator RutinaSpawnNotas()
    {
        while (juegoActivo)
        {
            // Seleccionamos al azar qué columna se va a activar en este pulso
            int columnaAleatoria = Random.Range(0, 4);

            GameObject prefabEspecíficoColumna = null;

            // Cada columna TIENE ASIGNADA SIEMPRE su propia flecha
            if (columnaAleatoria == 0) prefabEspecíficoColumna = prefabNotaIzquierda;
            else if (columnaAleatoria == 1) prefabEspecíficoColumna = prefabNotaAbajo;
            else if (columnaAleatoria == 2) prefabEspecíficoColumna = prefabNotaArriba;
            else if (columnaAleatoria == 3) prefabEspecíficoColumna = prefabNotaDerecha;

            if (prefabEspecíficoColumna != null && puntosSpawnAbajo[columnaAleatoria] != null)
            {
                // Instancia la flecha en la X exacta de su columna correspondiente
                GameObject nuevaNotaObj = Instantiate(prefabEspecíficoColumna, puntosSpawnAbajo[columnaAleatoria].position, Quaternion.identity);
                NotaBaile componenteNota = nuevaNotaObj.GetComponent<NotaBaile>();

                if (componenteNota != null)
                {
                    componenteNota.Inicializar(columnaAleatoria, velocidadAscenso, this);
                    notasActivasPorCarril[columnaAleatoria].Add(componenteNota);
                }
            }

            yield return new WaitForSeconds(frecuenciaSpawn);
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

        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }
    }

    private void GanarPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();
        if (panelVictoria != null) panelVictoria.SetActive(true);
        MusicSoundController.Instance.ReproducirSFX("Ganar");

    }

    private void PerderPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();
        if (panelDerrota != null) panelDerrota.SetActive(true);
        MusicSoundController.Instance.ReproducirSFX("Perder");

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