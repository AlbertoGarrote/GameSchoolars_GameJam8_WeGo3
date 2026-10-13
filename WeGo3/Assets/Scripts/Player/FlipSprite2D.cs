using UnityEngine;

public class FlipSprite2D : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Configuración")]
    [Tooltip("Si tu sprite original mira hacia la IZQUIERDA por defecto, activa esta casilla.")]
    [SerializeField] private bool mirandoIzquierdaPorDefecto = false;

    private Vector3 posicionAnterior;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        posicionAnterior = transform.position;
    }

    private void Update()
    {
        float movimientoX = transform.position.x - posicionAnterior.x;

        if (movimientoX > 0.001f)
        {
            ActualizarFlip(false); 
        }
        else if (movimientoX < -0.001f)
        {
            ActualizarFlip(true); 
        }

        // Actualizamos la posición para el siguiente frame
        posicionAnterior = transform.position;
    }

    private void ActualizarFlip(bool moviendoseAIzquierda)
    {
        if (spriteRenderer == null) return;

        if (mirandoIzquierdaPorDefecto)
        {
            spriteRenderer.flipX = !moviendoseAIzquierda;
        }
        else
        {
            spriteRenderer.flipX = moviendoseAIzquierda;
        }
    }
}