using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CadaOvejaMinigame : MonoBehaviour
{
    public static CadaOvejaMinigame Instance { get; private set; }

    [Header("Generación Aleatoria")]
    public List<GameObject> prefabsParejas = new List<GameObject>();

    public Transform[] puntosDeSpawn;

    public GameObject botonContinuar;

    public TMP_Text textoTiempo;
    public TMP_Text textoInicio;
    public GameObject panelInicio;

    public GameObject temporizadorInicial;

    public GameObject panelVictoria;
    public GameObject panelDerrota;

    public GameObject contador;

    [Header("Configuración de Tiempo")]
    public float tiempoLimite = 15f;
    private float tiempoRestante;
    private bool juegoActivo = false;
    private bool bloquearInteraccion = false; 
    private int parejasRestantes = 3;

    private ItemEmparejable primerSeleccionado;
    private ItemEmparejable segundoSeleccionado;

    private List<GameObject> ovejasInstanciadas = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        panelInicio.SetActive(true);
        temporizadorInicial.SetActive(false);
    }

    public void ComenzarBoton()
    {
        StartCoroutine(ComenzarJuego());
    }

    private IEnumerator ComenzarJuego()
    {
        panelInicio.SetActive(false);
        temporizadorInicial.SetActive(true);
        yield return new WaitForSeconds(2f);
        textoInicio.text = "LISTOS...?";
        yield return new WaitForSeconds(2f);
        textoInicio.text = "¡YA!";
        yield return new WaitForSeconds(1f);
        temporizadorInicial.SetActive(false);
        GenerarTableroAleatorio();
        IniciarJuego();
    }

    private void GenerarTableroAleatorio()
    {
        if (prefabsParejas.Count < 3 || puntosDeSpawn.Length < 6)
        {
            Debug.LogError("Necesitas al menos 3 prefabs y 6 puntos de spawn asignados.");
            return;
        }

        List<int> posicionesDisponibles = new List<int>();
        for (int i = 0; i < puntosDeSpawn.Length; i++)
        {
            posicionesDisponibles.Add(i);
        }

        for (int id = 0; id < prefabsParejas.Count; id++)
        {
            for (int copia = 0; copia < 2; copia++)
            {
                int indiceAleatorio = Random.Range(0, posicionesDisponibles.Count);
                int indiceSpawn = posicionesDisponibles[indiceAleatorio];
                posicionesDisponibles.RemoveAt(indiceAleatorio);

                GameObject nuevoObjeto = Instantiate(prefabsParejas[id], puntosDeSpawn[indiceSpawn].position, Quaternion.identity);

                ovejasInstanciadas.Add(nuevoObjeto);

                ItemEmparejable item = nuevoObjeto.GetComponent<ItemEmparejable>();
                if (item != null)
                {
                    item.parejaID = id;
                }
            }
        }
    }

    public void IniciarJuego()
    {
        tiempoRestante = tiempoLimite;
        parejasRestantes = prefabsParejas.Count;
        juegoActivo = true;
        bloquearInteraccion = false;
        ActualizarTextoTiempo();
    }

    private void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;
        ActualizarTextoTiempo();

        if (tiempoRestante <= 0)
        {
            StartCoroutine(Perder());
        }
    }

    private void ActualizarTextoTiempo()
    {
        if (textoTiempo != null)
        {
            int segundos = Mathf.Max(0, Mathf.CeilToInt(tiempoRestante));
            textoTiempo.text = segundos.ToString();
        }
    }

    public void SeleccionarItem(ItemEmparejable item)
    {
        if (!juegoActivo || bloquearInteraccion || item == primerSeleccionado) return;

        if (primerSeleccionado == null)
        {
            primerSeleccionado = item;
            primerSeleccionado.MarcarComoSeleccionado(true);
        }
        else if (segundoSeleccionado == null)
        {
            segundoSeleccionado = item;
            segundoSeleccionado.MarcarComoSeleccionado(true);
            bloquearInteraccion = true; 
            StartCoroutine(VerificarPareja());
        }
    }

    private IEnumerator VerificarPareja()
    {
        yield return new WaitForSeconds(0.3f);

        if (primerSeleccionado.parejaID == segundoSeleccionado.parejaID)
        {
            primerSeleccionado.BloquearEmparejado();
            segundoSeleccionado.BloquearEmparejado();
            parejasRestantes--;

            if (parejasRestantes <= 0)
            {
                StartCoroutine(Ganar());
            }
        }
        else
        {
            primerSeleccionado.MarcarComoSeleccionado(false);
            segundoSeleccionado.MarcarComoSeleccionado(false);
        }

        primerSeleccionado = null;
        segundoSeleccionado = null;
        bloquearInteraccion = false; 
    }

    private IEnumerator Ganar()
    {
        juegoActivo = false;
        contador.SetActive(false);
        panelVictoria.SetActive(true);
        yield return new WaitForSeconds(2f);
        botonContinuar.SetActive(true);
    }

    private IEnumerator Perder()
    {
        juegoActivo = false;
        contador.SetActive(false);

        foreach (GameObject oveja in ovejasInstanciadas)
        {
            if (oveja != null)
            {
                oveja.SetActive(false);
            }
        }

        panelDerrota.SetActive(true);
        yield return new WaitForSeconds(2f);
        botonContinuar.SetActive(true);
    }
}