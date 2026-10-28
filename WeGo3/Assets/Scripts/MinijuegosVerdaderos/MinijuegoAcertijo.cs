using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinijuegoAcertijo : MonoBehaviour
{
    [Header("Referencias de Retorno")]
    [SerializeField] private string flagVictoria = "Patos_ganado";
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string nombreEscenaGym = "Gym";

    [Header("Paneles de Victoria y Derrota")]
    [SerializeField] private GameObject panelVictoria;
    [SerializeField] private GameObject panelDerrota;

    [Header("Cinemática de Victoria (4 Paneles)")]
    [SerializeField] private List<GameObject> panelesCinematicaVictoria = new List<GameObject>();
    [SerializeField] private float tiempoPorPanelVictoria = 2f;

    [Header("Cinemática de Derrota (4 Paneles)")]
    [SerializeField] private List<GameObject> panelesCinematicaDerrota = new List<GameObject>();
    [SerializeField] private float tiempoPorPanelDerrota = 2f;

    [Header("Configuración del Fundido a Negro (UI)")]
    [SerializeField] private Image imagenFundidoNegro;       // Una Image negra a pantalla completa para los fundidos
    [SerializeField] private float duracionFundido = 0.5f;   // Lo que tarda en oscurecer/aclarar

    [Header("Configuración del Acertijo")]
    [SerializeField] private int numeroRespuestaCorrecta = 8;
    private int numeroSeleccionado = 0;
    private bool juegoActivo = false;

    [Header("UI de Selección de Número")]
    [SerializeField] private TextMeshProUGUI textoNumeroSeleccionado;

    void Start()
    {
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        OcultarListaPaneles(panelesCinematicaVictoria);
        OcultarListaPaneles(panelesCinematicaDerrota);

        // Asegurarnos de que el panel de fundido empieza transparente
        if (imagenFundidoNegro != null)
        {
            Color color = imagenFundidoNegro.color;
            color.a = 0f;
            imagenFundidoNegro.color = color;
            imagenFundidoNegro.gameObject.SetActive(false);
        }

        numeroSeleccionado = 0;
        ActualizarTextoUI();
        juegoActivo = true;
    }

    void Update()
    {
        if (!juegoActivo) return;

        ComprobarSeleccionTeclado();
        ComprobarConfirmacion();
    }

    private void ComprobarSeleccionTeclado()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            numeroSeleccionado++;
            MusicSoundController.Instance.ReproducirSFX("Interactuar");
            ActualizarTextoUI();
        }
        else if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            if (numeroSeleccionado > 0)
            {
                numeroSeleccionado--;
                MusicSoundController.Instance.ReproducirSFX("Interactuar");
                ActualizarTextoUI();
            }
        }
    }

    private void ComprobarConfirmacion()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ConfirmarRespuesta();
        }
    }

    private void ConfirmarRespuesta()
    {
        juegoActivo = false;
        MusicSoundController.Instance.ReproducirSFX("Interactuar");

        if (numeroSeleccionado == numeroRespuestaCorrecta)
        {
            StartCoroutine(SecuenciaCinematicaConFundido(panelesCinematicaVictoria, panelVictoria, tiempoPorPanelVictoria, true));
        }
        else
        {
            StartCoroutine(SecuenciaCinematicaConFundido(panelesCinematicaDerrota, panelDerrota, tiempoPorPanelDerrota, false));
        }
    }

    /// <summary>
    /// Corrutina que recorre los paneles aplicando un fundido a negro por código entre cada cambio.
    /// </summary>
    private IEnumerator SecuenciaCinematicaConFundido(List<GameObject> listaPaneles, GameObject panelFinal, float tiempoEspera, bool esVictoria)
    {
        OcultarListaPaneles(panelesCinematicaVictoria);
        OcultarListaPaneles(panelesCinematicaDerrota);

        MusicSoundController.Instance.DetenerMusica();

        for (int i = 0; i < listaPaneles.Count; i++)
        {
            if (listaPaneles[i] != null)
            {
                // 1. Mostrar el panel actual
                listaPaneles[i].SetActive(true);

                // Si no es el primer panel, hacemos un fundido desde negro para revelarlo
                if (i > 0)
                {
                    yield return EjectuarFundido(1f, 0f); // De negro a transparente
                }

                if (i == listaPaneles.Count - 1)
                {
                    if (esVictoria)
                    {
                        MusicSoundController.Instance.ReproducirSFX("Acierto");
                    }
                    else
                    {
                        MusicSoundController.Instance.ReproducirSFX("Fallo");
                    }
                }
                else
                {
                    MusicSoundController.Instance.ReproducirSFX("Duda");

                }


                // 2. Esperar el tiempo configurado viendo el panel
                yield return new WaitForSeconds(tiempoEspera);

                // 3. Hacer fundido a negro antes de cambiar al siguiente o al final
                yield return EjectuarFundido(0f, 1f); // De transparente a negro
                listaPaneles[i].SetActive(false);     // Ocultar el panel a oscuras
            }
        }

        if (esVictoria)
            MusicSoundController.Instance.ReproducirSFX("Ganar");
        else
            MusicSoundController.Instance.ReproducirSFX("Perder");

        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
        }
    }

    /// <summary>
    /// Corrutina auxiliar que modifica el canal Alfa (opacidad) de la imagen negra en tiempo de ejecución.
    /// </summary>
    private IEnumerator EjectuarFundido(float alfaInicial, float alfaFinal)
    {
        if (imagenFundidoNegro == null) yield break;

        imagenFundidoNegro.gameObject.SetActive(true);
        Color color = imagenFundidoNegro.color;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracionFundido)
        {
            tiempoTranscurrido += Time.deltaTime;
            float alfaActual = Mathf.Lerp(alfaInicial, alfaFinal, tiempoTranscurrido / duracionFundido);
            color.a = alfaActual;
            imagenFundidoNegro.color = color;
            yield return null;
        }

        color.a = alfaFinal;
        imagenFundidoNegro.color = color;
    }

    private void OcultarListaPaneles(List<GameObject> lista)
    {
        foreach (var panel in lista)
        {
            if (panel != null) panel.SetActive(false);
        }
    }

    private void ActualizarTextoUI()
    {
        if (textoNumeroSeleccionado != null)
        {
            textoNumeroSeleccionado.text = numeroSeleccionado.ToString();
        }
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