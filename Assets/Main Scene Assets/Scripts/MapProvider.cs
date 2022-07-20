using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapProvider : MonoBehaviour
{
	public MapGeneration Generator;
	[Range(10, 50)]
	public int MapSize;
	[Range(0.01f, 1f)]
	public float DebugPerlinNoiseMultiplierSlider = 1;
	[Range(0,1)]
	public float DebugProbabilityMultiplier = 0.5f;
	[Range(0,1)]
	public float TreesPercentage = 0.4f;
	public int RandomizerMaxValue;

	//Map contains:
	//	in R channel = HeightMap for the generation
	//	in G channel = House Spawn Probabilities
	//	in B channel = Tree Spawn Probabilities (~40%)
	private Texture2D currentMap;
	//mis contains the "guide" to translate the map
	public MapInfoSet MIS;
	private void OnEnable()
	{
		MapGeneration.MapChangeEvent.AddListener(GetNewMap);
	}

	private void OnDisable()
	{
		MapGeneration.MapChangeEvent.RemoveListener(GetNewMap);
	}

	public void GetNewMap()
	{
		GenerateMap();
		ModifyMIS();
		//pass the new created informations to generator
		Generator.Map = currentMap;
		Generator.MIS = MIS;
	}


	public void ModifyMIS()	//if personalization is requested, this method will handle it
	{
		MIS.MapSize = MapSize;
	}

	private void GenerateMap()
	{
		currentMap = new Texture2D(MapSize, MapSize);
		float randStartX = Random.Range(0, RandomizerMaxValue);
		float randStartY = Random.Range(0, RandomizerMaxValue);
		for (int y = 0; y < MapSize; y++)
		{
			for (int x = 0; x < MapSize; x++)
			{
				float r = Mathf.PerlinNoise((randStartX + x )* DebugPerlinNoiseMultiplierSlider,
					(randStartY + y) * DebugPerlinNoiseMultiplierSlider);

				float g = r * DebugProbabilityMultiplier;

				float b = Random.Range(0f, 1f) <= TreesPercentage ? 1 : 0;

				Color unitInfo = new Color(r, g, b);

				currentMap.SetPixel(x, y, unitInfo);
			}
		}

		currentMap.Apply();
	}
}
