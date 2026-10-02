using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CadaOvejaMinigame : MonoBehaviour
{
    public static CadaOvejaMinigame Instance { get; private set; }

    [Header("Generación Aleatoria")]
    public List<GameObject> prefabsParejas = new List<GameObject>();

    public Transform[] puntosDeSpawn;

    public GameObject botonContinuar;

    [Header("Configuración de Tiempo")]
    public float tiempoLimite = 15f;
    private float tiempoRestante;
    private bool juegoActivo = false;
    private bool bloquearInteraccion = false; // <-- EVITA CLICS MIENTRAS COMPRUEBA
    private int parejasRestantes = 3;

    private ItemEmparejable primerSeleccionado;
    private ItemEmparejable segundoSeleccionado;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
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
    }

    private void Update()
    {
        if (!juegoActivo) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0)
        {
            juegoActivo = false;
            Debug.Log("¡Se acabó el tiempo!");
            botonContinuar.SetActive(true);

        }
    }

    public void SeleccionarItem(ItemEmparejable item)
    {
        // Ignora el clic si el juego acabó, si se está comprobando una pareja, o si clicas el mismo objeto
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
            bloquearInteraccion = true; // <-- BLOQUEA NUEVOS CLICS
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
                juegoActivo = false;
                Debug.Log("¡Ganaste!");
                botonContinuar.SetActive(true);
            }
        }
        else
        {
            primerSeleccionado.MarcarComoSeleccionado(false);
            segundoSeleccionado.MarcarComoSeleccionado(false);
        }

        primerSeleccionado = null;
        segundoSeleccionado = null;
        bloquearInteraccion = false; // <-- DESBLOQUEA INTERACCIÓN
    }
}