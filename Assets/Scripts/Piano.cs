using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Piano : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    //public int NumberOfTapsToDestroy; TODO make piano destroy from not only one tap
    public Color32 Color;
    [HideInInspector]
    public float GameSpeed = 10f;

    #endregion // PUBLIC_MEMBER_VARIABLES


    #region PRIVATE_MEMBER_VARIABLES

    private RectTransform _rectTransform;
    private float _deadLine; // the line after, which piano will be destroyed. this line is under screen

    #endregion // PRIVATE_MEMBER_VARIABLES


    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Start ()
	{
        _rectTransform = GetComponent<RectTransform>();
        GetComponent<Image>().color = Color;

        _deadLine = -(GameController.Instance.ReferenceResolution.y + _rectTransform.sizeDelta.y / 2f); // setting deadline
	    if (GameController.Instance._bGame) // double-check if game is still running, then start moving
	    {
	        _rectTransform.DOAnchorPosY(_deadLine, GameSpeed);
	    }
	}

    void Update()
    {
        if (_rectTransform.anchoredPosition.y <= _deadLine + 1) // if we reach the deadling, destroy the piano or start move back if game is finished
        {
            if (GameController.Instance._bGame)
            {
                GameController.Instance.MakeDamage();
                GameController.Instance.CreatePiano();
                Destroy(gameObject);
            }
            else
            {
                _rectTransform.DOPlayBackwards();
            }
        }
    }
    #endregion // UNTIY_MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS

    public void Press() // if we pressed on a piano
    {
        GameController.Instance.IncreaseSpeedOfPianos(); // increase speed
        GameController.Instance.CreatePiano();  // create new piano
        Destroy(gameObject); // and destroy this
    }

    #endregion // PUBLIC_METHODS
}
