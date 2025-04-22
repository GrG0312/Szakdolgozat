namespace Model.UnityDependant
{
    public class UnitVisualData
    {
        public string UnitProfileSprite { get; }
        public string UnitFullSprite { get; }

        public UnitVisualData(string spritePath, string unitFullSprite)
        {
            UnitProfileSprite = spritePath;
            UnitFullSprite = unitFullSprite;
        }
    }
}
