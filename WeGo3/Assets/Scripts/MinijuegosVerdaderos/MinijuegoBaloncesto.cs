using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinijuegoBaloncesto : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria = "Triple_Anotado";
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Configuración del Minijuego")]
    [SerializeField] private int puntosObjetivo = 10;
    [SerializeField] private int penalizacionPorFallo = 1; // Puntos que se restan al fallar
    [SerializeField] private float tiempoTotalPartida = 45f;
    [SerializeField] private float velocidadRotacion = 180f;
    [SerializeField] private float radioOrbita = 2.0f;

    [Header("Transformes de Referencia en el Mundo")]
    [SerializeField] private Transform centroCircunferencia;
    [SerializeField] private Transform objetoZonaVerde;
    [SerializeField] private Transform objetoAgujaPelota;
    [SerializeField] private bool rotarSpriteHaciaCentro = true;

    [Header("Tolerancia del Acierto")]
    [SerializeField] private float toleranciaAciertoGrados = 20f;

    [Header("Referencias de Canasta y Pelota (Lanzamiento)")]
    [SerializeField] private GameObject prefabPelota;
    [SerializeField] private Transform puntoLanzamientoA;
    [SerializeField] private Transform puntoCanastaB;
    [SerializeField] private Transform puntoFalloC;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderPuntos;
    [SerializeField] private TextMeshProUGUI textoPuntos;
    [SerializeField] private TextMeshProUGUI textoTiempo;

    private int puntosActuales = 0;
    private float tiempoRestante;
    private bool juegoActivo = false;

    private float anguloActualAguja = 0f;
    private float anguloZonaVerde = 0f;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        puntosActuales = 0;
        tiempoRestante = tiempoTotalPartida;
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
        puntosActuales = 0;
        tiempoRestante = tiempoTotalPartida;
        MoverZonaVerdeAleatoria();
        ActualizarUI();
    }

    void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;
        ActualizarUI();

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();

            if (puntosActuales >= puntosObjetivo)
                GanarPartida();
            else
                PerderPartida();
            return;
        }

        // 2. ORBITAR LA AGUJA / PELOTA
        if (centroCircunferencia != null && objetoAgujaPelota != null)
        {
            anguloActualAguja += velocidadRotacion * Time.deltaTime;
            if (anguloActualAguja >= 360f) anguloActualAguja -= 360f;

            float rad = anguloActualAguja * Mathf.Deg2Rad;
            float posX = centroCircunferencia.position.x + Mathf.Cos(rad) * radioOrbita;
            float posY = centroCircunferencia.position.y + Mathf.Sin(rad) * radioOrbita;

            objetoAgujaPelota.position = new Vector3(posX, posY, centroCircunferencia.position.z);

            if (rotarSpriteHaciaCentro)
            {
                objetoAgujaPelota.rotation = Quaternion.Euler(0, 0, anguloActualAguja - 90f);
            }
        }

        // 3. DETECCIÓN DE CLIC
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            IntentarLanzamiento();
        }
    }

    private void MoverZonaVerdeAleatoria()
    {
        anguloZonaVerde = Random.Range(0f, 360f);

        if (centroCircunferencia != null && objetoZonaVerde != null)
        {
            float rad = anguloZonaVerde * Mathf.Deg2Rad;
            float posX = centroCircunferencia.position.x + Mathf.Cos(rad) * radioOrbita;
            float posY = centroCircunferencia.position.y + Mathf.Sin(rad) * radioOrbita;

            objetoZonaVerde.position = new Vector3(posX, posY, centroCircunferencia.position.z);

            if (rotarSpriteHaciaCentro)
            {
                objetoZonaVerde.rotation = Quaternion.Euler(0, 0, anguloZonaVerde - 90f);
            }
        }
    }

    private void IntentarLanzamiento()
    {
        float diferenciaAngulo = Mathf.Abs(anguloActualAguja - anguloZonaVerde);
        if (diferenciaAngulo > 180f)
        {
            diferenciaAngulo = 360f - diferenciaAngulo;
        }

        bool acierto = diferenciaAngulo <= toleranciaAciertoGrados;

        if (acierto)
        {
            puntosActuales++;
            MusicSoundController.Instance.ReproducirSFX("Acierto");
            LanzarPelotaAnimacion(true);
            MoverZonaVerdeAleatoria();
        }
        else
        {
            // Restamos puntos al fallar (evitando que bajen de 0)
            puntosActuales -= penalizacionPorFallo;
            if (puntosActuales < 0) puntosActuales = 0;

            MusicSoundController.Instance.ReproducirSFX("Fallo");
            LanzarPelotaAnimacion(false);
        }

        ActualizarUI();
        ComprobarCondicionVictoria();
    }

    private void LanzarPelotaAnimacion(bool acierto)
    {
        if (prefabPelota == null || puntoLanzamientoA == null) return;

        GameObject pelota = Instantiate(prefabPelota, puntoLanzamientoA.position, Quaternion.identity);
        Transform destinoFinal = acierto ? puntoCanastaB : puntoFalloC;
        if (destinoFinal == null) destinoFinal = puntoCanastaB;

        StartCoroutine(RutinaParabolaPelota(pelota.transform, puntoLanzamientoA.position, destinoFinal.position));
    }

    private IEnumerator RutinaParabolaPelota(Transform pelotaTransform, Vector3 inicio, Vector3 destino)
    {
        float duracion = 0.5f;
        float tiempo = 0f;
        float alturaParabola = 2.5f;

        while (tiempo < duracion && pelotaTransform != null)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;

            Vector3 posicionActual = Vector3.Lerp(inicio, destino, t);
            posicionActual.y += Mathf.Sin(t * Mathf.PI) * alturaParabola;

            pelotaTransform.position = posicionActual;
            yield return null;
        }

        if (pelotaTransform != null)
        {
            Destroy(pelotaTransform.gameObject, 0.2f);
        }
    }

    private void ComprobarCondicionVictoria()
    {
        if (puntosActuales >= puntosObjetivo)
        {
            GanarPartida();
        }
    }

    private void ActualizarUI()
    {
        if (sliderPuntos != null)
        {
            sliderPuntos.maxValue = puntosObjetivo;
            sliderPuntos.value = puntosActuales;
        }
        if (textoPuntos != null)
        {
            textoPuntos.text = $"Triples: {puntosActuales} / {puntosObjetivo}";
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