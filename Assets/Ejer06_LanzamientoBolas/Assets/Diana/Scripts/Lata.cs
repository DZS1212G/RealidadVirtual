using UnityEngine;

// Suma puntos una sola vez cuando la lata se vuelca o se cae de la peana.
public class Lata : MonoBehaviour
{
    [SerializeField] int puntos = 5;
    [SerializeField] AudioClip sonidoCaida;

    Vector3 posicionInicial;
    bool contada;

    void Start() => posicionInicial = transform.position;

    void Update()
    {
        if (contada || GameManager.Instancia == null) return;

        bool volcada = Vector3.Dot(transform.up, Vector3.up) < 0.5f;
        bool caida = transform.position.y < posicionInicial.y - 0.2f;
        if (!volcada && !caida) return;

        // Si se cae antes de empezar la partida no puntúa
        contada = true;
        if (!GameManager.Instancia.EnJuego) return;

        GameManager.Instancia.SumarPuntos(puntos);
        if (sonidoCaida) AudioSource.PlayClipAtPoint(sonidoCaida, transform.position);
    }
}
