using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//[RequireComponent(typeof(ScrollRect))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Mask))]
public class SwipeController : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    #region PUBLIC_MEMBER_VARIABLES

    public RectTransform Content;
    public List<Sprite> Dots = new List<Sprite>();
    public Image DotsImage;
    public float Speed = 100f;
    public int CurrentPlane;

    [Header("Animations For Seconds Slide")]
    public GameObject Sparkles;
    public Animation Hand;
    public Image Tochange;
    public Sprite Change;
    public GameObject ExitButton;

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private float _sizeOfImage;
    private float _initialSizeOfImage;

    private float _beginDrag; // to know in what direction did we swipe

    private int _amountOfImages;
    private bool _justOnce;

    #endregion // PRIVATE_MEMBER_VARIABLES



    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        _initialSizeOfImage = GetComponent<RectTransform>().rect.width;
        _amountOfImages = Content.childCount - 1;

        Content.transform.GetChild(0).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        for (int i = 1; i < Content.childCount; i++)
        {
            Content.transform.GetChild(i).GetComponent<RectTransform>().anchoredPosition = new Vector2(
                Content.transform.GetChild(i - 1).GetComponent<RectTransform>().anchoredPosition.x
                + 
                Content.transform.GetChild(i).GetComponent<RectTransform>().rect.width, 0);
        }
        FindObjectOfType<EventSystem>().pixelDragThreshold = 1;
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region PUBLIC_METHODS

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Mathf.Abs(Content.offsetMin.x - _beginDrag) >= 0) // if we swipped more than 0.25 of screen size
        {
            if (_beginDrag > Content.offsetMin.x) // right
            {
                if (CurrentPlane >= _amountOfImages)
                {
                    return;
                }
                _sizeOfImage -= _initialSizeOfImage;
                CurrentPlane++;
            }
            else // left
            {
                if (CurrentPlane <= 0)
                {
                    return;
                }
                _sizeOfImage += _initialSizeOfImage;
                CurrentPlane--;
            }
            NextSprite(CurrentPlane);
            StartCoroutine(Move());
        }
        else // if not
        {
            //Debug.Log("No");
        }
        if (CurrentPlane == 1)
        {
            StartCoroutine(StartAnimation());
        }
        if (CurrentPlane == 2)
        {
            ExitButton.SetActive(true);
        }
        else
        {
            ExitButton.SetActive(false);
        }
    }

    public void ClickButton(Button ToClick)
    {
        ToClick.onClick.Invoke();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _beginDrag = Content.offsetMin.x;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    private IEnumerator StartAnimation()
    {
        if (!_justOnce)
        {
            yield return new WaitForSeconds(1.5f);
            Hand.Play();
            yield return new WaitForSeconds(2.5f);
            Tochange.sprite = Change;
            yield return new WaitForSeconds(2f);
            Sparkles.SetActive(true);
            _justOnce = true;
        }
    }

    private IEnumerator Move()
    {
        RectTransform r = GetComponent<RectTransform>();
        r.offsetMin = new Vector2(Content.offsetMin.x, r.offsetMin.y);
        r.offsetMax = new Vector2(Content.offsetMax.x, r.offsetMax.y);

        float timer = 0f;
        while (!Mathf.Approximately(Mathf.Round(r.offsetMin.x), Mathf.Abs(_sizeOfImage)))
        {
            r.offsetMin = Vector2.Lerp(r.offsetMin, new Vector2(_sizeOfImage, r.offsetMin.y), Time.deltaTime * Speed);
            r.offsetMax = Vector2.Lerp(r.offsetMax, new Vector2(_sizeOfImage, r.offsetMax.y), Time.deltaTime * Speed);
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                break;
            }
            yield return null;
        }

        r.offsetMin = new Vector2(Mathf.Round(_sizeOfImage), r.offsetMin.y);
        r.offsetMax = new Vector2(Mathf.Round(_sizeOfImage), r.offsetMax.y);
    }

    private void NextSprite(int Which)
    {
       DotsImage.sprite = Dots[Which];
    }

    #endregion // PRIVATE_METHODS
}
