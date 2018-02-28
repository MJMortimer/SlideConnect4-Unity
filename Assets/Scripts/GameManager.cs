using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using UnityEngine.Analytics;

public class GameManager : MonoBehaviour
{
    private Color _red;
    private Color _yellow;
    private Color _pink;
    private Color _blue;
    private Color _gridColor;
    private IList<Color> _playerColours;

    private Tile[,] _board;

    private Color _activeColor;
    private GameObject _activeCoin;
    private Tile _activeTile;

    private GameObject _grid;

    private int _winLength;
    private int _playerCount;
    private bool _stacking;
    private bool _stealing;

    // UI components

    public Canvas Canvas; // Set in inspector

    public Transform GameContainer;
    private Transform _titleUi;
    private Transform _inGameUi;
    private Transform _settingsUi;
    private Transform _helpUi;

    // Prefabs
    public Transform Coin; // Set in inspector
    public Transform Grid; // Set in inspector

    // Win length sprites
    public Sprite ThreeWin;
    public Sprite FourWin;
    public Sprite FiveWin;

    // Player count sprites
    public Sprite TwoPlayer;
    public Sprite ThreePlayer;
    public Sprite FourPlayer;

    // States
    private bool _initialised;
    private bool _requiresReset;
    private bool _gameOver;
    private bool _stopGame;

    void Start()
    {
        Advertisement.Initialize("1709613");

        Initialise();

        StartCoroutine(PresentTitle());
    }

    private void Initialise()
    {
        FetchUiComponents();

        InitialiseColours();

        InitialiseSettings();

        _initialised = true;
    }

    private void FetchUiComponents()
    {
        _titleUi = Canvas.transform.Find("TitleUI");
        _inGameUi = Canvas.transform.Find("InGameUI");
        _settingsUi = Canvas.transform.Find("SettingsUI");
        _helpUi = Canvas.transform.Find("HelpUI");

        _titleUi.gameObject.SetActive(false);
        _inGameUi.gameObject.SetActive(false);
        _settingsUi.gameObject.SetActive(false);
        _helpUi.gameObject.SetActive(false);
    }

    private void InitialiseColours()
    {
        ColorUtility.TryParseHtmlString("#E88989FF", out _red);
        ColorUtility.TryParseHtmlString("#E7DF77FF", out _yellow);
        ColorUtility.TryParseHtmlString("#D490D0FF", out _pink);
        ColorUtility.TryParseHtmlString("#389DDAFF", out _blue);
        ColorUtility.TryParseHtmlString("#0000003D", out _gridColor);

        _playerColours = new List<Color>{_red, _yellow, _pink, _blue};

        // Red always starts
        _activeColor = _red;
    }

    private void InitialiseSettings()
    {
        
        // Win length button
        var winLengthButton = _settingsUi.Find("WinLength/WinLengthButton").GetComponent<Button>();
        if (PlayerPrefs.HasKey("winlength"))
        {
            _winLength = PlayerPrefs.GetInt("winlength");
            switch (_winLength)
            {
                case 3:
                    winLengthButton.GetComponent<Image>().sprite = ThreeWin;
                    break;
                case 4:
                    winLengthButton.GetComponent<Image>().sprite = FourWin;
                    break;
                case 5:
                    winLengthButton.GetComponent<Image>().sprite = FiveWin;
                    break;
                default:
                    PlayerPrefs.SetInt("winLength", 4);
                    winLengthButton.GetComponent<Image>().sprite = FourWin;
                    break;
            }
        }
        else
        {
            _winLength = 4;
            PlayerPrefs.SetInt("winlength", _winLength);
            winLengthButton.GetComponent<Image>().sprite = FourWin;
        }

        // Player count button
        var playerCountButton = _settingsUi.Find("PlayerCount/PlayerCountButton").GetComponent<Button>();
        if (PlayerPrefs.HasKey("playerCount"))
        {
            _playerCount = PlayerPrefs.GetInt("playerCount");
            switch (_playerCount)
            {
                case 2:
                    playerCountButton.GetComponent<Image>().sprite = TwoPlayer;
                    break;
                case 3:
                    playerCountButton.GetComponent<Image>().sprite = ThreePlayer;
                    break;
                case 4:
                    playerCountButton.GetComponent<Image>().sprite = FourPlayer;
                    break;
                default:
                    PlayerPrefs.SetInt("playerCount", 2);
                    playerCountButton.GetComponent<Image>().sprite = TwoPlayer;
                    break;
            }
        }
        else
        {
            _playerCount = 2;
            PlayerPrefs.SetInt("playerCount", _playerCount);
            playerCountButton.GetComponent<Image>().sprite = TwoPlayer;
        }

        // Stacking button
        var stackingButton = _settingsUi.Find("Stacking/StackingButton").GetComponent<Button>();
        if (PlayerPrefs.HasKey("stacking"))
        {
            _stacking = PlayerPrefs.GetInt("stacking") == 1;
            stackingButton.transform.Find("StackingButtonText").GetComponent<Text>().text = _stacking 
                ? string.Format("<color=#{0}>YEAH!</color>", ColorUtility.ToHtmlStringRGBA(_red)) 
                : string.Format("<color=#{0}>NAH..</color>", ColorUtility.ToHtmlStringRGBA(_yellow));
        }
        else
        {
            _stacking = true;
            PlayerPrefs.SetInt("stacking", 1);
            stackingButton.transform.Find("StackingButtonText").GetComponent<Text>().text = string.Format("<color=#{0}>YEAH!</color>", ColorUtility.ToHtmlStringRGBA(_red));
        }

        // Stealing button
        var stealingButton = _settingsUi.Find("Stealing/StealingButton").GetComponent<Button>();
        if (PlayerPrefs.HasKey("stealing"))
        {
            _stealing = PlayerPrefs.GetInt("stealing") == 1;
            stealingButton.transform.Find("StealingButtonText").GetComponent<Text>().text = _stealing
                ? string.Format("<color=#{0}>YEAH!</color>", ColorUtility.ToHtmlStringRGBA(_red))
                : string.Format("<color=#{0}>NAH..</color>", ColorUtility.ToHtmlStringRGBA(_yellow));
        }
        else
        {
            _stealing = true;
            PlayerPrefs.SetInt("stealing", 1);
            stealingButton.transform.Find("StealingButtonText").GetComponent<Text>().text = string.Format("<color=#{0}>YEAH!</color>", ColorUtility.ToHtmlStringRGBA(_red));
        }
    }

    private IEnumerator PresentTitle()
    {
        var titleText = _titleUi.Find("TitleText");

        var text = titleText.GetComponent<Text>();

        text.text = string.Format("<color=#{0}>FL</color><color=#{1}>I</color><color=#{2}>CK</color>", ColorUtility.ToHtmlStringRGBA(_red), ColorUtility.ToHtmlStringRGBA(_gridColor), ColorUtility.ToHtmlStringRGBA(_yellow));
        _titleUi.gameObject.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        text.GetComponent<Animation>().Play();
        yield return new WaitForSeconds(1.5f);

        var titleButtons = _titleUi.Find("TitleButtons");
        titleButtons.gameObject.SetActive(true);
    }

    public void Play()
    {
        PrepareNewGame();

        _titleUi.gameObject.SetActive(false);

        StartCoroutine(GameLoop());
    }

    private void PrepareNewGame()
    {
        _activeColor = _red;

        _requiresReset = true;

        _stopGame = false;
    }

    private IEnumerator GameLoop()
    {
        if (_stopGame)
        {
            _stopGame = false;
            yield break;
        }

        if (!_initialised)
        {
            Initialise();
            yield return null;
        }

        if (_requiresReset)
        {
            InititialiseBoard();
            _requiresReset = false;
        }

        // Create a coin
        if (_activeCoin == null)
        {
            InitialiseCoin();
        }

        // Wait for coin to say that it's taken it's turn (user slides it)
        while (!_activeCoin.GetComponent<CoinMovementManager>().Completed && !_requiresReset && !_stopGame)
        {
            yield return null;
        }

        yield return StartCoroutine(FinishTurn());

        CheckForWin();

        if (_gameOver)
        {
            yield return new WaitForSeconds(5);

            if (Advertisement.IsReady())
            {
                Advertisement.Show();
                yield return new WaitForSeconds(1);
            }

            SceneManager.LoadScene(0);
        }
        else
        {
            PrepareNextTurn();
            StartCoroutine(GameLoop());
        }
    }

    private void InititialiseBoard()
    {
        GameContainer.gameObject.SetActive(true);

        // Destroy previous in play objects
        if (_grid != null)
        {
            Destroy(_grid);
            _grid = null;
        }

        if (_activeCoin != null)
        {
            Destroy(_activeCoin);
            _activeCoin = null;
        }

        _board = new Tile[6, 7];

        var grid = Instantiate(Grid, GameContainer);
        _grid = grid.gameObject;

        grid.GetComponent<SpriteRenderer>().color = _gridColor;

        foreach (Transform child in grid.transform)
        {
            var childGameObject = child.gameObject;

            if (!childGameObject.name.StartsWith("tile"))
            {
                continue;
            }

            var tile = childGameObject.GetComponent<Tile>();

            _board[tile.Row, tile.Col] = tile;
        }

        _inGameUi.gameObject.SetActive(true);
    }

    private void InitialiseCoin()
    {
        _activeCoin = Instantiate(Coin, GameContainer).gameObject;
        _activeCoin.GetComponent<SpriteRenderer>().color = _activeColor;
        _activeCoin.GetComponent<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.On;

        _activeCoin.GetComponent<Animation>().Play("Grow");
    }

    private IEnumerator FinishTurn()
    {
        var circleCollider = _activeCoin.GetComponent<CircleCollider2D>();

        var results = new Collider2D[4];

        circleCollider.OverlapCollider(new ContactFilter2D(), results);

        if (!results.Any())
        {
            yield break;
        }

        Collider2D closestCollider = null;
        var closestDistance = float.MaxValue;

        foreach (var result in results.Where(it => it != null))
        {
            var thisPosition = circleCollider.bounds.center;
            var otherPosition = result.bounds.center;

            var distance = Vector3.Distance(thisPosition, otherPosition);

            if (distance < closestDistance)
            {
                closestCollider = result;
                closestDistance = distance;
            }
        }

        if (closestCollider != null)
        {
            var colisionObjectRender = closestCollider.gameObject.GetComponent<SpriteRenderer>();
            var tile = closestCollider.GetComponent<Tile>();
            var tileText = tile.transform.Find("tileText").GetComponent<TextMesh>();

            if (_stacking && colisionObjectRender.color.Equals(_activeColor)) // Increase tile stack count if stacking enabled
            {
                tile.Stack += 1;
                tileText.text = tile.Stack.ToString();
            }
            else if (_stacking && tile.Stack.HasValue && tile.Stack.Value > 1) // Decrease tile stack if stacking enabled
            {
                tile.Stack -= 1;
                tileText.text = tile.Stack.ToString();
            }
            else // Swap colour if stealing enabled. Set stack count to 1 if stacking enabled
            {
                if (_stealing || !_playerColours.Any(it => it.Equals(colisionObjectRender.color)))
                {
                    //Alter the animation curve so that we fade from the color it is the color we want
                    var a = closestCollider.GetComponent<Animation>();
                    var existingClipLength = a.clip.length;

                    var rCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.r, existingClipLength, _activeColor.r);
                    var gCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.g, existingClipLength, _activeColor.g);
                    var bCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.b, existingClipLength, _activeColor.b);
                    var aCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.a, existingClipLength, _activeColor.a);

                    a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.r", rCurve);
                    a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.g", gCurve);
                    a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.b", bCurve);
                    a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.a", aCurve);

                    a.Play();
                }

                if (_stacking)
                {
                    tile.Stack = 1;
                    tileText.text = tile.Stack.ToString();
                }
            }

            // Remember the change
            _board[tile.Row, tile.Col].Color = ColorUtility.ToHtmlStringRGBA(_activeColor);
            _activeTile = tile;
        }

        _activeCoin.GetComponent<CoinMovementManager>().ShrinkAudio.Play();
        _activeCoin.GetComponent<Animation>().Play("Shrink");

        yield return new WaitForSeconds(0.5f);

        Destroy(_activeCoin);
    }

    private void CheckForWin()
    {
        if (_activeTile == null)
        {
            return;
        }

        IEnumerable<Tile> winningTiles;

        if (WinLogic.CheckWin(_board, _activeTile, _winLength, out winningTiles))
        {
            // Do something with winning tiles. Highlight them or something
            foreach (var tile in winningTiles)
            {
                tile.GetComponent<SpriteRenderer>().color = Color.white;
            }

            var winnerText = _inGameUi.Find("WinnerText").GetComponent<Text>();
            winnerText.text = string.Format("<color=#{0}>WINNER</color>", ColorUtility.ToHtmlStringRGBA(_activeColor));

            _gameOver = true;
        }
    }

    private void PrepareNextTurn()
    {
        _activeColor = NextColor();
        _activeCoin = null;
        _activeTile = null;
    }

    private Color NextColor()
    {
        var activeIndex = _playerColours.IndexOf(_activeColor);
        var next = activeIndex + 1;

        if (next >= _playerCount)
        {
            next = 0;
        }

        return _playerColours[next];
    }

    public void Restart()
    {
        _requiresReset = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenSettings()
    {
        _titleUi.gameObject.SetActive(false);
        _settingsUi.gameObject.SetActive(true);
    }

    public void BackToMenu()
    {
        //Handle back from Game
        _stopGame = true;
        _inGameUi.gameObject.SetActive(false);
        GameContainer.gameObject.SetActive(false);

        //Handle back from Help or Settings
        _helpUi.gameObject.SetActive(false);
        _settingsUi.gameObject.SetActive(false);

        _titleUi.gameObject.SetActive(true);
    }

    public void OpenHelp()
    {
        _titleUi.gameObject.SetActive(false);
        _helpUi.gameObject.SetActive(true);
    }

    public void ChangeWinLength()
    {
        var buttonImage = _settingsUi.Find("WinLength/WinLengthButton").GetComponent<Button>().GetComponent<Image>();
        var currentValue = _winLength;

        switch (currentValue)
        {
            case 3:
                _winLength = 4;
                buttonImage.sprite = FourWin;
                break;
            case 4:
                _winLength = 5;
                buttonImage.sprite = FiveWin;
                break;
            case 5:
                _winLength = 3;
                buttonImage.sprite = ThreeWin;
                break;
        }
        
        PlayerPrefs.SetInt("winlength", _winLength);
    }

    public void ChangePlayerCount()
    {
        var buttonImage = _settingsUi.Find("PlayerCount/PlayerCountButton").GetComponent<Button>().GetComponent<Image>();
        var currentValue = _playerCount;

        switch (currentValue)
        {
            case 2:
                _playerCount = 3;
                buttonImage.sprite = ThreePlayer;
                break;
            case 3:
                _playerCount = 4;
                buttonImage.sprite = FourPlayer;
                break;
            case 4:
                _playerCount = 2;
                buttonImage.sprite = TwoPlayer;
                break;
        }

        PlayerPrefs.SetInt("playerCount", _playerCount);
    }

    public void ChangeStacking()
    {
        var buttonText = _settingsUi.Find("Stacking/StackingButton/StackingButtonText").GetComponent<Text>();
        var currentValue = _stacking;

        if (currentValue)
        {
            _stacking = false;
            buttonText.text = string.Format("<color=#{0}>NAH..</color>", ColorUtility.ToHtmlStringRGBA(_yellow));
        }
        else
        {
            _stacking = true;
            buttonText.text = string.Format("<color=#{0}>YEAH!</color>", ColorUtility.ToHtmlStringRGBA(_red));
        }
        
        PlayerPrefs.SetInt("stacking", _stacking ? 1 : 0);
    }

    public void ChangeStealing()
    {
        var buttonText = _settingsUi.Find("Stealing/StealingButton/StealingButtonText").GetComponent<Text>();
        var currentValue = _stealing;

        if (currentValue)
        {
            _stealing = false;
            buttonText.text = string.Format("<color=#{0}>NAH..</color>", ColorUtility.ToHtmlStringRGBA(_yellow));
        }
        else
        {
            _stealing = true;
            buttonText.text = string.Format("<color=#{0}>YEAH!</color>", ColorUtility.ToHtmlStringRGBA(_red));
        }

        PlayerPrefs.SetInt("stealing", _stealing ? 1 : 0);
    }
}
