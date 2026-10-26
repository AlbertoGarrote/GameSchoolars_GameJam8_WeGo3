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

    [Header("Configuración del Acertijo")]
    [SerializeField] private int numeroRespuestaCorrecta = 8;
    private int numeroSeleccionado = 0;
    private bool juegoActivo = false;

    [Header("UI de Selección de Número")]
    [SerializeField] private TextMeshProUGUI textoNumeroSeleccionado; // El texto que muestra el número actual (empezando en 0)

    void Start()
    {
        // Ocultamos los paneles de fin de partida al arrancar
        if (panelVictoria != null) panelVictoria.SetActive(false);
        if (panelDerrota != null) panelDerrota.SetActive(false);

        numeroSeleccionado = 0;
        ActualizarTextoUI();

        // El juego comienza directamente sin esperas ni cuentas atrás
        juegoActivo = true;
    }

    void Update()
    {
        if (!juegoActivo) return;

        // 1. CAMBIAR NÚMERO CON W / S o Flechas Arriba / Abajo
        ComprobarSeleccionTeclado();

        // 2. CONFIRMAR CON ESPACIO
        ComprobarConfirmacion();
    }

    private void ComprobarSeleccionTeclado()
    {
        if (Keyboard.current == null) return;

        // Subir número (W o Flecha Arriba)
        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            numeroSeleccionado++;
            MusicSoundController.Instance.ReproducirSFX("Interactuar");
            ActualizarTextoUI();
        }
        // Bajar número (S o Flecha Abajo) - Evitamos que baje de 0
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
            GanarPartida();
        }
        else
        {
            PerderPartida();
        }
    }

    private void ActualizarTextoUI()
    {
        if (textoNumeroSeleccionado != null)
        {
            textoNumeroSeleccionado.text = numeroSeleccionado.ToString();
        }
    }

    private void GanarPartida()
    {
        if (panelVictoria != null) panelVictoria.SetActive(true);
        MusicSoundController.Instance.ReproducirSFX("Ganar");

    }

    private void PerderPartida()
    {
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