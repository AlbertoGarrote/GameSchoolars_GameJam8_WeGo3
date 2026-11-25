using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPausaController : MonoBehaviour
{
    public static MenuPausaController Instance { get; private set; }

    [Header("Configuración de Pausa")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject panelConfirmarSalir;
    [SerializeField] private Slider sliderVolumenMusica;
    [SerializeField] private Slider sliderVolumenSFX;
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuPrincipal";
    [SerializeField] private string nombreEscenaMenuFinal = "MenuPrincipal3"; // Menú al que irá si el juego terminó
    [SerializeField] private string nombreEscenaMenuDLC = "MenuPrincipal4";   // Menú al que irá si se completó el DLC

    private const string PREF_JUEGO_TERMINADO = "juegoTerminado";
    private const string PREF_JUEGO_TERMINADO_DLC = "juegoTerminado_dlc";

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
    }

    void Update()
    {
        ComprobarTeclaPausa();
    }

    private void ComprobarTeclaPausa()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (EstaEnMinijuegoODialogo()) return;

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

    private bool EstaEnMinijuegoODialogo()
    {
        if (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
        {
            return true;
        }

        string escenaActual = SceneManager.GetActiveScene().name;
        if (escenaActual.Contains("Minijuego") || escenaActual.Contains("minijuego") || escenaActual.Contains("MenuPrincipal") || escenaActual.Contains("Minigame") || escenaActual.Contains("minigame"))
        {
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
            juegoPausado = false;

            if (panelPausa != null) panelPausa.SetActive(false);
            if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

            // Comprobamos si el DLC ya se ha terminado
            bool juegoTerminadoDLC = PlayerPrefs.GetInt(PREF_JUEGO_TERMINADO_DLC, 0) == 1;
            if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada(PREF_JUEGO_TERMINADO_DLC))
            {
                juegoTerminadoDLC = true;
            }

            // Comprobamos si el juego base ya se ha terminado
            bool juegoTerminado = PlayerPrefs.GetInt(PREF_JUEGO_TERMINADO, 0) == 1;
            if (EstadoJuegoManager.Instance != null && EstadoJuegoManager.Instance.EstaFlagActivada("juego_terminado"))
            {
                juegoTerminado = true;
            }

            // Jerarquía de decisión de menús (DLC primero)
            string escenaDestino = nombreEscenaMenuPrincipal;
            if (juegoTerminadoDLC)
            {
                escenaDestino = nombreEscenaMenuDLC;
            }
            else if (juegoTerminado)
            {
                escenaDestino = nombreEscenaMenuFinal;
            }

            if (TransicionController.Instance != null && !string.IsNullOrEmpty(escenaDestino))
            {
                TransicionController.Instance.CargarEscena(escenaDestino);
            }
            else if (!string.IsNullOrEmpty(escenaDestino))
            {
                SceneManager.LoadScene(escenaDestino);
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
}