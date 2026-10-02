using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems; // Requisito para detectar clics mediante IPointerClickHandler

public class ItemEmparejable : MonoBehaviour, IPointerClickHandler
{
    [HideInInspector] public int parejaID;
    [HideInInspector] public bool yaEmparejado = false;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Se ejecuta al hacer clic con el ratón o tocar la pantalla sobre el Collider
    public void OnPointerClick(PointerEventData eventData)
    {
        if (yaEmparejado) return;
        CadaOvejaMinigame.Instance.SeleccionarItem(this);
    }

    public void MarcarComoSeleccionado(bool seleccionado)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = seleccionado ? Color.yellow : Color.white;
        }
    }

    public void BloquearEmparejado()
    {
        yaEmparejado = true;
        StartCoroutine(CorrutinaFadeOut(0.5f));
    }

    private IEnumerator CorrutinaFadeOut(float duracion)
    {
        float tiempo = 0f;
        Color colorInicial = spriteRenderer.color;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, tiempo / duracion);
            spriteRenderer.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}