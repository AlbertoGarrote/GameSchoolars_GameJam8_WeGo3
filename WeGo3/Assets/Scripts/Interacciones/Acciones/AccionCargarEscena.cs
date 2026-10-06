using System;
using UnityEngine;

public class AccionCargarEscena : AccionSmartObject
{
    [SerializeField] private string nombreEscena;

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            TransicionController.Instance.CargarEscena(nombreEscena);
        }

        alTerminar?.Invoke();
    }
}