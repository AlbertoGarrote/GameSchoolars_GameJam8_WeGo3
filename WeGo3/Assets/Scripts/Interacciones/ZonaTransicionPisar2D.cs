using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZonaTransicionPisar2D : MonoBehaviour
{
    [Header("Escena Destino")]
    [SerializeField] private string nombreEscenaACargar;

    [Header("Posición al Aparecer")]
    [SerializeField] private PuntoAparicionSO memoriaPosicion;
    [SerializeField] private Vector2 posicionEnNuevaEscena;

    [Header("Ajustes")]
    [SerializeField] private float tiempoEsperaAlAparecer = 1.5f;

    private bool puedeTransicionar = false;

    private IEnumerator Start()
    {
        puedeTransicionar = false;
        yield return new WaitForSeconds(tiempoEsperaAlAparecer);
        puedeTransicionar = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!puedeTransicionar || string.IsNullOrEmpty(nombreEscenaACargar)) return;

        if (collision.CompareTag("Player") || collision.CompareTag("Caminante"))
        {
            puedeTransicionar = false;

            if (memoriaPosicion != null)
            {
                memoriaPosicion.posicionDestino = posicionEnNuevaEscena;
                memoriaPosicion.usarPosicionGuardada = true;
            }

            TransicionController.Instance.CargarEscena(nombreEscenaACargar);
        }
    }
}