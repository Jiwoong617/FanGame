using System;
using UnityEngine;

public class RestManager
{
    public event Action OnRestFinished;
    private RestUI restUI;

    public void SetUI(RestUI ui)
    {
        restUI = ui;
    }

    public void StartRest()
    {
        if (restUI == null)
        {
            Debug.LogError("[RestManager] RestUI is not set!");
            CompleteRest();
            return;
        }

        restUI.ShowRest();
    }

    public void CompleteRest()
    {
        Debug.Log("[RestManager] Rest Completed.");
        OnRestFinished?.Invoke();
    }
}
