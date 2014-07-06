using System;

namespace Bastet.Database.Model
{
    public interface IReading<out T>
    {
        long Id { get; }

        DateTime Timestamp { get; }

        long SensorId { get; set; }

        T Value { get; }
    }
}
