using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;


namespace Game.Stages
{
    //StageManagerのベースとなるもの、そのステージの地面の種類によって壁の生成方法を変える

    public enum GroundType
    {
        Terrain,
        Simple,
        Convex,
    }


    public class StageManagerBase : MonoBehaviour
    {
        [SerializeField] GroundType groundType;
        int wallCount = 4;//terrainとsimleな地形の場合

        GameObject wallParent;
        string groundLayerName = "Ground";
        string checkGroundLayerName = "CheckGround";
        string wallLayerName = "Wall";
        int groundLayer = -1;
        int checkGroundLayer = -1;
        int wallLayer = -1;

        int checkGroundLayerBit = -1;
        int groundLayerBit = -1;
        int wallLayerBit = -1;

        Terrain terrain;
        GameObject checkGround;

        MeshRenderer groundRenderer;

        Vector3 groundSize;
        Vector3 groundPos;

        Dictionary<int, Vector3> wallPos = new Dictionary<int, Vector3>();
        Dictionary<int, Vector3> wallSize = new Dictionary<int, Vector3>();

        bool isEndStageSet = false;
        public bool IsEndStageSet { get => isEndStageSet; }
        protected virtual void Start()
        {
            SetUpWall();
        }

       
        public void GenerateWall_Terrain(Transform parent)
        {
            wallPos[0] = groundPos + new Vector3(groundSize.x / 2.0f, 0f, 0f);//手前
            wallPos[1] = groundPos + new Vector3(groundSize.x, 0f, groundSize.z / 2.0f);//右横
            wallPos[2] = groundPos + new Vector3(groundSize.x / 2, 0f, groundSize.z);//奥
            wallPos[3] = groundPos + new Vector3(0f, 0f, groundSize.z / 2.0f);//左横

            wallSize[0] = new Vector3(groundSize.x, 1, 0f);
            wallSize[1] = new Vector3(0f, 1, groundSize.z);
            wallSize[2] = new Vector3(groundSize.x, 1, 0f);
            wallSize[3] = new Vector3(0f, 1, groundSize.z);

            for (int i = 0; i < wallCount; i++)
            {
                GameObject wall = CreateWall(wallPos[i], wallSize[i], parent);
            }
        }

        GameObject CreateWall(Vector3 pos, Vector3 size, Transform parent)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = "StageWall";

            wall.transform.position = Vector3.zero;
            wall.transform.localScale = size;
            pos.y += wall.transform.localScale.y / 2;
            wall.transform.position = pos;
            wall.layer = wallLayer;
            MeshRenderer renderer = wall.GetComponent<MeshRenderer>();
            renderer.enabled = false;

            wall.transform.SetParent(parent);
            return wall;
        }


        void SetUpWall()
        {
            groundLayer = LayerMask.NameToLayer(groundLayerName);
            checkGroundLayer = LayerMask.NameToLayer(checkGroundLayerName);
            wallLayer = LayerMask.NameToLayer(wallLayerName);
            wallParent = new GameObject("WallParent");
            if (groundType == GroundType.Terrain)
            {
                terrain = Terrain.activeTerrain;
                if (terrain != null)
                {
                    groundSize = terrain.terrainData.size;
                    groundPos = terrain.transform.position;

                    GenerateWall_Terrain(wallParent.transform);
                }
            }
            else if (groundType == GroundType.Simple)
            {

                GameObject[] sceneObjs = GameObject.FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.None);

                foreach (GameObject obj in sceneObjs)
                {
                    if (obj.layer == groundLayer)
                    {
                        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                        if (renderer != null) groundRenderer = renderer;
                        break;
                    }
                }

                groundSize = groundRenderer.bounds.size;
                groundSize.y = 300.0f;
                groundPos = groundRenderer.transform.position;

                GenerateWall_Simple(wallParent.transform);

            }
            else if (groundType == GroundType.Convex)
            {
                SetConvexTerrainField(wallParent.transform);
            }
        }

        public void GenerateWall_Simple(Transform parent)
        {

            wallPos[0] = groundPos + new Vector3(0f, 0f, -groundSize.z / 2.0f);//手前
            wallPos[1] = groundPos + new Vector3(groundSize.x / 2.0f, 0f, 0f);//右横
            wallPos[2] = groundPos + new Vector3(0f, 0f, groundSize.z / 2);//奥
            wallPos[3] = groundPos + new Vector3(-groundSize.x / 2.0f, 0f, 0f);//左横

            wallSize[0] = new Vector3(groundSize.x, groundSize.y, 0f);
            wallSize[1] = new Vector3(0f, groundSize.y, groundSize.z);
            wallSize[2] = new Vector3(groundSize.x, groundSize.y, 0f);
            wallSize[3] = new Vector3(0f, groundSize.y, groundSize.z);

            for (int i = 0; i < wallCount; i++)
            {
                GameObject wall = CreateWall(wallPos[i], wallSize[i], parent);
            }
        }


        void SetConvexTerrainField(Transform parent)
        {

            checkGroundLayerBit = 1 << checkGroundLayer;
            groundLayerBit = 1 << groundLayer;
            wallLayerBit = 1 << wallLayer;
            List<MeshRenderer> renderers = new List<MeshRenderer>();
            //MeshRenderer checkGroundRenrer = null;
            GameObject[] objs = GameObject.FindObjectsByType<GameObject>(sortMode: FindObjectsSortMode.None);
            foreach (GameObject obj in objs)
            {
                if (obj.layer == groundLayer)
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null) renderers.Add(renderer);
                }
                else if (obj.layer == checkGroundLayer) checkGround = obj;

            }

            List<Vector3> groundSizes = new List<Vector3>();
            List<Vector3> groundPoses = new List<Vector3>();

            foreach (MeshRenderer renderer in renderers)
            {
                groundSizes.Add(renderer.bounds.size);
                groundPoses.Add(renderer.transform.position);
            }

            Debug.Log($"{groundSizes[0]}{groundSizes[1]}");
            HashSet<Vector3> wallPoints = GetWallPoints(groundSizes, groundPoses);
            GenerateWall_Convex(wallPoints, parent);
        }

        HashSet<Vector3> GetWallPoints(List<Vector3> groundSizes, List<Vector3> groundPoses)
        {
            HashSet<Vector3> wallPoints = new HashSet<Vector3>();//yの座標も必要だからvector3
            float maxX = 0f;
            float maxZ = 0f;
            float minX = 0f;
            float minZ = 0f;
            int distance = 20;

            for (int i = 0; i < groundSizes.Count; i++)
            {
                maxX = groundSizes[i].x + groundPoses[i].x;
                maxZ = groundSizes[i].z + groundPoses[i].z;
                minX = -groundSizes[i].x + groundPoses[i].x;
                minZ = -groundSizes[i].z + groundPoses[i].z;

                for (float x = minX; x < maxX; x += distance)
                {
                    Debug.Log("ｘの回数");

                    for (float z = minZ; z < maxZ; z += distance)
                    {
                        Debug.Log("zの回数");
                        RaycastHit hit;
                        Vector3 rayPoint = new Vector3(x, 100.0f, z);
                        if (Physics.Raycast(rayPoint, Vector3.down, out hit, Mathf.Infinity, checkGroundLayerBit))
                        {
                            Debug.Log("追加");
                            wallPoints.Add(hit.point);
                        }
                    }
                }
            }
            Debug.Log(wallPoints.Count);
            return wallPoints;
        }

        public async void GenerateWall_Convex(HashSet<Vector3> wallPoints, Transform parent)
        {
            HashSet<Vector2> wallPosToPlane = new HashSet<Vector2>();
            foreach (var wallPoint in wallPoints)
            {
                Vector2 planePoint = new Vector2(wallPoint.x, wallPoint.z);
                wallPosToPlane.Add(planePoint);
            }

            Debug.Log(wallPosToPlane.Count);
            int distance = 20;

            List<GameObject> walls = new List<GameObject>();
            foreach (var point in wallPosToPlane)
            {
                //Dictionary<Vector2, bool> neighbors = new Dictionary<Vector2, bool>()
                //{
                //    { new Vector2(distance, 0f),false },
                //    { new Vector2(-distance,0f),false },
                //    { new Vector2(0f,distance),false},
                //    { new Vector2(0f, -distance),false },
                //};

                //foreach (var neighbor in neighbors.Keys.ToList())
                //{
                //    Vector2 neighborPos = point + neighbor;
                //    if (wallPosToPlane.Contains(neighborPos))
                //    {
                //        neighbors[neighbor] = true;
                //    }
                //}

                //if (!neighbors.ContainsValue(true))
                //{
                Vector3 rayPoint = new Vector3(point.x, 100.0f, point.y);
                RaycastHit hit;
                if (Physics.Raycast(rayPoint, Vector3.down, out hit, Mathf.Infinity, groundLayerBit))
                {
                    Vector3 size = new Vector3(distance, distance * 3.0f, distance);
                    GameObject wall = CreateWall(hit.point, size, parent);
                    walls.Add(wall);
                }
                //}

            }

            List<GameObject> removeWalls = new List<GameObject>();
            float rayDistance = distance + distance / 2;
            foreach (var wall in walls)
            {
                Debug.Log("壁チェック");
                var wallPos = wall.transform.position;
                //Dictionary<Vector3, bool> neighbors = new Dictionary<Vector3, bool>()
                //{
                //    { Vector3.forward,false },
                //    { Vector3.back,false },
                //    { Vector3.right,false},
                //    { Vector3.left,false },
                //};

               // Vector3[] directions =
               //{  Vector3.forward,
               //    Vector3.back,
               //    Vector3.right,
               //    Vector3.left
               // };
               // Dictionary<int, Dictionary<Vector3, Vector3>> neighbor = new Dictionary<int, Dictionary<Vector3, Vector3>>()
               // {
               //     {0,new Dictionary<Vector3, Vector3>() },
               //     {1,new Dictionary<Vector3, Vector3>() },
               //     {2,new Dictionary<Vector3, Vector3>() },
               //     {3,new Dictionary<Vector3, Vector3>() },
               // };

               // neighbor[0][directions[0]] = wallPos + new Vector3(0f, 0f, distance);
               // neighbor[1][directions[1]] = wallPos + new Vector3(0f, 0f, -distance);
               // neighbor[2][directions[2]] = wallPos + new Vector3(distance, 0f, 0f);
               // neighbor[3][directions[3]] = wallPos + new Vector3(-distance, 0f, 0f);
                Vector3[] neighbor =
                {  Vector3.forward,
                   Vector3.back,
                   Vector3.right,
                   Vector3.left
                };
                //Vector3 direction;
                //foreach (var neighbor in neighbors.Keys.ToList())
                //{
                //    Debug.Log(wallLayerBit);
                //    direction = neighbor;
                //    if(Physics.Raycast(wall.transform.position,direction,rayDistance,wallLayerBit))
                //    {
                //        Debug.Log("となりがいます");
                //        neighbors[neighbor] = true;
                //    }
                //    //Debug.DrawRay(wallPos, direction * rayDistance, Color.red, 1.0f);
                //    //await UniTask.Delay(1000);


                //}
                //if (neighbors.Values.All(v=>v == true)) existWalls.Add(wall);
                //await UniTask.Yield();
                List<UniTask<bool>> tasks = new List<UniTask<bool>>();
                for (int i = 0; i < neighbor.Length; i++)
                {
                    tasks.Add(CheckNeighbor(wallPos, neighbor[i], rayDistance, wallLayerBit));
                }

                bool[] isNeighborExist = await UniTask.WhenAll(tasks);

                //await UniTask.Yield();
                if (isNeighborExist.All(result => result))//isNeighborExist
                {
                    Debug.Log("となりがいます");
                    removeWalls.Add(wall);
                }
                    //existWalls.ForEach(removeWall => Destroy(removeWall));

                    //.Log(wallParent.transform.childCount);
            }
            removeWalls.ForEach(removeWall => Destroy(removeWall));
            Destroy(checkGround);
            isEndStageSet = true;
        }
        async UniTask<bool> CheckNeighbor(Vector3 wallPos, Vector3 direction, float distance, int layerBit)
        {
            await UniTask.DelayFrame(1);
            return Physics.Raycast(wallPos, direction, distance, layerBit);

        }
    } 
}


