using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionController : MonoBehaviour
{
    public static TransicionController Instance { get; private set; }

    [Header("UI Paneles")]
    [SerializeField] private GameObject panelEntrada;
    [SerializeField] private GameObject panelSalida;

    public bool CambiandoEscena { get; private set; }

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(CorrutinaEntrada());
    }

    public void CargarEscena(string nombreEscena, float duracionSalida = 1f)
    {
        if (CambiandoEscena || string.IsNullOrEmpty(nombreEscena)) return;
        StartCoroutine(CorrutinaTransicionCompleta(nombreEscena, duracionSalida));
    }

    private IEnumerator CorrutinaTransicionCompleta(string nombreEscena, float duracionSalida)
    {
        CambiandoEscena = true;

        if (panelEntrada != null) panelEntrada.SetActive(false);
        if (panelSalida != null) panelSalida.SetActive(true);

        yield return new WaitForSeconds(duracionSalida);

        SceneManager.LoadScene(nombreEscena);
    }

    private IEnumerator CorrutinaEntrada()
    {
        if (panelSalida != null) panelSalida.SetActive(false);

        if (panelEntrada != null)
        {
            panelEntrada.SetActive(true);
            yield return new WaitForSeconds(1f);
            panelEntrada.SetActive(false);
        }

        CambiandoEscena = false;
    }
}