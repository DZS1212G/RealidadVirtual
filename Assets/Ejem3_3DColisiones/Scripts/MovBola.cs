using UnityEngine;

public class MocBola : MonoBehaviour
{
    public int velocidad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       float movimientoHorizontal = Input.GetAxis("Horizontal");
       float movimientoVertical = Input.GetAxis("Vertical");
        this.transform.Translate(new Vector3(movimientoHorizontal, 0, movimientoVertical) * Time.deltaTime * velocidad,Space.World);
    }


    //private void OnCollisionEnter(Collision collision)
    //{
    //    if (collision.gameObject.CompareTag("Player"))
    //    {
    //        Destroy(collision.gameObject);
    //    }
    //}
}
