using UnityEngine;
using System.Threading.Tasks;

public class Example : MonoBehaviour
{
    private void Start()
    {
        RunCalculationInThreadAsync();
    }

    private async void RunCalculationInThreadAsync()
    {
        int result = await Task.Run(() => Calculate(10));

        Debug.Log($"Result: {result}");
    }

    private int Calculate(int number)
    {
        int result = 0;

        for (int i = 1; i <= number; i++)
        {
            result++;
            System.Threading.Thread.Sleep(100);
        }

        return result;
    }
}
