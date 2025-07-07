using UnityEngine;

//シングルトンクラスの親クラス
public class SingletonMonobehavier<T> : MonoBehaviour where T : MonoBehaviour
{
   public static T Instance { get; private set; }

    protected virtual void Awake()
    {
        if(Instance == null)
        {
            Instance = this as T;
            DontDestroyOnLoad(this.gameObject);
        }
        else if(Instance != null)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
