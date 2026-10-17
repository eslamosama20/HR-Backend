using SmartAndPro.ERP.Domains.Common.Models;

namespace SmartAndPro.ERP.Domains.ValueObjects;

public enum Period
{
    Am,
    Pm
}


public record TimeOfDay : IValueObject
{
    public int Hours { get; }
    public int Minutes { get; }
    public Period Period { get; } // "Am" or "Pm"
    public string TimeOfDayString => $"{Hours:00}:{Minutes:00} {Period}";
    
    public TimeOfDay()
    {}
    
    public TimeOfDay(int hours, int minutes, Period period)
    {
        Hours = hours;
        Minutes = minutes;
        Period = period;
    }
    
    public override string ToString()
    {
        return TimeOfDayString;
    }
    
        public static implicit operator string(TimeOfDay timeOfDay)
    {
        return timeOfDay.ToString();
    }
    
    public static implicit operator TimeOfDay(string timeOfDay)
    {
        var parts = timeOfDay.Split(' ');
        var timeParts = parts[0].Split(':');
        return new TimeOfDay(int.Parse(timeParts[0]), int.Parse(timeParts[1]), Enum.Parse<Period>(parts[1]) );
    }
}
