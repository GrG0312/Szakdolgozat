namespace Model.Units
{
    public class UnitVisualData
    {
        public string UnitName { get; }
        public string UnitProfileSprite { get; }
        public string UnitFullSprite { get; }

        public UnitVisualData(string name, string spritePath, string unitFullSprite)
        {
            UnitName = name;
            UnitProfileSprite = spritePath;
            UnitFullSprite = unitFullSprite;
        }
    }
}
