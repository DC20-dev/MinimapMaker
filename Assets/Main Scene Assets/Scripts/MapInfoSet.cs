using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TerrainType { Water, Sand, Grass, YellowGrass, LAST }
[System.Serializable]
public class MapInfoSet
{
    [HideInInspector]
    public int MapSize;
    public int[] Transponder;
    public Color WaterLight, WaterMedium, WaterDark, Sand, GrassGreen, GrassYellow;
    public GameObject[] HousePrefabs, HouseWithRoofPrefabs;
    public int NLevels;
    public GameObject[] TreePrefabs;
    public bool RandomTreeRotation;

    //Map contains:
    //	in R channel = HeightMap for the generation
    //	in G channel = House Spawn Probabilities
    //	in B channel = Tree Spawn Probabilities (~40%)
    public MapInfoSet()
	{
        Transponder = new int[(int)TerrainType.LAST];
        Transponder[0] = 0;
        Transponder[1] = 2;
        Transponder[2] = 3;
        Transponder[3] = 8;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="TerrainTransponder">Must have (int)TerrainType.LAST values. </param>
    public MapInfoSet(int[] TerrainTransponder)
    {
        Transponder = TerrainTransponder;
    }
}
