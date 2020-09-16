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
    private bool sterile = true;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (sterile) return;

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
            sterile = true;
            return;
        }

        GameObject newGameObject = Instantiate(treePrefab, transform.position, transform.rotation);
        ReproductionUtil.GerminateTree(genes, transform.position, newGameObject, fruitPrefab);

        Destroy(this.gameObject);
        Destroy(this);
    }
}
