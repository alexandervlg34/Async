using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.UI;

public class BootstrapEntryPoint : MonoBehaviour
{
    private const string SCENE_NAME = "Gameplay";
    private const float OPERATION_WEIGHT = 1f / 3f;

    [SerializeField] private Image targetImage;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private string imageUrl = "https://picsum.photos/200";
    [SerializeField] private string resourcePath = "Prefabs/ImagePrefab";
    [SerializeField] private Slider progressBar;

    private float totalProgress = 0f;

    private async void Start()
    {
        await LoadGameAsync();
    }

    private async Task LoadGameAsync()
    {
        totalProgress = 0f;
        UpdateProgressBar();

        bool imageLoaded = await LoadImageFromUrlAsync(imageUrl);
        if (!imageLoaded)
        {
            Debug.LogError("Failed to load image");
            return;
        }

        totalProgress += OPERATION_WEIGHT;
        UpdateProgressBar();

        
        Object loadedResource = await LoadResourceAsync<Object>(resourcePath);
        if (loadedResource == null)
        {
            Debug.LogError("Failed to load resource");
            return;
        }
        totalProgress += OPERATION_WEIGHT;
        UpdateProgressBar();
        

        bool sceneLoaded = await LoadSceneAsync(SCENE_NAME);
        if (!sceneLoaded)
        {
            Debug.LogError("Failed to load scene");
            return;
        }
        totalProgress = 1f;
        UpdateProgressBar();
    }

    private async Task<bool> LoadImageFromUrlAsync(string url)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                totalProgress = OPERATION_WEIGHT * operation.progress;
                UpdateProgressBar();
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Image load failed: {request.error}");
                return false;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

            if (targetImage != null)
                targetImage.sprite = sprite;
            else if (spriteRenderer != null)
                spriteRenderer.sprite = sprite;

            return true;
        }
    }

    private async Task<T> LoadResourceAsync<T>(string path) where T : Object
    {
        ResourceRequest request = Resources.LoadAsync<T>(path);
        while (!request.isDone)
        {
            totalProgress = OPERATION_WEIGHT + (OPERATION_WEIGHT * request.progress);
            UpdateProgressBar();
            await Task.Yield();
        }

        return request.asset as T;
    }

    private async Task<bool> LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(SCENE_NAME);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            totalProgress = (2 * OPERATION_WEIGHT) + (OPERATION_WEIGHT * operation.progress);
            UpdateProgressBar();
            await Task.Yield();
        }

        return true;
    }

    private void UpdateProgressBar()
    {
        if (progressBar != null)
        {
            progressBar.value = Mathf.Clamp01(totalProgress);
            Debug.Log($"Progress: {totalProgress * 100:F2}%");
        }
    }
}
