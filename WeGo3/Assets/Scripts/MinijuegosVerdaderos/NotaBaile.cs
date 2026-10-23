using UnityEngine;

public class NotaBaile : MonoBehaviour
{
    private int carrilAsignado;
    private float velocidad;
    private MinijuegoBaile controladorBaile;
    private bool inicializada = false;
    private float limiteSuperiorY = 4.3f; // Altura a la que se considera que la nota se ha pasado

    public void Inicializar(int carril, float vel, MinijuegoBaile controlador)
    {
        carrilAsignado = carril;
        velocidad = vel;
        controladorBaile = controlador;
        inicializada = true;
    }

    void Update()
    {
        if (!inicializada) return;

        // Mover la nota hacia arriba verticalmente
        transform.position += Vector3.up * velocidad * Time.deltaTime;

        // Si la nota sobrepasa la zona superior sin ser pulsada, cuenta como fallo
        if (transform.position.y > limiteSuperiorY)
        {
            if (controladorBaile != null)
            {
                controladorBaile.RegistrarNotaFallida(this, carrilAsignado);
            }
            Destroy(gameObject);
        }
    }
}