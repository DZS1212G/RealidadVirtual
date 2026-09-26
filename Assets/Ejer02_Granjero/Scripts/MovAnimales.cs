using UnityEngine;

public class MovAnimales : MonoBehaviour
{

    public float speed = -0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int posicionInic = Random.Range(9, 31);
        this.transform.position = new Vector3(posicionInic, 0, 38);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position += new Vector3(0, 0, (float)speed);
        if (this.transform.position.z < -50)
        {
            Destroy(this.gameObject);
        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(collision.gameObject);
            UnityEditor.EditorApplication.isPlaying = false;
        }
    }
}
