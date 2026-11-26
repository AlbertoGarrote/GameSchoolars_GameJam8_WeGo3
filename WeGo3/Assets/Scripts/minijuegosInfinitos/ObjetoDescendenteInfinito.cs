
using UnityEngine;

public class ObjetoDescendenteInifnito : MonoBehaviour
{
    private float velocidadSubida;
    private float alturaObjetivoSubida;
    private float velocidadBajada;
    private float limiteYDerrota;
    private float escalaMaximaBajada;

    private TitanesInfinito manager;

    private SpriteRenderer spriteRenderer;

    private Vector3 escalaInicial;

    private float alturaInicioBajada;
    private float distanciaBajadaTotal;

    private bool estaSubiendo = true;
    private bool derrotaDisparada = false;

    public bool EsVulnerable =>
        !estaSubiendo && !derrotaDisparada;

    public void Inicializar(
        float velSubida,
        float alturaSubida,
        float velBajada,
        float limiteY,
        float escalaMax,
        TitanesInfinito gestor)
    {
        velocidadSubida = velSubida;
        alturaObjetivoSubida = alturaSubida;
        velocidadBajada = velBajada;
        limiteYDerrota = limiteY;
        escalaMaximaBajada = escalaMax;
        manager = gestor;

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 1;
        }

        escalaInicial = transform.localScale;

        estaSubiendo = true;
        derrotaDisparada = false;

        if (transform.position.y >= alturaObjetivoSubida)
        {
            IniciarFaseBajada();
        }
    }

    private void IniciarFaseBajada()
    {
        estaSubiendo = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 3;
        }

        alturaInicioBajada = transform.position.y;

        distanciaBajadaTotal = Mathf.Max(
            0.01f,
            alturaInicioBajada - limiteYDerrota
        );
    }

    private void Update()
    {
        if (derrotaDisparada || manager == null)
            return;

        if (estaSubiendo)
        {
            // Fase 1: el objetivo sube y es invulnerable.
            Vector3 pos = transform.position;

            pos.y += velocidadSubida * Time.deltaTime;

            if (pos.y >= alturaObjetivoSubida)
            {
                pos.y = alturaObjetivoSubida;
                transform.position = pos;

                IniciarFaseBajada();
                return;
            }

            transform.position = pos;
        }
        else
        {
            // Fase 2: el objetivo baja y aumenta de tamaño.
            Vector3 pos = transform.position;

            pos.y -= velocidadBajada * Time.deltaTime;

            transform.position = pos;

            float recorridoActual = Mathf.Clamp(
                alturaInicioBajada - transform.position.y,
                0f,
                distanciaBajadaTotal
            );

            float progreso =
                recorridoActual / distanciaBajadaTotal;

            float escalaActual = Mathf.Lerp(
                escalaInicial.x,
                escalaMaximaBajada,
                progreso
            );

            transform.localScale = new Vector3(
                escalaActual,
                escalaActual,
                escalaInicial.z
            );

            // Al llegar al límite, se pierde una vida.
            if (transform.position.y <= limiteYDerrota)
            {
                derrotaDisparada = true;

                manager.NotificarObjetivoLlegoAlFondo(
                    gameObject
                );
            }
        }
    }
}
