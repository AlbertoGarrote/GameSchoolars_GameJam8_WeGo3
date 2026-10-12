using System;
using System.Collections;
using UnityEngine;

public class AccionDialogo : AccionSmartObject
{
    [SerializeField] private DialogoSO dialogoAExhibir;

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        if (dialogoAExhibir != null && DialogoManager.Instance != null)
        {
            DialogoManager.Instance.IniciarDialogo(dialogoAExhibir);

            StartCoroutine(EsperarAQueTermineDialogo(alTerminar));
        }
        else
        {
            alTerminar?.Invoke();
        }
    }

    private IEnumerator EsperarAQueTermineDialogo(Action alTerminar)
    {
        yield return null;

        yield return new WaitUntil(() => DialogoManager.Instance == null || !DialogoManager.Instance.EnDialogo);

        alTerminar?.Invoke();
    }
}