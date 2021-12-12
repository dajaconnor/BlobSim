using UnityEngine;

public class PercepticonBehavior : MonoBehaviour
{
    void Start()
    {
        transform.localScale = transform.parent.gameObject.transform.localScale;
    }
}
