using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Escala y aclara el texto al pasar el ratón, puntero o dedo (UI + EventSystem).
/// Va en el mismo GameObject que el <see cref="Button"/> (el Image recibe el raycast).
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class EdwinStartButtonHoverAnim : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [SerializeField] float hoverScale = 1.08f;
    [SerializeField] float pressScale = 0.94f;
    [SerializeField] float smooth = 14f;

    RectTransform _rt;
    Text _label;
    Vector3 _baseScale;
    Color _baseTextColor;
    bool _hover;
    bool _pressed;
    bool _hasLabel;

    void Awake()
    {
        _rt = (RectTransform)transform;
        _baseScale = _rt.localScale;
        _label = GetComponentInChildren<Text>(true);
        _hasLabel = _label != null;
        if (_hasLabel)
            _baseTextColor = _label.color;
    }

    void OnDisable()
    {
        _hover = false;
        _pressed = false;
        ApplyTargets(force: true);
    }

    void Update()
    {
        ApplyTargets(force: false);
    }

    void ApplyTargets(bool force)
    {
        float mul = _pressed ? pressScale : (_hover ? hoverScale : 1f);
        var scaleTarget = _baseScale * mul;
        if (force)
            _rt.localScale = scaleTarget;
        else
            _rt.localScale = Vector3.Lerp(_rt.localScale, scaleTarget, Time.unscaledDeltaTime * smooth);

        if (!_hasLabel)
            return;

        var colorTarget = _hover || _pressed ? Brighten(_baseTextColor) : _baseTextColor;
        if (force)
            _label.color = colorTarget;
        else
            _label.color = Color.Lerp(_label.color, colorTarget, Time.unscaledDeltaTime * smooth);
    }

    static Color Brighten(Color c) => Color.Lerp(c, Color.white, 0.28f);

    public void OnPointerEnter(PointerEventData eventData) => _hover = true;

    public void OnPointerExit(PointerEventData eventData) => _hover = false;

    public void OnPointerDown(PointerEventData eventData) => _pressed = true;

    public void OnPointerUp(PointerEventData eventData) => _pressed = false;
}
