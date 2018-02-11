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

    private Color _red;
    private Color _yellow;
    private Color _gridColor;

    private GameObject[,] _board;

    private Color _activeColor;
    private GameObject _activeCoin;

    public Text Text;

    // Prefabs
    public Transform Coin;
    public Transform Grid;

    private bool _gameOver = false;

    // Use this for initialization
	void Start ()
	{
        StartCoroutine(Initialise());

	    StartCoroutine(GameLoop());
	}

    private IEnumerator Initialise()
    {
        yield return StartCoroutine(InitialiseColours());

        yield return StartCoroutine(Splash());
        
        yield return StartCoroutine(InititialiseBoard());
        
        _initialised = true;

        yield return null;
    }

    private IEnumerator Splash()
    {
        Text.text = string.Format("<color=#{0}>SLIDE</color>\n<color=#{1}>AND</color>\n<color=#{2}>CONNECT</color>", ColorUtility.ToHtmlStringRGBA(_red), ColorUtility.ToHtmlStringRGBA(_gridColor), ColorUtility.ToHtmlStringRGBA(_yellow));

        yield return new WaitForSeconds(3);

        Text.text = string.Empty;

        yield return null;
    }

    private IEnumerator InititialiseBoard()
    {
        _board = new GameObject[6, 7];

        var grid = Instantiate(Grid);
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
            // Create a coin
            yield return StartCoroutine(InitialiseCoin());

            // Wait for coin to say that it's taken it's turn (user slides it)
            yield return StartCoroutine(WaitForTurnTaken());

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
            yield return new WaitForSeconds(0.25f);

            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator InitialiseCoin()
    {
        _activeCoin = Instantiate(Coin).gameObject;
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

        yield return null;
    }
}
