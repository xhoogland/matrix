using Matrix.Interfaces;
using System;

namespace Matrix.SpecificImplementations
{
    public class GpsCoordinates : Coordinates
    {
        public double X { get; set; }

        public double Y { get; set; }

        public bool AreCoordinatesInRange(Coordinates coordinates)
        {
            const double range = 0.00032;

            var maxX = Math.Max(X, coordinates.X);
            var maxY = Math.Max(Y, coordinates.Y);
            var minX = Math.Min(X, coordinates.X);
            var minY = Math.Min(Y, coordinates.Y);
            
            return maxX - minX < range && maxY - minY < range;
        }
    }
}
