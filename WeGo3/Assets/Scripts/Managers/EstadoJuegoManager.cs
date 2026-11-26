using System.Collections.Generic;
using UnityEngine;

public class EstadoJuegoManager : MonoBehaviour
{
    public static EstadoJuegoManager Instance { get; private set; }

    public const int MAX_MINIJUEGOS = 10;

    private HashSet<string> flagsCompletadas = new HashSet<string>();
    private List<PuntuacionMinijuego> puntuacionesMinijuegos = new List<PuntuacionMinijuego>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ActivarFlag(string claveFlag)
    {
        if (string.IsNullOrEmpty(claveFlag)) return;

        if (!flagsCompletadas.Contains(claveFlag))
        {
            flagsCompletadas.Add(claveFlag);
        }
    }

    public void DesactivarFlag(string claveFlag)
    {
        if (string.IsNullOrEmpty(claveFlag)) return;

        if (flagsCompletadas.Contains(claveFlag))
        {
            flagsCompletadas.Remove(claveFlag);
        }
    }

    public bool EstaFlagActivada(string claveFlag)
    {
        if (string.IsNullOrEmpty(claveFlag)) return false;
        return flagsCompletadas.Contains(claveFlag);
    }

    public List<string> ObtenerListaFlags()
    {
        return new List<string>(flagsCompletadas);
    }

    public void CargarListaFlags(List<string> listaCargada)
    {
        if (listaCargada != null)
        {
            flagsCompletadas = new HashSet<string>(listaCargada);
        }
    }

    public void GuardarPuntuacionMinijuego(string idMinijuego, int puntuacion, bool soloSiEsMayor = true)
    {
        if (string.IsNullOrEmpty(idMinijuego)) return;

        for (int i = 0; i < puntuacionesMinijuegos.Count; i++)
        {
            if (puntuacionesMinijuegos[i].idMinijuego == idMinijuego)
            {
                int nuevaPuntuacion = soloSiEsMayor
                    ? Mathf.Max(puntuacionesMinijuegos[i].puntuacion, puntuacion)
                    : puntuacion;

                puntuacionesMinijuegos[i] = new PuntuacionMinijuego
                {
                    idMinijuego = idMinijuego,
                    puntuacion = nuevaPuntuacion
                };
                return;
            }
        }

        if (puntuacionesMinijuegos.Count < MAX_MINIJUEGOS)
        {
            puntuacionesMinijuegos.Add(new PuntuacionMinijuego
            {
                idMinijuego = idMinijuego,
                puntuacion = puntuacion
            });
        }
        else
        {
            Debug.LogWarning($"EstadoJuegoManager: Se alcanzó el límite de {MAX_MINIJUEGOS} minijuegos registrados.");
        }
    }

    public int ObtenerPuntuacionMinijuego(string idMinijuego)
    {
        if (string.IsNullOrEmpty(idMinijuego)) return 0;

        for (int i = 0; i < puntuacionesMinijuegos.Count; i++)
        {
            if (puntuacionesMinijuegos[i].idMinijuego == idMinijuego)
            {
                return puntuacionesMinijuegos[i].puntuacion;
            }
        }

        return 0;
    }

    public List<PuntuacionMinijuego> ObtenerListaPuntuaciones()
    {
        return new List<PuntuacionMinijuego>(puntuacionesMinijuegos);
    }

    public void CargarListaPuntuaciones(List<PuntuacionMinijuego> listaCargada)
    {
        puntuacionesMinijuegos.Clear();
        if (listaCargada == null) return;

        int limite = Mathf.Min(listaCargada.Count, MAX_MINIJUEGOS);
        for (int i = 0; i < limite; i++)
        {
            if (!string.IsNullOrEmpty(listaCargada[i].idMinijuego))
            {
                puntuacionesMinijuegos.Add(listaCargada[i]);
            }
        }
    }
}