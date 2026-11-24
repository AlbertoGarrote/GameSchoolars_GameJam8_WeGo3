using System;
using UnityEngine;

public class AccionDesactivarFlag : AccionSmartObject
{
    [SerializeField] private string claveFlagAActivar;

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(claveFlagAActivar))
        {
            EstadoJuegoManager.Instance.DesactivarFlag(claveFlagAActivar);
        }

        alTerminar?.Invoke();
    }
}