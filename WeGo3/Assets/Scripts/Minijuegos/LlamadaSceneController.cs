using System.Collections;
using TMPro;
using UnityEngine;

public class LlamadaSceneController : MonoBehaviour
{
    public TMP_Text textoMadre;
    public GameObject llamadaMadre;
    public GameObject panelNegro;
    public GameObject panelFondo;
    public GameObject wegogymsonEspalda;

    public Transform puntoFueraPantalla;
    public float velocidadMovimiento = 15f;

    public MenuController menuController;


    void Start()
    {
        StartCoroutine(ComenzarSecuencia());
    }

    private IEnumerator ComenzarSecuencia()
    {
        llamadaMadre.SetActive(true);
        yield return new WaitForSeconds(2f);
        textoMadre.text = "¡NIÑO! Siempre igual. Estando con el ordenador perdiendo el tiempo en vez de hacer algo productivo...";
        yield return new WaitForSeconds(8f);
        textoMadre.text = "Y me ha dicho la Conchi que el otro día te vio por la ventana tomando cosas raras. ¡AY, QUÉ DISGUSTO MÁS GRANDE CUANDO ME LO DIJO!";
        yield return new WaitForSeconds(8);
        textoMadre.text = "Hijo, ¿no te cansas de estar todo el rato dando la nota? Que si la obsesión con el gimnasio, que si ahora esto... Me preocupas...";
        yield return new WaitForSeconds(8);
        textoMadre.text = "¡Y TIRA LA BASURA! No volveré a entrar a tu casa hasta que la saques del congelador. Es nauseabundo... ES-PA-BI-LA.";
        yield return new WaitForSeconds(8);
        textoMadre.text = "*Colgar*";
        yield return new WaitForSeconds(2f);
        textoMadre.text = " ";
        llamadaMadre.SetActive(false);
        yield return new WaitForSeconds(2f);
        panelNegro.SetActive(false);
        panelFondo.SetActive(true);
        yield return new WaitForSeconds(6f);

        Vector3 destino = puntoFueraPantalla.transform.position;

        while (Vector3.Distance(wegogymsonEspalda.transform.position, destino) > 0.1f)
        {
            wegogymsonEspalda.transform.position = Vector3.MoveTowards(
                wegogymsonEspalda.transform.position,
                destino,
                velocidadMovimiento * Time.deltaTime
            );
            yield return null; 
        }

        yield return new WaitForSeconds(2f);
        menuController.cambiarCondicion("EscenaInicial");

    }
}
