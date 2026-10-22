using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController8 : MonoBehaviour
{
    public TextMeshProUGUI contador;
    private int pulsaciones = 0;
    public Slider slider;
    public TMP_Dropdown dropwdown;
    public Toggle cb_MayorEdad;
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
    public void numero()
    {
        contador.text = slider.value.ToString();

    }
    public void recuperarOpcion()
    {
        Debug.Log("Desplegable: " + dropwdown.options[dropwdown.value].text);
    }
    public void recuperarTougle()
    {
        Debug.Log("Desplegable: " + cb_MayorEdad.isOn);
    }
}
