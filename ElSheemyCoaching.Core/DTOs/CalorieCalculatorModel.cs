namespace ElSheemyCoaching.Core.DTOs;

public class CalorieCalculatorModel
{
    // Inputs
    public double Height { get; set; }
    public double Weight { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; } = "Male";
    public string Activity { get; set; } = "Sedentary";
    public string Goal { get; set; } = "Maintain";
    public string Formula { get; set; } = "Mifflin-St Jeor";
    public double? BodyFat { get; set; }

    // Results
    public double BMR { get; set; }
    public double TDEE { get; set; }
    public double TargetCalories { get; set; }
}
