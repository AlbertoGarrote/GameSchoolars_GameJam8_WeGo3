using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogoOnTrigger : MonoBehaviour
{
    [Header("Condición y Registro")]
    [Tooltip("Flag que debe estar activada para que salte el diálogo. Si se deja vacía, saltará siempre.")]
    [SerializeField] private string claveFlagRequerida;

    [Tooltip("Flag que se activará automáticamente (solo si NO se repite siempre).")]
    [SerializeField] private string claveFlagAActivarAlSaltar;

    [Header("Configuración de Repetición")]
    [Tooltip("Si se marca, el diálogo se repetirá cada vez que se vuelva a entrar al trigger.")]
    [SerializeField] private bool repetirSiempre = false;

    [Header("Diálogo")]
    [SerializeField] private DialogoSO dialogoAMostrar;

    private bool yaSeEjecutoEnEstaSesion = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") && !collision.CompareTag("Caminante")) return;

        // 1. Si NO se repite siempre, comprobamos si ya se ejecutó o si la flag de guardado ya está activa
        if (!repetirSiempre)
        {
            if (yaSeEjecutoEnEstaSesion) return;

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

        // 3. Ejecución del diálogo
        if (dialogoAMostrar != null && DialogoManager.Instance != null)
        {
            if (!repetirSiempre)
            {
                yaSeEjecutoEnEstaSesion = true;

                if (!string.IsNullOrEmpty(claveFlagAActivarAlSaltar) && EstadoJuegoManager.Instance != null)
                {
                    EstadoJuegoManager.Instance.ActivarFlag(claveFlagAActivarAlSaltar);
                }
            }

            DialogoManager.Instance.IniciarDialogo(dialogoAMostrar);
        }
    }
}