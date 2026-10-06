using UnityEngine;

public class MenuController : MonoBehaviour
{
    public void CambiarEscena(string escenaCambiar)
    {
        TransicionController.Instance.CargarEscena(escenaCambiar);
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void CambiarCondicion(string escenaCambiar)
    {
        PlayerPrefs.SetInt("CondicionEspecialGuardada", 1);
        PlayerPrefs.Save();
        TransicionController.Instance.CargarEscena(escenaCambiar, 5f); // Salida especial de 5 segundos
    }

    public void AbrirEnlace(string urlDestino)
    {
        if (!string.IsNullOrEmpty(urlDestino))
        {
            Application.OpenURL(urlDestino);
        }
    }
}