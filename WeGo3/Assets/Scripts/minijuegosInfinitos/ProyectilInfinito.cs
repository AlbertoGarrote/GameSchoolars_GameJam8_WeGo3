
using UnityEngine;

public class ProyectilInfinito : MonoBehaviour
{
    private float velocidad;
    private TitanesInfinito manager;
    private bool yaImpacto = false;

    public void Inicializar(float velocidadCaida, TitanesInfinito gestor)
    {
        velocidad = velocidadCaida;
        manager = gestor;
    }

    private void Update()
    {
        if (yaImpacto)
            return;

        transform.position += Vector3.down * velocidad * Time.deltaTime;

        if (manager != null &&
            transform.position.y < manager.transform.position.y - 20f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (yaImpacto || manager == null)
            return;

        ObjetoDescendenteInifnito objetivo =
            other.GetComponentInParent<ObjetoDescendenteInifnito>();

        if (objetivo != null && objetivo.EsVulnerable)
        {
            yaImpacto = true;

            manager.RegistrarImpacto(
                gameObject,
                objetivo.gameObject
            );
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (yaImpacto || manager == null)
            return;

        ObjetoDescendenteInifnito objetivo =
            collision.gameObject.GetComponentInParent<ObjetoDescendenteInifnito>();

        if (objetivo != null && objetivo.EsVulnerable)
        {
            yaImpacto = true;

            manager.RegistrarImpacto(
                gameObject,
                objetivo.gameObject
            );
        }
    }
}
