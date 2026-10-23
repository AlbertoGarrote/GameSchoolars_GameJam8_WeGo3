using UnityEngine;

public class AtauqeSupervivencia : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si el objeto que ha entrado en contacto es el jugador
        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            // Usamos FindFirstObjectByType en lugar del obsoleto FindObjectOfType
            MinijuegoSupervivencia minijuego = Object.FindFirstObjectByType<MinijuegoSupervivencia>();

            if (minijuego != null)
            {
                minijuego.RecibirDanio();
            }

        }
    }
}
