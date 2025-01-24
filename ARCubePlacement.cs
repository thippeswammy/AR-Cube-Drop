using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARCubePlacement : MonoBehaviour
{
    public ARCameraManager cameraManager; // Reference to ARCameraManager
    public GameObject cubePrefab; // Assign a cube prefab in the Inspector

    private Texture2D capturedImage;
    private bool isImageCaptureScheduled = false;
    private int frameCountAfterTap = 0;

    void Update()
    {
        // Detect touch input and place cube
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            PlaceRandomColorCube(touchPosition);

            // Schedule image capture for 10 frames later
            isImageCaptureScheduled = true;
            frameCountAfterTap = 0;
        }

        // Check if image capture is scheduled
        if (isImageCaptureScheduled)
        {
            frameCountAfterTap++;
            if (frameCountAfterTap == 10) // Capture image on the 10th frame
            {
                CaptureARImage();
                isImageCaptureScheduled = false;
            }
        }
    }

    private void PlaceRandomColorCube(Vector2 screenPosition)
    {
        // Convert screen position to a world position in front of the camera
        Camera mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Vector3 worldPosition = ray.origin + ray.direction * 0.5f; // Place cube 0.5m in front of the camera

        // Instantiate cube at the calculated position
        GameObject newCube = Instantiate(cubePrefab, worldPosition, Quaternion.identity);
        Renderer cubeRenderer = newCube.GetComponent<Renderer>();
        cubeRenderer.material.color = new Color(Random.value, Random.value, Random.value); // Random color
    }

    private void CaptureARImage()
    {
        if (cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
        {
            // Convert the CPU image to Texture2D
            capturedImage = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
                outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            var rawTextureData = capturedImage.GetRawTextureData<byte>();
            cpuImage.Convert(conversionParams, rawTextureData);
            cpuImage.Dispose();
            capturedImage.Apply();

            // Save captured image to Application.persistentDataPath
            byte[] pngData = capturedImage.EncodeToPNG();
            if (pngData != null && pngData.Length > 0) // Ensure pngData is valid
            {
                string path = System.IO.Path.Combine(Application.persistentDataPath, "CapturedImage.png");
                System.IO.File.WriteAllBytes(path, pngData);
                Debug.Log("Image saved at: " + path);
            }
            else
            {
                Debug.LogError("Failed to encode the captured image to PNG.");
            }
        }
        else
        {
            Debug.LogError("Failed to acquire the latest CPU image.");
        }
    }

}
