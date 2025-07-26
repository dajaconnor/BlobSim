using UnityEngine;

public class PerceptionBehavior : MonoBehaviour
{

    public GameObject LatestFruit;
    public UnitBehavior LatestUnit;

    void OnTriggerEnter(Collider triggerCollider)
    {
        if (triggerCollider.gameObject.name.StartsWith("Fruit"))
        {
            LatestFruit = triggerCollider.gameObject;
            return;
        }

        if (triggerCollider.gameObject.name.StartsWith("Percepticon"))
        {
            LatestUnit = triggerCollider.gameObject.GetComponentInParent<UnitBehavior>();
        }
    }

    void OnMouseDown()
    {
        Camera.main.GetComponent<CameraBehavior>().target = GetComponentInParent<UnitBehavior>();
    }
}
