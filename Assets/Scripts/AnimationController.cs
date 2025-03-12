using UnityEngine;
using System.Threading.Tasks;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 startPosition = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 endPosition = new Vector3(5, 0.5f, 0);
    [SerializeField] private float duration = 8f;

    private int tapCount = 0; 
    private bool shouldCompleteInstantly = false;

    private void Start()
    {
        StartAnimationAsync();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tapCount++;
            if (tapCount >= 3)
            {
                shouldCompleteInstantly = true;
            }
        }
    }

    private async void StartAnimationAsync()
    {
        await MoveObjectAsync(startPosition, endPosition, duration);
        Debug.Log("Animation completed");
    }

    private Task MoveObjectAsync(Vector3 from, Vector3 to, float animationDuration)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        targetObject.position = from;

        float elapsedTime = 0f;

        async void Animate()
        {
            while (elapsedTime < animationDuration && !shouldCompleteInstantly)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);

                targetObject.position = Vector3.Lerp(from, to, t);

                await Task.Yield();
            }

            if (shouldCompleteInstantly)
            {
                targetObject.position = to;
                Debug.Log("Animation force completed due to triple tap");
            }

            tcs.SetResult(true);
        }

        Animate();
        return tcs.Task;
    }
}
