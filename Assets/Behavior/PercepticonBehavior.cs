using UnityEngine;

public class PercepticonBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        transform.localScale = transform.parent.gameObject.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
