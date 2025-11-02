// MemoryMonitor.cs
using UnityEngine;
using UnityEngine.UIElements;

public class MemoryMonitor : MonoBehaviour
{
    void Update()
    {
        if (Time.frameCount % 100 == 0)
        {
           // Debug.Log($"UI Elements in scene: {FindObjectsOfType<VisualElement>().Length}");
            Debug.Log($"UI Documents in scene: {FindObjectsOfType<UIDocument>().Length}");
        }
    }
}