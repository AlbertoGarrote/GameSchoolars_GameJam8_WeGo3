using UnityEngine;

public class AtauqeSupervivencia2 : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si el objeto que ha entrado en contacto es el jugador
        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            // Usamos FindFirstObjectByType en lugar del obsoleto FindObjectOfType
            cigarrosInfinito minijuego = Object.FindFirstObjectByType<cigarrosInfinito>();

            if (minijuego != null)
            {
                minijuego.RecibirDanio();
            }

        }
    }
}
