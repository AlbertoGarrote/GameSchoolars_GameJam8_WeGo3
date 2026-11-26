using UnityEngine;

public class NotaBaileInfinito : MonoBehaviour
{
    private int carrilAsignado;
    private float velocidad;
    private BaileInfinito controladorBaile;
    private bool inicializada = false;

    [SerializeField] private float limiteSuperiorY = 4.3f;

    public void Inicializar(int carril, float vel, BaileInfinito controlador)
    {
        carrilAsignado = carril;
        velocidad = vel;
        controladorBaile = controlador;
        inicializada = true;
    }

    public void EstablecerVelocidad(float nuevaVelocidad)
    {
        velocidad = nuevaVelocidad;
    }

    void Update()
    {
        if (!inicializada) return;

        transform.position += Vector3.up * velocidad * Time.deltaTime;

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