using UnityEngine;

[CreateAssetMenu(fileName = "MemoriaPuntoAparicion", menuName = "Sistema Mapa/Punto Aparicion SO")]
public class PuntoAparicionSO : ScriptableObject
{
    public Vector3 posicionDestino;
    public bool usarPosicionGuardada; // Controla si al cargar escena hay que reposicionar al jugador
}