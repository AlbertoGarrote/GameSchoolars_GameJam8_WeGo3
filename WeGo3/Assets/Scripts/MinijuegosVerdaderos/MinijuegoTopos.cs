using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System;

public class MinijuegoTopos : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private String flagVictoria;
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
    [SerializeField] private int puntosObjetivo = 15;
    [SerializeField] private float tiempoTotalBatalla = 30f;
    [SerializeField] private float tiempoEnPantallaTopo = 1.2f;
    [SerializeField] private float intervaloEntreTopos = 0.5f;

    [Header("Agujeros y Prefabs")]
    [SerializeField] private Transform[] agujeros;
    [SerializeField] private GameObject prefabTopoBueno;
    [SerializeField] private GameObject prefabTopoMalo;
    [Range(0f, 1f)]
    [SerializeField] private float probabilidadTopoMalo = 0.3f;

    [Header("UI de Progreso")]
    [SerializeField] private Slider sliderPuntos;
    [SerializeField] private TextMeshProUGUI textoPuntos;
    [SerializeField] private TextMeshProUGUI textoTiempo;

    private int puntosActuales = 0;
    private float tiempoRestante;
    private bool juegoActivo = false;

    // Array para controlar qué agujeros están ocupados actualmente
    private bool[] agujerosOcupados;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        if (agujeros != null)
        {
            agujerosOcupados = new bool[agujeros.Length];
        }

        puntosActuales = 0;
        tiempoRestante = tiempoTotalBatalla;
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
        tiempoRestante = tiempoTotalBatalla;
        ActualizarUI();

        StartCoroutine(RutinaSpawnTopos());
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
                GanarBatalla();
            else
                PerderBatalla();
        }
    }

    private IEnumerator RutinaSpawnTopos()
    {
        while (juegoActivo)
        {
            if (agujeros != null && agujeros.Length > 0)
            {
                // Buscamos un agujero libre de forma segura
                int indiceAgujero = ObtenerAgujeroLibreAleatorio();

                if (indiceAgujero != -1) // Si hay al menos un agujero libre
                {
                    agujerosOcupados[indiceAgujero] = true; // Lo marcamos como ocupado

                    bool esMalo = UnityEngine.Random.value < probabilidadTopoMalo;
                    GameObject prefabAUsar = esMalo ? prefabTopoMalo : prefabTopoBueno;

                    if (prefabAUsar != null)
                    {
                        GameObject topoInstanciado = Instantiate(prefabAUsar, agujeros[indiceAgujero].position, Quaternion.identity, agujeros[indiceAgujero]);

                        ElementoTopo scriptTopo = topoInstanciado.GetComponent<ElementoTopo>();
                        if (scriptTopo == null) scriptTopo = topoInstanciado.AddComponent<ElementoTopo>();

                        // Le pasamos el índice para que al morir libere su agujero
                        scriptTopo.Inicializar(esMalo, tiempoEnPantallaTopo, this, indiceAgujero);
                    }
                }
            }

            yield return new WaitForSeconds(intervaloEntreTopos);
        }
    }

    private int ObtenerAgujeroLibreAleatorio()
    {
        // Comprobamos si quedan agujeros libres
        List<int> indicesLibres = new List<int>();
        for (int i = 0; i < agujeros.Length; i++)
        {
            if (!agujerosOcupados[i])
            {
                indicesLibres.Add(i);
            }
        }

        if (indicesLibres.Count == 0) return -1; // Todos ocupados

        // Elegimos uno al azar de los libres
        int randomIndex = UnityEngine.Random.Range(0, indicesLibres.Count);
        return indicesLibres[randomIndex];
    }

    public void LiberarAgujero(int indice)
    {
        if (agujerosOcupados != null && indice >= 0 && indice < agujerosOcupados.Length)
        {
            agujerosOcupados[indice] = false;
        }
    }

    public void RegistrarGolpe(bool esMalo)
    {
        if (!juegoActivo) return;

        if (!esMalo)
        {
            puntosActuales++;
            MusicSoundController.Instance.ReproducirSFX("Acierto");
        }
        else
        {
            puntosActuales -= 2;
            if (puntosActuales < 0) puntosActuales = 0;
            MusicSoundController.Instance.ReproducirSFX("Fallo");
        }

        ActualizarUI();
        ComprobarCondicionVictoria();
    }

    public void RegistrarTopoExpirado(bool esMalo)
    {
        if (!juegoActivo) return;

        if (!esMalo)
        {
            puntosActuales--;
            if (puntosActuales < 0) puntosActuales = 0;
            ActualizarUI();
        }
    }

    private void ComprobarCondicionVictoria()
    {
        if (puntosActuales >= puntosObjetivo)
        {
            GanarBatalla();
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
            textoPuntos.text = $"Puntos: {puntosActuales} / {puntosObjetivo}";
        }
        if (textoTiempo != null)
        {
            textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}s";
        }
    }

    private void GanarBatalla()
    {
        juegoActivo = false;
        StopAllCoroutines();
        if (panelVictoria != null) panelVictoria.SetActive(true);
        MusicSoundController.Instance.ReproducirSFX("Ganar");
    }

    private void PerderBatalla()
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