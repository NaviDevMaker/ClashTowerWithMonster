using UnityEngine;

public class Test_SlimeSahder : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var mat = GetComponent<MeshRenderer>().material;
        mat.renderQueue = 1999;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
