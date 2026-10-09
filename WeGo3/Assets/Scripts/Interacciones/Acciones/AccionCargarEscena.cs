using System;
using UnityEngine;

public class AccionCargarEscena : AccionSmartObject
{
    [Header("Escena Destino")]
    [SerializeField] private string nombreEscena;

    [Header("Posición al Aparecer")]
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        if (!string.IsNullOrEmpty(nombreEscena))
        {
            // Guardamos la posición deseada en el ScriptableObject antes de cambiar
            if (memoriaPosicion != null)
            {
                memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
                memoriaPosicion.usarPosicionGuardada = true;
            }

            TransicionController.Instance.CargarEscena(nombreEscena);
        }

        alTerminar?.Invoke();
    }
}