using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText : MonoBehaviour
{
    private TextMeshPro textMesh;
    public Transform currentTarget { get; private set; }

    private float offset = 1f;

    public void Setup(Transform target, float amount, bool isCrit, bool isHeal, bool isFixed)
    {
        currentTarget = target;
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = Mathf.RoundToInt(amount).ToString();
        textMesh.alpha = 1f;

        transform.position = target.position + new Vector3(Random.Range(-0.2f, 0.2f), offset, 0);

        if (isHeal)
        {
            textMesh.color = Color.lightGreen;
            transform.localScale = Vector3.one * 1.2f;
        }
        else if (isCrit)
        {
            textMesh.color = Color.orangeRed;
            transform.localScale = Vector3.one * 1.8f;
        }
        else if (isFixed)
        {
            textMesh.color = Color.gray;
            transform.localScale = Vector3.one * 1.2f;
        }
        else
        {
            textMesh.color = Color.orange;
            transform.localScale = Vector3.one * 1.2f;
        }

        PlayAnimation();
    }

    public void Setup(Transform target, string message, Color textColor, float scaleMultiplier = 1.5f)
    {
        currentTarget = target;
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();

        textMesh.text = message;
        textMesh.color = textColor;
        textMesh.alpha = 1f;

        transform.localScale = Vector3.one * scaleMultiplier;

        transform.position = target.position + new Vector3(Random.Range(-0.2f, 0.2f), offset, 0);

        PlayAnimation();
    }

    private void PlayAnimation()
    {
        transform.DOKill();
        textMesh.DOKill();

        transform.DOPunchScale(Vector3.one * 0.5f, 0.2f, 1, 0.5f);
        transform.DOBlendableMoveBy(Vector3.up * 0.8f, 1.0f).SetEase(Ease.OutQuad);
        textMesh.DOFade(0f, 1.0f).SetEase(Ease.InQuart).OnComplete(() =>
        {
            GameManager.VFX.ReturnToPool(this);
        });
    }

    public void PushUp(float yOffset)
    {
        transform.DOBlendableMoveBy(Vector3.up * yOffset, 0.15f).SetEase(Ease.OutExpo);
    }

    private void OnDestroy()
    {
        transform.DOKill();
        textMesh.DOKill();
    }
}