using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class DialogoManager : MonoBehaviour
{
    public static DialogoManager Instance { get; private set; }

    [Header("UI Elementos")]
    [SerializeField] private GameObject panelDialogo;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoCuerpo;
    [SerializeField] private RawImage imagenRetrato;

    [Header("Configuración de Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoPorDefecto;
    [SerializeField] private float velocidadTipeo = 0.03f;
    [SerializeField] private int frecuenciaSonido = 2; // Suena cada N letras

    private Queue<LineaDialogo> colaLineas = new Queue<LineaDialogo>();
    private bool estaEscribiendo = false;
    private bool textoCompletoRevelado = false;
    private string textoActualCompleto = "";
    private Coroutine corrutinaTipeo;

    public bool EnDialogo { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (panelDialogo != null) panelDialogo.SetActive(false);
    }

    private void Update()
    {
        if (!EnDialogo) return;

        // Avanzar o acelerar el texto con clic/tap/espacio usando el nuevo Input System
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            if (estaEscribiendo)
            {
                // Si el jugador hace clic mientras se escribe, muestra la línea completa al instante
                CompletarLineaInstantanea();
            }
            else
            {
                // Si ya terminó de escribir la línea, pasa a la siguiente
                SiguienteLinea();
            }
        }
    }

    public void IniciarDialogo(DialogoSO dialogo)
    {
        if (dialogo == null || dialogo.lineas.Length == 0) return;

        EnDialogo = true;
        panelDialogo.SetActive(true);

        colaLineas.Clear();
        foreach (var linea in dialogo.lineas)
        {
            colaLineas.Enqueue(linea);
        }

        SiguienteLinea();
    }

    private void SiguienteLinea()
    {
        if (colaLineas.Count == 0)
        {
            FinalizarDialogo();
            return;
        }

        LineaDialogo lineaActual = colaLineas.Dequeue();

        if (textoNombre != null) textoNombre.text = lineaActual.nombrePersonaje;

        if (imagenRetrato != null)
        {
            imagenRetrato.gameObject.SetActive(lineaActual.retrato != null);
            imagenRetrato.texture = lineaActual.retrato != null ? lineaActual.retrato.texture : null;
        }

        textoActualCompleto = lineaActual.texto;

        if (corrutinaTipeo != null) StopCoroutine(corrutinaTipeo);
        corrutinaTipeo = StartCoroutine(EfectoTipeo(lineaActual));
    }

    private IEnumerator EfectoTipeo(LineaDialogo linea)
    {
        estaEscribiendo = true;
        textoCuerpo.text = "";
        int contadorCaracteres = 0;

        AudioClip clipAAprobar = linea.sonidoTipeo != null ? linea.sonidoTipeo : sonidoPorDefecto;

        foreach (char letra in linea.texto)
        {
            textoCuerpo.text += letra;
            contadorCaracteres++;

            if (audioSource != null && clipAAprobar != null && letra != ' ')
            {
                if (contadorCaracteres % frecuenciaSonido == 0)
                {
                    audioSource.PlayOneShot(clipAAprobar);
                }
            }

            yield return new WaitForSeconds(velocidadTipeo);
        }

        estaEscribiendo = false;
    }

    private void CompletarLineaInstantanea()
    {
        if (corrutinaTipeo != null) StopCoroutine(corrutinaTipeo);
        textoCuerpo.text = textoActualCompleto;
        estaEscribiendo = false;
    }

    public void FinalizarDialogo()
    {
        EnDialogo = false;
        panelDialogo.SetActive(false);
    }
}