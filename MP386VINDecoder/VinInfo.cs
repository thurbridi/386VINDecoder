namespace MP386VINDecoder
{
  internal class VinInfo
  {
    public string Vin { get; private set; }
    public string Country { get; private set; }
    public string AssemblyPlant { get; private set; }
    public string Model { get; private set; }
    public string BodyType { get; private set; }
    public string Version { get; private set; }
    public string Year { get; private set; }
    public string Month { get; private set; }
    public string SerialNumber { get; private set; }
    public string Drive { get; private set; }
    public string Engine { get; private set; }
    public string Gearbox { get; private set; }
    public string AxleRatio { get; private set; }
    public string AxleLock { get; private set; }
    public string BodyColour { get; private set; }
    public string VinylRoof { get; private set; }
    public string InteriorTrim { get; private set; }
    public string Radio { get; private set; }
    public string InstrumentPanel { get; private set; }
    public string Windshield { get; private set; }
    public string Seats { get; private set; }
    public string Suspension { get; private set; }
    public string Brakes { get; private set; }
    public string Wheels { get; private set; }
    public string RearWindow { get; private set; }

    public int GetSizeInBytes() =>
      Country.Length + AssemblyPlant.Length + Model.Length + BodyType.Length +
      Version.Length + Year.Length + Month.Length + SerialNumber.Length +
      Drive.Length + Engine.Length + Gearbox.Length + AxleRatio.Length +
      AxleLock.Length + BodyColour.Length + VinylRoof.Length + InteriorTrim.Length +
      Radio.Length + InstrumentPanel.Length + Windshield.Length + Seats.Length +
      Suspension.Length + Brakes.Length + Wheels.Length + RearWindow.Length;

    public VinInfo(
      string vin, string country, string assemblyPlant, string model, string bodyType,
      string version, string year, string month, string serialNumber,
      string drive, string engine, string gearbox, string axleRatio,
      string axleLock, string bodyColour, string vinylRoof, string interiorTrim,
      string radio, string instrumentPanel, string windshield, string seats,
      string suspension, string brakes, string wheels, string rearWindow)
    {
      Vin = vin;
      Country = country;
      AssemblyPlant = assemblyPlant;
      Model = model;
      BodyType = bodyType;
      Version = version;
      Year = year;
      Month = month;
      SerialNumber = serialNumber;
      Drive = drive;
      Engine = engine;
      Gearbox = gearbox;
      AxleRatio = axleRatio;
      AxleLock = axleLock;
      BodyColour = bodyColour;
      VinylRoof = vinylRoof;
      InteriorTrim = interiorTrim;
      Radio = radio;
      InstrumentPanel = instrumentPanel;
      Windshield = windshield;
      Seats = seats;
      Suspension = suspension;
      Brakes = brakes;
      Wheels = wheels;
      RearWindow = rearWindow;
    }
  }
}