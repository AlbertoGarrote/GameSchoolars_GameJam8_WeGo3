using UnityEngine;
using UnityEngine.InputSystem;

public enum TipoActivacion
{
    AlPulsarBotonMirando, // Requiere estar cerca, mirando al objeto y pulsar el botón
    AlEntrarEnZona        // Se activa automáticamente al pisar la zona (trampas, triggers)
}

[RequireComponent(typeof(Collider2D))]
public class ObjetoInteractuable2D : MonoBehaviour
{
    [Header("Configuración de Activación")]
    [SerializeField] private TipoActivacion tipoActivacion = TipoActivacion.AlPulsarBotonMirando;
    [SerializeField] private DialogoSO dialogoObjeto;
    [SerializeField] private GameObject iconoInteraccion;

    [Header("Opciones de Un Solo Uso")]
    [SerializeField] private bool desactivarTrasUsar = false;

    private bool jugadorEnRango = false;
    private PlayerMovement2D jugador;
    private bool yaFueActivado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (yaFueActivado) return;

        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            jugadorEnRango = true;
            jugador = collision.GetComponent<PlayerMovement2D>();

            // Si es tipo Trampa / Zona, se activa de inmediato al entrar
            if (tipoActivacion == TipoActivacion.AlEntrarEnZona)
            {
                LanzarDialogo();
            }
            else if (iconoInteraccion != null)
            {
                iconoInteraccion.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            jugadorEnRango = false;
            jugador = null;
            if (iconoInteraccion != null) iconoInteraccion.SetActive(false);
        }
    }

    private void Update()
    {
        if (yaFueActivado || tipoActivacion != TipoActivacion.AlPulsarBotonMirando) return;

        if (jugadorEnRango && !DialogoManager.Instance.EnDialogo)
        {
            // Detectar tecla de interacción (Espacio o E)
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame))
            {
                if (EstaJugadorMirandoAlObjeto())
                {
                    LanzarDialogo();
                }
            }
        }
    }

    private bool EstaJugadorMirandoAlObjeto()
    {
        if (jugador == null) return false;

        // Dirección desde el jugador hacia el objeto
        Vector2 direccionHaciaObjeto = (transform.position - jugador.transform.position).normalized;

        // Comparamos hacia dónde mira el jugador con la dirección donde está el objeto
        float alineacion = Vector2.Dot(jugador.DireccionMirada, direccionHaciaObjeto);

        // Retorna true si el ángulo es menor a ~45 grados (está mirando de frente al objeto)
        return alineacion > 0.5f;
    }

    private void LanzarDialogo()
    {
        if (DialogoManager.Instance != null && !DialogoManager.Instance.EnDialogo && dialogoObjeto != null)
        {
            DialogoManager.Instance.IniciarDialogo(dialogoObjeto);

            if (iconoInteraccion != null) iconoInteraccion.SetActive(false);

            if (desactivarTrasUsar)
            {
                yaFueActivado = true;
            }
        }
    }
}