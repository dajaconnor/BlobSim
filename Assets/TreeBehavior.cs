using Assets;
using BlobSimulation.Utils;
using System.Linq;
using UnityEngine;

public class TreeBehavior : MonoBehaviour
{
    public GameObject fruitPrefab;
    private float growthRate = 0.001f;
    internal float growDropRatio = 0.5f;
    internal int lifespan = 100000;
    internal int age = 1000;
    internal TreeBehavior closestTree;

    // Start is called before the first frame update
    internal void Start()
    {
        closestTree = FindClosestTree();
    }

    // Update is called once per frame
    void Update()
    {
        if (age < 250) Grow();
        else if (age < 500)
        {
            if (age % 2 == 0) Grow();
        }
        else if (age < 100)
        {
            if (age % 5 == 0) Grow();
        }

        else if (age % 10 == 0)
        {
            if (UnityEngine.Random.Range(0f, 1f) > growDropRatio)
            {
                Grow();
            }
            else if (age % 50 == 0)
            {
                DropFruit();
            }
        }

        Age();
    }

    private TreeBehavior FindClosestTree()
    {
        var trees = FindObjectsOfType<TreeBehavior>();

        if (trees.Length == 0) return null;

        TreeBehavior currentClosest = trees.First();
        var currentDistance = DistanceTo(this, currentClosest);

        for (int i = 1; i < trees.Length; i++)
        {
            var thisDistance = DistanceTo(this, trees[i]);

            if (thisDistance < currentDistance)
            {
                currentClosest = trees[i];
                currentDistance = thisDistance;
            }
        }

        if (currentClosest.closestTree == null || DistanceTo(currentClosest, currentClosest.closestTree) > currentDistance)
        {
            currentClosest.closestTree = this;
        }

        return currentClosest;
    }

    private static float DistanceTo(TreeBehavior thisTree, TreeBehavior thatTree)
    {
        if (thisTree.Equals(thatTree)) return float.MaxValue;
        return LocationUtil.GetDistance(thatTree.transform.position, thisTree.transform.position);
    }



    private void Grow()
    {
        transform.localScale += new Vector3(growthRate, growthRate, growthRate);

        if (IsOverlappingClosestTree())
        {
            if (transform.localScale.x > closestTree.transform.localScale.x)
            {
                Destroy(closestTree.gameObject);
                Destroy(closestTree);
                closestTree = null;
                FindClosestTree();
            }
            else
            {
                Destroy(this.gameObject);
                Destroy(this);
                closestTree.FindClosestTree();
            }
        }
    }

    // Has to actually overlap the trunk
    private bool IsOverlappingClosestTree()
    {
        if (closestTree == null) return false;

        var distance = DistanceTo(this, closestTree);
        var shadeRadius = transform.localScale.x * 5;

        return closestTree != null && distance < shadeRadius;
    }

    private void Age()
    {
        if (age++ > lifespan)
        {
            Destroy(this.gameObject);
            Destroy(this);
        }
    }

    private void DropFruit()
    {
        Vector3 randomSpawnPosition = LocationUtil.GetRandomSpotAroundPosition(transform.localScale.x * 3, transform.position);

        var fruit = FruitSpawner.MakeFruit(randomSpawnPosition, fruitPrefab);
        if (fruit == null) return;

        // pass those genes along!
        // genetic drift will happen on germination, not here
        fruit.gestation = lifespan / 100;
        fruit.genes = new TreeGenes(growDropRatio, lifespan);
    }
}
