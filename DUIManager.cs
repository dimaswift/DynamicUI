using UnityEngine;
using System.Collections;

public class DUIManager : MonoBehaviour
{
    #region Screens

    [SerializeField]
    TutorialScreen m_tutorialScreen;
    public TutorialScreen tutorialScreen
    {
        get
        {
            return m_tutorialScreen;
        }
    }


    #endregion

    public virtual void Init()
    {

    }
}
