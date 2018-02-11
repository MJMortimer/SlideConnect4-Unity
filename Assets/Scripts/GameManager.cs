using System.Collections;
using System.Collections.Generic;
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

    private Tile[,] _board;

    private Color _activeColor;
    private GameObject _activeCoin;
    private Tile _activeTile;

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
                    yield return new WaitForSeconds(5);

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

        yield return null;
    }

    private IEnumerator InitialiseCoin()
    {
        _activeCoin = Instantiate(Coin, GameContainer).gameObject;
        _activeCoin.GetComponent<SpriteRenderer>().color = _activeColor;
        _activeCoin.GetComponent<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.On;

        _activeCoin.GetComponent<Animation>().Play("SlideCoinToReadyPoint");
        
        yield return null;
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
            // Change colour of the tile

            var colisionObjectRender = closestCollider.gameObject.GetComponent<SpriteRenderer>();

            if (!colisionObjectRender.color.Equals(_activeColor))
            {
                //Alter the animation curve so that we fade from the color it is the color we want
                var a = closestCollider.GetComponent<Animation>();
                var existingClipLength = a.clip.length;

                var rCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.r, existingClipLength, _activeColor.r);
                var gCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.g, existingClipLength, _activeColor.g);
                var bCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.b, existingClipLength, _activeColor.b);
                var aCurve = AnimationCurve.EaseInOut(0, colisionObjectRender.color.a, existingClipLength, _activeColor.a);

                //AnimationClip clip = new AnimationClip();
                a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.r", rCurve);
                a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.g", gCurve);
                a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.b", bCurve);
                a.clip.SetCurve("", typeof(SpriteRenderer), "m_Color.a", aCurve);

                //a.RemoveClip("FadeIn");

                //a.AddClip(clip, "test");
                a.Play();
            }

            // Remember the change
            var tile = closestCollider.GetComponent<Tile>();
            _board[tile.Row, tile.Col].PlayerMarker = PlayerMarker;
            _activeTile = tile;
        }

        _activeCoin.GetComponent<Animation>().Play("Shrink");

        yield return new WaitForSeconds(1.5f);

        Destroy(_activeCoin);

        yield return null;
    }

    private IEnumerator CheckForWin()
    {
        if (_activeTile == null)
        {
            yield break;
        }

        IEnumerable<Tile> winningTiles;

        if (WinLogic.CheckWin(_board, _activeTile, out winningTiles))
        {
            // Do something with winning tiles. Highlight them or something
            foreach (var tile in winningTiles)
            {
                tile.GetComponent<SpriteRenderer>().color = Color.white;
            }

            var winnerText = _inGameUi.Find("WinnerText").GetComponent<Text>();
            winnerText.text = string.Format("<color=#{0}>WINNER</color>", _activeColor == _red ? ColorUtility.ToHtmlStringRGBA(_red) : ColorUtility.ToHtmlStringRGBA(_yellow));

            _gameOver = true;
        }

        yield return null;
    }

    private IEnumerator PrepareNextTurn()
    {
        _activeColor = _activeColor == _red ? _yellow : _red;
        _activeCoin = null;
        _activeTile = null;

        yield return null;
    }

    public void Restart()
    {
        _requiresReset = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public int PlayerMarker
    {
        get { return _activeColor == _red ? 1 : 2; }
    }
}
