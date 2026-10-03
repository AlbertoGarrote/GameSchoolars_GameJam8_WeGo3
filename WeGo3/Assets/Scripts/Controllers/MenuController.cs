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

    public void cambiarCondicion(string escenaCambiar)
    {
        PlayerPrefs.SetInt("CondicionEspecialGuardada", 1);
        PlayerPrefs.Save();
        StartCoroutine(CorrutinaCambioEscenaEspecial(escenaCambiar));
    }

    private IEnumerator CorrutinaCambioEscena(string escenaCambiar)
    {
        TransicionController.Instance.ReproducirSalida();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(escenaCambiar);
    }

    private IEnumerator CorrutinaCambioEscenaEspecial(string escenaCambiar)
    {
        TransicionController.Instance.ReproducirSalida();
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(escenaCambiar);
    }

    public void AbrirEnlace(string urlDestino)
    {
        if (!string.IsNullOrEmpty(urlDestino))
        {
            Application.OpenURL(urlDestino);
        }
    }

}
