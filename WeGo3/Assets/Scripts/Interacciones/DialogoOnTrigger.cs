using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogoOnTrigger : MonoBehaviour
{
    [Header("Condición y Registro")]
    [Tooltip("Flag que debe estar activada para que salte el diálogo. Si se deja vacía, saltará siempre la primera vez.")]
    [SerializeField] private string claveFlagRequerida;

    [Tooltip("Flag que se activará automáticamente al reproducir este diálogo para que NO vuelva a saltar más veces.")]
    [SerializeField] private string claveFlagAActivarAlSaltar;

    [Header("Diálogo")]
    [SerializeField] private DialogoSO dialogoAMostrar;

    private bool yaSeEjecutoEnEstaSesion = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.CompareTag("Caminante")) return;

        if (yaSeEjecutoEnEstaSesion) return;

        if (!string.IsNullOrEmpty(claveFlagRequerida))
        {
            if (EstadoJuegoManager.Instance == null || !EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagRequerida))
            {
                return; 
            }
        }

        if (!string.IsNullOrEmpty(claveFlagAActivarAlSaltar))
        {
            if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagAActivarAlSaltar))
            {
                return;
            }
        }

        if (dialogoAMostrar != null && DialogoManager.Instance != null)
        {
            yaSeEjecutoEnEstaSesion = true;

            if (!string.IsNullOrEmpty(claveFlagAActivarAlSaltar) && EstadoJuegoManager.Instance != null)
            {
                EstadoJuegoManager.Instance.ActivarFlag(claveFlagAActivarAlSaltar);
            }

            DialogoManager.Instance.IniciarDialogo(dialogoAMostrar);
        }
    }
}
