using Cysharp.Threading.Tasks;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public static class ScatteringHelper
{
    /// <summary>
    /// Mesh�𕪊����A�������܂Ƃ߂����X�g�����炤����
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="step"></param>���������̃I�u�W�F�N�g������̃��b�V���̐�
    /// <param name="triangles"></param>mesh���瓾����g���C�A���O���̍��W�̃C���f�b�N�X���i�[�����z��
    /// <param name="verticles"></param>���_���܂Ƃ߂�Vector3�^�̔z��
    /// <param name="scale"></param>���̃��b�V���̐e�i���Ȃ��ꍇ�͎��g�j��localScale
    /// <param name="trianglePos"></param>���̃��b�V�����g�̃|�W�V����
    /// <param name="material"></param>�\��t����}�e���A��
    /// <returns></returns>
    public static List<GameObject> GetDivisionMesh<T>(this MonoBehaviour obj,int step,
        int[] triangles, Vector3[] verticles,Vector2[] uvs,Vector3 scale,Vector3 trianglePos,Material material) where T : MonoBehaviour
    {
        var chunks = new List<GameObject>(); 
        for (int i = 0; i < triangles.Length; i += step)
        {
            List<Vector3> verts = new();
            List<int> tris = new();
            Dictionary<int, int> vertMap = new(); // oldIndex -> newIndex
            List<Vector2> newUvs = new();
            for (int t = i; t < i + step && t + 2 < triangles.Length; t += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    int oldIndex = triangles[t + j];
                    if (!vertMap.ContainsKey(oldIndex))
                    {
                        vertMap[oldIndex] = verts.Count;
                        verts.Add(verticles[oldIndex]);
                        newUvs.Add(uvs[oldIndex]);
                    }
                    tris.Add(vertMap[oldIndex]);
                }
            }
            var triangleMesh = new Mesh();
            triangleMesh.vertices = verts.ToArray();
            triangleMesh.triangles = tris.ToArray();
            triangleMesh.uv = newUvs.ToArray();
            Debug.Log($"uv length: {newUvs.Count}, vertices length: {verts.Count}");
            triangleMesh.RecalculateBounds();
            triangleMesh.RecalculateNormals();

            var triangleObj = new GameObject("Chunk");
            var size = scale;
            triangleObj.transform.position = trianglePos;
            triangleObj.transform.localScale = size;
            MeshFilter meshFilter = triangleObj.AddComponent<MeshFilter>();
            meshFilter.mesh = triangleMesh;
            Debug.Log($"MeshFilter.mesh: {meshFilter.mesh?.name ?? "NULL"}, vertex count: {meshFilter.mesh?.vertexCount ?? 0}");
            MeshRenderer meshRenderer = triangleObj.AddComponent<MeshRenderer>();
            var copiedMaterial = new Material(material);
            meshRenderer.material = copiedMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;

            triangleObj.AddComponent<BoxCollider>();
            var rb = triangleObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            Debug.Log("���b�V�����������܂���");
            chunks.Add(triangleObj);
        }
        return chunks;
    }
    public static async UniTask Scattering<T>(this MonoBehaviour obj,List<GameObject> chunks,string rawMaterialName,
        float min,float max) where T : MonoBehaviour
    {
        //Max�l���܂܂���������float    
        //var min = -5.0f;
        //var max = 5.0f;
        try
        {
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
                var task = FadeProcessHelper.FadeOutColor(fadeOutTime, material, obj.GetCancellationTokenOnDestroy());
                tasks.Add(task);
            });

            await UniTask.WhenAll(tasks);
        }
        catch (OperationCanceledException) { return; }
        finally
        {
            chunks.ForEach(chunk =>
            {
                if (chunk != null) UnityEngine.Object.Destroy(chunk);
            });
        }

    }
}


