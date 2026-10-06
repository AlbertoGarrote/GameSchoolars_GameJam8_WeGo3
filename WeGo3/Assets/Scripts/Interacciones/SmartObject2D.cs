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

    private void Update()
    {
        if (yaFueUsado || !jugadorEnRango) return;

        if (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo) return;

        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
        {
            if (EstaJugadorMirandoAlObjeto())
            {
                StartCoroutine(EjecutarSecuenciaAcciones());
            }
        }
    }

    private bool EstaJugadorMirandoAlObjeto()
    {
        if (jugador == null) return false;

        Vector2 direccionHaciaObjeto = (transform.position - jugador.transform.position).normalized;
        float alineacion = Vector2.Dot(jugador.DireccionMirada, direccionHaciaObjeto);

        return alineacion > 0.5f;
    }

    private IEnumerator EjecutarSecuenciaAcciones()
    {
        if (acciones == null || acciones.Count == 0) yield break;

        ejecutandoAcciones = true;

        if (iconoInteraccion != null)
            iconoInteraccion.SetActive(false);

        if (deUnSoloUso)
            yaFueUsado = true;

        // Ejecuta cada acción de la lista esperando a que la previa termine
        foreach (var accion in acciones)
        {
            if (accion != null)
            {
                bool accionFinalizada = false;

                // Ejecutamos la acción y le pasamos la orden de avisarnos al acabar
                accion.EjecutarAccion(jugador.gameObject, () => accionFinalizada = true);

                // Esperamos aquí mientras la acción esté en proceso (por ej. mientras dure el diálogo)
                yield return new WaitUntil(() => accionFinalizada);
            }
        }

        ejecutandoAcciones = false;
    }
}