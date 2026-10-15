using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Texto : MonoBehaviour
{
    private TextMeshProUGUI texto;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ganaste()
    {
        texto.text = "Ganaste Boludo";
    }
    public void noGanaste()
    {
        texto.text = "";
    }
}
