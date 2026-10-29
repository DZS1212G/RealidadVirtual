using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

// Dispara un proyectil cuando el jugador aprieta el gatillo con la pistola agarrada.
[RequireComponent(typeof(XRGrabInteractable))]
public class Pistola : MonoBehaviour
{
    [SerializeField] Rigidbody proyectilPrefab;
    [SerializeField] Transform boca;
    [SerializeField] float velocidad = 25f;
    [SerializeField] AudioSource audioDisparo;

    XRGrabInteractable grab;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        // "activated" salta al pulsar el gatillo mientras el objeto está agarrado
        grab.activated.AddListener(Disparar);
    }

    void OnDestroy() => grab.activated.RemoveListener(Disparar);

    void Disparar(ActivateEventArgs args)
    {
        Rigidbody bala = Instantiate(proyectilPrefab, boca.position, boca.rotation);
        bala.linearVelocity = boca.forward * velocidad;
        Destroy(bala.gameObject, 3f);

        if (audioDisparo) audioDisparo.Play();

        // Vibración en el mando que sostiene la pistola
        if (args.interactorObject is XRBaseInputInteractor mando)
            mando.SendHapticImpulse(0.5f, 0.1f);
    }
}
