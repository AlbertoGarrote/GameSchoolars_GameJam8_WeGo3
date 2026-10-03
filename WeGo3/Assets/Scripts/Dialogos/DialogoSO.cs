using UnityEngine;

[System.Serializable]
public struct LineaDialogo
{
    public string nombrePersonaje; 
    [TextArea(3, 5)]
    public string texto;
    public Sprite retrato;         
    public AudioClip sonidoTipeo;  
}

[CreateAssetMenu(fileName = "NuevoDialogo", menuName = "Sistema Dialogos/Dialogo")]
public class DialogoSO : ScriptableObject
{
    public LineaDialogo[] lineas;
}