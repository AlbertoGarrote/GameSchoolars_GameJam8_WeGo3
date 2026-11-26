using System.Collections.Generic;

[System.Serializable]
public struct PuntuacionMinijuego
{
    public string idMinijuego;
    public int puntuacion;
}

[System.Serializable]
public class DatosGuardado
{
    public string nombreEscena;
    public float posicionX;
    public float posicionY;
    public List<string> flagsCompletadas = new List<string>();
    public List<PuntuacionMinijuego> puntuacionesMinijuegos = new List<PuntuacionMinijuego>();
}