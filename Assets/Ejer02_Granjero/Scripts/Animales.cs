using UnityEngine;

public class Animales : MonoBehaviour
{
    public GameObject prefabCiervo;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("generarAnimal", 1, 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void generarAnimal()
    {
        Instantiate(prefabCiervo);
    }
}
