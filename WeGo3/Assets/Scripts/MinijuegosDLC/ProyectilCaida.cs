using UnityEngine;

public class ProyectilCaida : MonoBehaviour
{
    private float velocidad;
    private MinijuegoTitanes manager;
    private bool yaImpacto = false;

    public void Inicializar(float velocidadCaida, MinijuegoTitanes gestor)
    {
        velocidad = velocidadCaida;
        manager = gestor;
    }

    void Update()
    {
        if (yaImpacto) return;

        transform.position += Vector3.down * velocidad * Time.deltaTime;

        if (manager != null && transform.position.y < manager.transform.position.y - 20f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaImpacto || manager == null) return;

        ObjetivoDescendente objetivo = other.GetComponentInParent<ObjetivoDescendente>();
        if (objetivo != null && objetivo.EsVulnerable)
        {
            yaImpacto = true;
            manager.RegistrarImpacto(gameObject, objetivo.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (yaImpacto || manager == null) return;

        ObjetivoDescendente objetivo = collision.gameObject.GetComponentInParent<ObjetivoDescendente>();
        if (objetivo != null && objetivo.EsVulnerable)
        {
            yaImpacto = true;
            manager.RegistrarImpacto(gameObject, objetivo.gameObject);
        }
    }
}