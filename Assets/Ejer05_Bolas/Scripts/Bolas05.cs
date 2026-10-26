using UnityEngine;

public class Bolas05 : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody Rigidbody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Rigidbody = this.gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float movH = Input.GetAxis("Horizontal");
        float movV = Input.GetAxis("Vertical");
        this.transform.Translate(new Vector3(movH, 0, movV) * Time.deltaTime * speed);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Rigidbody.AddForce(Vector3.up * 10, ForceMode.Impulse);
        }
    }
}
