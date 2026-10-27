using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }

    [SerializeField] private PuntoAparicionSO memoriaPosicion;

    [Header("Referencias de UI")]
    [SerializeField] private Button botonContinuar;                // Referencia al botón de Continuar del menú

    [Header("Configuración de Pausa")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelConfirmarSalir;
    [SerializeField] private Slider sliderVolumenMusica;
    [SerializeField] private Slider sliderVolumenSFX;
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuPrincipal";

    private bool juegoPausado = false;

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
            return;
        }
    }

    void Start()
    {
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

        // Comprobar el estado del botón Continuar al iniciar
        ActualizarEstadoBotonContinuar();
    }

    void Update()
    {
        ComprobarTeclaPausa();
    }

    // Método para comprobar si hay archivo y habilitar/deshabilitar el botón
    public void ActualizarEstadoBotonContinuar()
    {
        if (botonContinuar != null)
        {
            if (SaveSystemManager.Instance != null)
            {
                // Si existe partida guardada, interactable = true; si no, false
                botonContinuar.interactable = SaveSystemManager.Instance.ExistePartidaGuardada();
            }
            else
            {
                botonContinuar.interactable = false;
            }
        }
    }

    private void ComprobarTeclaPausa()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (EstaEnMinijuegoDialogoOMenu()) return;

            if (juegoPausado)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    private bool EstaEnMinijuegoDialogoOMenu()
    {
        if (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
        {
            return true;
        }

        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual.Contains("Minijuego") || escenaActual.Contains("minijuego") || escenaActual == nombreEscenaMenuPrincipal)
        {
            // Nota: Si estás en el Menú Principal, puedes actualizar el estado del botón aquí también por si acaso
            ActualizarEstadoBotonContinuar();
            return true;
        }

        return false;
    }

    public void PausarJuego()
    {
        juegoPausado = true;
        if (panelPausa != null) panelPausa.SetActive(true);
        if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

        Time.timeScale = 0f;
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

        Time.timeScale = 1f;
    }

    public void BotonContinuarPausa()
    {
        ReanudarJuego();
    }

    public void BotonIntentarSalir()
    {
        if (panelConfirmarSalir != null)
        {
            if (panelPausa != null) panelPausa.SetActive(false);
            panelConfirmarSalir.SetActive(true);
        }
        else
        {
            ConfirmarSalirSinGuardar(true);
        }
    }

    public void ConfirmarSalirSinGuardar(bool confirmar)
    {
        Time.timeScale = 1f;

        if (confirmar)
        {
            if (TransicionController.Instance != null && !string.IsNullOrEmpty(nombreEscenaMenuPrincipal))
            {
                TransicionController.Instance.CargarEscena(nombreEscenaMenuPrincipal);
            }
            else
            {
                SceneManager.LoadScene(nombreEscenaMenuPrincipal);
            }
        }
        else
        {
            if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);
            if (panelPausa != null) panelPausa.SetActive(true);
            Time.timeScale = 0f;
        }
    }

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
        Time.timeScale = 1f;
        TransicionController.Instance.CargarEscena(escenaCambiar);
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
    TransicionCompany: TransicionController.Instance.CargarEscena(escenaCambiar, 5f);
    }

    public void AbrirEnlace(string urlDestino)
    {
        if (!string.IsNullOrEmpty(urlDestino))
        {
            Application.OpenURL(urlDestino);
        }
    }
}