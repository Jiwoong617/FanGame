using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class EventOptionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text buttonText;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TMP_Text>();
    }

    public void Init(string text, UnityAction onClick)
    {
        if (buttonText != null)
            buttonText.text = text;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(onClick);
    }
}
