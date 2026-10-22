using UnityEngine;

public class VerificadorFlagObjeto : MonoBehaviour
{
    [Header("Configuración de Flag")]
    [Tooltip("La clave de la flag a comprobar.")]
    [SerializeField] private string claveFlagRequerida;

    [Header("Modo de Operación")]
    [Tooltip("True: Se desactiva si la flag SÍ está activada.\nFalse: Se desactiva si la flag NO está activada (se muestra solo al cumplirla).")]
    [SerializeField] private bool ocultarSiFlagActivada = true;

    [Header("Referencia Opcional")]
    [Tooltip("Si se deja vacío, se desactiva/activa este mismo GameObject.")]
    [SerializeField] private GameObject objetoAfectado;

    private void Start()
    {
        if (objetoAfectado == null)
        {
            objetoAfectado = gameObject;
        }

        ComprobarEstadoFlag();
    }

    private void ComprobarEstadoFlag()
    {
        if (string.IsNullOrEmpty(claveFlagRequerida)) return;

        bool flagActivada = EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagRequerida);

        // Si se cumple la condición de ocultar, se desactiva; si no, se activa.
        bool debeOcultarse = ocultarSiFlagActivada ? flagActivada : !flagActivada;

        objetoAfectado.SetActive(!debeOcultarse);
    }
}