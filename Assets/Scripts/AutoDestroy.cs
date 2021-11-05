using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    public void destroy()
    {
        Destroy(gameObject);
    }
}
