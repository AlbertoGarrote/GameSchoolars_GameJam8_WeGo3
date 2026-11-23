using System.Collections;
using UnityEngine;

public class DialogoTransicionEscena : MonoBehaviour
{
    [Header("Diálogo Inicial")]
    [SerializeField] private DialogoSO dialogoAlEntrar;
    [Tooltip("Pequeña pausa tras cargar la escena antes de lanzar el diálogo.")]
    [SerializeField] private float retrasoInicial = 0.25f;

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
        while (DialogoManager.Instance == null && tiempoEspera < 3f)
        {
            tiempoEspera += Time.deltaTime;
            yield return null;
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

        // Cuando el jugador pulsa E o Espacio en la última línea y DialogoManager cierra el diálogo,
        // EnDialogo vuelve a ser false y aquí disparamos el cambio de escena.
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