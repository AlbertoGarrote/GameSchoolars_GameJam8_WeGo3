using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SmartObjectOnTrigger2D : MonoBehaviour
{
    [Header("Condición y Registro")]
    [Tooltip("Flag que debe estar activada para que se dispare la secuencia. Si se deja vacía, se disparará siempre.")]
    [SerializeField] private string claveFlagRequerida;

    [Tooltip("Flag que se activará automáticamente (solo si NO se repite siempre).")]
    [SerializeField] private string claveFlagAActivarAlSaltar;

    [Header("Configuración de Repetición")]
    [Tooltip("Si se marca, la secuencia se ejecutará cada vez que se vuelva a entrar al trigger.")]
    [SerializeField] private bool repetirSiempre = false;

    [Tooltip("Si se marca, bloquea el trigger de forma permanente tras ejecutarse (útil si usas un solo uso).")]
    [SerializeField] private bool deUnSoloUso = false;

    [Header("Detección")]
    [Tooltip("Si se marca, también permite que un objeto con tag 'Caminante' active el trigger.")]
    [SerializeField] private bool permitirCaminante = true;

    [Header("Secuencia de Acciones")]
    [SerializeField] private List<AccionSmartObject> acciones = new List<AccionSmartObject>();

    private bool yaSeEjecutoEnEstaSesion = false;
    private bool ejecutandoAcciones = false;

    private void OnDisable()
    {
        if (ejecutandoAcciones)
        {
            ejecutandoAcciones = false;
            SmartObject2D.HayAccionEjecutandoseGlobal = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ejecutandoAcciones) return;

        bool esJugador = collision.CompareTag("Player");
        bool esCaminante = permitirCaminante && collision.CompareTag("Caminante");
        if (!esJugador && !esCaminante) return;

        // 1. Comprobación de repetición y flags de activación previa
        if (!repetirSiempre)
        {
            if (yaSeEjecutoEnEstaSesion || (deUnSoloUso && yaSeEjecutoEnEstaSesion)) return;

            if (!string.IsNullOrEmpty(claveFlagAActivarAlSaltar))
            {
                if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagAActivarAlSaltar))
                {
                    return;
                }
            }
        }

        // 2. Comprobación de flag requerida (si la hay)
        if (!string.IsNullOrEmpty(claveFlagRequerida))
        {
            if (EstadoJuegoManager.Instance == null || !EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagRequerida))
            {
                return;
            }
        }

        if (acciones == null || acciones.Count == 0) return;

        // 3. Registrar ejecución y activar flag si corresponde
        if (!repetirSiempre)
        {
            yaSeEjecutoEnEstaSesion = true;

            if (!string.IsNullOrEmpty(claveFlagAActivarAlSaltar) && EstadoJuegoManager.Instance != null)
            {
                EstadoJuegoManager.Instance.ActivarFlag(claveFlagAActivarAlSaltar);
            }
        }

        GameObject objetivo = esJugador ? collision.gameObject : null;
        if (objetivo == null)
        {
            PlayerMovement2D player = FindFirstObjectByType<PlayerMovement2D>();
            if (player != null) objetivo = player.gameObject;
        }

        StartCoroutine(EjecutarSecuenciaAcciones(objetivo));
    }

    private IEnumerator EjecutarSecuenciaAcciones(GameObject objetivo)
    {
        ejecutandoAcciones = true;
        SmartObject2D.HayAccionEjecutandoseGlobal = true;

        foreach (var accion in acciones)
        {
            if (accion != null)
            {
                bool accionFinalizada = false;
                accion.EjecutarAccion(objetivo, () => accionFinalizada = true);
                yield return new WaitUntil(() => accionFinalizada);

                // Por si una acción lanza un diálogo y llama al callback antes de que EnDialogo pase a false
                while (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
                {
                    yield return null;
                }
            }
        }

        ejecutandoAcciones = false;
        SmartObject2D.HayAccionEjecutandoseGlobal = false;
    }
}