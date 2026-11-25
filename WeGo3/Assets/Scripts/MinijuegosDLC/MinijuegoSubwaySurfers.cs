using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MinijuegoSubwaySurfers : MonoBehaviour
{
    private enum TipoElemento
    {
        VallaBaja,      // Se salta o se cambia de carril
        BarreraAlta,    // Se agacha/desliza o se cambia de carril
        TrenCompleto,   // Solo se esquiva cambiando de carril
        Moneda          // Suma puntos extra
    }

    private class ElementoPista
    {
        public GameObject obj;
        public TipoElemento tipo;
        public int carril; // 0 = Izq, 1 = Centro, 2 = Der
        public bool procesado;
    }

    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Objetivo, Tiempo y Vidas")]
    [SerializeField] private float duracionMinijuego = 35f;
    [SerializeField] private int vidasMaximas = 3;
    [SerializeField] private TMP_Text textoTemporizador;
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private TMP_Text textoMonedas;
    [SerializeField] private Slider sliderProgresoTiempo;
    [SerializeField] private Slider sliderVidas;

    [Header("Fondo en Bucle (Pixel Art 192x108 -> 1920x1080)")]
    [Tooltip("Primer tramo del fondo (abajo/inicial).")]
    [SerializeField] private Transform fondoA;
    [Tooltip("Segundo tramo idéntico del fondo colocado justo encima del primero.")]
    [SerializeField] private Transform fondoB;
    [Tooltip("Si es > 0, usa esta altura exacta en unidades de mundo (o píxeles si es UI). Si es 0, intenta calcularla sola.")]
    [SerializeField] private float alturaTramoFondo = 0f;
    [Tooltip("Multiplicador de velocidad del fondo respecto a la velocidad de la pista.")]
    [SerializeField] private float multiplicadorVelocidadFondo = 1f;

    [Header("Jugador y 3 Carriles")]
    [SerializeField] private Transform jugador;
    [SerializeField] private SpriteRenderer spriteJugador;
    [SerializeField] private float posicionXCentro = 0f;
    [SerializeField] private float separacionCarriles = 2.5f;
    [SerializeField] private float velocidadCambioCarril = 18f;

    [Header("Salto y Agacharse (Subway Style)")]
    [SerializeField] private float fuerzaSalto = 14f;
    [SerializeField] private float gravedad = 38f;
    [SerializeField] private float escalaExtraAlSaltar = 0.45f;
    [SerializeField] private float duracionAgachado = 0.65f;
    [SerializeField] private float escalaYAgachado = 0.5f;

    [Header("Spawneo y Velocidad de Pista")]
    [SerializeField] private GameObject prefabVallaBaja;
    [SerializeField] private GameObject prefabBarreraAlta;
    [SerializeField] private GameObject prefabTrenCompleto;
    [SerializeField] private GameObject prefabMoneda;
    [SerializeField] private float alturaSpawnY = 7f;
    [SerializeField] private float alturaDestruccionY = -7f;
    [SerializeField] private float velocidadInicialPista = 7f;
    [SerializeField] private float velocidadMaximaPista = 14f;
    [SerializeField] private float intervaloInicialSpawn = 1.15f;
    [SerializeField] private float intervaloMinimoSpawn = 0.55f;

    [Header("Detección de Impacto e Inmunidad")]
    [SerializeField] private float rangoColisionY = 0.75f;
    [SerializeField] private float tiempoInvulnerable = 1.2f;

    private float tiempoRestante;
    private int vidasActuales;
    private int monedasRecogidas;
    private bool juegoActivo = false;

    // Fondo
    private float alturaCalculadaFondo = 0f;
    private Vector3 posInicialFondoA;
    private Vector3 posInicialFondoB;

    // Estado del jugador
    private int carrilActual = 1; // 0 = Izquierda, 1 = Centro, 2 = Derecha
    private float alturaSalto = 0f;
    private float velocidadVerticalSalto = 0f;
    private bool estaSaltando = false;
    private bool estaAgachado = false;
    private float temporizadorAgachado = 0f;
    private float temporizadorInvulnerable = 0f;

    private Vector3 escalaBaseJugador = Vector3.one;
    private readonly List<ElementoPista> elementosActivos = new List<ElementoPista>();
    private float temporizadorSpawn = 0f;

    private void Awake()
    {
        if (jugador != null)
        {
            escalaBaseJugador = jugador.localScale;
            if (spriteJugador == null)
            {
                spriteJugador = jugador.GetComponentInChildren<SpriteRenderer>();
            }
        }

        ConfigurarFondo();
    }

    private void ConfigurarFondo()
    {
        if (fondoA == null || fondoB == null) return;

        posInicialFondoA = fondoA.position;
        posInicialFondoB = fondoB.position;

        if (alturaTramoFondo > 0f)
        {
            alturaCalculadaFondo = alturaTramoFondo;
            return;
        }

        RectTransform rectA = fondoA.GetComponent<RectTransform>();
        if (rectA != null && rectA.rect.height > 0f)
        {
            alturaCalculadaFondo = rectA.rect.height * Mathf.Abs(fondoA.lossyScale.y);
            return;
        }

        SpriteRenderer srA = fondoA.GetComponent<SpriteRenderer>();
        if (srA != null && srA.sprite != null)
        {
            alturaCalculadaFondo = srA.bounds.size.y;
            return;
        }

        alturaCalculadaFondo = Mathf.Abs(fondoB.position.y - fondoA.position.y);
    }

    private void ResetearFondo()
    {
        if (fondoA == null || fondoB == null) return;

        fondoA.position = posInicialFondoA;
        fondoB.position = posInicialFondoB;
    }

    private void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        tiempoRestante = duracionMinijuego;
        vidasActuales = vidasMaximas;
        monedasRecogidas = 0;
        juegoActivo = false;

        ResetearJugador();
        ResetearFondo();
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
        LimpiarPista();
        ResetearJugador();
        ResetearFondo();

        tiempoRestante = duracionMinijuego;
        vidasActuales = vidasMaximas;
        monedasRecogidas = 0;
        temporizadorSpawn = 0.3f;
        temporizadorInvulnerable = 0f;
        juegoActivo = true;

        ActualizarUI();
    }

    private void ResetearJugador()
    {
        carrilActual = 1;
        alturaSalto = 0f;
        velocidadVerticalSalto = 0f;
        estaSaltando = false;
        estaAgachado = false;
        temporizadorAgachado = 0f;

        if (jugador != null)
        {
            Vector3 pos = jugador.position;
            pos.x = ObtenerXDeCarril(carrilActual);
            jugador.position = pos;
            jugador.localScale = escalaBaseJugador;
        }

        if (spriteJugador != null)
        {
            Color c = spriteJugador.color;
            c.a = 1f;
            spriteJugador.color = c;
        }
    }

    private float ObtenerXDeCarril(int indiceCarril)
    {
        return posicionXCentro + (indiceCarril - 1) * separacionCarriles;
    }

    private void Update()
    {
        if (!juegoActivo) return;

        // 1. Temporizador de supervivencia
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarUI();
            GanarMinijuego();
            return;
        }

        // 2. Controles y movimiento del jugador
        LeerControlesJugador();
        ActualizarMovimientoJugador();
        ActualizarInvulnerabilidad();

        // 3. Velocidad actual y scroll infinito del fondo + pista
        float progresoDificultad = 1f - Mathf.Clamp01(tiempoRestante / Mathf.Max(0.1f, duracionMinijuego));
        float velocidadActual = Mathf.Lerp(velocidadInicialPista, velocidadMaximaPista, progresoDificultad);
        float intervaloActual = Mathf.Lerp(intervaloInicialSpawn, intervaloMinimoSpawn, progresoDificultad);

        ActualizarScrollFondo(velocidadActual * multiplicadorVelocidadFondo);

        temporizadorSpawn -= Time.deltaTime;
        if (temporizadorSpawn <= 0f)
        {
            GenerarOleada();
            temporizadorSpawn = intervaloActual;
        }

        ActualizarElementosPista(velocidadActual);
        ActualizarUI();
    }

    private void ActualizarScrollFondo(float velocidadFondo)
    {
        if (fondoA == null || fondoB == null || alturaCalculadaFondo <= 0.01f) return;

        Vector3 desplazamiento = Vector3.down * velocidadFondo * Time.deltaTime;
        fondoA.position += desplazamiento;
        fondoB.position += desplazamiento;

        // Si un tramo baja por completo por debajo de su posición inicial menos su altura,
        // lo enviamos justo arriba del otro tramo para mantener el bucle sin cortes.
        float limiteInferiorA = posInicialFondoA.y - alturaCalculadaFondo;
        if (fondoA.position.y <= limiteInferiorA)
        {
            Vector3 pos = fondoA.position;
            pos.y = fondoB.position.y + alturaCalculadaFondo;
            fondoA.position = pos;
        }

        float limiteInferiorB = posInicialFondoB.y - alturaCalculadaFondo;
        if (fondoB.position.y <= limiteInferiorB)
        {
            Vector3 pos = fondoB.position;
            pos.y = fondoA.position.y + alturaCalculadaFondo;
            fondoB.position = pos;
        }
    }

    private void LeerControlesJugador()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            if (carrilActual > 0)
            {
                carrilActual--;
                ReproducirSFXSeguro("Interactuar");
            }
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            if (carrilActual < 2)
            {
                carrilActual++;
                ReproducirSFXSeguro("Interactuar");
            }
        }

        if (Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!estaSaltando)
            {
                estaSaltando = true;
                estaAgachado = false;
                velocidadVerticalSalto = fuerzaSalto;
                ReproducirSFXSeguro("Interactuar");
            }
        }

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (estaSaltando)
            {
                velocidadVerticalSalto = -fuerzaSalto * 1.4f;
            }

            estaAgachado = true;
            temporizadorAgachado = duracionAgachado;
            ReproducirSFXSeguro("Interactuar");
        }
    }

    private void ActualizarMovimientoJugador()
    {
        if (jugador == null) return;

        float xObjetivo = ObtenerXDeCarril(carrilActual);
        Vector3 pos = jugador.position;
        pos.x = Mathf.MoveTowards(pos.x, xObjetivo, velocidadCambioCarril * Time.deltaTime);
        jugador.position = pos;

        if (estaSaltando)
        {
            alturaSalto += velocidadVerticalSalto * Time.deltaTime;
            velocidadVerticalSalto -= gravedad * Time.deltaTime;

            if (alturaSalto <= 0f)
            {
                alturaSalto = 0f;
                velocidadVerticalSalto = 0f;
                estaSaltando = false;
            }
        }

        if (estaAgachado)
        {
            temporizadorAgachado -= Time.deltaTime;
            if (temporizadorAgachado <= 0f)
            {
                estaAgachado = false;
            }
        }

        Vector3 escalaObjetivo = escalaBaseJugador;
        if (estaSaltando)
        {
            float factorSalto = 1f + Mathf.Clamp01(alturaSalto / 3f) * escalaExtraAlSaltar;
            escalaObjetivo = escalaBaseJugador * factorSalto;
        }
        else if (estaAgachado)
        {
            escalaObjetivo = new Vector3(escalaBaseJugador.x * 1.15f, escalaBaseJugador.y * escalaYAgachado, escalaBaseJugador.z);
        }

        jugador.localScale = Vector3.Lerp(jugador.localScale, escalaObjetivo, 20f * Time.deltaTime);
    }

    private void ActualizarInvulnerabilidad()
    {
        if (temporizadorInvulnerable <= 0f) return;

        temporizadorInvulnerable -= Time.deltaTime;

        if (spriteJugador != null)
        {
            Color c = spriteJugador.color;
            c.a = (temporizadorInvulnerable > 0f) ? (Mathf.PingPong(Time.time * 14f, 1f) > 0.5f ? 1f : 0.35f) : 1f;
            spriteJugador.color = c;
        }
    }

    private void GenerarOleada()
    {
        int carrilLibre = Random.Range(0, 3);

        for (int c = 0; c < 3; c++)
        {
            if (c == carrilLibre)
            {
                if (prefabMoneda != null && Random.value < 0.65f)
                {
                    InstanciarElemento(TipoElemento.Moneda, c);
                }
                continue;
            }

            if (Random.value < 0.72f)
            {
                TipoElemento tipoElegido = ElegirTipoObstaculoAleatorio();
                InstanciarElemento(tipoElegido, c);
            }
        }
    }

    private TipoElemento ElegirTipoObstaculoAleatorio()
    {
        List<TipoElemento> disponibles = new List<TipoElemento>();
        if (prefabVallaBaja != null) disponibles.Add(TipoElemento.VallaBaja);
        if (prefabBarreraAlta != null) disponibles.Add(TipoElemento.BarreraAlta);
        if (prefabTrenCompleto != null) disponibles.Add(TipoElemento.TrenCompleto);

        if (disponibles.Count == 0) return TipoElemento.TrenCompleto;
        return disponibles[Random.Range(0, disponibles.Count)];
    }

    private void InstanciarElemento(TipoElemento tipo, int carril)
    {
        GameObject prefab = null;
        switch (tipo)
        {
            case TipoElemento.VallaBaja: prefab = prefabVallaBaja; break;
            case TipoElemento.BarreraAlta: prefab = prefabBarreraAlta; break;
            case TipoElemento.TrenCompleto: prefab = prefabTrenCompleto; break;
            case TipoElemento.Moneda: prefab = prefabMoneda; break;
        }

        if (prefab == null) return;

        Vector3 posSpawn = new Vector3(ObtenerXDeCarril(carril), alturaSpawnY, 0f);
        GameObject instancia = Instantiate(prefab, posSpawn, Quaternion.identity, transform);

        elementosActivos.Add(new ElementoPista
        {
            obj = instancia,
            tipo = tipo,
            carril = carril,
            procesado = false
        });
    }

    private void ActualizarElementosPista(float velocidadActual)
    {
        if (jugador == null) return;

        for (int i = elementosActivos.Count - 1; i >= 0; i--)
        {
            ElementoPista elem = elementosActivos[i];
            if (elem.obj == null)
            {
                elementosActivos.RemoveAt(i);
                continue;
            }

            elem.obj.transform.position += Vector3.down * velocidadActual * Time.deltaTime;

            if (!elem.procesado && Mathf.Abs(elem.obj.transform.position.y - jugador.position.y) <= rangoColisionY)
            {
                if (elem.carril == carrilActual)
                {
                    EvaluarInteraccion(elem);
                }
            }

            if (elem.obj != null && elem.obj.transform.position.y <= alturaDestruccionY)
            {
                Destroy(elem.obj);
                elementosActivos.RemoveAt(i);
            }
        }
    }

    private void EvaluarInteraccion(ElementoPista elem)
    {
        if (elem.tipo == TipoElemento.Moneda)
        {
            elem.procesado = true;
            monedasRecogidas++;
            ReproducirSFXSeguro("Acierto");
            Destroy(elem.obj);
            return;
        }

        bool esquivado = false;
        if (elem.tipo == TipoElemento.VallaBaja && estaSaltando && alturaSalto > 0.35f)
        {
            esquivado = true;
        }
        else if (elem.tipo == TipoElemento.BarreraAlta && estaAgachado && !estaSaltando)
        {
            esquivado = true;
        }

        if (esquivado)
        {
            elem.procesado = true;
            return;
        }

        if (temporizadorInvulnerable > 0f)
        {
            elem.procesado = true;
            return;
        }

        elem.procesado = true;
        vidasActuales = Mathf.Max(0, vidasActuales - 1);
        temporizadorInvulnerable = tiempoInvulnerable;
        ReproducirSFXSeguro("Fallo");

        if (elem.obj != null)
        {
            Destroy(elem.obj);
        }

        if (vidasActuales <= 0)
        {
            ActualizarUI();
            PerderMinijuego();
        }
    }

    private void LimpiarPista()
    {
        for (int i = elementosActivos.Count - 1; i >= 0; i--)
        {
            if (elementosActivos[i].obj != null)
            {
                Destroy(elementosActivos[i].obj);
            }
        }
        elementosActivos.Clear();
    }

    private void ActualizarUI()
    {
        if (textoTemporizador != null)
        {
            textoTemporizador.text = $"{Mathf.CeilToInt(tiempoRestante)}s";
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"{vidasActuales} / {vidasMaximas}";
        }

        if (textoMonedas != null)
        {
            textoMonedas.text = $"{monedasRecogidas}";
        }

        if (sliderProgresoTiempo != null)
        {
            sliderProgresoTiempo.minValue = 0f;
            sliderProgresoTiempo.maxValue = duracionMinijuego;
            sliderProgresoTiempo.value = duracionMinijuego - tiempoRestante;
        }

        if (sliderVidas != null)
        {
            sliderVidas.minValue = 0f;
            sliderVidas.maxValue = vidasMaximas;
            sliderVidas.value = vidasActuales;
        }
    }

    private void ReproducirSFXSeguro(string nombreSFX)
    {
        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX(nombreSFX);
        }
    }

    private void GanarMinijuego()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarPista();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        ReproducirSFXSeguro("Ganar");
    }

    private void PerderMinijuego()
    {
        juegoActivo = false;
        StopAllCoroutines();
        LimpiarPista();

        if (panelDerrota != null) panelDerrota.SetActive(true);
        ReproducirSFXSeguro("Perder");
    }

    public void volverGym(bool victoriaConseguida)
    {
        if (victoriaConseguida)
        {
            if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(flagVictoria))
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