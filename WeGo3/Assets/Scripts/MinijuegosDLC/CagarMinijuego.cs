using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CagarMinijuego : MonoBehaviour
{
    [System.Serializable]
    public class VigilanteEntrada
    {
        public string nombre;
        public GameObject objeto;
    }

    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("UI y Paneles de Inicio")]
    [SerializeField] private GameObject panelInicio;
    [SerializeField] private GameObject temporizadorInicial;
    [SerializeField] private TMP_Text textoInicio;
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Objetivo, Puntuación y Tiempo")]
    [SerializeField] private float puntuacionObjetivo = 100f;
    [SerializeField] private float velocidadSumaPorSegundo = 25f;
    [SerializeField] private float puntosRestaAlPillarte = 15f;
    [SerializeField] private float tiempoLimite = 25f;

    [Header("UI de Juego")]
    [SerializeField] private Slider sliderProgreso;
    [SerializeField] private TMP_Text textoPuntuacion;
    [SerializeField] private TMP_Text textoTiempo;
    [SerializeField] private TMP_Text textoAviso;

    [Header("Puerta y Recorrido Común")]
    [SerializeField] private GameObject puertaCerrada;
    [SerializeField] private GameObject puertaAbierta;
    [Tooltip("Pequeña pausa tras abrir la puerta antes de que empiece a moverse el objeto.")]
    [SerializeField] private float retrasoTrasAbrirPuerta = 0.2f;
    [SerializeField] private Transform puntoInicial;
    [SerializeField] private Transform puntoAsomarse;
    [Tooltip("Velocidad normal al asomarse y volver para todos los objetos.")]
    [SerializeField] private float velocidadMovimiento = 3f;
    [Tooltip("Multiplicador de velocidad al regresar si te pilla pulsando Espacio.")]
    [SerializeField] private float multiplicadorVelocidadRetorno = 2.5f;
    [Tooltip("Segundos que permanece asomado antes de volver.")]
    [SerializeField] private float tiempoEsperaAsomado = 1f;

    [Header("Sonidos")]
    [Tooltip("Cada cuántos segundos se vuelve a disparar el SFX \"Apretar\" mientras mantienes Espacio.")]
    [SerializeField] private float cadenciaSonidoApretar = 0.25f;

    [Header("Objetos que Se Asoman")]
    [SerializeField] private List<VigilanteEntrada> objetosVigilantes = new List<VigilanteEntrada>();
    [Tooltip("Tiempo mínimo (en segundos) entre una aparición y la siguiente.")]
    [SerializeField] private float intervaloMinimoAparicion = 1.5f;
    [Tooltip("Tiempo máximo (en segundos) entre una aparición y la siguiente.")]
    [SerializeField] private float intervaloMaximoAparicion = 3.5f;

    private float puntuacionActual = 0f;
    private float tiempoRestante = 0f;
    private bool juegoActivo = false;

    private bool manteniendoEspacio = false;
    private bool bloqueadoPorPillon = false;
    private float temporizadorSonidoApretar = 0f;

    private int objetosAsomadosActivos = 0;
    private readonly Dictionary<GameObject, Coroutine> rutinasObjetos = new Dictionary<GameObject, Coroutine>();

    private void OnValidate()
    {
        if (intervaloMinimoAparicion < 0f) intervaloMinimoAparicion = 0f;
        if (intervaloMaximoAparicion < intervaloMinimoAparicion)
        {
            intervaloMaximoAparicion = intervaloMinimoAparicion;
        }
        if (cadenciaSonidoApretar < 0.05f) cadenciaSonidoApretar = 0.05f;
    }

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        puntuacionActual = 0f;
        tiempoRestante = tiempoLimite;
        juegoActivo = false;

        DetenerSonidoApretar();
        EstablecerPuertaAbierta(false);
        PrepararObjetosIniciales();
        ActualizarUI();
    }

    private void EstablecerPuertaAbierta(bool abierta)
    {
        bool estabaAbierta = puertaAbierta != null && puertaAbierta.activeSelf;

        if (puertaCerrada != null) puertaCerrada.SetActive(!abierta);
        if (puertaAbierta != null) puertaAbierta.SetActive(abierta);

        if (abierta && !estabaAbierta && MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Abrir");
        }
    }

    private void GestionarSonidoApretar(bool debeSonar)
    {
        if (!debeSonar)
        {
            DetenerSonidoApretar();
            return;
        }

        temporizadorSonidoApretar -= Time.deltaTime;
        if (temporizadorSonidoApretar <= 0f)
        {
            if (MusicSoundController.Instance != null)
            {
                MusicSoundController.Instance.ReproducirSFX("Apretar");
            }
            temporizadorSonidoApretar = cadenciaSonidoApretar;
        }
    }

    private void DetenerSonidoApretar()
    {
        temporizadorSonidoApretar = 0f;
        // Si tu MusicSoundController tiene un método para detener un SFX en bucle, puedes llamarlo aquí:
        // if (MusicSoundController.Instance != null) MusicSoundController.Instance.DetenerSFX("Apretar");
    }

    private void PrepararObjetosIniciales()
    {
        foreach (var item in objetosVigilantes)
        {
            if (item == null || item.objeto == null) continue;

            if (puntoInicial != null)
            {
                item.objeto.transform.position = puntoInicial.position;
            }
            item.objeto.SetActive(false);
        }
        objetosAsomadosActivos = 0;
        EstablecerPuertaAbierta(false);
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
        DetenerRutinasObjetos();
        PrepararObjetosIniciales();

        puntuacionActual = 0f;
        tiempoRestante = tiempoLimite;
        manteniendoEspacio = false;
        bloqueadoPorPillon = false;
        juegoActivo = true;

        DetenerSonidoApretar();
        ActualizarUI();

        StartCoroutine(RutinaAparicionObjetos());
    }

    void Update()
    {
        if (!juegoActivo)
        {
            DetenerSonidoApretar();
            return;
        }

        // 1. Temporizador general del minijuego
        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            DetenerSonidoApretar();
            ActualizarUI();
            PerderBatalla();
            return;
        }

        // 2. Leer si se mantiene pulsada la barra espaciadora
        manteniendoEspacio = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        // Si el jugador suelta Espacio tras haber sido pillado y ya no hay nadie asomado, puede volver a pulsar
        if (!manteniendoEspacio && bloqueadoPorPillon && objetosAsomadosActivos <= 0)
        {
            bloqueadoPorPillon = false;
        }

        bool puedeSumarYSonar = manteniendoEspacio && !bloqueadoPorPillon;
        GestionarSonidoApretar(puedeSumarYSonar);

        // 3. Sumar puntuación mientras mantenga Espacio y no esté bloqueado
        if (puedeSumarYSonar)
        {
            puntuacionActual += velocidadSumaPorSegundo * Time.deltaTime;
            puntuacionActual = Mathf.Clamp(puntuacionActual, 0f, puntuacionObjetivo);

            if (puntuacionActual >= puntuacionObjetivo)
            {
                DetenerSonidoApretar();
                ActualizarUI();
                GanarBatalla();
                return;
            }
        }

        ActualizarUI();
    }

    private IEnumerator RutinaAparicionObjetos()
    {
        while (juegoActivo)
        {
            float minTiempo = Mathf.Max(0f, intervaloMinimoAparicion);
            float maxTiempo = Mathf.Max(minTiempo, intervaloMaximoAparicion);
            float espera = Random.Range(minTiempo, maxTiempo);
            yield return new WaitForSeconds(espera);

            if (!juegoActivo || objetosVigilantes.Count == 0) continue;

            List<VigilanteEntrada> disponibles = new List<VigilanteEntrada>();
            foreach (var candidato in objetosVigilantes)
            {
                if (candidato != null && candidato.objeto != null && !rutinasObjetos.ContainsKey(candidato.objeto))
                {
                    disponibles.Add(candidato);
                }
            }

            if (disponibles.Count > 0)
            {
                VigilanteEntrada elegido = disponibles[Random.Range(0, disponibles.Count)];
                Coroutine rutina = StartCoroutine(SecuenciaAsomarseYVolver(elegido.objeto));
                rutinasObjetos[elegido.objeto] = rutina;
            }
        }
    }

    private IEnumerator SecuenciaAsomarseYVolver(GameObject objetoVigilante)
    {
        if (objetoVigilante == null || puntoInicial == null || puntoAsomarse == null)
        {
            yield break;
        }

        objetosAsomadosActivos++;
        EstablecerPuertaAbierta(true);

        if (retrasoTrasAbrirPuerta > 0f)
        {
            yield return new WaitForSeconds(retrasoTrasAbrirPuerta);
            if (!juegoActivo) yield break;
        }

        objetoVigilante.transform.position = puntoInicial.position;
        objetoVigilante.SetActive(true);

        bool pillado = false;

        // 1. Moverse desde el punto inicial común hasta el punto de asomarse común
        while (Vector3.Distance(objetoVigilante.transform.position, puntoAsomarse.position) > 0.01f)
        {
            if (!juegoActivo) yield break;

            objetoVigilante.transform.position = Vector3.MoveTowards(
                objetoVigilante.transform.position,
                puntoAsomarse.position,
                velocidadMovimiento * Time.deltaTime
            );

            if (manteniendoEspacio && !pillado)
            {
                pillado = true;
                AplicarPenalizacionPorSerPillado();
                break;
            }

            yield return null;
        }

        // 2. Permanecer asomado un segundo (o lo configurado) si aún no te ha pillado
        if (!pillado)
        {
            float tiempoEspera = 0f;
            while (tiempoEspera < tiempoEsperaAsomado)
            {
                if (!juegoActivo) yield break;

                if (manteniendoEspacio)
                {
                    pillado = true;
                    AplicarPenalizacionPorSerPillado();
                    break;
                }

                tiempoEspera += Time.deltaTime;
                yield return null;
            }
        }

        // 3. Volver al punto inicial común (más rápido si te pilló pulsando Espacio)
        float velocidadRetorno = pillado
            ? velocidadMovimiento * Mathf.Max(1f, multiplicadorVelocidadRetorno)
            : velocidadMovimiento;

        while (Vector3.Distance(objetoVigilante.transform.position, puntoInicial.position) > 0.01f)
        {
            if (!juegoActivo) yield break;

            objetoVigilante.transform.position = Vector3.MoveTowards(
                objetoVigilante.transform.position,
                puntoInicial.position,
                velocidadRetorno * Time.deltaTime
            );

            yield return null;
        }

        objetoVigilante.transform.position = puntoInicial.position;
        objetoVigilante.SetActive(false);

        objetosAsomadosActivos = Mathf.Max(0, objetosAsomadosActivos - 1);
        rutinasObjetos.Remove(objetoVigilante);

        if (objetosAsomadosActivos <= 0)
        {
            EstablecerPuertaAbierta(false);
            if (!manteniendoEspacio)
            {
                bloqueadoPorPillon = false;
            }
        }
    }

    private void AplicarPenalizacionPorSerPillado()
    {
        puntuacionActual = Mathf.Max(0f, puntuacionActual - puntosRestaAlPillarte);
        bloqueadoPorPillon = true;
        DetenerSonidoApretar();

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Fallo");
        }

        ActualizarUI();
    }

    private void DetenerRutinasObjetos()
    {
        foreach (var par in rutinasObjetos)
        {
            if (par.Value != null)
            {
                StopCoroutine(par.Value);
            }
        }
        rutinasObjetos.Clear();
        objetosAsomadosActivos = 0;
        EstablecerPuertaAbierta(false);
    }

    private void ActualizarUI()
    {
        if (sliderProgreso != null)
        {
            sliderProgreso.minValue = 0f;
            sliderProgreso.maxValue = puntuacionObjetivo;
            sliderProgreso.value = puntuacionActual;
        }

        if (textoPuntuacion != null)
        {
            textoPuntuacion.text = $"{Mathf.FloorToInt(puntuacionActual)} / {Mathf.FloorToInt(puntuacionObjetivo)}";
        }

        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }

        if (textoAviso != null)
        {
            if (bloqueadoPorPillon)
                textoAviso.text = "¡TE PILLARON! Suelta ESPACIO y espera...";
            else if (objetosAsomadosActivos > 0)
                textoAviso.text = "¡CUIDADO! Se asoma...";
            else
                textoAviso.text = "Mantén ESPACIO para llenar la barra";
        }
    }

    private void GanarBatalla()
    {
        juegoActivo = false;
        DetenerSonidoApretar();
        StopAllCoroutines();
        DetenerRutinasObjetos();
        PrepararObjetosIniciales();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Ganar");
    }

    private void PerderBatalla()
    {
        juegoActivo = false;
        DetenerSonidoApretar();
        StopAllCoroutines();
        DetenerRutinasObjetos();
        PrepararObjetosIniciales();

        if (panelDerrota != null) panelDerrota.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Perder");
    }

    public void volverGym(bool victoriaConseguida)
    {
        DetenerSonidoApretar();

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