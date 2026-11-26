
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class subwayInfinito : MonoBehaviour
{
    private enum TipoElemento
    {
        VallaBaja,
        BarreraAlta,
        TrenCompleto
    }

    private class ElementoPista
    {
        public GameObject obj;
        public TipoElemento tipo;
        public int carril;
        public bool procesado;
    }

    [Header("Identificador y Retorno")]
    [SerializeField] private string idMinijuego = "Subway";
    [SerializeField] private string nombreEscenaMenuInfinito = "MenuInfinito";

    [Header("UI y Paneles")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelFin;
    [SerializeField] private TMP_Text textoPuntuacionFinal;
    [SerializeField] private TMP_Text textoNuevoRecord;

    [Header("Tiempo, Puntuacion y Vidas")]
    [SerializeField] private int vidasMaximas = 5;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoVidas;
    [SerializeField] private Slider sliderVidas;

    [Header("Dificultad Progresiva")]
    [SerializeField] private float velocidadInicialPista = 7f;
    [SerializeField] private float velocidadMaximaPista = 18f;
    [SerializeField] private float intervaloInicialSpawn = 1.15f;
    [SerializeField] private float intervaloMinimoSpawn = 0.4f;
    [SerializeField] private float tiempoParaDificultadMaxima = 90f;

    [Header("Fondo Infinito")]
    [SerializeField] private Transform fondoA;
    [SerializeField] private Transform fondoB;
    [SerializeField] private Transform fondoC;
    [SerializeField] private float limiteInferiorY = -10f;
    [SerializeField] private float separacionY = 10f;
    [SerializeField] private float multiplicadorVelocidadFondo = 1f;

    [Header("Jugador y Carriles")]
    [SerializeField] private Transform jugador;
    [SerializeField] private SpriteRenderer spriteJugador;
    [SerializeField] private string sortingLayerDebajoBarrera = "Default";
    [SerializeField] private int orderInLayerAgachado = 0;
    [SerializeField] private float posicionXCentro = 0f;
    [SerializeField] private float separacionCarriles = 2.5f;
    [SerializeField] private float velocidadCambioCarril = 18f;

    [Header("Salto y Agacharse")]
    [SerializeField] private float fuerzaSalto = 14f;
    [SerializeField] private float gravedad = 38f;
    [SerializeField] private float escalaExtraAlSaltar = 0.45f;
    [SerializeField] private float duracionAgachado = 0.65f;
    [SerializeField] private float escalaYAgachado = 0.5f;

    [Header("Spawneo de Obstaculos")]
    [SerializeField] private GameObject prefabVallaBaja;
    [SerializeField] private GameObject prefabBarreraAlta;
    [SerializeField] private GameObject prefabTrenCompleto;
    [SerializeField] private float alturaSpawnY = 7f;
    [SerializeField] private float alturaDestruccionY = -7f;

    [Header("Colisiones e Invulnerabilidad")]
    [SerializeField] private float rangoColisionY = 0.75f;
    [SerializeField] private float rangoColisionTrenY = 1.35f;
    [SerializeField] private float margenXColisionTren = 0.9f;
    [SerializeField] private float tiempoInvulnerable = 1.2f;

    // Estado de partida
    private float tiempoRestante;
    private float tiempoSobrevivido;
    private int vidasActuales;
    private bool juegoActivo = false;

    // Fondo
    private Transform trFondoA;
    private Transform trFondoB;
    private Transform trFondoC;

    private Vector3 posInicialFondoA;
    private Vector3 posInicialFondoB;
    private Vector3 posInicialFondoC;

    private float pasoFondoMundo = 0f;

    // Jugador
    private int carrilActual = 1;
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

    // Obstaculos
    private readonly List<ElementoPista> elementosActivos =
        new List<ElementoPista>();

    private float temporizadorSpawn = 0f;

    private void Awake()
    {
        if (jugador != null)
        {
            escalaBaseJugador = jugador.localScale;

            if (spriteJugador == null)
                spriteJugador = jugador.GetComponentInChildren<SpriteRenderer>();
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
                pasoFondoMundo =
                    sr.sprite.bounds.size.y * Mathf.Abs(trFondoA.lossyScale.y);
            }
            else if (trFondoB != null)
            {
                pasoFondoMundo =
                    Mathf.Abs(posInicialFondoB.y - posInicialFondoA.y);
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
        if (panelFin != null) panelFin.SetActive(false);

        if (textoNuevoRecord != null)
            textoNuevoRecord.gameObject.SetActive(false);

        tiempoSobrevivido = 0f;
        vidasActuales = vidasMaximas;
        juegoActivo = false;

        ResetearJugador();
        ResetearFondo();
        ActualizarUI();
    }

    public void ComenzarBoton()
    {
        if (juegoActivo) return;

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

        if (temporizadorInicial != null)
            temporizadorInicial.SetActive(false);

        IniciarJuegoReal();
    }

    private void IniciarJuegoReal()
    {
        StopAllCoroutines();

        LimpiarPista();
        ResetearJugador();
        ResetearFondo();

        tiempoSobrevivido = 0f;
        vidasActuales = vidasMaximas;

        temporizadorSpawn = 0.3f;
        temporizadorInvulnerable = 0f;

        juegoActivo = true;

        if (panelFin != null) panelFin.SetActive(false);

        if (textoNuevoRecord != null)
            textoNuevoRecord.gameObject.SetActive(false);

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
            spriteJugador.flipX = false;

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
                spriteJugador.sortingLayerID = idDestino;
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

        // 1. Tiempo sobrevivido: esta es la puntuacion
        tiempoSobrevivido += Time.deltaTime;

        // 2. Controles del jugador
        LeerControlesJugador();
        ActualizarMovimientoJugador();
        ActualizarInvulnerabilidad();

        // 3. Dificultad progresiva
        float progresoDificultad =
            Mathf.Clamp01(tiempoSobrevivido /
            Mathf.Max(0.1f, tiempoParaDificultadMaxima));

        float velocidadActual = Mathf.Lerp(
            velocidadInicialPista,
            velocidadMaximaPista,
            progresoDificultad
        );

        float intervaloActual = Mathf.Lerp(
            intervaloInicialSpawn,
            intervaloMinimoSpawn,
            progresoDificultad
        );

        // 4. Movimiento del fondo
        ActualizarScrollFondo(
            velocidadActual * multiplicadorVelocidadFondo
        );

        // 5. Generacion de obstaculos
        temporizadorSpawn -= Time.deltaTime;

        if (temporizadorSpawn <= 0f)
        {
            GenerarOleada();
            temporizadorSpawn = intervaloActual;
        }

        // 6. Movimiento y colisiones
        ActualizarElementosPista(velocidadActual);

        ActualizarUI();
    }

    private void ActualizarScrollFondo(float velocidadFondo)
    {
        if (fondoA == null || fondoB == null || fondoC == null)
            return;

        Vector3 bajada =
            Vector3.down * Mathf.Abs(velocidadFondo) * Time.deltaTime;

        fondoA.position += bajada;
        fondoB.position += bajada;
        fondoC.position += bajada;

        if (fondoA.position.y < limiteInferiorY)
        {
            fondoA.position = new Vector3(
                fondoA.position.x,
                fondoC.position.y + separacionY,
                fondoA.position.z
            );
        }

        if (fondoB.position.y < limiteInferiorY)
        {
            fondoB.position = new Vector3(
                fondoB.position.x,
                fondoA.position.y + separacionY,
                fondoB.position.z
            );
        }

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

        if (Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            if (spriteJugador != null)
                spriteJugador.flipX = true;

            if (carrilActual > 0)
            {
                carrilActual--;
                ReproducirSFXSeguro("Interactuar");
            }
        }
        else if (Keyboard.current.dKey.wasPressedThisFrame ||
                 Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            if (spriteJugador != null)
                spriteJugador.flipX = false;

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

                RestaurarSortingJugador();

                velocidadVerticalSalto = fuerzaSalto;
                ReproducirSFXSeguro("Interactuar");
            }
        }

        if (Keyboard.current.sKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (estaSaltando)
                velocidadVerticalSalto = -fuerzaSalto * 1.4f;

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
        pos.x = Mathf.MoveTowards(
            pos.x,
            xObjetivo,
            velocidadCambioCarril * Time.deltaTime
        );

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
            float factorSalto =
                1f + Mathf.Clamp01(alturaSalto / 3f) * escalaExtraAlSaltar;

            escalaObjetivo = escalaBaseJugador * factorSalto;
        }
        else if (estaAgachado)
        {
            escalaObjetivo = new Vector3(
                escalaBaseJugador.x * 1.15f,
                escalaBaseJugador.y * escalaYAgachado,
                escalaBaseJugador.z
            );
        }

        jugador.localScale = Vector3.Lerp(
            jugador.localScale,
            escalaObjetivo,
            20f * Time.deltaTime
        );
    }

    private void ActualizarInvulnerabilidad()
    {
        if (temporizadorInvulnerable <= 0f) return;

        temporizadorInvulnerable -= Time.deltaTime;

        if (spriteJugador != null)
        {
            Color c = spriteJugador.color;

            c.a = temporizadorInvulnerable > 0f
                ? (Mathf.PingPong(Time.time * 14f, 1f) > 0.5f ? 1f : 0.35f)
                : 1f;

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
                TipoElemento tipo = ElegirTipoObstaculoAleatorio();
                InstanciarElemento(tipo, c);
            }
        }
    }

    private TipoElemento ElegirTipoObstaculoAleatorio()
    {
        List<TipoElemento> disponibles = new List<TipoElemento>();

        if (prefabVallaBaja != null)
            disponibles.Add(TipoElemento.VallaBaja);

        if (prefabBarreraAlta != null)
            disponibles.Add(TipoElemento.BarreraAlta);

        if (prefabTrenCompleto != null)
            disponibles.Add(TipoElemento.TrenCompleto);

        if (disponibles.Count == 0)
            return TipoElemento.TrenCompleto;

        return disponibles[Random.Range(0, disponibles.Count)];
    }

    private void InstanciarElemento(TipoElemento tipo, int carril)
    {
        GameObject prefab = null;

        switch (tipo)
        {
            case TipoElemento.VallaBaja:
                prefab = prefabVallaBaja;
                break;

            case TipoElemento.BarreraAlta:
                prefab = prefabBarreraAlta;
                break;

            case TipoElemento.TrenCompleto:
                prefab = prefabTrenCompleto;
                break;
        }

        if (prefab == null) return;

        Vector3 posSpawn = new Vector3(
            ObtenerXDeCarril(carril),
            alturaSpawnY,
            0f
        );

        GameObject instancia = Instantiate(
            prefab,
            posSpawn,
            Quaternion.identity,
            transform
        );

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
        if (elementosActivos.Count<1) return;

        for (int i = elementosActivos.Count - 1; i >= 0; i--)
        {
            ElementoPista elem = elementosActivos[i];

            if (elem.obj == null)
            {
                elementosActivos.RemoveAt(i);
                continue;
            }

            elem.obj.transform.position +=
                Vector3.down * velocidadActual * Time.deltaTime;

            float rangoY = elem.tipo == TipoElemento.TrenCompleto
                ? rangoColisionTrenY
                : rangoColisionY;

            bool dentroEnY =
                Mathf.Abs(elem.obj.transform.position.y - jugador.position.y)
                <= rangoY;

            bool coincideCarril = elem.carril == carrilActual;

            if (!coincideCarril && elem.tipo == TipoElemento.TrenCompleto)
            {
                coincideCarril =
                    Mathf.Abs(elem.obj.transform.position.x - jugador.position.x)
                    <= margenXColisionTren;
            }

            if (!elem.procesado && dentroEnY && coincideCarril)
            {
                EvaluarInteraccion(elem);
            }

            if (elem.obj != null &&
                elem.obj.transform.position.y <= alturaDestruccionY)
            {
                Destroy(elem.obj);
                elementosActivos.RemoveAt(i);
            }
        }
    }

    private void EvaluarInteraccion(ElementoPista elem)
    {
        bool esquivado = false;

        if (elem.tipo == TipoElemento.VallaBaja &&
            estaSaltando && alturaSalto > 0.35f)
        {
            esquivado = true;
        }
        else if (elem.tipo == TipoElemento.BarreraAlta &&
                 estaAgachado && !estaSaltando)
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
            Destroy(elem.obj);

        ActualizarUI();

        if (vidasActuales <= 0)
        {
            FinalizarPartida();
        }
    }

    private void LimpiarPista()
    {
        for (int i = elementosActivos.Count - 1; i >= 0; i--)
        {
            if (elementosActivos[i].obj != null)
                Destroy(elementosActivos[i].obj);
        }

        elementosActivos.Clear();
    }

    private void ActualizarUI()
    {
        if (textoPuntuacion != null)
        {
            textoPuntuacion.text =
                $"Tiempo: {Mathf.FloorToInt(tiempoSobrevivido)}s";
        }

        if (textoVidas != null)
        {
            textoVidas.text = $"{vidasActuales} / {vidasMaximas}";
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

    private void FinalizarPartida()
    {
        if (!juegoActivo) return;

        juegoActivo = false;

        RestaurarSortingJugador();
        StopAllCoroutines();

        LimpiarPista();

        if (spriteJugador != null)
        {
            Color c = spriteJugador.color;
            c.a = 1f;
            spriteJugador.color = c;
        }

        int puntuacionFinal = Mathf.FloorToInt(tiempoSobrevivido);

        bool esNuevoRecord = false;

        if (EstadoJuegoManager.Instance != null &&
            !string.IsNullOrEmpty(idMinijuego))
        {
            int puntuacionAnterior =
                EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(idMinijuego);

            esNuevoRecord = puntuacionFinal > puntuacionAnterior;

            EstadoJuegoManager.Instance.GuardarPuntuacionMinijuego(
                idMinijuego,
                puntuacionFinal
            );
        }

        if (panelFin != null)
            panelFin.SetActive(true);

        if (textoPuntuacionFinal != null)
        {
            textoPuntuacionFinal.text =
                $"Tiempo sobrevivido: {puntuacionFinal}s";
        }

        if (textoNuevoRecord != null)
        {
            textoNuevoRecord.gameObject.SetActive(esNuevoRecord);

            if (esNuevoRecord)
                textoNuevoRecord.text = "¡Nuevo récord!";
        }

        ReproducirSFXSeguro("Perder");
    }


    public void VolverMenuInfinito()
    {
        if (!string.IsNullOrEmpty(nombreEscenaMenuInfinito) &&
            TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(nombreEscenaMenuInfinito);
        }
    }

}
