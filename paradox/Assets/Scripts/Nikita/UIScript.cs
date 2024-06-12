using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIScript : MonoBehaviour {
    #region Singletones
    public static UIScript uiScript { get; private set; }
    #endregion

    public bool isBlack { get; private set; } = false;

    [SerializeField]
    private GameObject uiObject;

    [SerializeField]
    private TextMeshProUGUI subtitlesTextUI;
    [SerializeField]
    private Image interactImage;
    [SerializeField]
    private Image blackoutImage;

    private void Awake() {
        uiScript = this;
        uiObject.SetActive(true);
        subtitlesTextUI.SetText("");
    }

    #region Blackout
    private Coroutine blackoutCoroutine;
    public void SetBlackoutImageState(float speed) {
        if (blackoutImage == null) 
            return;
        if (blackoutCoroutine != null) {
            StopCoroutine(blackoutCoroutine);
        }
        blackoutCoroutine = StartCoroutine(BlackoutImageSmooth(speed));
    }
    private IEnumerator BlackoutImageSmooth(float speed) {
        if (speed > 0f) {
            while (blackoutImage.color.a < 0.995f) {
                blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 
                    Mathf.Lerp(blackoutImage.color.a, 1, Time.deltaTime * speed));
                yield return new WaitForEndOfFrame();
            }
            blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 1);
            isBlack = true;
        } else {
            isBlack = false;
            while (blackoutImage.color.a > 0.005f) {
                blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b,
                    Mathf.Lerp(blackoutImage.color.a, 0, Time.deltaTime * speed * -1));
                yield return new WaitForEndOfFrame();
            }
            blackoutImage.color = new Color(blackoutImage.color.r, blackoutImage.color.g, blackoutImage.color.b, 0);
        }
        blackoutCoroutine = null;
        yield break;
    }
    #endregion

    #region HideVinete (REWORK)
    private Coroutine hideVineteCoroutine;
    private float hideAmmount = 0f;
    public void SetHideVineteState(bool isHiden) {
        if (hideVineteCoroutine != null) {
            StopCoroutine(hideVineteCoroutine);
        }
        hideVineteCoroutine = StartCoroutine(HideVineteSmooth(isHiden));
    }
    private IEnumerator HideVineteSmooth(bool isHiden) {
        if (isHiden) {
            while (hideAmmount <= 0.99f) {
                hideAmmount = Mathf.Lerp(hideAmmount, 1f, Time.deltaTime * 7);
                yield return new WaitForEndOfFrame();
            }
            hideAmmount = 1f;
        } else {
            while (hideAmmount >= 0.01f) {
                hideAmmount = Mathf.Lerp(hideAmmount, 0f, Time.deltaTime * 7);
                yield return new WaitForEndOfFrame();
            }
            hideAmmount = 0f;
        }
        hideVineteCoroutine = null;
        yield break;
    }
    #endregion

    #region InteractImage
    public void SetInteractImageVisibility(bool isVisible) {
        interactImage.enabled = isVisible;
    }

    public void SetInteractImagePosition(Vector3 pos) {
        if (pos == Vector3.zero) {
            SetInteractImageVisibility(false);
            return;
        }
        if (!interactImage.enabled) {
            SetInteractImageVisibility(true);
            interactImage.transform.position = Camera.main.WorldToScreenPoint(pos);
            return;
        }
        interactImage.transform.position = Vector3.Lerp(interactImage.transform.position, Camera.main.WorldToScreenPoint(pos), Time.deltaTime * 7f);
    }
    #endregion

    #region Subtitles
    public void SetSubtitlesText(string s) {
        subtitlesTextUI.SetText(s);
    }

    public void SetSubtitlesColor(Color color) {
        subtitlesTextUI.color = color;
    }
    #endregion
}