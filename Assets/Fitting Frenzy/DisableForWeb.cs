using UnityEngine;

public class DisableForWeb : MonoBehaviour
{
    private void Awake()
    {
#if UNITY_WEBGL
        gameObject.SetActive(false);
#endif
    }
}
