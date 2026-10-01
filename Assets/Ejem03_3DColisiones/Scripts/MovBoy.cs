using UnityEngine;

public class MovBoy : MonoBehaviour
{
    Animator animacionBoy;
    public int velocidad;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        animacionBoy = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Translate(new Vector3(0, 0, 1) * Time.deltaTime * velocidad);
    }

    private void OnCollisionEnter(Collision collision)
    {
        animacionBoy.SetBool("Rodar", true);
    }

    private void OnCollisionExit(Collision collision)
    {
        animacionBoy.SetBool("Rodar", false);
    }
}
