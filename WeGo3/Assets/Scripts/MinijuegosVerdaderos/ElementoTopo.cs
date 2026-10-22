using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ElementoTopo : MonoBehaviour
{
    private bool esMalo;
    private float tiempoVida;
    private MinijuegoTopos controlador;
    private int miIndiceAgujero;
    private bool fueGolpeado = false;

    [Header("Configuración de Animación Asomar")]
    [SerializeField] private float alturaSubida = 1.0f;
    [SerializeField] private float velocidadAnimacion = 0.15f;

    private Vector3 posicionOculta;
    private Vector3 posicionVisible;
    private Collider2D miCollider;

    public void Inicializar(bool esMalo, float tiempoVida, MinijuegoTopos controlador, int indiceAgujero)
    {
        this.esMalo = esMalo;
        this.tiempoVida = tiempoVida;
        this.controlador = controlador;
        this.miIndiceAgujero = indiceAgujero;
        this.fueGolpeado = false;

        miCollider = GetComponent<Collider2D>();

        posicionOculta = transform.localPosition;
        posicionVisible = posicionOculta + new Vector3(0, alturaSubida, 0);

        StartCoroutine(RutinaTopoCiclo());
    }

    void Update()
    {
        // Detección directa por el nuevo Input System para asegurar que registra el clic en el objeto
        if (!fueGolpeado && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosScreen = Mouse.current.position.ReadValue();
            Vector3 mousePosWorld = Camera.main.ScreenToWorldPoint(mousePosScreen);
            Vector2 mousePos2D = new Vector2(mousePosWorld.x, mousePosWorld.y);

            if (miCollider != null && miCollider.OverlapPoint(mousePos2D))
            {
                Golpear();
            }
        }
    }

    public void Golpear()
    {
        if (fueGolpeado) return;
        fueGolpeado = true;

        if (controlador != null)
        {
            controlador.RegistrarGolpe(esMalo);
            controlador.LiberarAgujero(miIndiceAgujero); // ¡Liberamos el agujero al golpearlo!
        }

        StopAllCoroutines();
        StartCoroutine(RutinaBajarYDestruir());
    }

    private IEnumerator RutinaTopoCiclo()
    {
        // 1. SUBIR
        float t = 0f;
        while (t < velocidadAnimacion)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(posicionOculta, posicionVisible, t / velocidadAnimacion);
            yield return null;
        }
        transform.localPosition = posicionVisible;

        // 2. ESPERAR EXPUESTO
        yield return new WaitForSeconds(tiempoVida);

        // 3. BAJAR SI NO FUE GOLPEADO
        if (!fueGolpeado)
        {
            fueGolpeado = true;
            if (controlador != null)
            {
                controlador.RegistrarTopoExpirado(esMalo);
                controlador.LiberarAgujero(miIndiceAgujero); // ¡Liberamos el agujero al expirar!
            }

            t = 0f;
            while (t < velocidadAnimacion)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(posicionVisible, posicionOculta, t / velocidadAnimacion);
                yield return null;
            }
            transform.localPosition = posicionOculta;

            Destroy(gameObject);
        }
    }

    private IEnumerator RutinaBajarYDestruir()
    {
        float t = 0f;
        Vector3 posActual = transform.localPosition;

        while (t < velocidadAnimacion)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(posActual, posicionOculta, t / velocidadAnimacion);
            yield return null;
        }
        transform.localPosition = posicionOculta;

        Destroy(gameObject);
    }
}