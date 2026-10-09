using System;
using UnityEngine;

public class NPCDialogoPorFlags : MonoBehaviour
{
    [System.Serializable]
    public struct CondicionDialogo
    {
        public string nombreIdentificador;
        [Tooltip("Si esta flag está activa en EstadoJuegoManager, se elegirá esta opción.")]
        public string flagRequerida;
        public SmartObject2D smartObject;
    }

    [Header("Condiciones ordenadas por Prioridad (De mayor a menor)")]
    [Tooltip("El script revisará de arriba a abajo. El primer SmartObject cuya flag sea TRUE será el que se active.")]
    [SerializeField] private CondicionDialogo[] flagsAactivables;

    [Header("Diálogo por Defecto")]
    [Tooltip("SmartObject que se activa si ninguna de las flags de arriba está completada.")]
    [SerializeField] private SmartObject2D smartObjectPorDefecto;

    private void Awake()
    {
        EvaluarEstadoNPC();
    }

    public void EvaluarEstadoNPC()
    {
        // 1. Apagamos TODOS inmediatamente al cargar la escena
        if (smartObjectPorDefecto != null)
            smartObjectPorDefecto.enabled = false;

        foreach (var condicion in flagsAactivables)
        {
            if (condicion.smartObject != null)
                condicion.smartObject.enabled = false;
        }

        // 2. Evaluamos la flag activa
        bool asignado = false;

        if (EstadoJuegoManager.Instance != null)
        {
            foreach (var condicion in flagsAactivables)
            {
                if (!string.IsNullOrEmpty(condicion.flagRequerida) &&
                    EstadoJuegoManager.Instance.EstaFlagActivada(condicion.flagRequerida))
                {
                    if (condicion.smartObject != null)
                    {
                        condicion.smartObject.enabled = true;
                        asignado = true;
                        break;
                    }
                }
            }
        }

        // 3. Si no hay flag o no se cumple ninguna, encendemos el por defecto
        if (!asignado && smartObjectPorDefecto != null)
        {
            smartObjectPorDefecto.enabled = true;
        }
    }
}