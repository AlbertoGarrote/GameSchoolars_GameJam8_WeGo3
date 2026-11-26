using UnityEngine;

public class MenuInifinito : MonoBehaviour
{
    [SerializeField] private string nombreEscenaDestino;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;

    public void EjecutarTransicion()
    {

        if (memoriaPosicion != null)
        {
            memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
            memoriaPosicion.usarPosicionGuardada = true;
        }

        if (!string.IsNullOrEmpty(nombreEscenaDestino) && TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(nombreEscenaDestino);
        }
    }

    public void IrAEscena(string nombreEscena)
    {
        if (!string.IsNullOrEmpty(nombreEscena) && TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(nombreEscena);
        }
    }
}
