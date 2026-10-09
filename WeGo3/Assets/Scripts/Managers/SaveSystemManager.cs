using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class SaveSystemManager : MonoBehaviour
{
    public static SaveSystemManager Instance { get; private set; }

    private string rutaArchivo;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            rutaArchivo = Path.Combine(Application.persistentDataPath, "partida.json");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GuardarPartida(Vector2 posicionGuardado, string nombreEscena)
    {
        DatosGuardado datos = new DatosGuardado();
        datos.nombreEscena = nombreEscena;
        datos.posicionX = posicionGuardado.x;
        datos.posicionY = posicionGuardado.y;

        // Obtenemos las flags activas de tu EstadoJuegoManager
        if (EstadoJuegoManager.Instance != null)
        {
            datos.flagsCompletadas = EstadoJuegoManager.Instance.ObtenerListaFlags();
        }

        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(rutaArchivo, json);

        Debug.Log("Partida guardada con éxito en: " + rutaArchivo);
    }

    public bool ExistePartidaGuardada()
    {
        return File.Exists(rutaArchivo);
    }

    public DatosGuardado CargarPartida()
    {
        if (!ExistePartidaGuardada()) return null;

        string json = File.ReadAllText(rutaArchivo);
        DatosGuardado datos = JsonUtility.FromJson<DatosGuardado>(json);

        // Restauramos las flags en EstadoJuegoManager
        if (EstadoJuegoManager.Instance != null)
        {
            EstadoJuegoManager.Instance.CargarListaFlags(datos.flagsCompletadas);
        }

        return datos;
    }
}