using UnityEngine;

public class ObjetivoDescendente : MonoBehaviour
{
    private float velocidadSubida;
    private float alturaObjetivoSubida;
    private float velocidadBajada;
    private float limiteYDerrota;
    private float escalaMaximaBajada;
    private MinijuegoTitanes manager;

    private SpriteRenderer spriteRenderer;
    private Vector3 escalaInicial;
    private float alturaInicioSubida;
    private float distanciaBajadaTotal;

    private bool estaSubiendo = true;
    private bool derrotaDisparada = false;

    public bool EsVulnerable => !estaSubiendo && !derrotaDisparada;

    public void Inicializar(
        float velSubida,
        float alturaSubida,
        float velBajada,
        float limiteY,
        float escalaMax,
        MinijuegoTitanes gestor)
    {
        velocidadSubida = velSubida;
        alturaObjetivoSubida = alturaSubida;
        velocidadBajada = velBajada;
        limiteYDerrota = limiteY;
        escalaMaximaBajada = escalaMax;
        manager = gestor;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 1; // Empieza en layer 1 saliendo del agua
        }

        escalaInicial = transform.localScale;
        alturaInicioSubida = transform.position.y;
        estaSubiendo = true;
        derrotaDisparada = false;

        // Si ya nació por encima o igual a la altura objetivo, pasa directo a bajar
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
            spriteRenderer.sortingOrder = 3; // Pasa a layer 3 al empezar a bajar
        }

        distanciaBajadaTotal = Mathf.Max(0.01f, transform.position.y - limiteYDerrota);
    }

    void Update()
    {
        if (derrotaDisparada || manager == null) return;

        if (estaSubiendo)
        {
            // Fase 1: Sube hacia el punto objetivo siendo inmortal
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
            // Fase 2: Baja hacia el límite e incrementa la escala progresivamente hasta escalaMaximaBajada (10)
            Vector3 pos = transform.position;
            pos.y -= velocidadBajada * Time.deltaTime;
            transform.position = pos;

            float recorridoActual = Mathf.Clamp(alturaObjetivoSubida - transform.position.y, 0f, distanciaBajadaTotal);
            float progreso = recorridoActual / distanciaBajadaTotal;

            float escalaActual = Mathf.Lerp(escalaInicial.x, escalaMaximaBajada, progreso);
            transform.localScale = new Vector3(escalaActual, escalaActual, escalaInicial.z);

            // Si llega a la altura límite sin ser interceptado, pierdes
            if (transform.position.y <= limiteYDerrota)
            {
                derrotaDisparada = true;
                manager.NotificarObjetivoLlegoAlFondo();
            }
        }
    }
}