using UnityEngine;

[CreateAssetMenu(menuName = "Tower")]
public class TowerSO : ScriptableObject
{
  public GameObject hoverTower;
  public TowerController realTower;
  public int price;
}