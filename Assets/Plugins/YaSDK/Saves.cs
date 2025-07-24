using System;

[Serializable]
public class Saves
{
    public int Score = 0;
    public int Money = 500;
    public CarData Car;
}

[Serializable]
public class CarData
{
    public string Model = "Matiz";
    public float MaxSpeed = 136;
    public int Level = 1;
}
