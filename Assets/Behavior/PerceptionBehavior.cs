using UnityEngine;

public class PerceptionBehavior : MonoBehaviour
{

    public GameObject LatestFruit;
    public BlobBehavior LatestBlob;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider triggerCollider)
    {
        if (triggerCollider.gameObject.name.StartsWith("Fruit"))
        {
            LatestFruit = triggerCollider.gameObject;
            return;
        }

        if (triggerCollider.gameObject.name.StartsWith("Percepticon"))
        {
            LatestBlob = triggerCollider.gameObject.GetComponentInParent<BlobBehavior>();
        }
    }

    void OnMouseDown()
    {
        Camera.main.GetComponent<CameraBehavior>().target = GetComponentInParent<BlobBehavior>();
    }
}
