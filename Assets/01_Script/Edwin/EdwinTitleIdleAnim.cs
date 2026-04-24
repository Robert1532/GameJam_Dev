using UnityEngine;

/// <summary>
/// Movimiento suave en el bloque del título (flotación vertical, ligera escala y balanceo).
/// Va en <c>TitleRoot</c>. Usa <see cref="Time.unscaledTime"/> para seguir con timeScale 0.
/// </summary>
[DisallowMultipleComponent]
public sealed class EdwinTitleIdleAnim : MonoBehaviour
{
    [SerializeField] float bobPixels = 5f;
    [SerializeField] float bobHz = 0.35f;
    [SerializeField] float scaleBreath = 0.015f;
    [SerializeField] float scaleHz = 0.55f;
    [SerializeField] float swayDegrees = 0.35f;
    [SerializeField] float swayHz = 0.28f;
    [SerializeField] bool swayRotation = true;

    RectTransform _rt;
    Vector2 _baseAnchored;
    Vector3 _baseScale;
    Vector3 _baseEuler;

    void Awake()
    {
        CacheBase();
    }

    void OnEnable()
    {
        CacheBase();
    }

    void CacheBase()
    {
        _rt = (RectTransform)transform;
        _baseAnchored = _rt.anchoredPosition;
        _baseScale = _rt.localScale;
        _baseEuler = _rt.localEulerAngles;
    }

    void Update()
    {
        if (_rt == null)
            return;

        float t = Time.unscaledTime;
        float bob = Mathf.Sin(t * (Mathf.PI * 2f * bobHz)) * bobPixels;
        _rt.anchoredPosition = _baseAnchored + new Vector2(0f, bob);

        float breathe = 1f + Mathf.Sin(t * (Mathf.PI * 2f * scaleHz)) * scaleBreath;
        _rt.localScale = _baseScale * breathe;

        if (swayRotation && swayDegrees > 0.001f)
        {
            float z = Mathf.Sin(t * (Mathf.PI * 2f * swayHz)) * swayDegrees;
            _rt.localEulerAngles = _baseEuler + new Vector3(0f, 0f, z);
        }
        else
            _rt.localEulerAngles = _baseEuler;
    }

    void OnDisable()
    {
        if (_rt == null)
            return;
        _rt.anchoredPosition = _baseAnchored;
        _rt.localScale = _baseScale;
        _rt.localEulerAngles = _baseEuler;
    }
}
