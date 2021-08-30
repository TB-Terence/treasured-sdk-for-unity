namespace Treasured.UnitySdk
{
    public interface IDataComponent<T> where T : TreasuredObjectData
    {
        T Data { get; }

        void BindData(T data);
    }
}
