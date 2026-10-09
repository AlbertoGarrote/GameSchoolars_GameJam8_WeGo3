using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AccionGuardarPartida : AccionSmartObject
{
    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        if (SaveSystemManager.Instance != null)
        {
            Vector2 posicionGuardado = transform.position;
            string escenaActual = SceneManager.GetActiveScene().name;

            SaveSystemManager.Instance.GuardarPartida(posicionGuardado, escenaActual);
        }

        alTerminar?.Invoke();
    }
}