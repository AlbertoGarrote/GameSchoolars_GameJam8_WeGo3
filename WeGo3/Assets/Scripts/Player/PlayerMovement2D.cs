using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float velocidadMovimiento = 5f;

    private Rigidbody2D rb;
    private Vector2 entradaMovimiento;

    // AÑADIDO: Guarda el último vector de dirección registrado (por defecto mira hacia abajo)
    public Vector2 DireccionMirada { get; private set; } = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
        {
            entradaMovimiento = Vector2.zero;
            return;
        }

        LeerEntradaInput();
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

        // AÑADIDO: Actualiza la dirección de mirada solo cuando el jugador se está moviendo
        if (entradaMovimiento != Vector2.zero)
        {
            DireccionMirada = entradaMovimiento;
        }
    }
}