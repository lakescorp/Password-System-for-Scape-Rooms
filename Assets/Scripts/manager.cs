using UnityEngine;
using UnityEngine.UI;
using SFB;
using System.Collections.Generic;

public class manager : MonoBehaviour
{
    // Edit this
    Dictionary<string, string> messages = new Dictionary<string, string>(){
        {"passwordError", "There is no password or is bigger than 8"},
        {"passwordCorrect", "Password field is correctly filled"},
        {"timeError", "There is no time or its less than 1"},
        {"timeCorrect", "Time is correctly filled"},
        {"remainingAtemps", "Remeaning attemps: "},
        {"atempsError", "Attemp is null or less than 0"},
        {"atempsCorrect", "Attemps is correcctly filled"},
        {"errorFields", "Please fill all the fields"}
    };

    [Header("Game Settings")]
    int _passwordLength;
    int passwordLength
    {
        get {
            if (_passwordLength == null)
            {
                _passwordLength = -1;
            }
            return _passwordLength;
        }
        set
        {
            if (value == null || (value > 0 && value <= 8))
            {
                _passwordLength = value;
            }
            else
            {
                _passwordLength = -1;
            }
        }
    }

    float _countDownTime;

    float countDownTime
    {
        get {
            if (_countDownTime == null)
            {
                _countDownTime = -1;
            }
            return _countDownTime;
        }
        set
        {
            if (value == null || value >= 0)
            {
                _countDownTime = value * 60;
            }
            else
            {
                _countDownTime = -1;
            }
        }
    }

    int _maxAttempts;

    int maxAttempts
    {
        get
        {
            if (_maxAttempts == null)
            {
                _maxAttempts = -1;
            }
            return _maxAttempts;
        }
        set
        {
            if (value == null || value >= 0)
            {
                _maxAttempts = value;
            }
            else
            {
                _maxAttempts = -1;
            }
        }
    }

    string winMessage;
    string loseMessage;

    string password;

    [Header("GameScene References")]
    public RawImage backgroundImage;

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

    // Program coounters
    float currentTime = 0f;
    bool gameFinished = true;
    int correctCounter = 0;
    int actualAttemps = 0;

    // Update is called once per frame
    void Update()
    {
        if (!gameFinished) {
            currentTime += Time.deltaTime;
            if (currentTime <= countDownTime)
            {
                text_countDown.text = (int)((countDownTime - currentTime) / 60) + ":" + (int)((countDownTime - currentTime)%60);
            }
            else
            {
                close();
            }
        }
    }

    void close()
    {
        gameFinished = true;
        canvas.GetComponent<Animator>().Play("close");
        text_finalMessage.text = loseMessage;
    }

    public bool checkIntroducedValue(int x, string newValue)
    {
        if (password[x] == newValue[0]) correctCounter++;
        if (correctCounter == password.Length) {
            gameFinished = true;
            canvas.GetComponent<Animator>().Play("win");
            text_finalMessage.text = winMessage;
        }
        return password[x] == newValue[0];
    }

    public void getImage()
    {
        var extensions = new[] {
            new ExtensionFilter("Image Files", "png", "jpg", "jpeg" ) };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, true);
        print("Obtenemos el path -> " + paths[0]);
        var rawData = System.IO.File.ReadAllBytes(paths[0]);
        Texture2D tex = new Texture2D(2, 2); // Create an empty Texture; size doesn't matter (she said)
        tex.LoadImage(rawData);
        backgroundImage.texture = tex;
    }

    public void setPassword(string input)
    {
        passwordLength = input.Length;
        if (passwordLength==-1) {
            text_errors.text = messages["passwordError"];
        }
        else
        {
            password = input;
            text_errors.text = messages["passwordCorrect"];
        }
    }

    public void setCountDown(string codigo)
    {
        try
        {
            countDownTime = int.Parse(codigo);
        }
        catch
        {
            countDownTime = -1;
        }

        if (countDownTime == -1)
        {
            text_errors.text = messages["timeError"];
        }
        else
        {
            text_errors.text = messages["timeCorrect"];
        }
    }

    public void setAttemps(string codigo)
    {
        try
        {
            maxAttempts = int.Parse(codigo);
        }
        catch
        {
            maxAttempts = -1;
        }

        if (maxAttempts == -1)
        {
            text_errors.text = messages["atempsError"];
        }
        else
        {
            text_errors.text = messages["atempsCorrect"];
        }
    }

    public void setWinnedMessage(string codigo)
    {
        winMessage = codigo;
    }

    public void setLoseMessage(string codigo)
    {
        loseMessage = codigo;
    }

    public void introduceData() {
        if (passwordLength != -1 && countDownTime != -1 && maxAttempts != -1 && winMessage != "" && loseMessage != "") {
            text_errors.transform.parent.gameObject.SetActive(false);
            waitScreen.gameObject.SetActive(true);
            text_attemps.text = messages["remainingAtemps"] + (maxAttempts - actualAttemps);
        }
        else
        {
            text_errors.text = messages["errorFields"];
        }
    }

    public void startGame() {
        gameFinished = false;
        waitScreen.gameObject.SetActive(false);
        gameScreen.gameObject.SetActive(true);
        configureInputs();
    }

    void configureInputs() {
        bool par = passwordLength % 2 == 0;
        inputs.localPosition = inputs.localPosition + new Vector3((inputs.childCount - passwordLength) * 117 /*+ (par ? 0 : -65)*/, 0, 0);


        for (int x = 0; x < passwordLength; x++)
        {
            int z = x;
            TMPro.TMP_InputField componente = inputs.GetChild(x).GetComponent<TMPro.TMP_InputField>();
            componente.onValueChanged.AddListener(
                delegate
                {
                    if (checkIntroducedValue(z, componente.text))
                    {
                        componente.interactable = false;
                    }
                    else
                    {
                        GetComponent<AudioSource>().PlayOneShot(sound_wrong);
                        actualAttemps += 1;
                        text_attemps.text = messages["remainingAtemps"] + (maxAttempts - actualAttemps);
                        if (actualAttemps == maxAttempts)
                        {
                            close();
                        }
                        else
                        {
                            Instantiate(errorMessagePrefab, canvas);
                        }
                    }
                });
        }

        for (int x = passwordLength; x < inputs.childCount; x++)
        {
            inputs.GetChild(x).gameObject.SetActive(false);
        }
    }

}
