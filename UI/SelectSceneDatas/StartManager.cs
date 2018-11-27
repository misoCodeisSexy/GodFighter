using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewired;
using DG.Tweening;

public class StartManager : MonoBehaviour
{
    public Image Logo;
    public Image Button;
    public Image Button2;

    public FxSoundData soundData;
    
    float alpha = 0.0f;
    float alpha_2 = 0.0f;
    float alpha_3 = 0.0f;

    [SerializeField] private GameObject background;
    private bool is_TitleShow = true;
    private bool is_ButtonShow = false;
    private bool is_Alpha = false;
    private Sequence backgroundScroll;

    private void Start()
    {
        CGameManager.rewiredPlayer1 = ReInput.players.GetPlayer(0);
        CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Joystick, 0);
        CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Keyboard, 0);

        CGameManager.rewiredPlayer2 = ReInput.players.GetPlayer(1);
        CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Joystick, 0);
        CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Keyboard, 0);

        Sequence initBackground = DOTween.Sequence();
        initBackground.Insert(0.0f, background.transform.DOMoveY(-300, 0.0f));

        backgroundScroll = DOTween.Sequence();
        backgroundScroll.Insert(0.0f, background.transform.DOMoveY(950, 50).SetEase(Ease.Linear));
        backgroundScroll.Insert(50, background.transform.DOMoveY(-300, 50).SetEase(Ease.Linear));

        backgroundScroll.SetLoops(-1, LoopType.Restart);

        CGameManager.fxSounds = new FxSoundData();
        CGameManager.OnInitSoundFx(soundData);
        CGameManager.OnInitAudioSystem(false);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (is_TitleShow)
            On_Title();

        if (is_ButtonShow)
            On_Button();

        if (CGameManager.rewiredPlayer1.GetButtonDown("Select"))
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            SceneManager.LoadScene("MainMenu");
        }
    }

    void On_Title()
    {
        alpha += Time.deltaTime * 0.3f;
        Logo.color = new Color(Logo.color.r, Logo.color.b, Logo.color.g, alpha);

        if (alpha >= 0.5f) is_ButtonShow = true;
        if (alpha >= 1.0f) is_TitleShow = false;
    }

    void On_Button()
    {
        if (alpha_2 >= 1.0f)
            Fade_Button();
        else
        {
            alpha_2 += Time.deltaTime * 0.5f;
            Button.color = new Color(Button.color.r, Button.color.b, Button.color.g, alpha_2);
        }
    }

    void Fade_Button()
    {
        if (is_Alpha)
        {
            alpha_3 -= Time.deltaTime * 2.0f;
            if (alpha_3 <= 0.0f)
                is_Alpha = false;
        }
        else if (!is_Alpha)
        {
            alpha_3 += Time.deltaTime * 2.0f;
            if (alpha_3 >= 1.0f)
                is_Alpha = true;
        }

        Button2.color = new Color(Button2.color.r, Button2.color.b, Button2.color.g, alpha_3);
    }

}
