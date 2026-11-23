using System.Collections;
using TMPro;
using UnityEngine;

public class MadreMinijuego : MonoBehaviour
{
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

    [Header("Configuración Placeholder")]
    [SerializeField] private float duracionMinijuego = 5f;
    [SerializeField] private TMP_Text textoTemporizador;

    private float tiempoRestante;
    private bool juegoActivo = false;

    void Start()
    {
        if (panelInicio != null) panelInicio.SetActive(true);
        if (temporizadorInicial != null) temporizadorInicial.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        tiempoRestante = duracionMinijuego;
        juegoActivo = false;

        ActualizarTextoTiempo();
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
        tiempoRestante = duracionMinijuego;
        juegoActivo = true;
        ActualizarTextoTiempo();
    }

    void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;

        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            ActualizarTextoTiempo();
            GanarMinijuego();
            return;
        }

        ActualizarTextoTiempo();
    }

    private void ActualizarTextoTiempo()
    {
        if (textoTemporizador != null)
        {
            textoTemporizador.text = $"{Mathf.CeilToInt(tiempoRestante)}s";
        }
    }

    private void GanarMinijuego()
    {
        juegoActivo = false;
        StopAllCoroutines();

        if (panelVictoria != null) panelVictoria.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Ganar");
    }

    public void ForzarDerrota()
    {
        if (!juegoActivo) return;

        juegoActivo = false;
        StopAllCoroutines();

        if (panelDerrota != null) panelDerrota.SetActive(true);
        if (MusicSoundController.Instance != null) MusicSoundController.Instance.ReproducirSFX("Perder");
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