using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    [Header("Componentes")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Referencia al Canvas")]
    [SerializeField] private MenuController menuController; 

    private void OnEnable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += AlTerminarVideo;
        }
    }

    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }

    // Este método se ejecuta automáticamente cuando el vídeo llega al final
    private void AlTerminarVideo(VideoPlayer vp)
    {
        Debug.Log("El vídeo ha terminado. Llamando al método del Canvas...");

        if (menuController != null)
        {
            menuController.CambiarEscena("Casa");
        }
        else
        {
            Debug.LogError("No se ha asignado la referencia al script del Canvas en el Inspector.");
        }
    }
}
