using System.Collections;
using UnityEngine;

public class SplashController : MonoBehaviour
{
    public bool condicionEspecial = false;
    public bool juegoTerminado = false;

    private const string PREF_KEY = "CondicionEspecialGuardada";
    private const string PREF_JUEGO_TERMINADO = "juegoTerminado";

    void Start()
    {
        // Comprobamos los PlayerPrefs o las Flags correspondientes
        condicionEspecial = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;

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

        // Jerarquía de decisión de menús
        if (juegoTerminado)
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