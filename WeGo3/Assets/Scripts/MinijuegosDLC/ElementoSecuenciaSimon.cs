using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class ElementoSecuenciaSimon : MonoBehaviour
{
    [Header("Sprites de Estado")]
    [SerializeField] private Sprite spriteApagado;
    [SerializeField] private Sprite spriteIluminado;

    private int indiceElemento = -1;
    private CristalMinijuego manager;

    private SpriteRenderer spriteRenderer;
    private Image imagenUI;
    private Button botonUI;
    private Coroutine rutinaFeedback;
    private int ultimoFramePulsado = -1;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        imagenUI = GetComponentInChildren<Image>();
        botonUI = GetComponentInChildren<Button>();

        if (botonUI != null)
        {
            botonUI.onClick.AddListener(AlPulsar);
        }

        EstablecerIluminado(false);
    }

    void Update()
    {
        // Respaldo con el nuevo InputSystem + BoxCollider2D en 2D
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector2 mundoPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                RaycastHit2D hit = Physics2D.Raycast(mundoPos, Vector2.zero);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    AlPulsar();
                }
            }
        }
    }

    public void Configurar(int indice, CristalMinijuego gestor)
    {
        indiceElemento = indice;
        manager = gestor;
        EstablecerIluminado(false);
    }

    public void EstablecerIluminado(bool iluminado)
    {
        Sprite spriteDestino = iluminado ? spriteIluminado : spriteApagado;
        if (spriteDestino == null) return;

        if (spriteRenderer != null) spriteRenderer.sprite = spriteDestino;
        if (imagenUI != null) imagenUI.sprite = spriteDestino;
    }

    public void Parpadear(float duracion)
    {
        if (rutinaFeedback != null) StopCoroutine(rutinaFeedback);
        rutinaFeedback = StartCoroutine(RutinaParpadeo(duracion));
    }

    private IEnumerator RutinaParpadeo(float duracion)
    {
        EstablecerIluminado(true);
        yield return new WaitForSeconds(duracion);
        EstablecerIluminado(false);
        rutinaFeedback = null;
    }

    public void AlPulsar()
    {
        if (Time.frameCount == ultimoFramePulsado) return;
        ultimoFramePulsado = Time.frameCount;

        if (manager == null || indiceElemento < 0) return;
        manager.RegistrarPulsacionJugador(indiceElemento, this);
    }

    private void OnMouseDown()
    {
        if (botonUI != null && botonUI.interactable) return;
        AlPulsar();
    }
}