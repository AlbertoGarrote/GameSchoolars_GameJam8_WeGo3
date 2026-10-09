using System.Collections.Generic;

[System.Serializable]
public class DatosGuardado
{
    public string nombreEscena;
    public float posicionX;
    public float posicionY;
    public List<string> flagsCompletadas = new List<string>();
}