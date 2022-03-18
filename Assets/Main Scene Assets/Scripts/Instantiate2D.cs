using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
public class Instantiate2D : MonoBehaviour
{
    public Transform Parent;
    public GameObject Prefab;
    float OffsetX = 1, OffsetZ = 1;
    //public int Nx, Nz;

    //public bool CreateInstances, DestroyAll;

	private void OnEnable()
	{
        MapGeneration.MapRefreshEvent.AddListener(PerformDestroyAll);
        MapGeneration.MapInstantiateEvent.AddListener(PerformInstantiateN);
	}

	private void OnDisable()
	{
        MapGeneration.MapRefreshEvent.RemoveListener(PerformDestroyAll);
        MapGeneration.MapInstantiateEvent.RemoveListener(PerformInstantiateN);
    }

	// Update is called once per frame
	//void Update()
 //   {
 //       if (CreateInstances)
 //       {
 //           CreateInstances = false;
 //           PerformInstantiateN();
 //       }
 //       if (DestroyAll)
 //       {
 //           DestroyAll = false;
 //           PerformDestroyAll();
 //       }
 //   }

    private void PerformDestroyAll()
    {
        Transform[] children = Parent.GetAllChildren();
        for (int i = Parent.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(children[i].gameObject);
            }
            else
            {
                DestroyImmediate(children[i].gameObject);
            }
        }
    }

 //   private void PerformInstantiateN()
	//{
 //       //proprieta' base azzerate
 //       Vector3 pos = Parent.position;
 //       Vector3 rot = Vector3.zero;
 //       Vector3 scale = Vector3.one;

 //       if (Parent == null)
 //       {
 //           Parent = transform;
 //       }

 //       GameObject go;

 //       for (int z = 0; z < Nz; z++)
 //       {
	//		for (int x = 0; x < Nx; x++)
	//		{
 //               go = (GameObject)PrefabUtility.InstantiatePrefab(Prefab, Parent);

 //               go.transform.position = pos;
 //               go.transform.rotation = Quaternion.Euler(rot);
 //               go.transform.localScale = scale;

 //               pos = new Vector3(pos.x + OffsetX, pos.y, pos.z);
 //           }
 //           pos = new Vector3(Parent.position.x, pos.y, pos.z + OffsetZ);
 //       }
 //   }

    private void PerformInstantiateN(int N)
    {
        //proprieta' base azzerate
        Vector3 pos = Parent.position;
        Vector3 rot = Vector3.zero;
        Vector3 scale = Vector3.one;

        if (Parent == null)
        {
            Parent = transform;
        }

        GameObject go;

        for (int z = 0; z < N; z++)
        {
            for (int x = 0; x < N; x++)
            {
#if UNITY_EDITOR
                go = (GameObject)PrefabUtility.InstantiatePrefab(Prefab, Parent);
#else
                go = GameObject.Instantiate(Prefab, Parent);
#endif
                go.transform.position = pos;
                go.transform.rotation = Quaternion.Euler(rot);
                go.transform.localScale = scale;

                pos = new Vector3(pos.x + OffsetX, pos.y, pos.z);
            }
            pos = new Vector3(Parent.position.x, pos.y, pos.z + OffsetZ);
        }
    }
}
