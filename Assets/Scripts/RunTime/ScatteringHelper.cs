using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public static class ScatteringHelper
{
    /// <summary>
    /// Meshを分割し、それらをまとめたリストをもらう処理
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="step"></param>生成する一つのオブジェクトあたりのメッシュの数
    /// <param name="triangles"></param>meshから得られるトライアングルの座標のインデックスを格納した配列
    /// <param name="verticles"></param>頂点をまとめたVector3型の配列
    /// <param name="scale"></param>元のメッシュの親（いない場合は自身）のlocalScale
    /// <param name="trianglePos"></param>元のメッシュ自身のポジション
    /// <param name="material"></param>貼り付けるマテリアル
    /// <returns></returns>
    public static List<GameObject> GetDivisionMesh<T>(this MonoBehaviour obj,int step,
        int[] triangles, Vector3[] verticles,Vector3 scale,Vector3 trianglePos,Material material) where T : MonoBehaviour
    {
        var chunks = new List<GameObject>(); 
        for (int i = 0; i < triangles.Length; i += step)
        {
            List<Vector3> verts = new();
            List<int> tris = new();
            Dictionary<int, int> vertMap = new(); // oldIndex -> newIndex

            for (int t = i; t < i + step && t + 2 < triangles.Length; t += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int oldIndex = triangles[t + j];
                    if (!vertMap.ContainsKey(oldIndex))
                    {
                        vertMap[oldIndex] = verts.Count;
                        verts.Add(verticles[oldIndex]);
                    }
                    tris.Add(vertMap[oldIndex]);
                }
            }
            var triangleMesh = new Mesh();
            triangleMesh.vertices = verts.ToArray();
            triangleMesh.triangles = tris.ToArray();

            var triangleObj = new GameObject("Chunk");
            var size = scale;
            triangleObj.transform.position = trianglePos;
            triangleObj.transform.localScale = size;
            triangleObj.AddComponent<MeshFilter>().mesh = triangleMesh;
            triangleObj.AddComponent<MeshRenderer>().material = material;
            triangleObj.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
            triangleObj.AddComponent<BoxCollider>();
            var rb = triangleObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            Debug.Log("メッシュが完成しました");
            chunks.Add(triangleObj);
        }

        return chunks;
    }

    public static async UniTask Scattering<T>(this MonoBehaviour obj,List<GameObject> chunks,string rawMaterialName,
        float min,float max) where T : MonoBehaviour
    {
        //Max値を含ませたいからfloat    

        //var min = -5.0f;
        //var max = 5.0f;
        var fadeOutTime = 4.0f;
        var tasks = new List<UniTask>();
        chunks.ForEach(chunk =>
        {
            var rb = chunk.GetComponent<Rigidbody>();
            var material = chunk.GetComponent<MeshRenderer>().material;
            if (material.name.StartsWith(rawMaterialName))
            {
                if (material.HasProperty("_Surface")) FadeProcessHelper.ChangeToTranparent(material);
            }
            rb.isKinematic = false;
            var minY = Terrain.activeTerrain.SampleHeight(obj.transform.position);
            var maxY = minY + max;
            var forceVector = new Vector3(UnityEngine.Random.Range(min, max), UnityEngine.Random.Range(minY, maxY), UnityEngine.Random.Range(min, max));           
            rb.AddForce(forceVector, ForceMode.Impulse);
            rb.AddTorque(forceVector, ForceMode.Impulse);
            var task = FadeProcessHelper.FadeOutColor(fadeOutTime, obj.GetCancellationTokenOnDestroy(), material);
            tasks.Add(task);
        });

        await UniTask.WhenAll(tasks);
        chunks.ForEach(chunk => UnityEngine.Object.Destroy(chunk));
    }
}


