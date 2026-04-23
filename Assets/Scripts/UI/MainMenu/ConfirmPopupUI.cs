using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ConfirmPopupUI : UI_Base
{
    enum Texts { MessageText }
    enum Buttons { ConfirmButton, CancelButton }

    private RectTransform panel;
    private Action onConfirm;

    public bool IsVisible => gameObject.activeSelf;

    protected override void Init()
    {
        panel = GetComponent<RectTransform>();

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Button>(typeof(Buttons));

        Get<Button>(Buttons.ConfirmButton).onClick.AddListener(OnClickConfirm);
        Get<Button>(Buttons.CancelButton).onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

    public void Show(string message, Action confirmAction)
    {
        onConfirm = confirmAction;
        Get<TextMeshProUGUI>(Texts.MessageText).text = message;

        gameObject.SetActive(true);
        panel.DOKill();
        panel.localScale = Vector3.zero;
        panel.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public override void Hide()
    {
        panel.DOKill();
        panel.DOScale(Vector3.zero, 0.15f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() => gameObject.SetActive(false));
    }

    private void OnClickConfirm()
    {
        gameObject.SetActive(false);
        onConfirm?.Invoke();
    }
}
