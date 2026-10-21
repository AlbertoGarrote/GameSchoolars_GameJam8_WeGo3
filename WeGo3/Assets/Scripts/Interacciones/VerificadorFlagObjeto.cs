using UnityEngine;

public class VerificadorFlagObjeto : MonoBehaviour
{
    [Header("Configuración de Flag")]
    [Tooltip("La clave de la flag que indica si el objeto ya fue recogido (ej: 'Llave_Casa_Cogida').")]
    [SerializeField] private string claveFlagRequerida;

    [Header("Comportamiento")]
    [Tooltip("Si es true, se destruye. Si es false, simplemente se desactiva (SetActive(false)).")]
    [SerializeField] private bool destruirSiActivada = true;
    [SerializeField] private bool Aparecer = false;

    private void Start()
    {
        ComprobarEstadoFlag();
    }

    private void ComprobarEstadoFlag()
    {
        if (string.IsNullOrEmpty(claveFlagRequerida)) return;

        // Comprobamos si el EstadoJuegoManager existe y si la flag ya está activada
        if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagRequerida))
        {
            if (destruirSiActivada)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(Aparecer);
            }
        }
    }
}