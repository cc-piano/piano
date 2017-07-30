using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    #region PUBLIC_MEMBER_VARIABLES

    [Header("Basic rules")]
    [Tooltip("The maximum amount of pianos that you allow to be at the same time")]
    public int TotalAmountAllowed = 12;
    [Tooltip("Your screen will be divided on this amount of pianos. The size of pianos will be set automatically")]
    public int AmountOfPianosOverScreenWidth = 4;
    [Tooltip("Cool down. Once this time is elapsed one piano will be created")]
    public float CoolDown;
    [SerializeField]
    [Tooltip("The amount of pianos, that you can miss. If there is no health left, the game will be ended")]
    private int _health = 3;
    [SerializeField]
    [Tooltip("The speed with which the pianos move")]
    private float GameSpeed = 10f;

    [Header("The variables, that need to be assigned")]
    public Animation Damage; // TODO move animation to AnimationController class
    public AnimationController AnimController;
    public GameObject Health;
    public GameObject PianosParent;
    public bool _bGame{get; private set;} // The variable to controll the game
    public float _widthOfPiano { get; private set; }

    /// <summary>
    /// Instance of an object of a GameController class.
    /// </summary>
    public static GameController Instance { get { return _instance; } }

    #endregion // PUBLIC_MEMBER_VARIABLES



    #region PRIVATE_MEMBER_VARIABLES

    private static GameController _instance;

    
    private int _currentAmount;
    private int _healthCopy;
    private float _x;
    private float _gameSpeedCopy;
    private bool _bSpawning;
    
    #endregion // PRIVATE_MEMBER_VARIABLES



    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        if (_bGame)
        {

            _healthCopy = _health;
            _gameSpeedCopy = GameSpeed;
            _widthOfPiano = Screen.width / AmountOfPianosOverScreenWidth;
            FindObjectOfType<CanvasScaler>().referenceResolution =
                new Vector2(Screen.width, Screen.height); // we need to set up the resolution in that way, because 
            _x = _widthOfPiano / 2; // we do not need canvas scaler's help

            //Create health
            for (int i = 0; i < _health; i++)
            {
                Instantiate(Resources.Load("HealthPrefab"), Health.transform).name = i + "";
            }
            //Create pianos
            StartCoroutine(CreateNewPianoWithCoolDown());
        }
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS



    #region PUBLIC_METHODS

    public void StartGame()
    {
        _bGame = true;
        Start();
    }

    public void IncreaseSpeedOfPianos(float howMuch = 0.25f)
    {
        GameSpeed -= howMuch;
    }

    public void MakeDamage()
    {
        _health--;
        if (_health <= 0)
        {
            FinishGame();
        }
        else
        {
#if UNITY_ANDROID && !UNITY_EDITOR
        Vibrate();
#endif

            AnimController.StartCoroutine(AnimController.Remove1Health(_health));
            Damage.Play();
        }

    }

    public void CreatePiano()
    {
        _currentAmount--;
        StartCoroutine(CreateNewPianoWithCoolDown());
    }

    public void Restart() // simply restart game
    {
        AnimController.StopAllCoroutines();
        for (int i = 0; i < PianosParent.transform.childCount; i++)
        {
            Destroy(PianosParent.transform.GetChild(i).gameObject);
        }
        AnimController.HideFinishGame();
        GameSpeed = _gameSpeedCopy;
        _health = _healthCopy;
        _currentAmount = 0;
        _bGame = true;
        _bSpawning = false;
        //Destroy all health
        foreach (Transform HealthTr in Health.transform)
        {
            Destroy(HealthTr.gameObject);
        }
        //Create health
        for (int i = 0; i < _health; i++)
        {
            Instantiate(Resources.Load("HealthPrefab"), Health.transform);
        }
        //Create pianos
        StartCoroutine(CreateNewPianoWithCoolDown());
    }

    public void SpawnWithNoCoolDown()
    {
        Spawn();
    }

    #endregion // PUBLIC_METHODS



    #region PRIVATE_METHODS

    private void FinishGame()
    {
        _bGame = false;
        StopAllCoroutines();
#if UNITY_ANDROID && !UNITY_EDITOR
          Vibrate(500);
#endif
        Damage.Play();
        AnimController.PressBlock.SetActive(true);
        Debug.Log("You lost");
        for (int i = 0; i < PianosParent.transform.childCount; i++)
        {
            RectTransform tempRectTransform = PianosParent.transform.GetChild(i).GetComponent<RectTransform>();
            tempRectTransform.DOPause();
            tempRectTransform.DOPlayBackwards();
        }

        StartCoroutine(AnimController.FinishAnimationWithFade());
    }

    private void Vibrate(long milliseconds = 0) // the source of this plugin will be in plugins/android folder
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass javavClass = new AndroidJavaClass("com.cc.vibration.Vibration");

        if (milliseconds == 0)
        {
            javavClass.CallStatic("Vibrate", currentActivity);
        }
        else
        {
            javavClass.CallStatic("VibrateForSeconds", currentActivity, milliseconds);
        }
    }

    private IEnumerator CreateNewPianoWithCoolDown()
    {
        if (_bSpawning)
        {
            yield break;
        }
        _bSpawning = true;
        while (_currentAmount < TotalAmountAllowed)
        {
            Spawn();
            _currentAmount++;
            yield return new WaitForSeconds(CoolDown);
        }
        _bSpawning = false;
    }

    private void Spawn()
    {
        GameObject SpawnedGO = Instantiate(Resources.Load("PianoPrefab"), PianosParent.transform) as GameObject;
        SpawnedGO.GetComponent<Piano>().GameSpeed = GameSpeed;
        RectTransform tempRectTransform = SpawnedGO.GetComponent<RectTransform>();

        tempRectTransform.anchoredPosition = new Vector2(_x, tempRectTransform.anchoredPosition.y);
        if (_x >= Screen.width)
        {
            _x = _widthOfPiano / 2;
            tempRectTransform.anchoredPosition = new Vector2(_x, tempRectTransform.anchoredPosition.y);
        }
        _x += _widthOfPiano;
        tempRectTransform.sizeDelta = new Vector2(_widthOfPiano, 550);
    }

#endregion // PRIVATE_METHODS
}
