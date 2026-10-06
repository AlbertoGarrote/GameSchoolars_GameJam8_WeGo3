using System.Collections;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    public bool condicionEspecial = false;

    private const string PREF_KEY = "CondicionEspecialGuardada";

    void Start()
    {
        condicionEspecial = PlayerPrefs.GetInt(PREF_KEY, 0) == 1;

        StartCoroutine(EsperarYCambiarEscena());
    }

    IEnumerator EsperarYCambiarEscena()
    {
        yield return new WaitForSeconds(4f);
        string escenaDestino = condicionEspecial ? "MenuPrincipal2" : "MenuPrincipal1";

        TransicionController.Instance.CargarEscena(escenaDestino);
    }

}