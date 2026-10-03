using UnityEngine;

public class GotaLluvia : MonoBehaviour
{
    public float velocidadCaida = 8f;

    private void Update()
    {
        transform.Translate(Vector3.down * velocidadCaida * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca con el paraguas, la gota se destruye
        if (collision.CompareTag("Paraguas"))
        {
            Destroy(gameObject);
        }
        // Si choca con el caminante, pierdes el minijuego
        else if (collision.CompareTag("Caminante"))
        {
            CuandoLlueveMinigame.Instance.ReportarColisionProyectil();
            Destroy(gameObject);
        }
    }
}