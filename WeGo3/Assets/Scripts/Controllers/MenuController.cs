using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    public void BotonContinuar()
    {
        if (SaveSystemManager.Instance != null && SaveSystemManager.Instance.ExistePartidaGuardada())
        {
            DatosGuardado datos = SaveSystemManager.Instance.CargarPartida();

            if (memoriaPosicion != null)
            {
                memoriaPosicion.posicionDestino = new Vector3(datos.posicionX, datos.posicionY, 0f);
                memoriaPosicion.usarPosicionGuardada = true;
            }

            TransicionController.Instance.CargarEscena(datos.nombreEscena);
        }
    }

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