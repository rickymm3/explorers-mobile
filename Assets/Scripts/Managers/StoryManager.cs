using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ExtensionMethods;

public class StoryManager : Singleton<StoryManager> {
    public Image BackDrop;
    public RectTransform StoryContainer;
    public bool waitForInteraction = true;
    public TextMeshProUGUI TapToContinue;

    [Header("Story Components")]
    public Image backgroundImage;

    [Header("Character Details")]
    public TextMeshProUGUI charactername;
    public TextMeshProUGUI textarea;
    
    public RectTransform LeftCharacterRect;
    public RectTransform RightCharacterRect;
    public Image LeftCharacter;
    public Image RightCharacter;

    [Header("Question and Answers")]
    public GameObject ChoiceContainer;
    public List<GameObject> Choices = new List<GameObject>();

    [Header("Loot Box")]
    public Transform LootInterfaceContainer;
    public GameObject LootInterface;

    IEnumerator storyRoutine;

    float characterOffset = 600f;
    float characterPostion = 25f;
    float storyBackgroundOffset = -1500f;

    List<StorySection> currentStory = new List<StorySection>();

    bool waitingOnQuestionAnswer = false;
    bool hasInput = false;
    bool openingChest = false;
    bool pauseInteraction = false;
    bool inTap = false;

    void Update() {
        if (Input.GetMouseButtonDown(0) && !pauseInteraction)
            hasInput = true;
    }

    IEnumerator FadeTapToContinue() {
        while (true) {
            if (TapToContinue.gameObject.activeSelf) {
                yield return TapToContinue.DOColor(new Color(1f, 1f, 1f, 0.75f), 1f).WaitForCompletion();
                yield return TapToContinue.DOColor(new Color(1f, 1f, 1f, 0f), 1f).WaitForCompletion();
            }
            yield return new WaitForEndOfFrame();
        }
    }

    [ContextMenu("Test")]
    void test() {
        DisplayStory("story_test");
    }

    [ContextMenu("Question Test")]
    void choicetest() {
        DisplayStory("story_choice_test");
    }

    void Reset() {
        charactername.text = "";
        textarea.text = "";
        StoryContainer.DOAnchorPosY(storyBackgroundOffset, 0f);
        BackDrop.DOColor(new Color(0f, 0f, 0f, 0f), 0f);
        BackDrop.raycastTarget = true;
        TapToContinue.DOColor(new Color(1f, 1f, 1f, 0f), 0f);
        LeftCharacter.sprite = null;
        RightCharacter.sprite = null;
        LeftCharacterRect.DOAnchorPosX(-characterOffset, 0f);
        RightCharacterRect.DOAnchorPosX(characterOffset, 0f);
        TapToContinue.gameObject.SetActive(false);
        backgroundImage.DOColor(new Color(0f, 0f, 0f, 0f), 0f);

        StartCoroutine(FadeTapToContinue());
    }
    
    // Display Story Segment
    public void DisplayStory(string story, Action callback = null, bool inTap = false) {
        this.inTap = inTap;
        currentStory = DataManager.Instance.GetStorySegmentsBySection(story); ;
        Reset();
        GameManager.Instance.InStory = true;

        // Take an index or story segment as argument?
        if (storyRoutine != null) {
            StopCoroutine(storyRoutine);
        }
        storyRoutine = DisplayStoryCR(callback);
        StartCoroutine(storyRoutine);
    }

    IEnumerator DisplayStoryCR(Action callback) {
        yield return new WaitForEndOfFrame();
        // Fade in backdrop
        BackDrop.DOColor(new Color(0f, 0f, 0f, 0.9f), 0.75f);
        yield return new WaitForSeconds(0.5f);

        // swipe in background
        StoryContainer.DOAnchorPosY(0f, 0.5f);
        yield return new WaitForSeconds(0.5f);
        
        // cycle through text and fade characters not talking
        while (currentStory.Count > 0) {
            yield return StartCoroutine(ManageStorySection(currentStory[0]));
            currentStory.RemoveAt(0);
        }

        yield return new WaitForSeconds(0.5f);
        // Fade out the text and clean up the scene
        StoryContainer.DOAnchorPosY(storyBackgroundOffset, 0.5f);
        GameManager.Instance.InStory = false;
        yield return new WaitForSeconds(0.25f);
        BackDrop.DOColor(new Color(0f, 0f, 0f, 0f), 0.75f);
        BackDrop.raycastTarget = false;
        if (callback != null) callback();
    }

    IEnumerator ManageStorySection(StorySection story) {
        //Debug.Log("[Story] Executing Action: " + story.Action + " [" + story.Order + "]");
        switch(story.Emotion) {
            case StoryCharacterEmotion.Jump:
                switch (story.Focus) {
                    case StoryCharacterPosition.Left:
                        LeftCharacterRect.DOPunchPosition(new Vector3(0f, 10f, 0f), 0.2f, 0);
                        break;
                    case StoryCharacterPosition.Right:
                        RightCharacterRect.DOPunchPosition(new Vector3(0f, 10f, 0f), 0.2f, 0);
                        break;
                }
                break;
            case StoryCharacterEmotion.Sink:
                switch (story.Focus) {
                    case StoryCharacterPosition.Left:
                        LeftCharacterRect.DOPunchPosition(new Vector3(0f, -15f, 0f), 1f, 0);
                        break;
                    case StoryCharacterPosition.Right:
                        RightCharacterRect.DOPunchPosition(new Vector3(0f, -15f, 0f), 1f, 0);
                        break;
                }
                break;
            default:
                break;
        }
        switch (story.Action) {
            case StoryAction.Talk:
                charactername.text = story.Name;

                // Show the focused target
                switch(story.Focus) {
                    case StoryCharacterPosition.Left:
                        LeftCharacter.DOColor(new Color(1f, 1f, 1f, 1f), 0.3f);
                        RightCharacter.DOColor(new Color(0.8f, 0.8f, 0.8f, 1f), 0.3f);
                        break;
                    case StoryCharacterPosition.Right:
                        RightCharacter.DOColor(new Color(1f, 1f, 1f, 1f), 0.3f);
                        LeftCharacter.DOColor(new Color(0.8f, 0.8f, 0.8f, 1f), 0.3f);
                        break;
                }

                yield return StartCoroutine(RevealText(story.Text));
                break;
            case StoryAction.Enter:
                switch (story.Focus) {
                    case StoryCharacterPosition.Left:
                        LeftCharacter.sprite = story.CharacterArt;
                        yield return StartCoroutine(MoveCharacter(LeftCharacterRect, characterPostion, true));
                        break;
                    case StoryCharacterPosition.Right:
                        RightCharacter.sprite = story.CharacterArt;
                        yield return StartCoroutine(MoveCharacter(RightCharacterRect, -characterPostion, true));
                        break;
                }
                break;
            case StoryAction.Leave:
                switch (story.Focus) {
                    case StoryCharacterPosition.Left:
                        yield return StartCoroutine(MoveCharacter(LeftCharacterRect, -characterOffset));
                        break;
                    case StoryCharacterPosition.Right:
                        yield return StartCoroutine(MoveCharacter(RightCharacterRect, characterOffset));
                        break;
                }
                break;
            case StoryAction.Question:
                Debug.Log(" -- [Question] Story Text: " + story.Text);
                string[] questionAndAnswers = story.Text.Split('\n');
                charactername.text = questionAndAnswers[0];
                textarea.text = "";

                ChoiceContainer.SetActive(true);

                foreach(GameObject c in Choices) {
                    c.SetActive(false);
                }

                for (int i = 1; i < questionAndAnswers.Length; i++) {
                    Choices[i - 1].SetActive(true);
                    string responce = questionAndAnswers[i].Split(':')[0];
                    string answer = questionAndAnswers[i].Split(':')[1];
                    Debug.Log(" -- [Question] Answer " + i + ": " + responce + " [" + answer + "]");
                    Choices[i - 1].GetComponentInChildren<TextMeshProUGUI>().text = responce;
                    Choices[i - 1].GetComponent<Button>().onClick.AddListener(delegate { BtnChoice(answer); });
                    Choices[i - 1].GetComponent<RectTransform>().DOAnchorPosY(-((i - 1) * 125f), 0.25f);
                    yield return new WaitForSeconds(0.1f);
                }
                waitingOnQuestionAnswer = true;
                yield return StartCoroutine(HandleQuestionInput());
                break;
            case StoryAction.Skip:
                yield return StartCoroutine(RemoveSectionsBefore(int.Parse(story.Text)));
                break;
            case StoryAction.Background:
                yield return StartCoroutine(ChangeBackground(story.Text));
                break;
            case StoryAction.TapChange:
                pauseInteraction = true;
                string[] changes = story.Text.Split('-');
                //NewBoss-bossfight_1
                //ExtendTime-1.5

                switch (changes[0]) {
                    case "ExtendTime":
                        if (inTap) {
                            float multiplier = float.Parse(changes[1]);
                            float totalZoneHP = (float) PlayerManager.Instance.SelectedBattle.Zone.ZoneHP;
                            PlayerManager.Instance.SelectedBattle.AccumulatedDamage -= totalZoneHP * multiplier;
                        }
                        break;
                    case "NewBoss":
                        if (inTap) {
                            // Set the new boss here
                            // This needs to be hooked up to the server
                            PlayerManager.Instance.SelectedBattle.Zone.BossFight = DataManager.Instance.bossFightDataList.GetByIdentity(changes[1]);
                        }
                        break;
                    default:
                        Debug.Log("No reward issue");
                        break;
                }
                pauseInteraction = false;
                break;
            case StoryAction.Reward:
                pauseInteraction = true;
                Debug.Log("Reward the player here");
                //FreeLoot-lt_zone_training_boss_1-40-20
                //ZoneLoot-a0zone_1
                string[] rewards = story.Text.Split('-');
                LootCrate crate = null;

                switch (rewards[0]) {
                    case "FreeLoot":
                        crate = new LootCrate(rewards[1], float.Parse(rewards[2]), float.Parse(rewards[3]), DataManager.Instance.crateTypeDataList.GetByIdentity(rewards[4]));
                        break;
                    case "ZoneLoot":
                        ZoneData zone = DataManager.Instance.zoneDataList.GetByIdentity(rewards[1]);

                        crate = new LootCrate(zone.LootTable.Identity, zone.MinItemLevel, zone.Variance, MathHelper.WeightedRandom(zone.LootTable.crateType).Key);
                        break;
                    default:
                        Debug.Log("No reward issue");
                        break;
                }
                openingChest = true;
                Debug.Log("   [crate] " + crate.lootTableIdentity + " | " + crate.LootTableData);
                OpenChest(crate);

                while(openingChest) {
                    yield return new WaitForEndOfFrame();
                }
                pauseInteraction = false;
                break;
            case StoryAction.ClearText:
                charactername.text = "";
                textarea.text = "";
                break;
        }
    }
    GameObject lootCrateInterface;
    void CompleteLoot() {
        openingChest = false;
        Destroy(lootCrateInterface);
    }

    void OpenChest(LootCrate crate) {
        lootCrateInterface = Instantiate(LootInterface, LootInterfaceContainer);
        LootCrateInterface lci = lootCrateInterface.GetComponent<LootCrateInterface>();
        
        lci.Initialize(crate, null, CompleteLoot, true);
    }

    IEnumerator RemoveSectionsBefore(int index) {
        List<StorySection> temp = new List<StorySection>();
        foreach(StorySection section in currentStory) {
            if (section.Order >= (index-1)) // -1 because we will be removing the first section, so we need to keep one to remove
                temp.Add(section);
        }

        currentStory = temp;

        yield return new WaitForEndOfFrame();
    }

    IEnumerator HandleQuestionInput() {
        while (waitingOnQuestionAnswer) {
            yield return new WaitForEndOfFrame();
        }

        for (int i = Choices.Count-1; i >= 0; i--) {
            Choices[i].GetComponent<RectTransform>().DOAnchorPosY(-1500f, 0.25f);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.55f);
        ChoiceContainer.SetActive(false);
    }

    void BtnChoice(string input) {
        if (string.IsNullOrEmpty(input)) return;

        string[] args = input.Replace('[', ' ').Replace(']', ' ').Split('-');
        switch(args[0].ToLower().Trim()) {
            case "goto":
                StartCoroutine(RemoveSectionsBefore(int.Parse(args[1])));
                break;
            default:
                Debug.Log(" [Story] Argument " + args[0] + " not yet implemented.");
                break;
        }

        Debug.Log("answer with input: " + input);
        waitingOnQuestionAnswer = false;
    }

    IEnumerator ChangeBackground(string background) {
        if (backgroundImage.color.a > 0f)
            yield return backgroundImage.DOColor(new Color(0f, 0f, 0f, 0f), 0.1f).WaitForCompletion();
        backgroundImage.sprite = Resources.Load<Sprite>("StoryBackgrounds/" + background);
        yield return new WaitForSeconds(0.1f);
        yield return backgroundImage.DOColor(new Color(1f, 1f, 1f, 1f), 0.1f).WaitForCompletion();
    }

    IEnumerator RevealText(string message) {
        textarea.text = "<color=#ffffff00>" + message + "</color>";
        hasInput = false;

        var numCharsRevealed = 0;
        while (numCharsRevealed < message.Length) {
            while (message[numCharsRevealed] == ' ')
                ++numCharsRevealed;

            ++numCharsRevealed;

            if (hasInput)
                numCharsRevealed = message.Length;

            textarea.text = message.Substring(0, numCharsRevealed) + "<color=#ffffff00>" + message.Substring(numCharsRevealed) + "</color>";

            yield return new WaitForSeconds(0.035f);
        }

        hasInput = false;

        TapToContinue.gameObject.SetActive(true);
        if (waitForInteraction) {
            bool done = false;
            while (!done) {
                yield return new WaitForEndOfFrame();

                if (hasInput) {
                    done = true;
                    hasInput = false;
                }
            }
        }
        TapToContinue.gameObject.SetActive(false);
    }

    IEnumerator MoveCharacter(RectTransform Character, float target, bool wait = false) {
        if (wait)
            yield return Character.DOAnchorPosX(target, 0.5f).WaitForCompletion();
        else
            Character.DOAnchorPosX(target, 0.5f);

        yield return new WaitForSeconds(0.05f);
    }
}
