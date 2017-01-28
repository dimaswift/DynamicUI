using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace DynamicUI
{
    public class DUIDialog : DUIScreen
    {
        static DUIDialog m_instance;

        public static DUIDialog instance
        {
            get
            {
                return m_instance;
            }
        }

        [SerializeField]
        Button m_okayButton;
        [SerializeField]
        Text m_messageText;

        public override void Init()
        {
            if (m_instance == null)
                m_instance = this;
            m_resetPositionOnLoad = true;
            base.Init();
            m_okayButton.onClick.AddListener(Hide);
            if (m_instance == this)
                Hide();
        }

        public void Open(string message)
        {
            m_messageText.text = message;
            Show();
        }
    }

}
