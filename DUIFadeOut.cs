using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace DynamicUI
{
    public class DUIFadeOut : MonoBehaviour
    {
        [SerializeField]
        Image m_fadeoutImage;
        [SerializeField]
        Text m_fadeoutMessageText;

        [SerializeField]
        float m_fadeDuration = .4f;
        [SerializeField]
        float m_actionDuration = .4f;
        [SerializeField]
        float m_messageShowDelay = 0f;
        [SerializeField]
        float m_messageHideDelay = 0f;
        [SerializeField]
        float m_messageDuration = 1f;

        public bool isFadingOut { get; private set; }

        public void Init()
        {
            Hide();
        }

        public void Hide()
        {
            m_fadeoutImage.gameObject.SetActive(false);
            m_fadeoutImage.CrossFadeAlpha(0, 0, true);
            m_fadeoutMessageText.gameObject.SetActive(false);
            m_fadeoutMessageText.CrossFadeAlpha(0, 0, true);
        }

        public void FadeOut(System.Action action, float delay, float fadeDuration, float actionDuration)
        {
            StopAllCoroutines();
            StartCoroutine(FadingOut(action, delay, fadeDuration, actionDuration));

        }
        public void FadeOutMessage(System.Action action, 
            float delay,
            string message, 
            float fadeDuration, 
            float messageShowDelay, 
            float messageDuration)
        {
            StopAllCoroutines();
            StartCoroutine(FadingOutWithMessage(action, delay, message, fadeDuration,
                messageShowDelay, m_messageHideDelay, messageDuration));
        }

        public void FadeOut(System.Action action, float delay = 0)
        {
            StopAllCoroutines();
            StartCoroutine(FadingOut(action, delay, m_fadeDuration, m_actionDuration));
        }

        public void FadeOutMessage(System.Action action, string message, float delay = 0)
        {
            StopAllCoroutines();
            StartCoroutine(FadingOutWithMessage(action, delay, message, m_fadeDuration, 
                m_messageShowDelay, m_messageHideDelay, m_messageDuration));
        }

        IEnumerator FadingOut(System.Action action, float delay, float fadeDuration, float actionDuration)
        {
            isFadingOut = true;
            yield return new WaitForSeconds(delay);
            m_fadeoutImage.CrossFadeAlpha(0, 0, true);
            m_fadeoutImage.gameObject.SetActive(true);
            m_fadeoutImage.CrossFadeAlpha(1, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration);
            action();
            yield return new WaitForSeconds(actionDuration);
            m_fadeoutImage.CrossFadeAlpha(0, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration);
            m_fadeoutImage.gameObject.SetActive(false);
            isFadingOut = false;
        }

        IEnumerator FadingOutWithMessage(System.Action action, 
            float delay,
            string message, 
            float fadeDuration, 
            float messageShowDelay,
            float messageHideDelay, 
            float messageDuration)
        {
            isFadingOut = true;
            yield return new WaitForSeconds(delay);
            m_fadeoutMessageText.text = message;
            m_fadeoutMessageText.CrossFadeAlpha(0, 0, true);
            m_fadeoutMessageText.gameObject.SetActive(true);
            m_fadeoutImage.CrossFadeAlpha(0, 0, true);
            m_fadeoutImage.gameObject.SetActive(true);
            m_fadeoutImage.CrossFadeAlpha(1, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration);
            yield return new WaitForSeconds(messageShowDelay);
            m_fadeoutMessageText.CrossFadeAlpha(1, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration);
            action();
            yield return new WaitForSeconds(messageDuration);
            m_fadeoutMessageText.CrossFadeAlpha(0, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration + messageHideDelay);
            m_fadeoutImage.CrossFadeAlpha(0, fadeDuration, true);
            yield return new WaitForSeconds(fadeDuration);
            m_fadeoutImage.gameObject.SetActive(false);
            m_fadeoutMessageText.gameObject.SetActive(false);
            isFadingOut = false;
        }
    }
}
