namespace Model.Units
{
    public struct UnitVisualData
    {
        public string UnitName { get; }
        public string UnitSprite { get; }

        public UnitVisualData(string name, string spritePath)
        {
            UnitName = name;
            UnitSprite = spritePath;
        }
    }
}
