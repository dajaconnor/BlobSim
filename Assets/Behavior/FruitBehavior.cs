using Assets;
using Assets.Utils;
using BlobSimulation.Utils;
using UnityEngine;

public class FruitBehavior : MonoBehaviour
{
    public GameObject treePrefab;
    public GameObject fruitPrefab;
    public int gestation = 100;
    public TreeGenes genes;
    private int age = 0;
    public bool isSterile = true;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isSterile || Camera.main.GetComponent<CameraBehavior>().paused) return;

        age++;

        if (age > gestation)
        {
            Germinate();
        }
    }

    private void Germinate()
    {
        if (genes == null || LocationUtil.IsInShade(transform.position))
        {
            isSterile = true;
            return;
        }

        GameObject newGameObject = Instantiate(treePrefab, transform.position, transform.rotation);
        ReproductionUtil.GerminateTree(genes, transform.position, newGameObject, fruitPrefab, FindObjectOfType<MapGenerator>());

        Destroy(this.gameObject);
        Destroy(this);
    }
}
