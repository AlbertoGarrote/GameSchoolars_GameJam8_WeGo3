using System;
using UnityEngine;

public class AccionVisibilidadPersonaje : AccionSmartObject
{
    [Header("Configuración")]
    [SerializeField] private GameObject objetivo;
    [SerializeField] private bool visibilidadDeseada = false; // false = Desaparecer, true = Aparecer

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        GameObject objetivoAfectado = objetivo != null ? objetivo : interactor;

        if (objetivoAfectado != null)
        {
            objetivoAfectado.SetActive(visibilidadDeseada);
        }

        // Es una acción instantánea, pasa a la siguiente inmediatamente
        alTerminar?.Invoke();
    }
}