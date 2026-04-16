using UnityEngine;
using UnityEngine.InputSystem; // 추가됨
using System.IO;

public class TutorialScreenshot : MonoBehaviour
{
    public int superSize = 1;
    public string folderName = "TutorialScreenshots";

    void Update()
    {
        // New Input System 방식: G 키가 이번 프레임에 눌렸는지 확인
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            Capture();
        }
    }

    public void Capture()
    {
        string directoryPath = Path.Combine(Application.dataPath, "../", folderName);

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"Screenshot_{timestamp}.png";
        string fullPath = Path.Combine(directoryPath, fileName);

        ScreenCapture.CaptureScreenshot(fullPath, superSize);
        Debug.Log($"<color=green>캡처 완료!</color> 경로: {fullPath}");
    }
}