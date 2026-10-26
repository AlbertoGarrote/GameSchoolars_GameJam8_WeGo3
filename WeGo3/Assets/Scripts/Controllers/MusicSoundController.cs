using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicSoundController : MonoBehaviour
{
    public static MusicSoundController Instance { get; private set; }

    [System.Serializable]
    public struct SonidoAudio
    {
        public string nombreIdentificador;
        public AudioClip clip;
        [Range(0f, 1f)] public float volumen;
        [Range(0.1f, 3f)] public float pitch;
        public bool esMusica; // Si es true va al canal de música, si es false a efectos (SFX)
        public bool loop;     // Si debe repetirse en bucle
    }

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource fuenteMusica;
    [SerializeField] private AudioSource fuenteSFX;
    [SerializeField] private AudioMixer audioMixer; // Opcional, para conectar los Sliders de Volumen del menú de opciones

    [Header("Base de Datos de Sonidos")]
    [SerializeField] private List<SonidoAudio> catalogoSonidos = new List<SonidoAudio>();


    [System.Serializable]
    public struct MusicaPorEscena
    {
        public string nombreEscena;
        public string nombreSonidoMusica;
    }
    [SerializeField] private List<MusicaPorEscena> musicaPorEscenaList = new List<MusicaPorEscena>();

    private Dictionary<string, SonidoAudio> diccionarioSonidos = new Dictionary<string, SonidoAudio>();

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

        InicializarCatalogo();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InicializarCatalogo()
    {
        diccionarioSonidos.Clear();
        foreach (var sonido in catalogoSonidos)
        {
            if (!diccionarioSonidos.ContainsKey(sonido.nombreIdentificador))
            {
                diccionarioSonidos.Add(sonido.nombreIdentificador, sonido);
            }
            else
            {
                Debug.LogWarning($"[MusicSoundController] El sonido con nombre '{sonido.nombreIdentificador}' está repetido en el catálogo.");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Al cargar una escena, comprobamos si tiene música asignada automáticamente
        foreach (var item in musicaPorEscenaList)
        {
            if (item.nombreEscena == scene.name)
            {
                ReproducerMusica(item.nombreSonidoMusica);
                return;
            }
        }
    }

    // --- MÉTODOS PÚBLICOS PARA LLAMAR DESDE CUALQUIER SCRIPT ---

    /// <summary>
    /// Reproduce un efecto de sonido (SFX) por su identificador en string.
    /// </summary>
    public void ReproducirSFX(string nombreSonido)
    {
        if (diccionarioSonidos.TryGetValue(nombreSonido, out SonidoAudio sonido))
        {
            if (!sonido.esMusica)
            {
                fuenteSFX.pitch = sonido.pitch;
                fuenteSFX.PlayOneShot(sonido.clip, sonido.volumen);
            }
            else
            {
                Debug.LogWarning($"[MusicSoundController] Intentaste reproducir '{nombreSonido}' como SFX pero está configurado como Música.");
            }
        }
        else
        {
            Debug.LogWarning($"[MusicSoundController] No se encontró el sonido SFX: '{nombreSonido}'");
        }
    }

    /// <summary>
    /// Reproduce un tema musical por su identificador en string. (Reemplaza la música actual).
    /// </summary>
    public void ReproducerMusica(string nombreSonido)
    {
        if (diccionarioSonidos.TryGetValue(nombreSonido, out SonidoAudio sonido))
        {
            if (sonido.esMusica)
            {
                // Si ya está sonando exactamente la misma pista, no la reiniciamos
                if (fuenteMusica.isPlaying && fuenteMusica.clip == sonido.clip) return;

                fuenteMusica.Stop();
                fuenteMusica.clip = sonido.clip;
                fuenteMusica.volume = sonido.volumen;
                fuenteMusica.pitch = sonido.pitch;
                fuenteMusica.loop = sonido.loop;
                fuenteMusica.Play();
            }
            else
            {
                Debug.LogWarning($"[MusicSoundController] Intentaste reproducir '{nombreSonido}' como Música pero está configurado como SFX.");
            }
        }
        else
        {
            Debug.LogWarning($"[MusicSoundController] No se encontró el tema musical: '{nombreSonido}'");
        }
    }

    /// <summary>
    /// Detiene la música actual.
    /// </summary>
    public void DetenerMusica()
    {
        if (fuenteMusica.isPlaying)
        {
            fuenteMusica.Stop();
        }
    }

    // --- MÉTODOS PREPARADOS PARA EL MENÚ DE OPCIONES (AUDIO MIXER) ---

    /// <summary>
    /// Ajusta el volumen general de la Música (Valor de 0.0001 a 1 desde un Slider, se convierte a decibelios).
    /// </summary>
    public void CambiarVolumenMusica(float valorSlider)
    {
        if (audioMixer != null)
        {
            // Si el slider está en el mínimo (0), silenciamos por completo con -80 dB
            // Si tiene valor, aplicamos la curva logarítmica estándar de Unity (-80dB a 0dB)
            float decibelios = (valorSlider <= 0.0001f) ? -80f : Mathf.Log10(valorSlider) * 20f;
            audioMixer.SetFloat("ParametroVolumenMusica", decibelios);
        }
    }

    public void CambiarVolumenSFX(float valorSlider)
    {
        if (audioMixer != null)
        {
            float decibelios = (valorSlider <= 0.0001f) ? -80f : Mathf.Log10(valorSlider) * 20f;
            audioMixer.SetFloat("ParametroVolumenSFX", decibelios);
        }
    }
}