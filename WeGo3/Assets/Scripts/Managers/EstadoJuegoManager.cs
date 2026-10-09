using System.Collections.Generic;
using UnityEngine;

public class EstadoJuegoManager : MonoBehaviour
{
    public static EstadoJuegoManager Instance { get; private set; }

    private HashSet<string> flagsCompletadas = new HashSet<string>();

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
}