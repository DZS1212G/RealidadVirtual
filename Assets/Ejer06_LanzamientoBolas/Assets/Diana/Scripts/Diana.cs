using System.Collections;
using UnityEngine;

// Suma puntos cuando la golpea un proyectil y reaparece al cabo de unos segundos.
public class Diana : MonoBehaviour
{
    [SerializeField] int puntos = 10;
    [SerializeField] float tiempoReaparicion = 2f;
    [SerializeField] AudioClip sonidoImpacto;

    Collider col;
    Renderer[] renderers;

    void Awake()
    {
        col = GetComponent<Collider>();
        renderers = GetComponentsInChildren<Renderer>();
    }

    void OnCollisionEnter(Collision choque)
    {
        if (!choque.collider.CompareTag("Proyectil")) return;
        if (GameManager.Instancia == null || !GameManager.Instancia.EnJuego) return;

        GameManager.Instancia.SumarPuntos(puntos);
        if (sonidoImpacto) AudioSource.PlayClipAtPoint(sonidoImpacto, transform.position);
        StartCoroutine(Reaparecer());
    }

    IEnumerator Reaparecer()
    {
        Mostrar(false);
        yield return new WaitForSeconds(tiempoReaparicion);
        Mostrar(true);
    }

    void Mostrar(bool visible)
    {
        col.enabled = visible;
        foreach (var r in renderers) r.enabled = visible;
    }
}
