using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMirrorHorizontalPosition2D : MonoBehaviour
{
    [Header("Referencia al Jugador Real")]
    [SerializeField] private PlayerMovement2D jugadorRealMovement; // Arrastra aquí el script del jugador real

    [Header("Configuración de Espejo")]
    [Tooltip("La coordenada Y exacta del mundo que actúa como espejo horizontal.")]
    [SerializeField] private float ejeYEspejo = 0f;

    [Header("Animación")]
    [SerializeField] private Animator animator; // El animator del reflejo

    private Rigidbody2D rbReflejo;

    private void Awake()
    {
        rbReflejo = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        if (jugadorRealMovement == null && GameObject.FindGameObjectWithTag("Player") != null)
        {
            jugadorRealMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement2D>();
        }

        if (jugadorRealMovement == null)
        {
            Debug.LogError("¡Falta asignar el script PlayerMovement2D del jugador real!", this);
        }
    }

    private void FixedUpdate()
    {
        if (jugadorRealMovement == null) return;

        Transform jugadorTransform = jugadorRealMovement.transform;

        // 1. POSICIÓN EN ESPEJO: La X es igual, la Y se refleja respecto al eje
        float xReal = jugadorTransform.position.x;
        float yReal = jugadorTransform.position.y;

        float distanciaAlEjeY = yReal - ejeYEspejo;
        float yEspejo = ejeYEspejo - distanciaAlEjeY;

        Vector2 posicionReflejo = new Vector2(xReal, yEspejo);
        rbReflejo.MovePosition(posicionReflejo);
    }

    private void Update()
    {
        if (jugadorRealMovement == null || animator == null) return;

        // 2. SINCRONIZACIÓN DE ANIMACIONES DEL JUGADOR REAL
        // Preguntamos directamente al Animator del jugador real si está caminando
        Animator animatorJugador = jugadorRealMovement.GetComponentInChildren<Animator>();

        if (animatorJugador != null)
        {
            bool estaMoviendo = animatorJugador.GetBool("IsWalking");
            animator.SetBool("IsWalking", estaMoviendo);

            // Copiamos los valores exactos, pero INVERTIMOS el MoveY para que el reflejo mire hacia arriba/abajo en espejo
            float moveXReal = animatorJugador.GetFloat("MoveX");
            float moveYReal = animatorJugador.GetFloat("MoveY");

            animator.SetFloat("MoveX", moveXReal);
            animator.SetFloat("MoveY", -moveYReal); // <-- Aquí está la clave del espejo horizontal
        }
    }
}