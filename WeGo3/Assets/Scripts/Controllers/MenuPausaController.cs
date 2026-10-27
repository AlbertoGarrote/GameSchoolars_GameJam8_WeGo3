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
}