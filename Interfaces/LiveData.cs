using Matrix.Enums;

namespace Matrix.Interfaces
{
    public interface LiveData
    {
        string Id { get; }

        string Sign { get; }

        DataType DataType { get; }
    }
}
