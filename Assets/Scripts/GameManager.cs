using System.Collections;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private bool _initialised;
    private bool _requiresReset;

    private Color _red;
    private Color _yellow;
    private Color _gridColor;

    private GameObject[,] _board;

    private Color _activeColor;
    private GameObject _activeCoin;

    private GameObject _grid;

    public Transform GameContainer;

    // UI components
    public Canvas Canvas; // Set in inspector

    private Transform _splashScreenUi;
    private Transform _inGameUi;
    private Transform _menuUi;

    // Prefabs
    public Transform Coin; // Set in inspector
    public Transform Grid; // Set in inspector

    private bool _gameOver;
    

    // Use this for initialization
	void Start ()
	{
        StartCoroutine(Initialise());

	    StartCoroutine(GameLoop());
	}

    private IEnumerator Initialise()
    {
        yield return StartCoroutine(FetchUiComponents());

        yield return StartCoroutine(InitialiseColours());

        yield return StartCoroutine(Splash());

        _requiresReset = true;

        _initialised = true;

        yield return null;
    }

    private IEnumerator FetchUiComponents()
    {
        _splashScreenUi = Canvas.transform.Find("SplashScreenUI");
        _inGameUi = Canvas.transform.Find("InGameUI");
        _menuUi = Canvas.transform.Find("MenuUI");

        _splashScreenUi.gameObject.SetActive(false);
        _inGameUi.gameObject.SetActive(false);
        _menuUi.gameObject.SetActive(false);

        yield return null;
    }

    private IEnumerator Splash()
    {
        var splashScreenTextTransform = _splashScreenUi.Find("SplashScreenText");

        var text = splashScreenTextTransform.GetComponent<Text>();

        text.text = string.Format("<color=#{0}>SLIDE</color>\n<color=#{1}>AND</color>\n<color=#{2}>CONNECT</color>", ColorUtility.ToHtmlStringRGBA(_red), ColorUtility.ToHtmlStringRGBA(_gridColor), ColorUtility.ToHtmlStringRGBA(_yellow));
        _splashScreenUi.gameObject.SetActive(true);

        yield return new WaitForSeconds(3);

        text.text = string.Empty;
        _splashScreenUi.gameObject.SetActive(false);
        yield return null;
    }

    private IEnumerator InitialiseColours()
    {
        ColorUtility.TryParseHtmlString("#E88989FF", out _red);
        ColorUtility.TryParseHtmlString("#E7DF77FF", out _yellow);
        ColorUtility.TryParseHtmlString("#0000003D", out _gridColor);

        // Red always starts
        _activeColor = _red;

        yield return null;
    }

    private IEnumerator GameLoop()
    {
        if (_initialised)
        {
            if (_requiresReset)
            {
                yield return StartCoroutine(InititialiseBoard());
                _requiresReset = false;
                yield return null;
            }

            // Create a coin
            if (_activeCoin == null)
            {
                yield return StartCoroutine(InitialiseCoin());
            }

            // Wait for coin to say that it's taken it's turn (user slides it)
            //yield return StartCoroutine(WaitForTurnTaken());

            if (_activeCoin.GetComponent<CoinMovementManager>().Completed)
            {
                yield return StartCoroutine(FinishTurn());

                yield return StartCoroutine(CheckForWin());

                yield return StartCoroutine(PrepareNextTurn());

                if (_gameOver)
                {
                    SceneManager.LoadScene(0);
                }
                else
                {
                    StartCoroutine(GameLoop());
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
                StartCoroutine(GameLoop());
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);

            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator InititialiseBoard()
    {
        // Destroy previous in play objects
        if (_grid != null)
        {
            Destroy(_grid);
        }

        if (_activeCoin != null)
        {
            Destroy(_activeCoin);
        }

        _board = new GameObject[6, 7];

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

            var indexString = childGameObject.name.Substring(4);

            var row = int.Parse(indexString.Substring(0, 1));
            var col = int.Parse(indexString.Substring(1));

            _board[row, col] = childGameObject;
        }

        _inGameUi.gameObject.SetActive(true);

        yield return null;
    }

    private IEnumerator InitialiseCoin()
    {
        _activeCoin = Instantiate(Coin, GameContainer).gameObject;
        _activeCoin.GetComponent<SpriteRenderer>().color = _activeColor;
        _activeCoin.GetComponent<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.On;

        yield return null;
    }

    private IEnumerator WaitForTurnTaken()
    {
        while (!_activeCoin.GetComponent<CoinMovementManager>().Completed)
        {
            yield return null;
        }
    }

    private IEnumerator FinishTurn()
    {
        var circleCollider = _activeCoin.GetComponent<CircleCollider2D>();

        var results = new Collider2D[4];

        circleCollider.OverlapCollider(new ContactFilter2D(), results);

        if (!results.Any())
        {
            yield return null;
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
            closestCollider.gameObject.GetComponent<SpriteRenderer>().color = _activeColor;
        }

        Destroy(_activeCoin);

        yield return new WaitForSeconds(1.5f);
    }

    private IEnumerator CheckForWin()
    {
        if (false)
        {
            _gameOver = true;
        }

        yield return null;
    }

    private IEnumerator PrepareNextTurn()
    {
        _activeColor = _activeColor == _red ? _yellow : _red;
        _activeCoin = null;

        yield return null;
    }

    public void Restart()
    {
        _requiresReset = true;
    }
}
