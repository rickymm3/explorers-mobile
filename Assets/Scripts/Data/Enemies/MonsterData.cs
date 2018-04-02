public class MonsterData :Entity {

    public ElementalTypes Element;
    public TapMonsterType TapType;
    public string Sprite;

    public MonsterData(string Identity, string name, ElementalTypes Element, TapMonsterType TapType, string Sprite) : base(Identity, name, 0) {
        this.Element = Element;
        this.TapType = TapType;
        this.Sprite = Sprite;
    }
}
