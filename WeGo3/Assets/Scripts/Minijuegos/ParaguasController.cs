using UnityEngine;
using UnityEngine.InputSystem; 

public class ParaguasController : MonoBehaviour
{
    private Camera camaraPrincipal;

    private void Awake()
    {
        camaraPrincipal = Camera.main;
    }

    private void Update()
    {
        // Verificar que el ratón o puntero táctil exista mediante el nuevo Input System
        if (Pointer.current != null)
        {
            // Lee la posición en pantalla usando el nuevo sistema
            Vector2 posicionPantalla = Pointer.current.position.ReadValue();

            // Convierte la posición de la pantalla al mundo 2D
            Vector3 posicionMundo = camaraPrincipal.ScreenToWorldPoint(posicionPantalla);
            posicionMundo.z = 0f; // Mantener la profundidad en plano 2D

            transform.position = posicionMundo;
        }
    }
}