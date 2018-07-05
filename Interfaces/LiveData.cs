namespace Matrix.Interfaces
{
    public interface LiveData
    {
        string Id { get; }

        string Sign { get; }

        bool IsValid { get; }
    }
}
