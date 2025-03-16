namespace Persistence
{
    public interface IDataManager<T>
    {
        public void Save(T data);
        public T Load();
    }
}
