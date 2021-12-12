using UnityEngine;

public class PerceptionBehavior : MonoBehaviour
{

    public GameObject LatestFruit;
    public BlobBehavior LatestBlob;

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
