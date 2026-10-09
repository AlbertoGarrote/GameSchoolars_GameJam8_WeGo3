using System;
using UnityEngine;

public class AccionActivarFlag : AccionSmartObject
{
    [SerializeField] private string claveFlagAActivar;

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(claveFlagAActivar))
        {
            EstadoJuegoManager.Instance.ActivarFlag(claveFlagAActivar);
        }

        alTerminar?.Invoke();
    }
}