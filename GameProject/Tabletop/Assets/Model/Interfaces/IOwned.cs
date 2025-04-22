namespace Model.Interfaces
{
    /// <summary>
    /// An object that is owned by someone
    /// </summary>
    /// <typeparam name="IdType">The type which is used to identify the owner</typeparam>
    public interface IOwned<IdType>
    {
        public IdType Owner { get; }
    }
}
