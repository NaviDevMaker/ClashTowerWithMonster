using UnityEngine;
using UnityEngine.UI;

public class Test1 : MonoBehaviour
{
    Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            image.Fader(new FadeSet(0f, 1.0f));
        }
    }
}
