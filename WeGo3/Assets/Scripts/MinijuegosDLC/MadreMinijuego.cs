using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MadreMinijuego : MonoBehaviour
{
    [Header("Diálogo Inicial")]
    [SerializeField] private DialogoSO dialogoAlEntrar;
    [Tooltip("Pequeña pausa tras cargar la escena antes de lanzar el diálogo.")]
    [SerializeField] private float retrasoInicial = 0.25f;

    [Header("Botón Tras Terminar el Diálogo")]
    [SerializeField] private Button botonContinuar;

    [Header("Objetos y Movimiento")]
    [SerializeField] private GameObject objeto1;
    [SerializeField] private Transform destinoObjeto1;
    [SerializeField] private GameObject objeto2;
    [SerializeField] private Transform destinoObjeto2;
    [SerializeField] private float velocidadMovimiento = 4f;
    [Tooltip("Segundos que esperan los dos objetos tras llegar a su destino antes de desaparecer.")]
    [SerializeField] private float tiempoAntesDeDesaparecer = 2f;

    [Header("Aparición y Diálogo Final")]
    [Tooltip("GameObject que aparecerá cuando los dos objetos desaparezcan.")]
    [SerializeField] private GameObject objetoAparecer;
    [Tooltip("Segundos que esperamos tras aparecer el objeto antes de lanzar el diálogo final (o cambiar de escena si no hay diálogo final).")]
    [SerializeField] private float segundosAntesDeCambiarEscena = 2f;
    [Tooltip("Diálogo que se lanzará al final; al terminarlo se cambiará de escena. Si se deja vacío, cambia directo.")]
    [SerializeField] private DialogoSO dialogoFinal;

    [Header("Destino y Flag al Terminar")]
    [SerializeField] private string flagAActivarAlTerminar;
    [SerializeField] private string nombreEscenaDestino;
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;

    private bool dialogoIniciado = false;
    private bool dialogoTerminado = false;
    private bool secuenciaIniciada = false;

    private void Awake()
    {
        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(false);
            botonContinuar.onClick.AddListener(AlPulsarBoton);
        }

        if (objetoAparecer != null)
        {
            objetoAparecer.SetActive(false);
        }
    }

    private void Start()
    {
        StartCoroutine(IniciarSecuencia());
    }

    private IEnumerator IniciarSecuencia()
    {
        if (retrasoInicial > 0f)
        {
            yield return new WaitForSeconds(retrasoInicial);
        }

        float tiempoEspera = 0f;
        while (DialogoManager.Instance == null && tiempoEspera < 3f)
        {
            tiempoEspera += Time.deltaTime;
            yield return null;
        }

        if (dialogoAlEntrar != null && DialogoManager.Instance != null)
        {
            dialogoIniciado = true;
            DialogoManager.Instance.IniciarDialogo(dialogoAlEntrar);
        }
        else
        {
            dialogoTerminado = true;
            MostrarBotonContinuar();
        }
    }

    private void Update()
    {
        if (!dialogoIniciado || dialogoTerminado || secuenciaIniciada) return;

        if (DialogoManager.Instance != null && !DialogoManager.Instance.EnDialogo)
        {
            dialogoTerminado = true;
            MostrarBotonContinuar();
        }
    }

    private void MostrarBotonContinuar()
    {
        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(true);
        }
        else
        {
            // Por seguridad, si no asignaste botón en el Inspector, arranca la secuencia directamente
            AlPulsarBoton();
        }
    }

    public void AlPulsarBoton()
    {
        if (secuenciaIniciada) return;
        secuenciaIniciada = true;

        if (botonContinuar != null)
        {
            botonContinuar.gameObject.SetActive(false);
        }

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.ReproducirSFX("Interactuar");
        }

        StartCoroutine(EjecutarSecuenciaCompleta());
    }

    private IEnumerator EjecutarSecuenciaCompleta()
    {
        // 1. Mover ambos objetos hacia sus puntos de destino en paralelo
        while (NecesitaMoverse(objeto1, destinoObjeto1) || NecesitaMoverse(objeto2, destinoObjeto2))
        {
            MoverHaciaDestino(objeto1, destinoObjeto1);
            MoverHaciaDestino(objeto2, destinoObjeto2);
            yield return null;
        }

        FijarEnDestino(objeto1, destinoObjeto1);
        FijarEnDestino(objeto2, destinoObjeto2);

        // 2. Esperar 2 segundos (o el valor configurado) antes de desaparecer
        if (tiempoAntesDeDesaparecer > 0f)
        {
            yield return new WaitForSeconds(tiempoAntesDeDesaparecer);
        }

        if (MusicSoundController.Instance != null)
        {
            MusicSoundController.Instance.DetenerMusica();
            MusicSoundController.Instance.ReproducirSFX("Acierto");
        }

        // 3. Desaparecen los dos objetos y aparece el tercero
        if (objeto1 != null) objeto1.SetActive(false);
        if (objeto2 != null) objeto2.SetActive(false);
        if (objetoAparecer != null) objetoAparecer.SetActive(true);

        // 4. Espera configurable en el editor tras aparecer el objeto
        if (segundosAntesDeCambiarEscena > 0f)
        {
            yield return new WaitForSeconds(segundosAntesDeCambiarEscena);
        }

        // 5. Lanzar diálogo final si está asignado y esperar a que termine
        if (dialogoFinal != null && DialogoManager.Instance != null)
        {
            DialogoManager.Instance.IniciarDialogo(dialogoFinal);
            yield return null;

            while (DialogoManager.Instance != null && DialogoManager.Instance.EnDialogo)
            {
                yield return null;
            }
        }

        EjecutarTransicion();
    }

    private bool NecesitaMoverse(GameObject obj, Transform destino)
    {
        if (obj == null || destino == null || !obj.activeInHierarchy) return false;
        return Vector3.Distance(obj.transform.position, destino.position) > 0.01f;
    }

    private void MoverHaciaDestino(GameObject obj, Transform destino)
    {
        if (!NecesitaMoverse(obj, destino)) return;

        obj.transform.position = Vector3.MoveTowards(
            obj.transform.position,
            destino.position,
            velocidadMovimiento * Time.deltaTime
        );
    }

    private void FijarEnDestino(GameObject obj, Transform destino)
    {
        if (obj != null && destino != null)
        {
            obj.transform.position = destino.position;
        }
    }

    private void EjecutarTransicion()
    {
        if (!string.IsNullOrEmpty(flagAActivarAlTerminar) && EstadoJuegoManager.Instance != null)
        {
            EstadoJuegoManager.Instance.ActivarFlag(flagAActivarAlTerminar);
        }

        if (memoriaPosicion != null)
        {
            memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
            memoriaPosicion.usarPosicionGuardada = true;
        }

        if (!string.IsNullOrEmpty(nombreEscenaDestino) && TransicionController.Instance != null)
        {
            TransicionController.Instance.CargarEscena(nombreEscenaDestino);
        }
    }
}