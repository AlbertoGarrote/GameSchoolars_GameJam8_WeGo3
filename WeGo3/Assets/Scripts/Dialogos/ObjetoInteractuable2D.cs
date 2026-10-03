using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class ObjetoInteractuable2D : MonoBehaviour
{
    [Header("Datos de Diálogo")]
    [SerializeField] private DialogoSO dialogoObjeto;

    [Header("Indicador Visual (Opcional)")]
    [SerializeField] private GameObject iconoInteraccion; // Ej: Un icono de 'Exclamación' o 'E' encima del objeto

    private bool jugadorEnRango = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            jugadorEnRango = true;
            if (iconoInteraccion != null) iconoInteraccion.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            jugadorEnRango = false;
            if (iconoInteraccion != null) iconoInteraccion.SetActive(false);
        }
    }

    // Para interacción mediante Clic directo sobre el objeto
    private void OnMouseDown()
    {
        LanzarDialogo();
    }

    // Para interacción por proximidad + Botón de acción (Teclado/Mando)
    private void Update()
    {
        if (jugadorEnRango && !DialogoManager.Instance.EnDialogo)
        {
            // Detecta la tecla Espacio o Botón Sur del gamepad si usas Input System
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                LanzarDialogo();
            }
        }
    }

    private void LanzarDialogo()
    {
        if (DialogoManager.Instance != null && !DialogoManager.Instance.EnDialogo && dialogoObjeto != null)
        {
            DialogoManager.Instance.IniciarDialogo(dialogoObjeto);
        }
    }
}