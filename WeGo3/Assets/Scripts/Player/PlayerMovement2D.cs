using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float velocidadMovimiento = 5f;
    [SerializeField] private Animator animator; // Arrastra aquí tu Animator desde el inspector

    private Rigidbody2D rb;
    private Vector2 entradaMovimiento;

    public Vector2 DireccionMirada { get; private set; } = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        // Bloqueo de movimiento si hay diálogo, transición o cinemática activa
        bool estaEnDialogo = DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo;
        bool estaCambiandoEscena = TransicionController.Instance != null && TransicionController.Instance.CambiandoEscena;
        bool smartObjectOcupado = SmartObject2D.HayAccionEjecutandoseGlobal;

        if (estaEnDialogo || estaCambiandoEscena || smartObjectOcupado)
        {
            entradaMovimiento = Vector2.zero;
            ActualizarAnimator(Vector2.zero);
            return;
        }

        LeerEntradaInput();
        ActualizarAnimator(entradaMovimiento);
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + entradaMovimiento * velocidadMovimiento * Time.fixedDeltaTime);
    }

    private void LeerEntradaInput()
    {
        float x = 0f;
        float y = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) y = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) y = -1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x = -1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x = 1f;
        }

        entradaMovimiento = new Vector2(x, y).normalized;

        if (entradaMovimiento != Vector2.zero)
        {
            DireccionMirada = entradaMovimiento;
        }
    }

    private void ActualizarAnimator(Vector2 movimiento)
    {
        if (animator == null) return;

        bool estaMoviendo = movimiento != Vector2.zero;

        // Pasamos si se está moviendo o no
        animator.SetBool("IsWalking", estaMoviendo);

        if (estaMoviendo)
        {
            // Mientras camina, actualizamos los ejes de movimiento exactos
            animator.SetFloat("MoveX", movimiento.x);
            animator.SetFloat("MoveY", movimiento.y);
        }
        else
        {
            // Cuando se para, dejamos grabada la última dirección de mirada 
            // para que el personaje no "resetee" su pose de reposo a otra dirección
            animator.SetFloat("MoveX", DireccionMirada.x);
            animator.SetFloat("MoveY", DireccionMirada.y);
        }
    }
}