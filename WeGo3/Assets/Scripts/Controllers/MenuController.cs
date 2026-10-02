using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuController : MonoBehaviour
{
    public void CambiarEscena(string escenaCambiar)
    {
        StartCoroutine(CorrutinaCambioEscena(escenaCambiar));  
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void cambiarCondicion()
    {
        PlayerPrefs.SetInt("CondicionEspecialGuardada", 1);
        PlayerPrefs.Save();
    }

    private IEnumerator CorrutinaCambioEscena(string escenaCambiar)
    {
        TransicionController.Instance.ReproducirSalida();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(escenaCambiar);
    }

}
