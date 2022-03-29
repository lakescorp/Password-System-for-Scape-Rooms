using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.Collections.Generic;

public class Manager : MonoBehaviour
{
    // Edit this
    private Dictionary<string, string> _messages = new Dictionary<string, string>(){
        {"passwordError", "There is no password or is bigger than 8"},
        {"passwordCorrect", "Password field is correctly filled"},
        {"timeError", "There is no time or its less than 1"},
        {"timeCorrect", "Time is correctly filled"},
        {"remainingAtemps", "Remeaning attemps: "},
        {"atempsError", "Attemp is null or less than 0"},
        {"atempsCorrect", "Attemps is correcctly filled"},
        {"errorFields", "Please fill all the fields"}
    };

    // Program counters
    private float _currentTime = 0f;
    private bool _gameFinished = true;
    private int _correctCounter = 0;
    private int _actualAttemps = 0;


    private int _passwordLength, _maxAttempts;
    private float _countDownTime;
    private string _winMessage, _loseMessage, _password;

    int PasswordLength
    {
        get => _passwordLength;
        set
        {
            if (value > 0 && value <= 8)
            {
                _passwordLength = value;
            }
            else
            {
                _passwordLength = -1;
            }
        }
    }

    float CountDownTime
    {
        get => _countDownTime;

        set
        {
            if (value >= 0)
            {
                _countDownTime = value * 60;
            }
            else
            {
                _countDownTime = -1;
            }
        }
    }

    int MaxAttempts
    {
        get => _maxAttempts;

        set
        {
            _maxAttempts = value >= 0 ? value : -1;
        }
    }

    [Header("GameScene References")]
    public RawImage BackgroundImage;

    public Transform inputs;
    public Transform errorMessagePrefab;
    public Transform canvas;

    public TMPro.TMP_Text text_countDown;
    public TMPro.TMP_Text text_finalMessage;
    public TMPro.TMP_Text text_attemps;

    public AudioClip sound_wrong;

    [Header("SettingsScene References")]
    public TMPro.TMP_Text text_errors;
    public Transform waitScreen;
    public Transform gameScreen;

    // Update is called once per frame
    void Update()
    {
        if (_gameFinished) return;
        _currentTime += Time.deltaTime;
        if (_currentTime <= CountDownTime)
        {
            text_countDown.text = (int)((CountDownTime - _currentTime) / 60) + ":" + (int)((CountDownTime - _currentTime)%60);
        }
        else
        {
            Close();
        }
    }

    /// <summary>
    /// Shows lose message
    /// </summary>
    void Close()
    {
        _gameFinished = true;
        canvas.GetComponent<Animator>().Play("close");
        text_finalMessage.text = _loseMessage;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="newValue"></param>
    /// <returns></returns>
    public bool CheckIntroducedValue(int x, string newValue)
    {
        if (_password[x] == newValue[0]) _correctCounter++;
        if (_correctCounter == _password.Length) {
            _gameFinished = true;
            canvas.GetComponent<Animator>().Play("win");
            text_finalMessage.text = _winMessage;
        }
        return _password[x] == newValue[0];
    }

    /// <summary>
    /// Opens the file browser to select a Image 
    /// </summary>
    public void GetImage()
    {
        var extensions = new[] { new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ) };
        string path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false)[0];
        try
        {
            var rawData = System.IO.File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
            tex.LoadImage(rawData);
            BackgroundImage.texture = tex;
        }
        catch
        {

        }

    }

    /// <summary>
    /// Set a password that must be guessed
    /// </summary>
    /// <param name="password">Password</param>
    public void SetPassword(string password)
    {
        PasswordLength = password.Length;
        if (PasswordLength==-1) {
            text_errors.text = _messages["passwordError"];
        }
        else
        {
            _password = password;
            text_errors.text = _messages["passwordCorrect"];
        }
    }

    /// <summary>
    /// Set the countdown
    /// </summary>
    /// <param name="time"></param>
    public void SetCountDown(string time)
    {
        try
        {
            CountDownTime = int.Parse(time);
        }
        catch
        {
            CountDownTime = -1;
        }
        text_errors.text = CountDownTime == -1 ? _messages["timeError"] : _messages["timeCorrect"];
    }

    /// <summary>
    /// Set the max attemps to solve the password
    /// </summary>
    /// <param name="attemps"></param>
    public void SetAttemps(string attemps)
    {
        try
        {
            MaxAttempts = int.Parse(attemps);
        }
        catch
        {
            MaxAttempts = -1;
        }

        text_errors.text = MaxAttempts == -1 ? _messages["atempsError"] : _messages["atempsCorrect"];
    }

    /// <summary>
    /// Set the Won messgae
    /// </summary>
    /// <param name="wonMessage"></param>
    public void SetWonMessage(string wonMessage) => _winMessage = wonMessage;

    /// <summary>
    /// Set the lose message
    /// </summary>
    /// <param name="loseMessage"></param>
    public void SetLoseMessage(string loseMessage) => _loseMessage = loseMessage;

    /// <summary>
    /// Checks all fields are correct
    /// </summary>
    public void IntroduceData() {
        if (PasswordLength != -1 && CountDownTime != -1 && MaxAttempts != -1 && _winMessage != "" && _loseMessage != "") {
            text_errors.transform.parent.gameObject.SetActive(false);
            waitScreen.gameObject.SetActive(true);
            text_attemps.text = _messages["remainingAtemps"] + (MaxAttempts - _actualAttemps);
        }
        else
        {
            text_errors.text = _messages["errorFields"];
        }
    }

    /// <summary>
    /// Starts the game
    /// </summary>
    public void StartGame() {
        _gameFinished = false;
        waitScreen.gameObject.SetActive(false);
        gameScreen.gameObject.SetActive(true);
        ConfigureInputs();
    }

    /// <summary>
    /// Prepares all the input fields of the pasword
    /// </summary>
    void ConfigureInputs() {
        for (int x = 0; x < PasswordLength; x++)
        {
            int z = x;
            TMPro.TMP_InputField componente = inputs.GetChild(x).GetComponent<TMPro.TMP_InputField>();
            componente.onValueChanged.AddListener(
                delegate
                {
                    if (CheckIntroducedValue(z, componente.text))
                    {
                        componente.interactable = false;
                    }
                    else
                    {
                        GetComponent<AudioSource>().PlayOneShot(sound_wrong);
                        _actualAttemps += 1;
                        text_attemps.text = _messages["remainingAtemps"] + (MaxAttempts - _actualAttemps);
                        if (_actualAttemps == MaxAttempts)
                        {
                            Close();
                        }
                        else
                        {
                            Instantiate(errorMessagePrefab, canvas);
                        }
                    }
                });
        }

        for (int x = PasswordLength; x < inputs.childCount; x++)
        {
            inputs.GetChild(x).gameObject.SetActive(false);
        }
    }

}
