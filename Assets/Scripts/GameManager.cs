﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{   
    private Color _red;
    private Color _yellow;
    private Color _gridColor;

    private Tile[,] _board;

    private Color _activeColor;
    private GameObject _activeCoin;
    private Tile _activeTile;

    private GameObject _grid;

    private int _winLength;

    // UI components

    public Canvas Canvas; // Set in inspector

    public Transform GameContainer;
    private Transform _splashScreenUi;
    private Transform _inGameUi;
    private Transform _menuUi;

    // Prefabs
    public Transform Coin; // Set in inspector
    public Transform Grid; // Set in inspector

    //States
    private bool _initialised;
    private bool _requiresReset;
    private bool _gameOver;

    //Temporary setting changes
    private int? _desiredWinLength;
    
    void Start ()
	{
        StartCoroutine(Initialise());

	    StartCoroutine(GameLoop());
	}

    private IEnumerator Initialise()
    {
        FetchUiComponents();

        InitialiseColours();

        InitialiseSettings();

        yield return StartCoroutine(Splash());
        
    }

    private void InitialiseSettings()
    {
        // Win length slider
        var winLengthSlider = _menuUi.Find("WinLengthSlider").GetComponent<Slider>();
        if (PlayerPrefs.HasKey("winlength"))
        {
            _winLength = PlayerPrefs.GetInt("winlength");
            winLengthSlider.value = _winLength;
        }
        else
        {
            _winLength = 4;
            PlayerPrefs.SetInt("winlength", _winLength);
            winLengthSlider.value = _winLength;
        }
    }

    private void FetchUiComponents()
    {
        _splashScreenUi = Canvas.transform.Find("SplashScreenUI");
        _inGameUi = Canvas.transform.Find("InGameUI");
        _menuUi = Canvas.transform.Find("MenuUI");

        _splashScreenUi.gameObject.SetActive(false);
        _inGameUi.gameObject.SetActive(false);
        _menuUi.gameObject.SetActive(false);
    }

    private void InitialiseColours()
    {
        ColorUtility.TryParseHtmlString("#E88989FF", out _red);
        ColorUtility.TryParseHtmlString("#E7DF77FF", out _yellow);
        ColorUtility.TryParseHtmlString("#0000003D", out _gridColor);

        // Red always starts
        _activeColor = _red;
    }

    private IEnumerator Splash()
    {
        var splashScreenTextTransform = _splashScreenUi.Find("SplashScreenText");

        var text = splashScreenTextTransform.GetComponent<Text>();

        text.text = string.Format("<color=#{0}>FL</color><color=#{1}>I</color><color=#{2}>CK</color>", ColorUtility.ToHtmlStringRGBA(_red), ColorUtility.ToHtmlStringRGBA(_gridColor), ColorUtility.ToHtmlStringRGBA(_yellow));
        _splashScreenUi.gameObject.SetActive(true);

        yield return new WaitForSeconds(3);

        text.text = string.Empty;
        _splashScreenUi.gameObject.SetActive(false);

        _requiresReset = true;

        _initialised = true;
    }

    private IEnumerator GameLoop()
    {
        while (!_initialised)
        {
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
        while (!_activeCoin.GetComponent<CoinMovementManager>().Completed && !_requiresReset)
        {
            yield return null;
        }

        yield return StartCoroutine(FinishTurn());

        CheckForWin();

        if (_gameOver)
        {
            yield return new WaitForSeconds(5);
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
    }

    private void InitialiseCoin()
    {
        _activeCoin = Instantiate(Coin, GameContainer).gameObject;
        _activeCoin.GetComponent<SpriteRenderer>().color = _activeColor;
        _activeCoin.GetComponent<SpriteRenderer>().shadowCastingMode = ShadowCastingMode.On;

        _activeCoin.GetComponent<Animation>().Play("SlideCoinToReadyPoint");
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

            if (colisionObjectRender.color.Equals(_activeColor)) // Increase tile stack count
            {
                tile.Stack += 1;
                tileText.text = tile.Stack.ToString();
            }
            else if (tile.Stack.HasValue && tile.Stack.Value > 1) // Decrease tile stack
            {
                tile.Stack -= 1;
                tileText.text = tile.Stack.ToString();
            }
            else // Swap colour. Set stack count to 1
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

                tile.Stack = 1;
                tileText.text = tile.Stack.ToString();
            }

            // Remember the change
            _board[tile.Row, tile.Col].Color = ColorUtility.ToHtmlStringRGBA(_activeColor);
            _activeTile = tile;
        }

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
            winnerText.text = string.Format("<color=#{0}>WINNER</color>", _activeColor == _red ? ColorUtility.ToHtmlStringRGBA(_red) : ColorUtility.ToHtmlStringRGBA(_yellow));

            _gameOver = true;
        }
    }

    private void PrepareNextTurn()
    {
        _activeColor = _activeColor == _red ? _yellow : _red;
        _activeCoin = null;
        _activeTile = null;
    }

    public void Restart()
    {
        _requiresReset = true;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OpenMenu()
    {
        ChangeWinLengthText();

        _menuUi.gameObject.SetActive(true);
    }

    private void ChangeWinLengthText()
    {
        var winLengthSlider = _menuUi.Find("WinLengthSlider").GetComponent<Slider>();
        var winLengthText = winLengthSlider.transform.Find("WinLengthText").GetComponent<Text>();
        winLengthText.text = string.Format("LENGTH TO WIN: {0}", (int) winLengthSlider.value);
    }

    public void CloseMenu()
    {
        _menuUi.gameObject.SetActive(false);

        if (_desiredWinLength.HasValue)
        {
            _winLength = _desiredWinLength.Value;
            PlayerPrefs.SetInt("winlength", _desiredWinLength.Value);
            _requiresReset = true;
            _desiredWinLength = null;
        }
    }

    public void ChangeWinLength(Slider slider)
    {
        var value = (int) slider.value;

        ChangeWinLengthText();

        if (value != _winLength)
        {
            _desiredWinLength = (int) slider.value;
        }
    }
}
