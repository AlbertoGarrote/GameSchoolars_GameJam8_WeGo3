using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(BoxCollider2D))]
public class ActivadorCamaraZona : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera camaraFija;

    [Header("Movimiento Automático")]
    [SerializeField] private Transform puntoDestino;
    [SerializeField] private float velocidadMovimiento = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 1. Cambiar la prioridad de la cámara para que enfoque la zona
            if (camaraFija != null)
            {
                camaraFija.Priority = 20;
            }

            // 2. Mover automáticamente al personaje si se ha asignado un destino
            if (puntoDestino != null)
            {
                StartCoroutine(RutinaMoverYBloquearJugador(collision.gameObject));
            }
        }
    }

    private IEnumerator RutinaMoverYBloquearJugador(GameObject jugador)
    {
        Rigidbody2D rb = jugador.GetComponent<Rigidbody2D>();
        Animator animator = jugador.GetComponentInChildren<Animator>();

        // Desactivamos el script de movimiento manual desde el principio
        MonoBehaviour scriptMovimiento = jugador.GetComponent("PlayerMovement2D") as MonoBehaviour;
        if (scriptMovimiento != null) scriptMovimiento.enabled = false;

        Vector2 destino = puntoDestino.position;

        // Bucle de movimiento continuo hacia el destino
        while (rb != null && Vector2.Distance(rb.position, destino) > 0.05f)
        {
            Vector2 direccion = (destino - rb.position).normalized;

            if (animator != null)
            {
                animator.SetFloat("MoveX", direccion.x);
                animator.SetFloat("MoveY", direccion.y);
                animator.SetBool("IsWalking", true);
            }

            Vector2 nuevaPosicion = Vector2.MoveTowards(rb.position, destino, velocidadMovimiento * Time.fixedDeltaTime);
            rb.MovePosition(nuevaPosicion);

            yield return new WaitForFixedUpdate();
        }

        // Al llegar a la posición final:
        if (rb != null)
        {
            rb.MovePosition(destino);
        }

        // Paramos la animación de andar para que se quede en Idle
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        // ¡LISTO! Al NO reactivar 'scriptMovimiento', el jugador se queda 
        // bloqueado para siempre y no volverá a responder al teclado.
    }
}