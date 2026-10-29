using UnityEngine;

// Mueve la diana de un lado a otro alrededor de su posición inicial.
public class MoverDiana : MonoBehaviour
{
    [SerializeField] Vector3 recorrido = new Vector3(2f, 0f, 0f);
    [SerializeField] float velocidad = 0.5f;

    Vector3 centro;

    void Start() => centro = transform.position;

    void Update()
    {
        float t = Mathf.PingPong(Time.time * velocidad, 1f);
        transform.position = centro + recorrido * (t - 0.5f);
    }
}
