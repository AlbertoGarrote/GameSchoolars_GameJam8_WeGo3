using UnityEngine;
using UnityEngine.UI;

public class MenuPrincipalController : MonoBehaviour
{
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Button botonContinuar; // Referencia al botón "Continuar" del menú principal

    void Start()
    {
        ActualizarEstadoBotonContinuar();
    }

    public void ActualizarEstadoBotonContinuar()
    {
        if (botonContinuar != null)
        {
            if (SaveSystemManager.Instance != null)
            {
                // Habilita el botón solo si existe una partida guardada
                botonContinuar.interactable = SaveSystemManager.Instance.ExistePartidaGuardada();
            }
            else
            {
                botonContinuar.interactable = false;
            }
        }
    }

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

            if (TransicionController.Instance != null)
            {
                TransicionController.Instance.CargarEscena(datos.nombreEscena);
            }
        }
    }

    public void CambiarEscena(string escenaCambiar)
    {
        Time.timeScale = 1f;
        if (TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(escenaCambiar);
        }
    }

    public void Salir()
    {
        Application.Quit();
    }

    public void CambiarCondicion(string escenaCambiar)
    {
        Time.timeScale = 1f;
        PlayerPrefs.SetInt("CondicionEspecialGuardada", 1);
        PlayerPrefs.Save();

        if (TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(escenaCambiar, 5f);
        }
    }

    public void AbrirEnlace(string urlDestino)
    {
        if (!string.IsNullOrEmpty(urlDestino))
        {
            Application.OpenURL(urlDestino);
        }
    }

    // --- MÉTODOS PARA LOS SLIDERS DE AUDIO EN EL MENÚ PRINCIPAL ---

    public void OnSliderMusicaChanged(float valor)
    {
        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.CambiarVolumenMusica(valor);
        }
    }

    public void OnSliderSFXChanged(float valor)
    {
        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.CambiarVolumenSFX(valor);
        }
    }
}