namespace Model.GameModel
{
    public struct PlayerPointsChangedEventArgs<T>
    {
        public readonly T Owner;
        public readonly int Current;
        public readonly int PerTurn;

        public PlayerPointsChangedEventArgs(T o, int c, int p)
        {
            Owner = o;
            Current = c;
            PerTurn = p;
        }
    }
}
