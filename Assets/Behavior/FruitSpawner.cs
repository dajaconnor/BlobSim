using UnityEngine;
using BlobSimulation.Utils;
using Random = UnityEngine.Random;
using System.Linq;

public class FruitSpawner : MonoBehaviour
{

    public GameObject fruitPrefab;
    private int initialSpawn = 1000;
    private int ticks = 1;
    private TreeBehavior firstTree;

    // Use this for initialization
    void Start()
    {
        var map = FindObjectOfType<MapGenerator>();
        map.GenerateMap();

        firstTree = Object.FindObjectsOfType<TreeBehavior>().First();
        firstTree.transform.position = new Vector3(firstTree.transform.position.x, LocationUtil.GetHeight(firstTree.transform.position), firstTree.transform.position.z);

        for (var i = 0; i < initialSpawn; i++)
        {
            firstTree.DropFruit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ticks < 1000) firstTree.DropFruit();

        if (ticks % Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate == 0)
        {
            SpawnRandomFruit();
        }

        if (ticks % 2000 == 0)
        {
            Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate += 1;
        }

        ticks++;

        if (Input.GetKeyDown(KeyCode.Minus) && Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate > 1)
        {
            Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate--;
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate++;
        }
    }

    private void OnGUI()
    {
        GUI.contentColor = Color.black;

        var behavior = Camera.main.GetComponent<CameraBehavior>();

        if (behavior.target == null && behavior.displayType != Assets.Enums.DisplayType.None)
        {
            GUI.Label(new Rect(Screen.width - 200, 10, 200, 20), "Fruit spawn rate " + Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate);
            GUI.Label(new Rect(Screen.width - 200, 25, 200, 20), "Simulation speed " + Time.timeScale.ToString("F2"));
        }
    }

    private void SpawnRandomFruit()
    {
        Vector3 randomSpawnPosition = LocationUtil.GetRandomSpot(50);
        
        MakeFruit(randomSpawnPosition, fruitPrefab);
    }

    public static FruitBehavior MakeFruit(Vector3 spawnPosition, GameObject fruitPrefab)
    {
        if (!LocationUtil.IsOnMap(spawnPosition)) return null;

        spawnPosition = new Vector3(spawnPosition.x, LocationUtil.GetHeight(spawnPosition) + LocationUtil.groundHeight, spawnPosition.z);
        Vector3 randomSpawnRotation = Vector3.up * Random.Range(0, 360);
        var fruit = Instantiate(fruitPrefab, spawnPosition, Quaternion.Euler(randomSpawnRotation));
        fruit.name = "Fruit";

        return fruit.GetComponent<FruitBehavior>();
    }
}
