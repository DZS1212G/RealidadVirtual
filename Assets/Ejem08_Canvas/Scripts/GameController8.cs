using TMPro;
using UnityEngine;

public class GameController8 : MonoBehaviour
{
    public TextMeshProUGUI contador;
    private int pulsaciones = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void aumentarPulsaciones()
    {
        pulsaciones++;
        contador.text = "Pulsacion: " + pulsaciones;
    }
}
