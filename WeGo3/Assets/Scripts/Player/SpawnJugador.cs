using UnityEngine;

public class SpawnJugador : MonoBehaviour
{
    [SerializeField] private PuntoAparicionSO memoriaPosicion;

    private void Start()
    {
        if (memoriaPosicion != null && memoriaPosicion.usarPosicionGuardada)
        {
            transform.position = memoriaPosicion.posicionDestino;

            memoriaPosicion.usarPosicionGuardada = false;
        }
    }
}