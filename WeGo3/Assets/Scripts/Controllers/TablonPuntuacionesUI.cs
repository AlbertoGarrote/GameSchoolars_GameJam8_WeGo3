using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TablonPuntuacionesUI : MonoBehaviour
{
    [System.Serializable]
    public struct EntradaPuntuacionUI
    {
        public string idMinijuego;
        public TMP_Text textoPuntuacion;
    }

    [Header("Puntuaciones de los 10 Minijuegos")]
    [SerializeField] private List<EntradaPuntuacionUI> entradasMinijuegos = new List<EntradaPuntuacionUI>(10);

    private void OnEnable()
    {
        ActualizarTablon();
    }

    public void ActualizarTablon()
    {
        int total = 0;

        for (int i = 0; i < entradasMinijuegos.Count; i++)
        {
            EntradaPuntuacionUI entrada = entradasMinijuegos[i];
            int puntos = 0;

            if (EstadoJuegoManager.Instance != null && !string.IsNullOrEmpty(entrada.idMinijuego))
            {
                puntos = EstadoJuegoManager.Instance.ObtenerPuntuacionMinijuego(entrada.idMinijuego);
            }

            total += puntos;

            if (entrada.textoPuntuacion != null)
            {
                entrada.textoPuntuacion.text = puntos.ToString();
            }
        }
    }
}