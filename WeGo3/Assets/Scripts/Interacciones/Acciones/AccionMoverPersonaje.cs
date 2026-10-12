using System;
using System.Collections;
using UnityEngine;

public class AccionMoverPersonaje : AccionSmartObject
{
    [Header("Configuración del Movimiento")]
    [SerializeField] private Transform personajeAMover;
    [SerializeField] private Vector2 posicionDestino;
    [SerializeField] private float velocidad = 3f;

    [Header("Animación (Opcional)")]
    [SerializeField] private Animator animatorPersonaje;
    [SerializeField] private string parametroCaminando = "IsWalking";

    public override void EjecutarAccion(GameObject interactor, Action alTerminar)
    {
        // Si no se asigna un personaje específico, intentamos usar al que interactúa
        Transform objetivo = personajeAMover != null ? personajeAMover : interactor.transform;

        if (objetivo != null)
        {
            StartCoroutine(RutinaMover(objetivo, alTerminar));
        }
        else
        {
            alTerminar?.Invoke();
        }
    }

    private IEnumerator RutinaMover(Transform objetivo, Action alTerminar)
    {
        // Activar animación de caminar si existe
        if (animatorPersonaje != null && !string.IsNullOrEmpty(parametroCaminando))
        {
            animatorPersonaje.SetBool(parametroCaminando, true);
        }

        Vector3 destino3D = new Vector3(posicionDestino.x, posicionDestino.y, objetivo.position.z);

        while (Vector3.Distance(objetivo.position, destino3D) > 0.05f)
        {
            objetivo.position = Vector3.MoveTowards(
                objetivo.position,
                destino3D,
                velocidad * Time.deltaTime
            );
            yield return null; // Esperar al siguiente frame
        }

        // Asegurar posición final exacta
        objetivo.position = destino3D;

        // Desactivar animación de caminar
        if (animatorPersonaje != null && !string.IsNullOrEmpty(parametroCaminando))
        {
            animatorPersonaje.SetBool(parametroCaminando, false);
        }

        // Notificar que el movimiento ha terminado para continuar con la siguiente acción
        alTerminar?.Invoke();
    }
}