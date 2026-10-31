using Unity.Cinemachine;
using System.Collections;
using UnityEngine;

public class CinematicaFinal : MonoBehaviour
{
    [Header("Referencias de Diálogo")]
    [SerializeField] private DialogoSO dialogoCinematica;
    [SerializeField] private string nombreFlagJuegoTerminado = "juego_terminado";

    [Header("Referencias de Cámaras")]
    [SerializeField] private CinemachineCamera camaraCinematica;
    [SerializeField] private int prioridadAlta = 20;

    [Header("Referencias de Audio y UI")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip musicaFinal;
    [SerializeField] private GameObject objetoTextoDifuminado;          // El objeto con la animación de texto

    [Header("Créditos y Movimiento")]
    [SerializeField] private RectTransform objetoCreditos;              // El texto/panel de los créditos (si es UI, usa RectTransform)
    [SerializeField] private float velocidadMovimientoCreditos = 50f;   // Velocidad a la que suben los créditos
    [SerializeField] private float posicionYObjetivo = 800f;            // Coordenada Y exacta en la que cambiará de escena
    [SerializeField] private string nombreEscenaSiguiente = "MenuPrincipal"; // Escena a la que se viajará al terminar

    [Header("Configuración de Tiempos")]
    [SerializeField] private float tiempoEsperaAntesDelTexto = 2f;
    [SerializeField] private float tiempoEsperaAntesDeCamara = 3f;
    [SerializeField] private float tiempoEsperaAntesDeCreditos = 3f;    // 3 segundos después de que se acerque la cámara

    private bool cinematicaIniciada = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!cinematicaIniciada && (collision.CompareTag("Player") || collision.CompareTag("Caminante")))
        {
            cinematicaIniciada = true;
            StartCoroutine(SecuenciaCinematicaFinal());
        }
    }

    private IEnumerator SecuenciaCinematicaFinal()
    {
        MusicSoundController.Instance.DetenerMusica();

        // PASO 1: Iniciar el diálogo y esperar a que termine
        if (DialogoManager.Instance != null && dialogoCinematica != null)
        {
            DialogoManager.Instance.IniciarDialogo(dialogoCinematica);
        }

        yield return null;

        while (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
        {
            yield return null;
        }

        // --- EL DIÁLOGO HA TERMINADO ---

        // Activar la flag de juego terminado inmediatamente aquí
        if (EstadoJuegoManager.Instance != null)
        {
            EstadoJuegoManager.Instance.ActivarFlag(nombreFlagJuegoTerminado);
        }

        // PASO 2: Empieza a sonar la música
        if (audioSource != null && musicaFinal != null)
        {
            audioSource.clip = musicaFinal;
            audioSource.Play();
        }

        // PASO 3: Pasados 2 segundos, aparece el texto difuminado
        yield return new WaitForSeconds(tiempoEsperaAntesDelTexto);

        MusicSoundController.Instance.ReproducerMusica("Creditos");
        if (objetoTextoDifuminado != null)
        {
            objetoTextoDifuminado.SetActive(true);
        }

        // PASO 4: Pasados 3 segundos más, la cámara de Cinemachine gana prioridad
        yield return new WaitForSeconds(tiempoEsperaAntesDeCamara);

        if (camaraCinematica != null)
        {
            camaraCinematica.Priority = prioridadAlta;
        }

        // PASO 5: Pasados 3 segundos después de que se acerque la cámara
        yield return new WaitForSeconds(tiempoEsperaAntesDeCreditos);

        // Cambiar el PlayerPref pedido
        PlayerPrefs.SetInt("juegoTerminado", 1);
        PlayerPrefs.Save();

        // Activar y mover los créditos hacia arriba hasta llegar a la Y objetivo
        if (objetoCreditos != null)
        {
            objetoCreditos.gameObject.SetActive(true);

            // Bucle que mueve los créditos fotograma a fotograma
            while (objetoCreditos.anchoredPosition.y < posicionYObjetivo)
            {
                objetoCreditos.anchoredPosition += Vector2.up * velocidadMovimientoCreditos * Time.deltaTime;
                yield return null;
            }
        }

        // PASO 6: Al llegar a la Y en concreto, cambiar de escena con TransicionController
        if (TransicionController.Instance != null && !string.IsNullOrEmpty(nombreEscenaSiguiente))
        {
            TransicionController.Instance.CargarEscena(nombreEscenaSiguiente);
        }

        // Destruir el trigger
        Destroy(gameObject);
    }
}