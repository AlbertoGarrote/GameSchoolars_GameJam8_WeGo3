using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class SmartObject2D : MonoBehaviour
{
    [Header("UI Feedback")]
    [SerializeField] private GameObject iconoInteraccion;

    [Header("Acciones a Ejecutar")]
    [SerializeField] private List<AccionSmartObject> acciones;

    [Header("Opciones")]
    [SerializeField] private bool deUnSoloUso = false;

    public static bool HayAccionEjecutandoseGlobal { get; set; } = false;

    private bool jugadorEnRango = false;
    private PlayerMovement2D jugador;
    private bool yaFueUsado = false;
    private bool ejecutandoAcciones = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (yaFueUsado) return;

        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = true;
            jugador = collision.GetComponent<PlayerMovement2D>();

            if (iconoInteraccion != null)
                iconoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            jugadorEnRango = false;
            jugador = null;

            if (iconoInteraccion != null)
                iconoInteraccion.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (ejecutandoAcciones)
        {
            ejecutandoAcciones = false;
            HayAccionEjecutandoseGlobal = false;
        }
    }

    private void Update()
    {
        if (yaFueUsado || !jugadorEnRango || ejecutandoAcciones) return;

        if (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo) return;

        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
        {
            MusicSoundController.Instance.ReproducirSFX("Interactuar");
            StartCoroutine(EjecutarSecuenciaAcciones());
        }
    }

    private IEnumerator EjecutarSecuenciaAcciones()
    {
        if (acciones == null || acciones.Count == 0) yield break;

        ejecutandoAcciones = true;
        HayAccionEjecutandoseGlobal = true; // <--- Bloquea globalmente el movimiento

        if (iconoInteraccion != null)
            iconoInteraccion.SetActive(false);

        if (deUnSoloUso)
            yaFueUsado = true;

        foreach (var accion in acciones)
        {
            if (accion != null)
            {
                bool accionFinalizada = false;
                accion.EjecutarAccion(jugador.gameObject, () => accionFinalizada = true);
                yield return new WaitUntil(() => accionFinalizada);
            }
        }

        ejecutandoAcciones = false;
        HayAccionEjecutandoseGlobal = false; // <--- Libera el movimiento al terminar
    }
}