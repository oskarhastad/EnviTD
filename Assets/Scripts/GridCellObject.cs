using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridCell", menuName = "TowerDefense/Grid Cell")]

public class GridCellObject : ScriptableObject
{
    public GameObject cellPrefab;
    public int yRotation;

}
