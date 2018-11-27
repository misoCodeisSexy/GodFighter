using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameControl : DefaultUIControl
{
    [SerializeField] private GameObject[] buttonImgs;
    private int curIdx;

    private bool enableInput;
    private float axisTimer;
    private const float axisWaitingTime = 0.2f;

    public override void FixedUpdateGOF()
    {
        if (!enableInput)
        {
            axisTimer += Time.fixedDeltaTime;
            if (axisTimer > axisWaitingTime) enableInput = true;
        }
        else
        {
            if (CGameManager.rewiredPlayer1.GetAxis("Move Vertical") < 0)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                curIdx++;
                if (curIdx >= buttonImgs.Length) curIdx = 0;
                OnImage(curIdx, ref buttonImgs);
                ReseMyEnableTime();
            }
            else if (CGameManager.rewiredPlayer1.GetAxis("Move Vertical") > 0)
            {
                CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
                curIdx--;
                if (curIdx < 0) curIdx = buttonImgs.Length - 1;
                OnImage(curIdx, ref buttonImgs);
                ReseMyEnableTime();
            }
        }
        PauseMenuSelect();
    }

    public override void PauseMenuSelect()
    {
        if (CGameManager.rewiredPlayer1.GetButtonDown("Select") && enableInput)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            switch (curIdx)
            {
                case 0:      //캐릭터&맵선택
                    {
                        CSceneManager.Instance.ResetRoundCast();
                        SceneManager.LoadScene("SelectMenu");
                    }
                    break;
                case 1:      //메인메뉴
                    {
                        CSceneManager.Instance.ResetRoundCast();
                        SceneManager.LoadScene("MainMenu");
                    }
                    break;
                default:
                    Debug.LogError("EndGame Menu Index Error!");
                    break;
            }
            return;
        }
    }

    private void ReseMyEnableTime()
    {
        enableInput = false;
        axisTimer = 0;
    }
}
