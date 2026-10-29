using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Controla la ronda: espera, cuenta atrás, puntuación y reinicio.
public class GameManager : MonoBehaviour
{
    public static GameManager Instancia { get; private set; }

    [SerializeField] float duracionPartida = 90f;
    [SerializeField] TMP_Text textoPuntos;
    [SerializeField] TMP_Text textoTiempo;
    [SerializeField] TMP_Text textoMensaje;
    [SerializeField] GameObject botonReiniciar;
    [SerializeField] AudioSource audioInicio;
    [SerializeField] AudioSource audioFin;

    public bool EnJuego { get; private set; }

    int puntos;
    float tiempoRestante;

    void Awake() => Instancia = this;

    void OnDestroy()
    {
        if (Instancia == this) Instancia = null;
    }

    void Start()
    {
        tiempoRestante = duracionPartida;
        botonReiniciar.SetActive(false);
        textoMensaje.text = "Coloca la gema en el pedestal para empezar";
        ActualizarUI();
    }

    void Update()
    {
        if (!EnJuego) return;

        tiempoRestante -= Time.deltaTime;
        if (tiempoRestante <= 0f)
        {
            tiempoRestante = 0f;
            TerminarPartida();
        }
        ActualizarUI();
    }

    // Evento Select Entered del XR Socket Interactor del pedestal
    public void EmpezarPartida()
    {
        if (EnJuego || tiempoRestante <= 0f) return;
        EnJuego = true;
        textoMensaje.text = "¡Dispara a las dianas y derriba las latas!";
        if (audioInicio) audioInicio.Play();
    }

    public void SumarPuntos(int cantidad)
    {
        if (!EnJuego) return;
        puntos += cantidad;
        ActualizarUI();
    }

    void TerminarPartida()
    {
        EnJuego = false;
        textoMensaje.text = $"¡Tiempo! Puntuación final: {puntos}";
        botonReiniciar.SetActive(true);
        if (audioFin) audioFin.Play();

        int record = PlayerPrefs.GetInt("Record", 0);
        if (puntos > record)
        {
            PlayerPrefs.SetInt("Record", puntos);
            textoMensaje.text += "\n¡Nuevo récord!";
        }
    }

    // On Click() del botón "Otra vez"
    public void Reiniciar() =>
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    void ActualizarUI()
    {
        textoPuntos.text = $"Puntos: {puntos}";
        textoTiempo.text = $"Tiempo: {Mathf.CeilToInt(tiempoRestante)}";
    }
}
