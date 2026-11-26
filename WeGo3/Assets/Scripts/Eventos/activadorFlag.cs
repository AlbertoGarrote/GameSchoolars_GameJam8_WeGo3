using UnityEngine;

public class activadorFlag : MonoBehaviour
{
    void Awake()
    {
        EstadoJuegoManager.Instance.ActivarFlag("juego_terminado");
    }

    
}
