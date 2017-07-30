using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnimationController : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    public GameObject Health;
    public GameObject PianosParent;
    public GameObject PressBlock;
    public GameObject RestartText;
    public RectTransform FinishLine;
    [HideInInspector]
    public bool _bRemovingHealth;

    #endregion // PUBLIC_MEMBER_VARIABLES


    #region PUBLIC_METHODS

    public void HideFinishGame()
    {
        RestartText.SetActive(false);
        FinishLine.gameObject.SetActive(false);
        PressBlock.gameObject.SetActive(false);
    }

    public IEnumerator Remove1Health(int index) // removing health with visual effects, also destroys one hp- bar
    {
        _bRemovingHealth = true;
        RectTransform healthToDestroyRectTransform = Health.transform.GetChild(index).GetComponent<RectTransform>();
        healthToDestroyRectTransform.DOScaleX(0.1f, 1.25f);
        yield return new WaitForSeconds(0.85f);
        healthToDestroyRectTransform.DOAnchorPosY(15f, 0.75f);
        yield return new WaitForSeconds(0.75f);
        Destroy(Health.transform.GetChild(index).gameObject);
        _bRemovingHealth = false;
    }

    public IEnumerator FinishAnimationWithFade()
    {
        if (PianosParent.transform.childCount < GameController.Instance.AmountOfPianosOverScreenWidth) // Make sure we always have all pianos to fit the screen
        {
            for (int i = 0; i < GameController.Instance.AmountOfPianosOverScreenWidth; i++)         // if we have smaller amount that we need, we shall increase it
            {
                GameController.Instance.SpawnWithNoCoolDown();
                if (i != GameController.Instance.AmountOfPianosOverScreenWidth / 2)
                {
                    Transform child = PianosParent.transform.GetChild(0);
                    Color color = child.GetComponent<Image>().color;
                    color.a = 0;
                    child.GetComponent<Image>().color = color;
                    StartCoroutine(FadeOnePiano(child, i, true));
                }
            }
        }
        else
        {
            for (int i = 0; i < PianosParent.transform.childCount - GameController.Instance.AmountOfPianosOverScreenWidth; i++) // if we have bigger amount that we need, we shall decrease it
            {
                Destroy(PianosParent.transform.GetChild(i).gameObject);
            }
        }
        PressBlock.GetComponent<EventTrigger>().triggers.Clear(); // clean all buttons events from old object
        yield return new WaitForSeconds(2f);

        for (int i = 0; i < PianosParent.transform.childCount; i++) // we need to return all pianos to their's default positions to start finish effect
        {
            PianosParent.transform.GetChild(i).DOPause();
            RectTransform tempRectTransform = PianosParent.transform.GetChild(i).GetComponent<RectTransform>();
            tempRectTransform.anchoredPosition = new Vector2(tempRectTransform.anchoredPosition.x, -tempRectTransform.sizeDelta.y / 2);
        }

        float middle = GameController.Instance._widthOfPiano * (GameController.Instance.AmountOfPianosOverScreenWidth / 2); // to make piano effect we need to take always middle element
        middle += GameController.Instance._widthOfPiano / 2;
        PressBlock.GetComponent<EventTrigger>().triggers.Clear();
            for (int i = 0; i < PianosParent.transform.childCount; i++)
        {
            Transform Child = PianosParent.transform.GetChild(i);
            if (PianosParent.transform.childCount % 2 == 0) // if number is even we need to take two pianos
            {
                if (Mathf.Approximately(Child.GetComponent<RectTransform>().anchoredPosition.x, middle) ||
                    Mathf.Approximately(Child.GetComponent<RectTransform>().anchoredPosition.x,
                        middle - GameController.Instance._widthOfPiano))
                {
                    SetPianoToButton(Child);
                }
                else
                {
                    StartCoroutine(FadeOnePiano(Child, i));
                }
            }
            else // if number is odd we need to take one piano
            {
                if (Mathf.Approximately(Child.GetComponent<RectTransform>().anchoredPosition.x, middle))
                {
                    SetPianoToButton(Child);
                }
                else
                {
                    StartCoroutine(FadeOnePiano(Child, i));
                }
            }
        }
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    private void SetPianoToButton(Transform Child)  // bind one big button for piano
    {
        Child.GetComponent<RectTransform>().DOSizeDelta(new Vector2(Child.GetComponent<RectTransform>().sizeDelta.x, 2500f), 1f);
        Child.gameObject.AddComponent<Button>();
        PressBlock.SetActive(true);

        StartCoroutine(Move(Child.GetComponent<RectTransform>()));

        EventTrigger.Entry PointerDown = new EventTrigger.Entry();
        PointerDown.eventID = EventTriggerType.PointerDown;
        PointerDown.callback.AddListener((eventData) => { Child.GetComponent<Image>().color = Color.gray; });
        
        EventTrigger.Entry PointerUp = new EventTrigger.Entry();
        PointerUp.eventID = EventTriggerType.PointerUp;
        PointerUp.callback.AddListener((eventData) => { Child.GetComponent<Image>().color = Color.black; });

        EventTrigger.Entry PointerClick = new EventTrigger.Entry();
        PointerClick.eventID = EventTriggerType.PointerClick;
        PointerClick.callback.AddListener((eventData) => { GameController.Instance.Restart(); });

        PressBlock.GetComponent<EventTrigger>().triggers.Add(PointerDown);
        PressBlock.GetComponent<EventTrigger>().triggers.Add(PointerUp);
        if (PressBlock.GetComponent<EventTrigger>().triggers.All(a => a.eventID != EventTriggerType.PointerClick)) // we do not need 2 restart events at one time
        {
            PressBlock.GetComponent<EventTrigger>().triggers.Add(PointerClick);
        }

        RestartText.SetActive(true);
        FinishLine.gameObject.SetActive(true);
        FinishLine.GetComponent<RectTransform>().DOAnchorPosY(-480f, 0.5f);
    }

    private IEnumerator Move(RectTransform r)
    {
        yield return new WaitForSeconds(1.25f);
        r.DOAnchorPosY(-r.sizeDelta.y / 2, 1.0f);
        //r.anchoredPosition = new Vector2(r.anchoredPosition.x, -r.sizeDelta.y / 2);
    }

    private IEnumerator FadeOnePiano(Transform Child, int index, bool backwards=false)  // fade the piano to the darkness or bring a piano to the light 
    {
        if (!backwards)
        {
            while (Child.GetComponent<Image>().color.a >= 0f)
            {
                Color c = Child.GetComponent<Image>().color;
                c.a = Mathf.Lerp(c.a, c.a - 0.1f, Time.deltaTime * 5f);
                Child.GetComponent<Image>().color = c;
                yield return null;
            }
            PianosParent.transform.GetChild(index).gameObject.SetActive(false);
        }
        else
        {
            while (Child.GetComponent<Image>().color.a <= 1f)
            {
                Color c = Child.GetComponent<Image>().color;
                c.a = Mathf.Lerp(c.a, c.a + 0.1f, Time.deltaTime * 5f);
                Child.GetComponent<Image>().color = c;
                yield return null;
            }
        }
    }

    #endregion // PRIVATE_METHODS
}
