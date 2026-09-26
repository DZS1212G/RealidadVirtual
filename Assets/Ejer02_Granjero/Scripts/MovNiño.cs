using UnityEngine;

public class MovNiño : MonoBehaviour
{
    public int velocidad = 5;
    public double leftLimit = 9;
    public double rightLimit = 31;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float movimientoHorizontal = Input.GetAxis("Horizontal");
        
            this.transform.Translate(new Vector3(movimientoHorizontal, 0, 0) * Time.deltaTime * velocidad, Space.World);
        
    }
    

}
