using UnityEngine;

public class EnhanceUITest : MonoBehaviour
{
    [SerializeField] private EnhanceUI enhanceUI;

    private void Start()
    {
        int[] ids = { 1, 2, 3, 4, 5 };
        float[] weights = { 30f, 25f, 20f, 15f, 10f };

        enhanceUI.ShowCardsByIds(ids, weights);
    }
}