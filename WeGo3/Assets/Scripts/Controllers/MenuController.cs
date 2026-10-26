using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    [SerializeField] private PuntoAparicionSO memoriaPosicion;

    [Header("Configuración de Pausa")]
    [SerializeField] private GameObject panelPausa;               // Panel principal de pausa
    [SerializeField] private GameObject panelConfirmarSalir;      // Panel de "¿Estás seguro? No se guardará..."
    [SerializeField] private Slider sliderVolumenMusica;          // Slider para la música
    [SerializeField] private Slider sliderVolumenSFX;             // Slider para los efectos de sonido
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuPrincipal"; // Escena a la que vuelve al confirmar salir

    private bool juegoPausado = false;

    void Start()
    {
        // Asegurarnos de que los paneles de pausa estén ocultos al iniciar la escena
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

        // Inicializar los valores de los sliders si MusicSoundController está presente (opcional)
        // (Si usas un script global de audio, puedes sincronizar aquí los valores iniciales)
    }

    void Update()
    {
        // Comprobar la tecla Escape para pausar o despausar
        ComprobarTeclaPausa();
    }

    private void ComprobarTeclaPausa()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // REQUISITO: Comprobar que NO se esté en un minijuego ni en un diálogo
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
        // 1. Comprobar si hay un diálogo abierto
        if (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
        {
            return true;
        }

        // 2. Comprobar si estamos en un minijuego (puedes verificar por nombre de escena o si existe algún script de minijuego activo)
        // Aquí detectamos si la escena actual contiene la palabra "Minijuego" en su nombre
        string escenaActual = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (escenaActual.Contains("Minijuego") || escenaActual.Contains("minijuego"))
        {
            return true;
        }

        return false;
    }

    public void PausarJuego()
    {
        juegoPausado = true;
        if (panelPausa != null) panelPausa.SetActive(true);

        // Ocultar el panel de confirmación por si acaso se quedó abierto
        if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

        Time.timeScale = 0f; // Congela el tiempo del juego
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);

        Time.timeScale = 1f; // Restaura el tiempo normal
    }

    // --- MÉTODOS PARA LOS BOTONES DE LA PAUSA ---

    public void BotonContinuarPausa()
    {
        ReanudarJuego();
    }

    public void BotonIntentarSalir()
    {
        // Muestra el panel de advertencia antes de salir sin guardar
        if (panelConfirmarSalir != null)
        {
            if (panelPausa != null) panelPausa.SetActive(false);
            panelConfirmarSalir.SetActive(true);
        }
        else
        {
            // Si por lo que sea no hay panel de confirmación asignado, sale directamente
            ConfirmarSalirSinGuardar(true);
        }
    }

    public void ConfirmarSalirSinGuardar(bool confirmar)
    {
        Time.timeScale = 1f; // Restaurar el tiempo antes de cambiar de escena

        if (confirmar)
        {
            if (TransicionController.Instance != null && !string.IsNullOrEmpty(nombreEscenaMenuPrincipal))
            {
                TransicionController.Instance.CargarEscena(nombreEscenaMenuPrincipal);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(nombreEscenaMenuPrincipal);
            }
        }
        else
        {
            // Si dice que "No", ocultamos la advertencia y volvemos al menú de pausa principal
            if (panelConfirmarSalir != null) panelConfirmarSalir.SetActive(false);
            if (panelPausa != null) panelPausa.SetActive(true);
            Time.timeScale = 0f; // Volver a congelar
        }
    }

    // --- MÉTODOS PARA LOS SLIDERS DE AUDIO (Vinculados con MusicSoundController) ---

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

    // --- MÉTODOS ANTERIORES DEL MENÚ PRINCIPAL ---

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
        Time.timeScale = 1f; // Asegurar tiempo normal al cambiar de escena
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
        TransicionController.Instance.CargarEscena(escenaCambiar, 5f);
    }

    public void AbrirEnlace(string urlDestino)
    {
        if (!string.IsNullOrEmpty(urlDestino))
        {
            Application.OpenURL(urlDestino);
        }
    }
}