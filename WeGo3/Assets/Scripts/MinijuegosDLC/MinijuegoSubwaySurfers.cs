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
        BarreraAlta,    // Se agacha/desliza (pasa por debajo cambiando Sorting Layer / Order) o se cambia de carril
        TrenCompleto    // Solo se esquiva cambiando de carril
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
    [SerializeField] private Slider sliderProgresoTiempo;
    [SerializeField] private Slider sliderVidas;

    [Header("Fondo Infinito (3 GameObjects)")]
    [SerializeField] private Transform fondoA;
    [SerializeField] private Transform fondoB;
    [SerializeField] private Transform fondoC;
    [SerializeField] private float limiteInferiorY = -10f; // Cuando baje de esta Y, salta arriba
    [SerializeField] private float separacionY = 10f;      // Distancia vertical entre fondos (10)
    [SerializeField] private float multiplicadorVelocidadFondo = 1f;

    [Header("Jugador y 3 Carriles")]
    [SerializeField] private Transform jugador;
    [SerializeField] private SpriteRenderer spriteJugador;
    [Tooltip("Nombre del Sorting Layer al que pasa el jugador al pulsar S para pasar por debajo de barreras altas.")]
    [SerializeField] private string sortingLayerDebajoBarrera = "Default";
    [Tooltip("Order in Layer que toma el jugador al pulsar S.")]
    [SerializeField] private int orderInLayerAgachado = 0;
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
    [SerializeField] private float alturaSpawnY = 7f;
    [SerializeField] private float alturaDestruccionY = -7f;
    [SerializeField] private float velocidadInicialPista = 7f;
    [SerializeField] private float velocidadMaximaPista = 14f;
    [SerializeField] private float intervaloInicialSpawn = 1.15f;
    [SerializeField] private float intervaloMinimoSpawn = 0.55f;

    [Header("Detección de Impacto e Inmunidad")]
    [SerializeField] private float rangoColisionY = 0.75f;
    [Tooltip("Rango vertical mayor para trenes por su longitud en Y.")]
    [SerializeField] private float rangoColisionTrenY = 1.35f;
    [Tooltip("Margen horizontal respecto al centro del carril para detectar golpe con trenes.")]
    [SerializeField] private float margenXColisionTren = 0.9f;
    [SerializeField] private float tiempoInvulnerable = 1.2f;

    private float tiempoRestante;
    private int vidasActuales;
    private bool juegoActivo = false;

    // Fondo en mundo 2D (3 GameObjects)
    private Transform trFondoA;
    private Transform trFondoB;
    private Transform trFondoC;
    private Vector3 posInicialFondoA;
    private Vector3 posInicialFondoB;
    private Vector3 posInicialFondoC;
    private float pasoFondoMundo = 0f;

    // Estado del jugador y Sorting Layer / Order
    private int carrilActual = 1; // 0 = Izquierda, 1 = Centro, 2 = Derecha
    private float alturaSalto = 0f;
    private float velocidadVerticalSalto = 0f;
    private bool estaSaltando = false;
    private bool estaAgachado = false;
    private float temporizadorAgachado = 0f;
    private float temporizadorInvulnerable = 0f;

    private int sortingLayerIDOriginal;
    private int orderInLayerOriginal;
    private bool tieneSortingOriginal = false;

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

        if (spriteJugador != null)
        {
            sortingLayerIDOriginal = spriteJugador.sortingLayerID;
            orderInLayerOriginal = spriteJugador.sortingOrder;
            tieneSortingOriginal = true;
        }

        ConfigurarFondo();
    }

    private void ConfigurarFondo()
    {
        trFondoA = fondoA != null ? fondoA.transform : null;
        trFondoB = fondoB != null ? fondoB.transform : null;
        trFondoC = fondoC != null ? fondoC.transform : null;

        if (trFondoA != null) posInicialFondoA = trFondoA.position;
        if (trFondoB != null) posInicialFondoB = trFondoB.position;
        if (trFondoC != null) posInicialFondoC = trFondoC.position;

        if (trFondoA != null)
        {
            SpriteRenderer sr = trFondoA.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                pasoFondoMundo = sr.sprite.bounds.size.y * Mathf.Abs(trFondoA.lossyScale.y);
            }
            else if (trFondoB != null && Mathf.Abs(posInicialFondoB.y - posInicialFondoA.y) > 0.01f)
            {
                pasoFondoMundo = Mathf.Abs(posInicialFondoB.y - posInicialFondoA.y);
            }
        }
    }

    private void ResetearFondo()
    {
        if (trFondoA != null) trFondoA.position = posInicialFondoA;
        if (trFondoB != null) trFondoB.position = posInicialFondoB;
        if (trFondoC != null) trFondoC.position = posInicialFondoC;
    }

    private void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        tiempoRestante = duracionMinijuego;
        vidasActuales = vidasMaximas;
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

        RestaurarSortingJugador();

        if (jugador != null)
        {
            Vector3 pos = jugador.position;
            pos.x = ObtenerXDeCarril(carrilActual);
            jugador.position = pos;
            jugador.localScale = escalaBaseJugador;
        }

        if (spriteJugador != null)
        {
            spriteJugador.flipX = false; // Mira a la derecha por defecto
            Color c = spriteJugador.color;
            c.a = 1f;
            spriteJugador.color = c;
        }
    }

    private void AplicarSortingAgachado()
    {
        if (spriteJugador == null) return;

        if (!string.IsNullOrEmpty(sortingLayerDebajoBarrera))
        {
            int idDestino = SortingLayer.NameToID(sortingLayerDebajoBarrera);
            if (idDestino != 0 || sortingLayerDebajoBarrera == "Default")
            {
                spriteJugador.sortingLayerID = idDestino;
            }
        }

        spriteJugador.sortingOrder = orderInLayerAgachado;
    }

    private void RestaurarSortingJugador()
    {
        if (spriteJugador != null && tieneSortingOriginal)
        {
            spriteJugador.sortingLayerID = sortingLayerIDOriginal;
            spriteJugador.sortingOrder = orderInLayerOriginal;
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
        if (fondoA == null || fondoB == null || fondoC == null) return;

        Vector3 bajada = Vector3.down * Mathf.Abs(velocidadFondo) * Time.deltaTime;
        fondoA.position += bajada;
        fondoB.position += bajada;
        fondoC.position += bajada;

        // Si A baja de limiteInferiorY -> se coloca encima de C
        if (fondoA.position.y < limiteInferiorY)
        {
            fondoA.position = new Vector3(
                fondoA.position.x,
                fondoC.position.y + separacionY,
                fondoA.position.z
            );
        }

        // Si B baja de limiteInferiorY -> se coloca encima de A
        if (fondoB.position.y < limiteInferiorY)
        {
            fondoB.position = new Vector3(
                fondoB.position.x,
                fondoA.position.y + separacionY,
                fondoB.position.z
            );
        }

        // Si C baja de limiteInferiorY -> se coloca encima de B
        if (fondoC.position.y < limiteInferiorY)
        {
            fondoC.position = new Vector3(
                fondoC.position.x,
                fondoB.position.y + separacionY,
                fondoC.position.z
            );
        }
    }

    private void LeerControlesJugador()
    {
        if (Keyboard.current == null) return;

        // Cambio de carril (Izquierda / Derecha) + orientación del sprite
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            if (spriteJugador != null) spriteJugador.flipX = true;

            if (carrilActual > 0)
            {
                carrilActual--;
                ReproducirSFXSeguro("Interactuar");
            }
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            if (spriteJugador != null) spriteJugador.flipX = false;

            if (carrilActual < 2)
            {
                carrilActual++;
                ReproducirSFXSeguro("Interactuar");
            }
        }

        // Saltar (W / Flecha Arriba / Espacio)
        if (Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (!estaSaltando)
            {
                estaSaltando = true;
                estaAgachado = false;
                RestaurarSortingJugador();
                velocidadVerticalSalto = fuerzaSalto;
                ReproducirSFXSeguro("Interactuar");
            }
        }

        // Agacharse / Pasar por debajo de barrera alta (S / Flecha Abajo)
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (estaSaltando)
            {
                velocidadVerticalSalto = -fuerzaSalto * 1.4f;
            }

            estaAgachado = true;
            temporizadorAgachado = duracionAgachado;
            AplicarSortingAgachado();
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
                RestaurarSortingJugador();
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
            if (c == carrilLibre) continue;

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

        if (elementosActivos.Count < 0) return;

        for (int i = elementosActivos.Count - 1; i >= 0; i--)
        {
            ElementoPista elem = elementosActivos[i];
            if (elem.obj == null)
            {
                elementosActivos.RemoveAt(i);
                continue;
            }

            elem.obj.transform.position += Vector3.down * velocidadActual * Time.deltaTime;

            float rangoY = (elem.tipo == TipoElemento.TrenCompleto) ? rangoColisionTrenY : rangoColisionY;
            bool dentroEnY = Mathf.Abs(elem.obj.transform.position.y - jugador.position.y) <= rangoY;

            bool coincideCarrilOEnTren = (elem.carril == carrilActual);
            if (!coincideCarrilOEnTren && elem.tipo == TipoElemento.TrenCompleto)
            {
                coincideCarrilOEnTren = Mathf.Abs(elem.obj.transform.position.x - jugador.position.x) <= margenXColisionTren;
            }

            if (!elem.procesado && dentroEnY && coincideCarrilOEnTren)
            {
                EvaluarInteraccion(elem);
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
        RestaurarSortingJugador();
        StopAllCoroutines();
        LimpiarPista();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        ReproducirSFXSeguro("Ganar");
    }

    private void PerderMinijuego()
    {
        juegoActivo = false;
        RestaurarSortingJugador();
        StopAllCoroutines();
        LimpiarPista();

        if (panelDerrota != null) panelDerrota.SetActive(true);
        ReproducirSFXSeguro("Perder");
    }

    public void volverGym(bool victoriaConseguida)
    {
        RestaurarSortingJugador();

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