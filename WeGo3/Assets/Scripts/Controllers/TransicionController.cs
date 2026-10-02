using System.Collections;
using System.Transactions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransicionController : MonoBehaviour
{
    public static TransicionController Instance { get; private set; }
    public GameObject panelEntrada;

    public GameObject panelSalida;

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

    public void ReproducirSalida()
    {
        StartCoroutine(CorrutinaSalida());
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
    }

    private IEnumerator CorrutinaSalida()
    {
        if (panelEntrada != null) panelEntrada.SetActive(false);

        if (panelSalida != null)
        {
            panelSalida.SetActive(true);

            yield return new WaitForSeconds(1f);
        }
    }
}
