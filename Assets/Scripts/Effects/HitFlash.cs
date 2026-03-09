using UnityEngine;
using DG.Tweening;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeStrength = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Material originalMaterial;
    private Material flashMaterial;
    private Shader flashShader;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        // 쉐이더 로드
        flashShader = Shader.Find("Custom/SpriteFlash");
        if (flashShader == null)
            flashShader = Resources.Load<Shader>("Shaders/SpriteFlash");
        
        if (flashShader == null)
        {
            Debug.LogError("Custom/SpriteFlash shader not found!");
            return;
        }

        originalMaterial = spriteRenderer.material;
        flashMaterial = new Material(flashShader);
        flashMaterial.color = originalMaterial.color;

        flashMaterial.SetColor("_FlashColor", flashColor);
    }

    public void Flash(bool shake = true)
    {
        if (spriteRenderer == null || flashMaterial == null) return;

        //연속 피격 방지
        spriteRenderer.transform.DOKill();
        flashMaterial.DOKill();

        // 머티리얼 교체 (Flash Shader 사용)
        spriteRenderer.material = flashMaterial;
        
        // FlashAmount를 1로 설정 (완전 흰색)
        flashMaterial.SetFloat("_FlashAmount", 1f);

        // DOTween으로 0까지 줄이기
        flashMaterial.DOFloat(0f, "_FlashAmount", flashDuration).OnComplete(() =>
        {
            // 끝나면 원래 머티리얼로 복구 (선택 사항, 성능상 FlashMaterial을 계속 써도 됨)
            // 하지만 다른 효과(Outline 등)와 겹칠 수 있으니 복구하는 게 안전
            if (spriteRenderer != null)
                spriteRenderer.material = originalMaterial;
        });

        if(shake)
        {
            // TODO : 만약 막 튀는게 싫으면 shakeStrength 이거 벡터로 바꿔서 하면 될듯
            spriteRenderer.transform.DOShakePosition(shakeDuration, shakeStrength, vibrato: 20, randomness: 90, snapping: false, fadeOut: true)
                        .OnComplete(() =>
                        {
                            spriteRenderer.transform.localPosition = Vector3.zero;
                        });
        }
    }

    private void OnDestroy()
    {
        if (spriteRenderer != null)
            spriteRenderer.transform.DOKill(true);

        if (flashMaterial != null)
        {
            flashMaterial.DOKill(true);
            Destroy(flashMaterial);
        }
    }
}
