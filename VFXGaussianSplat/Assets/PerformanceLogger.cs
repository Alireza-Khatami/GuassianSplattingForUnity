using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PerformanceLogger : MonoBehaviour
{
    private float logInterval = 1f;
    private float timeSinceLastLog = 0f;
    private List<string> logLines = new List<string>();
    private int frameCount = 0;
    private float timePassed = 0f;
    private string filePath;

    void Start()
    {
        string filename = "UnityPerformanceLog_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";

        // Save in the project root directory (outside Assets/)
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        filePath = Path.Combine(projectRoot, filename);

        logLines.Add("Time,FPS,Resolution,QualityLevel,VSync,TargetFrameRate,GPUName,GPU VRAM MB");

        Debug.Log($"[PerformanceLogger] Logging to: {filePath}");
    }

    void Update()
    {
        frameCount++;
        timeSinceLastLog += Time.unscaledDeltaTime;
        timePassed += Time.unscaledDeltaTime;

        if (timeSinceLastLog >= logInterval)
        {
            float fps = frameCount / timeSinceLastLog;
            string resolution = $"{Screen.width}x{Screen.height}";
            string quality = QualitySettings.names[QualitySettings.GetQualityLevel()];
            int vsync = QualitySettings.vSyncCount;
            int targetFps = Application.targetFrameRate;

            string gpuName = SystemInfo.graphicsDeviceName;
            int vram = SystemInfo.graphicsMemorySize;

            string log = $"{System.DateTime.Now:HH:mm:ss},{fps:F2},{resolution},{quality},{vsync},{targetFps},{gpuName},{vram}";
            logLines.Add(log);

            timeSinceLastLog = 0f;
            frameCount = 0;
    }
//#if UNITY_EDITOR
//        if (timePassed >= 30)
//        {
//            Debug.Log("[PerformanceLogger] 25 seconds elapsed. Stopping Play Mode...");
//            UnityEditor.EditorApplication.isPlaying = false;
//        }
//#endif
    }




#if UNITY_EDITOR
    void OnDisable()
    {
        if (!Application.isPlaying) return;
        SaveLog();
    }
#endif

    void OnApplicationQuit()
    {
        SaveLog();
    }

    private void SaveLog()
    {
        try
        {
            File.WriteAllLines(filePath, logLines);
            Debug.Log($"[PerformanceLogger] Performance log saved to: {filePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[PerformanceLogger] Failed to save log: {ex.Message}");
        }
    }
}
