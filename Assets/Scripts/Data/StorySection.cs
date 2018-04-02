using UnityEngine;

public enum StoryCharacterPosition { Left, Right }
public enum StoryAction { Enter, Talk, Leave, ClearText, Background, Question, Reward, Skip, TapChange }
public enum StoryCharacterEmotion { None, Jump, Sink }
[System.Serializable]
public class StorySection : BaseData {
    public string Section;
    public int Order;

    public StoryCharacterPosition Focus;
    public StoryAction Action;
    public StoryCharacterEmotion Emotion;

    public Sprite CharacterArt;
    public string Name;
    public string Text;
    // If we ever want it voice acted we can just add that file here.

    public StorySection(int ID, string Identity, string Section, int Order, string Name = "", string Text = "", Sprite CharacterArt = null, StoryAction Action = StoryAction.Enter, StoryCharacterPosition Focus = StoryCharacterPosition.Left, StoryCharacterEmotion Emotion = StoryCharacterEmotion.None) {
        this.ID = ID;
        this.Identity = Identity;

        this.Section = Section;
        this.Order = Order;

        this.Name = Name;
        this.Text = Text;
        this.CharacterArt = CharacterArt;

        this.Action = Action;
        this.Focus = Focus;
        this.Emotion = Emotion;
    }
}