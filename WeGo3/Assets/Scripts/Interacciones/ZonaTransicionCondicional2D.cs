using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZonaTransicionCondicional2D : MonoBehaviour
{
    [Header("Condición de Salida")]
    [Tooltip("Flag que el jugador debe tener activada para poder salir (ej: 'Llave_Recogida'). Si está vacía, pasa siempre.")]
    [SerializeField] private string claveFlagRequerida;
    [Tooltip("Diálogo que salta si el jugador intenta salir sin cumplir la flag.")]
    [SerializeField] private DialogoSO dialogoBloqueo;

    [Header("Escena Destino (Si se cumple la condición)")]
    [SerializeField] private string nombreEscenaACargar;

    [Header("Posición al Aparecer")]
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;

    [Header("Ajustes")]
    [SerializeField] private float tiempoEsperaAlAparecer = 1.5f;

    private bool puedeTransicionar = false;

    private IEnumerator Start()
    {
        puedeTransicionar = false;
        yield return new WaitForSeconds(tiempoEsperaAlAparecer);
        puedeTransicionar = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!puedeTransicionar) return;

        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            // 1. Comprobamos si hay una flag requerida y si NO está activada
            if (!string.IsNullOrEmpty(claveFlagRequerida))
            {
                if (EstadoJuegoManager.Instance == null || !EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagRequerida))
                {
                    // No cumple la condición: Mostramos el diálogo de bloqueo
                    if (dialogoBloqueo != null && DialogoManager.Instance != null)
                    {
                        DialogoManager.Instance.IniciarDialogo(dialogoBloqueo);
                    }
                    else
                    {
                        Debug.Log("¡Tienes que coger la llave antes de salir!");
                    }

                    return; // Detenemos la ejecución aquí, no cambia de escena
                }
            }

            // 2. Si se cumple la condición (o no hay flag requerida), procede la transición normal
            if (string.IsNullOrEmpty(nombreEscenaACargar)) return;

            puedeTransicionar = false;

            if (memoriaPosicion != null)
            {
                memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
                memoriaPosicion.usarPosicionGuardada = true;
            }

            TransicionController.Instance.CargarEscena(nombreEscenaACargar);
        }
    }
}