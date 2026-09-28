using UnityEngine;

public class MovNiño : MonoBehaviour
{
    public int velocidad = 5;
    public GameObject prefabComida;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        
            this.transform.Translate(new Vector3(movimientoHorizontal, 0, 0) * Time.deltaTime * velocidad, Space.World);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 posicion = this.transform.position;
            posicion += new Vector3(0, 2, 2);
            Instantiate(prefabComida,posicion,prefabComida.transform.rotation);
            
        }
    }
    

}
