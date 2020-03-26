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
        if (Camera.main.GetComponent<CameraBehavior>().target == null)
        {
            GUI.Label(new Rect(10, 10, 200, 20), "Spawn rate is " + Camera.main.GetComponent<CameraBehavior>().fruitSpawnRate);
        }
    }

    private void SpawnRandomFruit()
    {
        Vector3 randomSpawnPosition = LocationUtil.GetRandomSpot(transform.localScale.x / 2 * 0.95f);
        Vector3 randomSpawnRotation = Vector3.up * Random.Range(0, 360);

        Instantiate(fruitPrefab, randomSpawnPosition, Quaternion.Euler(randomSpawnRotation));
    }


}
