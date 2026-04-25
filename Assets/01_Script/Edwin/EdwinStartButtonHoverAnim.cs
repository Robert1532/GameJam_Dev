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
    IPointerUpHandler,
    IPointerClickHandler
{
    [SerializeField] float hoverScale = 1.08f;
    [SerializeField] float pressScale = 0.94f;
    [SerializeField] float smooth = 14f;
    [SerializeField] AudioClip hoverClip;
    [SerializeField] AudioClip clickClip;
    [SerializeField, Range(0f, 1f)] float hoverVolume = 1f;
    [SerializeField, Range(0f, 1f)] float clickVolume = 1f;

    AudioSource _uiSfx;
    AudioClip _hoverClip;
    AudioClip _clickClip;
    float _hoverVolume = 1f;
    float _clickVolume = 1f;

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

        _uiSfx = GetComponent<AudioSource>();
        if (_uiSfx == null)
            _uiSfx = gameObject.AddComponent<AudioSource>();
        _uiSfx.playOnAwake = false;
        _uiSfx.loop = false;
        _uiSfx.spatialBlend = 0f;
        _uiSfx.volume = 1f;
        _uiSfx.dopplerLevel = 0f;
        _uiSfx.ignoreListenerPause = true;
        if (hoverClip != null)
            _hoverClip = hoverClip;
        if (clickClip != null)
            _clickClip = clickClip;
        _hoverVolume = Mathf.Clamp01(hoverVolume);
        _clickVolume = Mathf.Clamp01(clickVolume);
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

    /// <summary>
    /// Asigna el origen 2D para <see cref="AudioSource.PlayOneShot"/> al entrar el puntero (hover).
    /// </summary>
    public void SetUiSfxSource(AudioSource uiSfx, AudioClip hoverClip, float volumeScale)
    {
        _uiSfx = uiSfx;
        _hoverClip = hoverClip;
        _hoverVolume = Mathf.Clamp01(volumeScale);
    }

    /// <summary>
    /// Asigna el origen 2D y los clips para hover/click desde código.
    /// </summary>
    public void SetUiSfxSource(AudioSource uiSfx, AudioClip hoverClip, AudioClip clickClip, float hoverVol, float clickVol)
    {
        _uiSfx = uiSfx;
        _hoverClip = hoverClip;
        _clickClip = clickClip;
        _hoverVolume = Mathf.Clamp01(hoverVol);
        _clickVolume = Mathf.Clamp01(clickVol);
    }

    void PlayHoverSound()
    {
        if (_hoverClip == null || _uiSfx == null)
            return;
        if (_hoverClip.loadState == AudioDataLoadState.Unloaded)
            _hoverClip.LoadAudioData();
        _uiSfx.PlayOneShot(_hoverClip, _hoverVolume);
    }

    void PlayClickSound()
    {
        if (_clickClip == null || _uiSfx == null)
            return;
        if (_clickClip.loadState == AudioDataLoadState.Unloaded)
            _clickClip.LoadAudioData();
        _uiSfx.PlayOneShot(_clickClip, _clickVolume);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hover = true;
        PlayHoverSound();
    }

    public void OnPointerExit(PointerEventData eventData) => _hover = false;

    public void OnPointerDown(PointerEventData eventData) => _pressed = true;

    public void OnPointerUp(PointerEventData eventData) => _pressed = false;

    public void OnPointerClick(PointerEventData eventData) => PlayClickSound();

    /// <summary>
    /// Tras cambiar el color del <see cref="Text"/> hijo por código, actualiza el color base del hover.
    /// </summary>
    public void SyncBaseTextColorFromLabel()
    {
        if (_label == null)
            _label = GetComponentInChildren<Text>(true);
        _hasLabel = _label != null;
        if (_hasLabel)
            _baseTextColor = _label.color;
        ApplyTargets(force: true);
    }
}
