using System.Collections;
using UnityEngine;

public class SplashController : MonoBehaviour
{
    public bool condicionEspecial = false;
    public bool juegoTerminado = false;
    public bool juegoTerminadoDLC = false;

    private const string PREF_KEY = "CondicionEspecialGuardada";
    private const string PREF_JUEGO_TERMINADO = "juegoTerminado";
    private const string PREF_JUEGO_TERMINADO_DLC = "juegoTerminado_dlc";

    void Start()
    {
        // Comprobamos los PlayerPrefs o las Flags correspondientes
        condicionEspecial = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;

        // Comprobamos si el DLC ya se ha terminado
        juegoTerminadoDLC = PlayerPrefs.GetInt(PREF_JUEGO_TERMINADO_DLC, 0) == 1;
        if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada(PREF_JUEGO_TERMINADO_DLC))
        {
            juegoTerminadoDLC = true;
        }

        // Comprobamos también si el juego ya se ha terminado (por PlayerPref o por EstadoJuegoManager si existe)
        juegoTerminado = PlayerPrefs.GetInt(PREF_JUEGO_TERMINADO, 0) == 1;

        if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada("juego_terminado"))
        {
            juegoTerminado = true;
        }

        StartCoroutine(EsperarYCambiarEscena());
    }

    IEnumerator EsperarYCambiarEscena()
    {
        yield return new WaitForSeconds(4f);

        string escenaDestino = "MenuPrincipal1"; // Por defecto

        // Jerarquía de decisión de menús (DLC primero)
        if (juegoTerminadoDLC)
        {
            escenaDestino = "MenuPrincipal4";
        }
        else if (juegoTerminado)
        {
            escenaDestino = "MenuPrincipal3"; // Si el juego terminó, va al menú nuevo
        }
        else if (condicionEspecial)
        {
            escenaDestino = "MenuPrincipal2"; // Si tiene la condición especial
        }

        if (TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(escenaDestino);
        }
    }
}