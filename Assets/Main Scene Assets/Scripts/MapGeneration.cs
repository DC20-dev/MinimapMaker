using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class MapGeneration : MonoBehaviour
{
    //Main Events
    public static UnityEvent MapChangeEvent = new UnityEvent();
    public static UnityEvent MapRefreshEvent = new UnityEvent();
    public static UnityEvent<int> MapInstantiateEvent = new UnityEvent<int>();


    public Transform TerrainRoot;
    public Transform DetailRoot;
    [Range(1,15)]
    public int MaxCubeHeight;
    [HideInInspector]
    public Texture2D Map;
    public Texture2D TransitionMap;
    public bool DoFadeBeforeDestroy = false;
    public bool UseObjectPooling = true;
    [HideInInspector]
    public MapInfoSet MIS;
    public bool GenerateMap = false, ClearMap = false, RefreshMap = false;//debug for MapTexture generation

    List<Queue<GameObject>> housesInstances = new List<Queue<GameObject>>();
    List<Queue<GameObject>> housesWithRoofInstances = new List<Queue<GameObject>>();
    List<Queue<GameObject>> treesInstances = new List<Queue<GameObject>>();
    GameObject poolRoot;
    int poolSize = 50;

    #region LIFECYCLE
    private IEnumerator Start()
	{
        DOTween.SetTweensCapacity(19530, 4875);

        yield return new WaitForSeconds(1);
        InstantiateMap();
	}
	void Update()
    {
		if (GenerateMap)
		{
            GenerateMap = false;
            InstantiateMap();
		}

		if (ClearMap)
		{
            ClearMap = false;
            MapRefreshEvent.Invoke();
            ClearDetails();
            ClearPools();
        }

		if (RefreshMap)
		{
            RefreshMap = false;
            UpdateMap();
		}
    }
    #endregion

    public void GenerateMapOnClick()
    {
        InstantiateMap();
    }

    public void RefreshMapOnClick()
    {
        UpdateMap();
    }

    public void ClearMapOnClick()
    {
        MapRefreshEvent.Invoke();
        ClearDetails();
        ClearPools();
    }

    void InstantiateMap()
	{
        MapChangeEvent.Invoke();
        MapInstantiateEvent.Invoke(MIS.MapSize);

        if (UseObjectPooling)
        {
            poolRoot = new GameObject("PoolRoot");
            poolRoot.transform.SetParent(transform);
            PoolInstantiation();
        }

        SetMapTiles();
    }

    void UpdateMap()
	{
        //this will LERP sizes for all tiles
        MapChangeEvent.Invoke();
        ClearDetails();
        SetMapTiles();
    }

    void SetMapTiles()
	{
        Transform[] blocks = TerrainRoot.GetAllChildren();
        int index;

        for (int y = 0; y < Map.height; y++)
		{
			for (int x = 0; x < Map.width; x++)
			{
                index = (Map.width * y) + x;
                //set animations to new scale for tile
                SetTerrainAnimations(blocks[index], x, y);

                //decide terrain type
                float r = Map.GetPixel(x, y).r;
                int terrainValue = (int)(r * 10);   //this will be 0-10
                SetTerrainSlab(GetTerrainType(terrainValue), blocks[index], TransitionMap.GetPixel(x, y).r, r);

                //decide detail spawn
                float g = Map.GetPixel(x, y).g;
                float b = Map.GetPixel(x, y).b;
                SpawnRandomDetails(r, g, b, blocks[index]);
			} 
		}
	}

    /// <summary>
    /// Sets to each block the tweens necessary to move to the new map layout.
    /// </summary>
    /// <param name="block"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    void SetTerrainAnimations(Transform block, int x, int y)
	{
        float r = Map.GetPixel(x, y).r;
        Vector3 scale = new Vector3(1, 0.1f + r * MaxCubeHeight, 1);
        Transform toScale = block.GetChild(1);
        float delay = TransitionMap.GetPixel(x, y).r;
        Sequence TileSeq = DOTween.Sequence().PrependInterval(delay);
        TileSeq.Append(toScale.DOScale(scale, 0.4f));
        TileSeq.Append(toScale.DOScale(scale + new Vector3(0, 0.5f, 0), 0.2f));
        TileSeq.Append(toScale.DOScale(scale, 0.2f));
        //set animations for slabs
        Transform toMove = block.GetChild(0);
        float endPos = toScale.position.y + scale.y + toMove.localScale.y * 0.5f;
        Sequence SlabSeq = DOTween.Sequence().PrependInterval(delay);
        SlabSeq.Append(toMove.DOMoveY(endPos, 0.4f));
        SlabSeq.Append(toMove.DOMoveY(endPos + 0.5f, 0.2f));
        SlabSeq.Append(toMove.DOMoveY(endPos, 0.2f));
    }

    /// <summary>
    /// Returns TerrainType accordingly to the given integer.
    /// </summary>
    /// <param name="n"></param>
    TerrainType GetTerrainType(int n)
	{
        TerrainType type = TerrainType.Water;
        for (int i = 0; i < MIS.Transponder.Length; i++)
        {
            type = MIS.Transponder[i] <= n ? (TerrainType)i : type;
        }
        return type;
    }

    /// <summary>
    /// Sets to each slab the tween for color.
    /// If the type changes to/from water at the end of the tween it also changes properties.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="bottomBlock"></param>
    void SetTerrainSlab(TerrainType newType, Transform bottomBlock, float delay, float height)
	{
        Transform slab = bottomBlock.GetChild(0);
        Material mat = slab.GetComponent<Renderer>().material;
        float tweenTime = 0.4f;
        float val;
        Color newCol;

        TweenCallback changeShader = () =>
        {
            mat.SetFloat("_Dynamic", 0);
        };

        switch (newType)
		{
			case TerrainType.Water:
                //
                //Change this block to get the start color depending on the poisiton
                //similar to the lerp done with the grass but with the height
                //leave the actual color change to the shader itself
				int rand = Random.Range(0, 3);
				newCol = rand == 0 ? MIS.WaterDark : MIS.WaterMedium;
				newCol = rand == 2 ? MIS.WaterLight : newCol;
                //
                 changeShader = () => 
                    {
                        mat.SetFloat("_RandomTimeOffset", Random.Range(0f,10f));
                        mat.SetFloat("_StartDepth", height);
                        mat.SetFloat("_Dynamic", 1);
                    };

                mat.DOColor(newCol, tweenTime).SetDelay(delay).OnComplete(changeShader);
				break;
			case TerrainType.Sand:
                mat.DOColor(MIS.Sand, tweenTime).SetDelay(delay).OnComplete(changeShader);
                break;
			case TerrainType.Grass:
                val = Mathf.InverseLerp(MIS.Transponder[2], MIS.Transponder[3], height*10);
                newCol = Color.Lerp(MIS.GrassGreen, MIS.GrassYellow, val);
                mat.DOColor(newCol, tweenTime).SetDelay(delay).OnComplete(changeShader);
                break;
			case TerrainType.YellowGrass:
                val = Mathf.Lerp(MIS.Transponder[2], MIS.Transponder[3], height * 10);
                newCol = Color.Lerp(MIS.GrassGreen, MIS.GrassYellow, val);
                mat.DOColor(newCol, tweenTime).SetDelay(delay).OnComplete(changeShader);
                break;
			default:
				break;
		}
	}

    void SpawnRandomDetails(float r, float g, float b, Transform block)
	{
        bool isHouseSpawned = false;
        int val = Random.Range(0, 101);
        //HOUSE SPAWN
        g *= 100;
        if(val < g && r*10 >= MIS.Transponder[1])   //higher than 
		{
            Vector3 housePos = block.position + new Vector3(0, block.localScale.y + 0.1f);
            RandomHouseSpawner(housePos);
            isHouseSpawned = true;
		}

        if (UseObjectPooling)
        {
            //TREE SPAWN (if house isn't already spawned)
            if (r * 10 >= MIS.Transponder[2] && !isHouseSpawned && b >= 0.9f)   //>=0.9f just in case, against . errors
            {
                int treeIndex = Random.Range(0, MIS.TreePrefabs.Length);

                GameObject go = treesInstances[treeIndex].Dequeue();
                if (go.activeInHierarchy) return;

                go.SetActive(true);
                go.transform.SetParent(DetailRoot);
                go.transform.position = block.position + new Vector3(0, block.localScale.y + 0.1f);
                if (MIS.RandomTreeRotation)
                    go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 359), 0);
                go.transform.DOLocalMoveY(MaxCubeHeight + 1, 0.4f);

                treesInstances[treeIndex].Enqueue(go);
            }
            return;
        }

		//TREE SPAWN (if house isn't already spawned)
		if (r * 10 >= MIS.Transponder[2] && !isHouseSpawned && b >= 0.9f)   //>=0.9f just in case, against . errors
		{
            int treeIndex = Random.Range(0, MIS.TreePrefabs.Length);
#if UNITY_EDITOR
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(MIS.TreePrefabs[treeIndex], DetailRoot);
#else
            GameObject go = GameObject.Instantiate(MIS.TreePrefabs[treeIndex], DetailRoot);
#endif
            go.transform.position = block.position + new Vector3(0, block.localScale.y + 0.1f);
            if (MIS.RandomTreeRotation)
                go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 359), 0);
            go.transform.DOLocalMoveY(MaxCubeHeight + 1, 0.4f);
        }
    }

    void ClearDetails()
	{
        if (UseObjectPooling)
        {
            Transform[] details = DetailRoot.GetAllChildren();
            for (int i = 0; i < details.Length; i++)
            {
                if(details[i].gameObject.tag == "HouseParent")
                {
                    Transform[] housePieces = details[i].GetAllChildren();
                    foreach (Transform t in housePieces)
                    {
                        t.gameObject.SetActive(false);
                        t.SetParent(poolRoot.transform);
                    }
                    Destroy(details[i].gameObject);
                }
                else
                {
                    details[i].gameObject.SetActive(false);
                    details[i].SetParent(poolRoot.transform);
                }
            }
            return;
        }

		if (DoFadeBeforeDestroy) 
        {
            float fadeTime = 0.25f;
		    Transform[] objs = DetailRoot.GetAllChildren();
            for (int i = 0; i < objs.Length; i++)
            {
                //fade gameobjects
                GameObject go = objs[i].gameObject;
                Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

                for (int j = 0; j < renderers.Length; j++)
                {
                    //renderers[j].material.DOFade(0, fadeTime);
                    Material[] mats = renderers[j].materials;

                    for (int n = 0; n < mats.Length; n++)
                    {
                        mats[n].DOFade(0, fadeTime);
                    }
                }
                //destroy go after fade time has expired
                Destroy(go, fadeTime + 0.1f);
            }
		}
		else
		{
            Transform[] objs = DetailRoot.GetAllChildren();
            for (int i = 0; i < objs.Length; i++)
            {
                Destroy(objs[i].gameObject);
            }
        }
	}

    void RandomHouseSpawner(Vector3 position)
	{
        //get random House Level (floor, first, second)
        int houseN = Random.Range(0, MIS.NLevels);
        Vector3 lastTopPos = position;

        GameObject House = new GameObject();
        House.transform.SetParent(DetailRoot);
        House.transform.position = position;
        House.tag = "HouseParent";

        int roofIndex = Random.Range(0, MIS.HouseWithRoofPrefabs.Length);
        GameObject topPiece = MIS.HouseWithRoofPrefabs[roofIndex];

        if (UseObjectPooling)
        {
            for (int i = 0; i < houseN; i++)
            {
                GameObject housePiece;
                int queueIndex = Random.Range(0, MIS.HousePrefabs.Length);

                housePiece = housesInstances[queueIndex].Dequeue();//contains instance now
                //if (housePiece.activeInHierarchy) return;         //Might break house setup, TO CHECK

                housePiece.SetActive(true);
                housePiece.transform.SetParent(House.transform);
                housePiece.transform.position = lastTopPos;

                float pieceScaleY = housePiece.transform.GetChild(0).localScale.y;
                lastTopPos += new Vector3(0, pieceScaleY, 0);

                housesInstances[queueIndex].Enqueue(housePiece);
            }

            topPiece = housesWithRoofInstances[roofIndex].Dequeue();//contains instance now
            //if (topPiece.activeInHierarchy) return;   this check would break the House Setup

            topPiece.SetActive(true);
            topPiece.transform.SetParent(House.transform);
            topPiece.transform.position = lastTopPos;

            housesWithRoofInstances[roofIndex].Enqueue(topPiece);

            House.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
            House.transform.DOMoveY(MaxCubeHeight + 1, 0.2f);

            return;
        }

		for (int i = 0; i < houseN; i++)
		{
            GameObject housePiece = MIS.HousePrefabs[Random.Range(0, MIS.HousePrefabs.Length)];
#if UNITY_EDITOR
            housePiece = (GameObject)PrefabUtility.InstantiatePrefab(housePiece, House.transform);//contains instance now
#else
            housePiece = GameObject.Instantiate(housePiece, House.transform);
#endif
            housePiece.transform.position = lastTopPos;

            float pieceScaleY = housePiece.transform.GetChild(0).localScale.y;
            lastTopPos += new Vector3(0, pieceScaleY, 0);
        }
#if UNITY_EDITOR
        topPiece = (GameObject)PrefabUtility.InstantiatePrefab(topPiece, House.transform);//contains instance now
#else
        topPiece = GameObject.Instantiate(topPiece, House.transform);
#endif
        topPiece.transform.position = lastTopPos;
        House.AddComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll & ~RigidbodyConstraints.FreezePositionY;
        House.transform.DOMoveY(MaxCubeHeight + 1, 0.2f);
    }

    void PoolInstantiation()
    {
        for (int i = 0; i < MIS.HousePrefabs.Length; i++)
        {
            housesInstances.Add(new Queue<GameObject>(poolSize));
            for (int j = 0; j < poolSize; j++)
            {
#if UNITY_EDITOR
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(
                    MIS.HousePrefabs[i], poolRoot.transform);
#else
                GameObject go = GameObject.Instantiate(MIS.HousePrefabs[i], poolRoot.transform);
#endif
                go.SetActive(false);
                housesInstances[i].Enqueue(go);
            }
        }
        for (int i = 0; i < MIS.HouseWithRoofPrefabs.Length; i++)
        {
            housesWithRoofInstances.Add(new Queue<GameObject>(poolSize));
            for (int j = 0; j < poolSize; j++)
            {
#if UNITY_EDITOR
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(
                    MIS.HouseWithRoofPrefabs[i], poolRoot.transform);
#else
                GameObject go = GameObject.Instantiate(MIS.HouseWithRoofPrefabs[i], poolRoot.transform);
#endif
                go.SetActive(false);
                housesWithRoofInstances[i].Enqueue(go);
            }
        }
        for (int i = 0; i < MIS.TreePrefabs.Length; i++)
        {
            treesInstances.Add(new Queue<GameObject>(poolSize));
            for (int j = 0; j < poolSize; j++)
            {
#if UNITY_EDITOR
                GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(
                    MIS.TreePrefabs[i], poolRoot.transform);
#else
                GameObject go = GameObject.Instantiate(MIS.TreePrefabs[i], poolRoot.transform);
#endif
                go.SetActive(false);
                treesInstances[i].Enqueue(go);
            }
        }
    }

    void ClearPools()
    {
        for (int i = 0; i < MIS.HousePrefabs.Length; i++)
        {
            for (int j = 0; j < housesInstances.Count; j++)
            {
                housesInstances[i].Clear();
            }
            housesInstances.Clear();
        }
        for (int i = 0; i < MIS.HouseWithRoofPrefabs.Length; i++)
        {
            for (int j = 0; j < housesWithRoofInstances.Count; j++)
            {
                housesWithRoofInstances[i].Clear();
            }
            housesWithRoofInstances.Clear();
        }
        for (int i = 0; i < MIS.TreePrefabs.Length; i++)
        {
            for (int j = 0; j < treesInstances.Count; j++)
            {
                treesInstances[i].Clear();
            }
            treesInstances.Clear();
        }
    }  
}
