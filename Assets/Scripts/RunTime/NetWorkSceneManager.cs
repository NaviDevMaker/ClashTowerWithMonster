using UnityEngine;
using UnityEngine.SceneManagement;

public class NetWorkSceneManager : SingletonMonobehavier<NetWorkSceneManager>
{
    public SceneNames sceneNames { get; private set; }
    // Start is called once before the firstColor execution of Update after the MonoBehaviour is created
    async void Start()
    {
        sceneNames = await SetFieldFromAssets.SetField<SceneNames>("Datas/SceneNames");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}


