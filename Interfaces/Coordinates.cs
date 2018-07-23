namespace Matrix.Interfaces
{
    public interface Coordinates
    {
        double X { get; set; }

        double Y { get; set; }

        bool AreCoordinatesInRange(Coordinates coordinates);
    }
}
