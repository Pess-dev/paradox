using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtitlesScript : MonoBehaviour
{
    #region Singletones
    public static SubtitlesScript subtitlesScript { get; private set; }

    private UIScript uiScript;
    #endregion

    private int curLinePriority = 0;
    private Coroutine curSmoothNewLineUpdateCoroutine = null;

    private float lineSpeedUp = 1f;

    [SerializeField]
    private bool language = false;

    private void Awake() {
        subtitlesScript = this;
    }

    private void Start() {
        uiScript = UIScript.uiScript;
    }

    public void NewLine(SubtitlesLine line, int priority, float duration) {
        if (curLinePriority > priority)
            return;

        if (curSmoothNewLineUpdateCoroutine != null)
            StopCoroutine(curSmoothNewLineUpdateCoroutine);

        curSmoothNewLineUpdateCoroutine = StartCoroutine(SmoothNewLineUpdate(line, duration));
    }

    public void ClearLine() {
        if (curSmoothNewLineUpdateCoroutine != null) {
            StopCoroutine(curSmoothNewLineUpdateCoroutine);
        }
        curLinePriority = 0;
        uiScript.SetSubtitlesText("");
    }

    public bool IsPrintingLine() {
        return (curSmoothNewLineUpdateCoroutine != null);
    }

    public void SpeedUpLine(float times) {
        if (curSmoothNewLineUpdateCoroutine != null)
            lineSpeedUp = times;
    }

    private IEnumerator SmoothNewLineUpdate(SubtitlesLine line, float duration) {
        uiScript.SetSubtitlesText("");
        string testString = "";
        string[] sM = line.line.Split('@');
        string s = "";
        if (sM.Length <= 1) {
            s = line.line;
        } else {
            s = (language) ? sM[0] : sM[1];
        }
        uiScript.SetSubtitlesColor(line.lineColor);
        foreach (char item in s) {
            testString += item;
            uiScript.SetSubtitlesText(testString);
            float timeToWait = 1f / line.lineSpeed;
            timeToWait *= ((item == '.') ? 3f : ((item == ',' || item == '!' || item == '?') ? 2f : 1f));
            yield return new WaitForSeconds(timeToWait / lineSpeedUp);
        }
        lineSpeedUp = 1f;
        if (duration != 0f) {
            yield return new WaitForSeconds(duration);
            lineSpeedUp = 1f;
            ClearLine();
        }
        curSmoothNewLineUpdateCoroutine = null;
    }
}
