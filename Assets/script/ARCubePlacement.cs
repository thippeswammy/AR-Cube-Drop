using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARCubePlacement : MonoBehaviour
{
    public ARCameraManager cameraManager; // Reference to ARCameraManager
    public GameObject cubePrefab;         // Prefab for the cube to place in AR
    public bool SaveImages=false;
    private Queue<Texture2D> imageQueue = new Queue<Texture2D>(); // Queue for storing images to save
    private bool isCapturingImages = false; // Flag to indicate capturing process
    private int imagesCaptured = 0;         // Counter for captured images
    private int totalImagesToCapture = 20;  // Total number of images to capture
    private float captureInterval = 0.5f;   // Interval between captures (in seconds)

    void Update()
    {
        // Detect touch input and place cube
        if (!isCapturingImages && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            Debug.Log("Screen tapped at: " + touchPosition);

            PlaceRandomColorCube(touchPosition);

            // Start capturing images
            if (SaveImages)
                StartCoroutine(CaptureImages());
            Debug.Log("Started capturing images.");
        }
    }

    private void PlaceRandomColorCube(Vector2 screenPosition)
    {
        // Convert screen position to a world position in front of the camera
        Camera mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        Vector3 worldPosition = ray.origin + ray.direction * 0.5f; // Place cube 2m in front of the camera

        // Instantiate cube at the calculated position
        GameObject newCube = Instantiate(cubePrefab, worldPosition, Quaternion.identity);
        Renderer cubeRenderer = newCube.GetComponent<Renderer>();
        cubeRenderer.material.color = new Color(Random.value, Random.value, Random.value); // Assign random color

        Debug.Log("Cube placed at: " + worldPosition);
    }

    private IEnumerator CaptureImages()
    {
        isCapturingImages = true;
        imagesCaptured = 0;

        while (imagesCaptured < totalImagesToCapture)
        {
            yield return new WaitForEndOfFrame(); // Wait until the current frame is rendered

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Main Camera is not found.");
                isCapturingImages = false;
                yield break;
            }

            // Create a RenderTexture to capture the camera's output
            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 24);
            mainCamera.targetTexture = renderTexture;

            // Render the camera's output to the RenderTexture
            Texture2D capturedImage = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            mainCamera.Render();
            RenderTexture.active = renderTexture;
            capturedImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            capturedImage.Apply();

            // Clean up RenderTexture
            mainCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);

            // Add captured image to the queue
            imageQueue.Enqueue(capturedImage);
            imagesCaptured++;
            Debug.Log($"Captured image {imagesCaptured}/{totalImagesToCapture}");

            // Start saving images asynchronously
            if (imageQueue.Count == 1)
            {
                StartCoroutine(SaveImagesFromQueue());
            }

            // Wait for the capture interval
            yield return new WaitForSeconds(captureInterval);
        }

        isCapturingImages = false;
        Debug.Log("Finished capturing images.");
    }

    private IEnumerator SaveImagesFromQueue()
    {
        while (imageQueue.Count > 0)
        {
            Texture2D imageToSave = imageQueue.Dequeue();

            // Save the image as a PNG
            byte[] pngData = imageToSave.EncodeToPNG();
            if (pngData != null && pngData.Length > 0)
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string path = System.IO.Path.Combine(Application.persistentDataPath, "CapturedImage_" + timestamp + ".png");
                System.IO.File.WriteAllBytes(path, pngData);
                Debug.Log("Image saved successfully at: " + path);
            }
            else
            {
                Debug.LogError("Failed to encode the captured image to PNG.");
            }

            yield return null; // Allow other processes to run
        }
    }
}
