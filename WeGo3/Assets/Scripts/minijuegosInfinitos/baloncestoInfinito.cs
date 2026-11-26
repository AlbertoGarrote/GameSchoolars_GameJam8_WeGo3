using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class baloncestoInfinito : MonoBehaviour
{
    [Header("Identificador y Retorno Arcade")]
    [SerializeField] private string idMinijuego = "Baloncesto";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Configuración del Minijuego")]
    [SerializeField] private int penalizacionPorFallo = 1; // Puntos que se restan al fallar
    [SerializeField] private float tiempoTotalPartida = 30f;
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
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

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
        if (panelFin != null) panelFin.SetActive(false);
        if (textoNuevoRecord != null) textoNuevoRecord.gameObject.SetActive(false);

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
            FinalizarPartida();
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
            if (MusicSoundController.Instance != null)
                MusicSoundController.Instance.ReproducirSFX("Acierto");
            LanzarPelotaAnimacion(true);
            MoverZonaVerdeAleatoria();
        }
        else
        {
            puntosActuales -= penalizacionPorFallo;
            if (puntosActuales < 0) puntosActuales = 0;

            if (MusicSoundController.Instance != null)
                MusicSoundController.Instance.ReproducirSFX("Fallo");
            LanzarPelotaAnimacion(false);
        }

        ActualizarUI();
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

    private void ActualizarUI()
    {
        if (textoPuntos != null)
        {
            textoPuntos.text = $"Puntos: {puntosActuales}";
        }
        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }
    }

    private void FinalizarPartida()
    {
        juegoActivo = false;
        StopAllCoroutines();

        bool esNuevoRecord = false;
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);
            esNuevoRecord = puntosActuales > puntuacionAnterior;
            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(idMinijuego, puntosActuales);
        }

        if (panelFin != null) panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text = $"Puntuacion final: {puntosActuales}";
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
            MusicSoundController.Instance.ReproducirSFX("Ganar");
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