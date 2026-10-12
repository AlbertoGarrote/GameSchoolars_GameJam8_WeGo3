using System;
using System.Collections;
using UnityEngine;

public class AccionEsperarTiempo : AccionSmartObject
{
    [Header("Tiempo de Espera")]
    [SerializeField] private float segundosDeEspera = 1.5f;

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        StartCoroutine(RutinaEsperar(alTerminar));
    }

    private IEnumerator RutinaEsperar(Action alTerminar)
    {
        yield return new WaitForSeconds(segundosDeEspera);
        alTerminar?.Invoke();
    }
}