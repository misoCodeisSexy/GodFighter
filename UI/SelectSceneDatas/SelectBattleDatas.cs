using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GodFighter;
using UnityEngine.UI;
using DG.Tweening;
using Rewired;

public enum SelectMenuType
{
    map,
    character
}

[System.Serializable]
public struct SelectMapOption
{
    public Sprite mapBackGroundImg;
    public Sprite mapSelectSprite;

    public GameObject mapBase;
    public GameObject mapName;
    public StageMapOptions mapInfos;
}

[System.Serializable]
public struct SelectMap
{
    public SelectMapOption[] mapOptions;
    [HideInInspector] public int curIdx;

    public float btnTimer;
    public float btnWaitingTime;
}

[System.Serializable]
public struct CharacterModelData
{
    public GameObject model;
    public Animator animator;
}

[System.Serializable]
public struct CharacterDatas
{
    public CharacterModelData[] leftModelObjects;
    public CharacterModelData[] rightModelObjects;
}

[System.Serializable]
public class CharacterDataInfo
{
    public CharacterDatas modelDatas;

    public GameObject[] P1Lines_red;
    public GameObject[] P2Lines_blue;
    public GameObject[] focusingCharacter;
    public Image[] selectCharacter;

    public GameObject agniSameSelectImgObj;
    public GameObject mihoSameSelectImgObj;
    public GameObject valkyrieSameSelectImgObj;
    public GameObject randomSameSelectImgObj;

    [HideInInspector] public bool is1pFirst;
    [HideInInspector] public int currnetOnIdx = 0;

    [HideInInspector] public int P1SelectIdx;
    [HideInInspector] public int P2SelectIdx;
}

public class SelectBattleDatas : MonoBehaviour {

    public string nextSceneName;

    private SelectMenuType menuType;
    private int maxIdx;
    private float timer;
    private float waitingTime = 0.25f;
    private bool Input_on;
    private bool isSelectEnd = false;

    // map datas
    [SerializeField] GameObject mapObj;
    [SerializeField] GameObject chrObj;

    [SerializeField] GameObject arrowLeftOn;
    [SerializeField] GameObject arrowRightOn;

    [SerializeField] SelectMap maps;
    [SerializeField] GameObject curMapBgObj;

    private Image curMapBgImg;
    private readonly Vector3 bgResetOffset = new Vector3(115, -24, 0);  
    private readonly Vector3 baseResetOffset = new Vector3(-213, -1.3f, 0);
    private readonly Vector3 baseResetOffset2 = new Vector3(213, -1.3f, 0);
    private readonly Vector3 nameResetOffset = new Vector3(-123, 0, 0);
    private readonly Vector3 nameResetOffset2 = new Vector3(123, 0, 0);

    private bool arrowDirection = true; //Right

    private const float baseStartValue = 0.1f;
    private const float baseMoveOffsetX = -0.5f;
    private const float baseDuration = 0.2f;

    // character datas
    [SerializeField] private CharacterDataInfo characters;
    [SerializeField] private Image selectBgImg;

    [SerializeField] private GameObject madolPack;

    private void Start()
    {
        // debugging /////////////////////////////////////////

        //CGameManager.rewiredPlayer1 = ReInput.players.GetPlayer(0);
        //CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Joystick, 0);
        //CGameManager.rewiredPlayer1.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Keyboard, 0);

        //CGameManager.rewiredPlayer2 = ReInput.players.GetPlayer(1);
        //CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Joystick, 0);
        //CGameManager.rewiredPlayer2.controllers.maps.SetMapsEnabled(true, Rewired.ControllerType.Keyboard, 0);

        //CGameManager.gameMode = GameMode.TrainingRoom;

        //////////////////////////////////////////////////////

        CGameManager.OnInitAudioSystem(false);

        if (CGameManager.gameMode == GameMode.TrainingRoom)
            maxIdx = maps.mapOptions.Length;
        else if (CGameManager.gameMode == GameMode.BattleRoom)
            maxIdx = maps.mapOptions.Length - 1;

        menuType = SelectMenuType.map;
        
        curMapBgImg = curMapBgObj.GetComponent<Image>();

        OnInitMap();
    }

    private void FixedUpdate()
    {
        if (!Input_on)
        {
            timer += Time.deltaTime;
            if (timer > waitingTime)
            {
                Input_on = true;
            }
        }

        if (menuType == SelectMenuType.map)
        {
            OnFixedUpdateMap();
        }
        else if (menuType == SelectMenuType.character)
        {
            OnFixedUpdateCharacter();
        }
    }

    #region maps
    private void OnInitMap()
    {
        maps.btnTimer = 0;
        maps.btnWaitingTime = 0.12f;
        maps.curIdx = 0;
        OnImage(maps.curIdx);
    }

    private void OnImage(int idx)
    {
        for (int i = 0; i < maxIdx; i++) 
        {
            if (i == idx)
            {
                curMapBgImg.sprite = maps.mapOptions[i].mapBackGroundImg;
                curMapBgImg.rectTransform.localPosition = bgResetOffset;

                maps.mapOptions[i].mapBase.SetActive(true);
                maps.mapOptions[i].mapName.SetActive(true);
                if (arrowDirection)
                {
                    maps.mapOptions[i].mapBase.transform.localPosition = baseResetOffset;
                    maps.mapOptions[i].mapName.transform.localPosition = nameResetOffset;
                }
                else
                {
                    maps.mapOptions[i].mapBase.transform.localPosition = baseResetOffset2;
                    maps.mapOptions[i].mapName.transform.localPosition = nameResetOffset2;
                }
            }
            else
            {
                maps.mapOptions[i].mapBase.SetActive(false);
                maps.mapOptions[i].mapName.SetActive(false);
            }
        }

        /// bg move
        StartDoMove(curMapBgObj, 0, -0.5f, 20, true);
        /// base image move
        StartDoMove(maps.mapOptions[maps.curIdx].mapBase, baseStartValue, baseMoveOffsetX, baseDuration, false);
        /// name move
        StartDoMove(maps.mapOptions[maps.curIdx].mapName, 0.1f, -0.55f, 0.2f, false);
    }

    private void StartDoMove(GameObject moveObj, float startVelue, float value, float duration, bool loop)
    {
        Sequence move = DOTween.Sequence();
        move.Insert(startVelue, moveObj.transform.DOMoveX(value, duration));

        if (loop)
        {
            move.Insert(startVelue + duration, moveObj.transform.DOMoveX(-value, duration));
            move.SetLoops(-1, LoopType.Restart);
        }
    }
    
    private void OnFixedUpdateMap()
    {
        if (arrowLeftOn.activeSelf || arrowRightOn.activeSelf)
        {
            maps.btnTimer += Time.fixedDeltaTime;
            if (maps.btnTimer > maps.btnWaitingTime)
            {
                maps.btnTimer = 0;
                arrowLeftOn.SetActive(false);
                arrowRightOn.SetActive(false);
            }
        }

        // select
        if (CGameManager.rewiredPlayer1.GetButtonDown("Select") && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            SetTimer();
            if (CGameManager.selectStateOption != null)
                CGameManager.selectStateOption = null;
            CGameManager.selectStateOption = maps.mapOptions[maps.curIdx].mapInfos;

            menuType = SelectMenuType.character;

            mapObj.SetActive(false);
            chrObj.SetActive(true);
            madolPack.SetActive(true);

            OnInitcharacter();
            return;
        }

        // axis move
        if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            SetTimer();
            maps.curIdx++;
            if (maps.curIdx >= maxIdx) maps.curIdx = 0;
            // late add sound
            arrowRightOn.SetActive(true);
            arrowDirection = true;
            OnImage(maps.curIdx);
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            SetTimer();
            maps.curIdx--;
            if (maps.curIdx < 0) maps.curIdx = maxIdx - 1;
            // late add sound
            arrowLeftOn.SetActive(true);
            arrowDirection = false;
            OnImage(maps.curIdx);
        }
        else if (CGameManager.rewiredPlayer1.GetButtonDown("Back"))
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
            SceneManager.LoadScene("MainMenu");
        }
    }
    #endregion

    #region character
    private void OnInitcharacter()
    {
        waitingTime = 0.4f;
        characters.is1pFirst = true;

        characters.P1Lines_red[characters.currnetOnIdx].SetActive(true);
        characters.focusingCharacter[characters.currnetOnIdx].SetActive(true);

        characters.modelDatas.rightModelObjects[3].model.transform.DORotate(new Vector3(0.0f, -360f), 8, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        characters.modelDatas.leftModelObjects[3].model.transform.DORotate(new Vector3(0.0f, -360f), 8, RotateMode.WorldAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);

        for (int i = 0; i < characters.P2Lines_blue.Length; i++)
        {
            characters.P2Lines_blue[i].SetActive(false);
        }

        characters.currnetOnIdx = 0;

        selectBgImg.sprite = maps.mapOptions[maps.curIdx].mapSelectSprite;
        selectBgImg.gameObject.SetActive(true);

        OnCharacterModelLeft();
        OnCharacterModelRight();
    }

    private void OnCharacterModelLeft()
    {
        for (int i = 0; i < characters.modelDatas.leftModelObjects.Length; i++)
        {
            if (i == characters.currnetOnIdx)
                characters.modelDatas.leftModelObjects[i].model.SetActive(true);
            else
                characters.modelDatas.leftModelObjects[i].model.SetActive(false);
        }
    }

    private void OnCharacterModelRight()
    {
        for (int i = 0; i < characters.modelDatas.rightModelObjects.Length; i++)
        {
            if (i == characters.currnetOnIdx)
                characters.modelDatas.rightModelObjects[i].model.SetActive(true);
            else
                characters.modelDatas.rightModelObjects[i].model.SetActive(false);
        }
    }

    private void OnFixedUpdateCharacter()
    {
        if (characters.is1pFirst)
        {
            OnSelect_P1();
        }
        else
        {
            OnSelect_P2();
        }
    }

    private void OnSelect_P1()
    {
        if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (characters.currnetOnIdx >= 0 && characters.currnetOnIdx < characters.P1Lines_red.Length - 1) // 0, 1 ...
            {
                SetTimer();
                characters.P1Lines_red[characters.currnetOnIdx].SetActive(false);
                characters.focusingCharacter[characters.currnetOnIdx].SetActive(false);
                characters.P1Lines_red[++characters.currnetOnIdx].SetActive(true);
                characters.focusingCharacter[characters.currnetOnIdx].SetActive(true);
                OnCharacterModelLeft();
            }
        }
        else if (CGameManager.rewiredPlayer1.GetAxisRaw("Move Horizontal") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (characters.currnetOnIdx > 0 && characters.currnetOnIdx <= characters.P1Lines_red.Length - 1)
            {
                SetTimer();
                characters.P1Lines_red[characters.currnetOnIdx].SetActive(false);
                characters.focusingCharacter[characters.currnetOnIdx].SetActive(false);
                characters.P1Lines_red[--characters.currnetOnIdx].SetActive(true);
                characters.focusingCharacter[characters.currnetOnIdx].SetActive(true);
                OnCharacterModelLeft();
            }
        }

        if (CGameManager.rewiredPlayer1.GetButtonDown("Select"))
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            SetTimer();
            if (characters.currnetOnIdx != 3)
                characters.modelDatas.leftModelObjects[characters.currnetOnIdx].animator.SetTrigger("select");
            characters.P1SelectIdx = characters.currnetOnIdx;
            Sequence sequence = DOTween.Sequence();
            sequence.Insert(0.0f, characters.selectCharacter[characters.P1SelectIdx].DOFade(1.0f, 0.4f).SetEase(Ease.Flash, 16, 1));

            for (int i = 0; i < characters.P1Lines_red.Length; i++)
            {
                if (i != characters.currnetOnIdx)
                    characters.P1Lines_red[i].SetActive(false);
                else
                {
                    switch (i)
                    {
                        case 0:
                            {
                                CGameManager.P1_name = CharacterNames.agni;
                            }
                            break;
                        case 1:
                            {
                                CGameManager.P1_name = CharacterNames.miho;
                            }
                            break;
                        case 2:
                            {
                                CGameManager.P1_name = CharacterNames.valkiri;
                            }
                            break;
                        case 3:
                            {
                                CGameManager.P1_name = CharacterNames.random;
                                characters.modelDatas.leftModelObjects[3].model.transform.DORotate(new Vector3(0.0f, -900f), 0.5f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
                            }
                            break;
                    }
                }
            }
            characters.P2Lines_blue[characters.currnetOnIdx].SetActive(true);
            characters.is1pFirst = false;
        }
        //캐릭터선택에서 맵선택으로 변경
        else if (CGameManager.rewiredPlayer1.GetButtonDown("Back"))
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
            mapObj.SetActive(true);
            chrObj.SetActive(false);
            madolPack.SetActive(false);
            selectBgImg.gameObject.SetActive(false);

            CGameManager.selectStateOption = null;
            menuType = SelectMenuType.map;
            OnInitMap();
        }
    }

    private void OnSelect_P2()
    {
        if (isSelectEnd) return;

        if (CGameManager.rewiredPlayer2.GetAxisRaw("Move Horizontal") > 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (characters.currnetOnIdx >= 0 && characters.currnetOnIdx < characters.P1Lines_red.Length - 1)
            {
                SetTimer();
                characters.P2Lines_blue[characters.currnetOnIdx].SetActive(false);
                if (characters.P1SelectIdx != characters.currnetOnIdx)
                    characters.focusingCharacter[characters.currnetOnIdx].SetActive(false);
                characters.P2Lines_blue[++characters.currnetOnIdx].SetActive(true);
                characters.focusingCharacter[characters.currnetOnIdx].SetActive(true);
            }
        }
        else if (CGameManager.rewiredPlayer2.GetAxisRaw("Move Horizontal") < 0 && Input_on)
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.move);
            if (characters.currnetOnIdx > 0 && characters.currnetOnIdx <= characters.P1Lines_red.Length - 1)
            {
                SetTimer();
                characters.P2Lines_blue[characters.currnetOnIdx].SetActive(false);
                if (characters.P1SelectIdx != characters.currnetOnIdx)
                    characters.focusingCharacter[characters.currnetOnIdx].SetActive(false);
                characters.P2Lines_blue[--characters.currnetOnIdx].SetActive(true);
                characters.focusingCharacter[characters.currnetOnIdx].SetActive(true);
            }
        }
        OnCharacterModelRight();

        characters.P2SelectIdx = characters.currnetOnIdx;
        if (characters.P1SelectIdx == characters.P2SelectIdx &&
            (!characters.agniSameSelectImgObj.activeSelf && !characters.mihoSameSelectImgObj.activeSelf &&
            !characters.valkyrieSameSelectImgObj.activeSelf && !characters.randomSameSelectImgObj.activeSelf))
        {
            characters.P1Lines_red[characters.P1SelectIdx].SetActive(true);
            characters.P2Lines_blue[characters.P2SelectIdx].SetActive(true);

            if (CGameManager.P1_name == CharacterNames.agni)
                characters.agniSameSelectImgObj.SetActive(true);
            else if (CGameManager.P1_name == CharacterNames.miho)
                characters.mihoSameSelectImgObj.SetActive(true);
            else if (CGameManager.P1_name == CharacterNames.valkiri)
                characters.valkyrieSameSelectImgObj.SetActive(true);
            else if (CGameManager.P1_name == CharacterNames.random)
                characters.randomSameSelectImgObj.SetActive(true);
        }
        else if (characters.P1SelectIdx != characters.P2SelectIdx &&
            (characters.agniSameSelectImgObj.activeSelf || characters.mihoSameSelectImgObj.activeSelf
            || characters.valkyrieSameSelectImgObj.activeSelf || characters.randomSameSelectImgObj.activeSelf))
        {
            characters.P1Lines_red[characters.P1SelectIdx].SetActive(true);
            characters.P2Lines_blue[characters.P2SelectIdx].SetActive(true);
            characters.agniSameSelectImgObj.SetActive(false);
            characters.mihoSameSelectImgObj.SetActive(false);
            characters.valkyrieSameSelectImgObj.SetActive(false);
            characters.randomSameSelectImgObj.SetActive(false);
        }

        if (CGameManager.rewiredPlayer2.GetButtonDown("Select"))
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.select);
            SetTimer();
            if (characters.currnetOnIdx != 3)
                characters.modelDatas.rightModelObjects[characters.currnetOnIdx].animator.SetTrigger("select");
            Sequence sequence = DOTween.Sequence();
            sequence.Insert(0.0f, characters.selectCharacter[characters.P2SelectIdx].DOFade(1.0f, 0.4f).SetEase(Ease.Flash, 16, 1));

            for (int i = 0; i < characters.P1Lines_red.Length; i++)
            {
                if (i == characters.currnetOnIdx)
                {
                    switch (i)
                    {
                        case 0:
                            {
                                CGameManager.P2_name = CharacterNames.agni;
                            }
                            break;
                        case 1:
                            {
                                CGameManager.P2_name = CharacterNames.miho;
                            }
                            break;
                        case 2:
                            {
                                CGameManager.P2_name = CharacterNames.valkiri;
                            }
                            break;
                        case 3:
                            {
                                characters.modelDatas.rightModelObjects[3].model.transform.DORotate(new Vector3(0.0f, -900f), 0.5f, RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
                                int ran = Random.Range(0, 3); // 발키리 추가하면 (0,3)
                                switch (ran)
                                {
                                    case 0:
                                        {
                                            CGameManager.P2_name = CharacterNames.agni;
                                        }
                                        break;
                                    case 1:
                                        {
                                            CGameManager.P2_name = CharacterNames.miho;
                                        }
                                        break;
                                    case 2:
                                        {
                                            CGameManager.P2_name = CharacterNames.valkiri;
                                        }
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            if (CGameManager.P1_name == CharacterNames.random)
            {
                int ran = Random.Range(0, 3); // 발키리 추가하면 (0,3)
                switch (ran)
                {
                    case 0:
                        {
                            CGameManager.P1_name = CharacterNames.agni;
                        }
                        break;
                    case 1:
                        {
                            CGameManager.P1_name = CharacterNames.miho;
                        }
                        break;
                    case 2:
                        {
                            CGameManager.P1_name = CharacterNames.valkiri;
                        }
                        break;
                }
            }

            isSelectEnd = true;
            CSceneManager.freezePhysics = false;
            CSceneManager.freeCamera = false;
            Invoke("NextStage", 5.0f);
        }
        //캐릭터선택에서 맵선택으로 변경
        else if (CGameManager.rewiredPlayer2.GetButtonDown("Back"))
        {
            CGameManager.PlaySoundFX(CGameManager.fxSounds.back);
            mapObj.SetActive(true);
            chrObj.SetActive(false);
            madolPack.SetActive(false);
            selectBgImg.gameObject.SetActive(false);

            characters.P1Lines_red[characters.P1SelectIdx].SetActive(false);
            characters.focusingCharacter[characters.P1SelectIdx].SetActive(false);

            characters.P2Lines_blue[characters.currnetOnIdx].SetActive(false);
            characters.focusingCharacter[characters.currnetOnIdx].SetActive(false);
            characters.currnetOnIdx = 0;

            characters.agniSameSelectImgObj.SetActive(false);
            characters.mihoSameSelectImgObj.SetActive(false);
            characters.valkyrieSameSelectImgObj.SetActive(false);
            characters.randomSameSelectImgObj.SetActive(false);

            CGameManager.selectStateOption = null;
            menuType = SelectMenuType.map;
            OnInitMap();
        }
    }

    private void NextStage()
    {
       SceneManager.LoadScene(nextSceneName);
    }

    private void SetTimer()
    {
        Input_on = false;
        timer = 0.0f;
    }

    #endregion
}
