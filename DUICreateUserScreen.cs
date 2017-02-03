namespace DynamicUI
{
    using UnityEngine;
    using DynamicUI;
    using UnityEngine.UI;
    using UnityEngine.Events;
    [ExecuteInEditMode]
    public class DUICreateUserScreen : DUIScreen
    {
        [SerializeField]
        Elements m_elements;

        [SerializeField]
        UserEventHandler m_onUserCreated;

        public UserEventHandler onUserCreated { get { return m_onUserCreated; } }

        public Elements elements 
        { 
            get 
            {
                return m_elements;
            }
        }

        public override bool allowUpdatingElements
        {
            get
            {
                return false; 
            }
        }

        #region Initilization

        protected virtual void BindElements()
        {
            elements.Bind(this);
        }

        public override void Init(DUICanvas canvas)
        {
            base.Init(canvas);
            elements.passwordField.validator = GetPasswordValidator;
            elements.passwordConfirmField.validator = GetConfirmPasswordValidator;
            elements.userNameField.validator = GetUsernameValidator;
            elements.passwordField.inputField.onValueChanged.AddListener(OnFieldEditEnd);
            elements.passwordConfirmField.inputField.onValueChanged.AddListener(OnFieldEditEnd);
            elements.userNameField.inputField.onValueChanged.AddListener(OnFieldEditEnd);
            elements.createButton.onClick.AddListener(OnCreateButtonPressed);
        }

#endregion Initilization

#region Elements

        [System.Serializable]
        public class Elements
        {
            [SerializeField]
            DUIValidationField m_passwordField;

            [SerializeField]
            DUIValidationField m_userNameField;

            [SerializeField]
            Button m_createButton;

            [SerializeField]
            DUIValidationField m_passwordConfirmField;

            public DUIValidationField passwordField 
            { 
                get 
                {
                    return m_passwordField;
                }
            }

            public DUIValidationField userNameField 
            { 
                get 
                {
                    return m_userNameField;
                }
            }

            public Button createButton 
            { 
                get 
                {
                    return m_createButton;
                }
            }

            public DUIValidationField passwordConfirmField 
            { 
                get 
                {
                    return m_passwordConfirmField;
                }
            }

            public void Bind(DUICreateUserScreen screen)
            {
#if UNITY_EDITOR
                var root = screen.transform;
                var so = new UnityEditor.SerializedObject(screen).FindProperty("m_elements");
                so.FindPropertyRelative("m_passwordField").objectReferenceValue = root.FindChild("passwordField").GetComponent<DUIValidationField>();
                so.FindPropertyRelative("m_userNameField").objectReferenceValue = root.FindChild("userNameField").GetComponent<DUIValidationField>();
                so.FindPropertyRelative("m_passwordConfirmField").objectReferenceValue = root.FindChild("passwordConfirmField").GetComponent<DUIValidationField>();
                so.FindPropertyRelative("m_createButton").objectReferenceValue = root.FindChild("createButton").GetComponent<Button>();
                so.serializedObject.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(screen);
#endif
            }
        }

#endregion Elements

#region Events

        Validator GetUsernameValidator(string userName)
        {
            var validator = new Validator();
            if (string.IsNullOrEmpty(userName) || userName.Length < 3)
            {
                validator.errorMessage = Local.Translate("User name should be at least 3 characters long!");
                return validator;
            }
            validator.isValid = true;
            return validator;
        }

        Validator GetPasswordValidator(string password)
        {
            var validator = new Validator();
            if(string.IsNullOrEmpty(password) || password.Length < 3)
            {
                validator.errorMessage = Local.Translate("Password should be at least 3 characters long!");
                return validator;
            }
            validator.isValid = true;
            return validator;
        }

        Validator GetConfirmPasswordValidator(string passwordConfirm)
        {
            var validator = new Validator();
            if (elements.passwordField.isValid == false)
            {
                validator.errorMessage = Local.Translate("Invalid password!");
                return validator;
            }
            if (passwordConfirm != elements.passwordField.inputField.text)
            {
                validator.errorMessage = Local.Translate("Passwords do not match!");
                return validator;
            }
            validator.isValid = true;
            return validator;
        }

        void OnCreateButtonPressed()
        {
            elements.userNameField.Check();
            elements.passwordField.Check();
            elements.passwordConfirmField.Check();
            if(IsReadyToSubmit())
            {
                Hide();
                onUserCreated.Invoke(elements.userNameField.inputField.text, elements.passwordField.inputField.text);
            }
        }


        bool IsReadyToSubmit()
        {
            return elements.userNameField.isValid && elements.passwordConfirmField.isValid && elements.passwordField.isValid;
        }

        void OnFieldEditEnd(string text)
        {
            bool ready = IsReadyToSubmit(); 
            if(ready)
            {
                CheckInputs();
            }
            elements.createButton.interactable = ready;
        }

        void CheckInputs()
        {
            if (elements.userNameField.inputField.text != string.Empty)
                elements.userNameField.Check();
            if (elements.passwordField.inputField.text != string.Empty)
                elements.passwordField.Check();
            if (elements.passwordConfirmField.inputField.text != string.Empty)
                elements.passwordConfirmField.Check();
        }

        void ClearInputs()
        {
            elements.passwordConfirmField.Clear();
            elements.passwordField.Clear();
            elements.userNameField.Clear();
        }

        public override void Show()
        {
            ClearInputs();
            CheckInputs();
            base.Show();
        }

#endregion Events

    }

    [System.Serializable]
    public class UserEventHandler : UnityEvent<string, string>
    {
        public UserEventHandler() { }
    }
}
