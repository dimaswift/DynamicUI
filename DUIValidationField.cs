using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DynamicUI
{
    [RequireComponent(typeof(InputField))]
    public class DUIValidationField : DUIElement
    {
        [SerializeField]
        Text m_errorText;
        [SerializeField]
        Image m_successIcon;

        InputField m_inputField;
        [SerializeField]
        float m_fadeSpeed = .5f;
        public delegate Validator ValidationHandler(string input);

        public ValidationHandler validator { get; set; }

        public InputField inputField
        {
            get
            {
                if (m_inputField == null)
                    m_inputField = GetComponent<InputField>();
                return m_inputField;
            }
        }

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            m_inputField = GetComponent<InputField>();
            m_inputField.onEndEdit.AddListener(OnEndEdit);
            Clear();
        }

        public void Clear()
        {
            m_errorText.CrossFadeAlpha(0, 0, true);
            m_successIcon.CrossFadeAlpha(0, 0, true);
        }

        public void Check()
        {
            OnEndEdit(inputField.text);
        }

        public bool isValid { get { return validator(inputField.text).isValid; } }

        public virtual void OnEndEdit(string str)
        {
            var result = validator(str);
            if (result.isValid)
            {
                m_errorText.CrossFadeAlpha(0, m_fadeSpeed, true);
                m_successIcon.CrossFadeAlpha(1, m_fadeSpeed, true);
            }
            else
            {
                m_errorText.text = result.errorMessage;
                m_errorText.CrossFadeAlpha(1, m_fadeSpeed, true);
                m_successIcon.CrossFadeAlpha(0, m_fadeSpeed, true);
            }
        }
    }
    public struct Validator
    {
        public bool isValid;
        public string errorMessage;
    }
}
