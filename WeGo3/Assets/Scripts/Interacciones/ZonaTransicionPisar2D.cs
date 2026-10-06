using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ZonaTransicionPisar2D : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [SerializeField] private string nombreEscenaACargar;

    private bool activado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (activado || string.IsNullOrEmpty(nombreEscenaACargar)) return;

        if (collision.CompareTag("Player"))
        {
            activado = true;
            TransicionController.Instance.CargarEscena(nombreEscenaACargar);
        }
    }
}