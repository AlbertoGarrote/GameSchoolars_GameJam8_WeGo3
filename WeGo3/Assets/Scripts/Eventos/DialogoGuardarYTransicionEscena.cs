using System.Collections;
using UnityEngine;

public class DialogoGuardarYTransicionEscena : MonoBehaviour
{
    [Header("Guardado Antes del Diálogo")]
    [SerializeField] private string escenaGuardado = "Fiestas";
    [SerializeField] private Vector2 posicionGuardado = new Vector2(-4.5f, -10.75f);

    [Header("Diálogo Inicial")]
    [SerializeField] private DialogoSO dialogoAlEntrar;
    [Tooltip("Pequeña pausa tras cargar la escena antes de lanzar el diálogo.")]
    [SerializeField] private float retrasoInicial = 0.75f;

    [Header("Destino al Terminar el Diálogo")]
    [SerializeField] private string nombreEscenaDestino;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;
    [SerializeField] private string flagAActivarAlTerminar;

    private bool dialogoIniciado = false;
    private bool transicionIniciada = false;

    private void Start()
    {
        StartCoroutine(IniciarSecuencia());
    }

    private IEnumerator IniciarSecuencia()
    {
        if (retrasoInicial > 0f)
        {
            yield return new WaitForSeconds(retrasoInicial);
        }

        float tiempoEspera = 0f;
        while ((DialogoManager.Instance == null || SaveSystemManager.Instance == null) && tiempoEspera < 3f)
        {
            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        // Guardamos partida (escena, posición y flags activas) antes de iniciar el diálogo
        if (SaveSystemManager.Instance != null)
        {
            SaveSystemManager.Instance.GuardarPartida(posicionGuardado, escenaGuardado);
        }

        if (dialogoAlEntrar != null && DialogoManager.Instance != null)
        {
            dialogoIniciado = true;
            DialogoManager.Instance.IniciarDialogo(dialogoAlEntrar);
        }
        else
        {
            EjecutarTransicion();
        }
    }

    private void Update()
    {
        if (!dialogoIniciado || transicionIniciada) return;

        if (DialogoManager.Instance != null && !DialogoManager.Instance.EnDialogo)
        {
            EjecutarTransicion();
        }
    }

    private void EjecutarTransicion()
    {
        if (transicionIniciada) return;
        transicionIniciada = true;

        if (!string.IsNullOrEmpty(flagAActivarAlTerminar) && EstadoJuegoManager.Instance != null)
        {
            EstadoJuegoManager.Instance.ActivarFlag(flagAActivarAlTerminar);
        }

        if (memoriaPosicion != null)
        {
            memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
            memoriaPosicion.usarPosicionGuardada = true;
        }

        if (!string.IsNullOrEmpty(nombreEscenaDestino) && TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(nombreEscenaDestino);
        }
    }
}