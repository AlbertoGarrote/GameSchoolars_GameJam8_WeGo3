using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinematicaEntradaEscena : MonoBehaviour
{
    [Header("Configuración de Flag")]
    [SerializeField] private string claveFlagCinematica = "Cinematica_Inicio_Escena_Vista";

    [Header("Secuencia de Acciones")]
    [Tooltip("Arrastra aquí las acciones reutilizables (Mover, Esperar, Diálogo, etc.) que formarán la cinemática.")]
    [SerializeField] private List<AccionSmartObject> accionesCinematica;

    private IEnumerator Start()
    {
        // 1. Esperamos un frame para asegurarnos de que todo (Player, Managers) ha inicializado bien
        yield return null;

        if (EstadoJuegoManager.Instance == null) yield break;

        // 2. Comprobamos si la cinemática ya se reprodujo en el pasado
        if (EstadoJuegoManager.Instance.EstaFlagActivada(claveFlagCinematica))
        {
            // Si ya se vio, desactivamos este objeto para que no moleste
            gameObject.SetActive(false);
            yield break;
        }

        GameObject jugador = GameObject.FindWithTag("Player");
        SmartObject2D.HayAccionEjecutandoseGlobal = true;

        // 3. Ejecutamos cada acción de la cinemática en orden
        foreach (var accion in accionesCinematica)
        {
            if (accion != null)
            {
                bool accionTerminada = false;

                accion.EjecutarAccion(jugador, () => accionTerminada = true);

                yield return new WaitUntil(() => accionTerminada);
            }
        }
        SmartObject2D.HayAccionEjecutandoseGlobal = false;

        // 4. Activamos la flag para que NUNCA VUELVA A REPRODUCIRSE
        EstadoJuegoManager.Instance.ActivarFlag(claveFlagCinematica);

        // 5. Finalizamos y desactivamos el manager de cinemática
        gameObject.SetActive(false);
    }
}