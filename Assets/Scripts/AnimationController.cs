using UnityEngine;
using System.Threading.Tasks;
using System.Threading;
using System;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private Vector3 startPosition = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 endPosition = new Vector3(5, 0.5f, 0);
    [SerializeField] private float duration = 10f;

    private int tapCount = 0; 
    private CancellationTokenSource token;

    private void Start()
    {
        token = new CancellationTokenSource();
        StartAnimationAsync(token.Token);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            tapCount++;
            if (tapCount >= 3)
            {
                token?.Cancel();
            }
        }
    }

    private void OnDestroy()
    {
        token?.Cancel();
        token?.Dispose(); 
    }

    private async void StartAnimationAsync(CancellationToken token)
    {
        try
        {
            await MoveObjectAsync(startPosition, endPosition, duration, token);
        }
        catch (OperationCanceledException)
        {
            targetObject.position = endPosition;
        }
    }

    private Task MoveObjectAsync(Vector3 from, Vector3 to, float animationDuration, CancellationToken token)
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        targetObject.position = from;

        float elapsedTime = 0f;

        async void Animate()
        {
            while (elapsedTime < animationDuration)
            {

                if (token.IsCancellationRequested)
                {
                    targetObject.position = to;
                    tcs.SetCanceled();
                    return;
                }

                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                targetObject.position = Vector3.Lerp(from, to, t);

                await Task.Yield();
            }

            tcs.SetResult(true);
        }

        Animate();
        return tcs.Task;
    }
}
