using UnityEngine;
using BlobSimulation.Utils;
using Random = UnityEngine.Random;

public class FruitSpawner : MonoBehaviour
{

    public GameObject fruitPrefab;
    private int initialSpawn = 1000;
    private int ticks = 1;

    // Use this for initialization
    void Start()
    {
        for (var i = 0; i < initialSpawn; i++)
        {
            SpawnRandomFruit();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (ticks % Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate == 0)
        {
            SpawnRandomFruit();
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
        Vector3 randomSpawnPosition = LocationUtil.GetRandomSpot(transform.localScale.x / 2 * 0.95f);
        
        MakeFruit(randomSpawnPosition, fruitPrefab);
    }

    public static FruitBehavior MakeFruit(Vector3 spawnPosition, GameObject fruitPrefab)
    {
        if (!LocationUtil.IsOnMap(spawnPosition)) return null;

        Vector3 randomSpawnRotation = Vector3.up * Random.Range(0, 360);
        var fruit = Instantiate(fruitPrefab, spawnPosition, Quaternion.Euler(randomSpawnRotation));
        fruit.name = "Fruit";

        return fruit.GetComponent<FruitBehavior>();
    }
}
