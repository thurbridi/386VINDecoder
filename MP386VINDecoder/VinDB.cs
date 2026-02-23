using System.Collections.Generic;

namespace MP386VINDecoder
{
  // VIN layout (29 chars total):
  //  0      : Country
  //  1      : Assembly Plant
  //  2      : Model
  //  3      : Body Type
  //  4      : Version
  //  5      : Year
  //  6      : Month
  //  7-11   : Serial Number (5 chars)
  //  12     : Drive
  //  13-14  : Engine (2 chars)
  //  15     : Gearbox
  //  16     : Axle Ratio
  //  17     : Axle Lock
  //  18     : Body Colour
  //  19     : Vinyl Roof
  //  20     : Interior Trim
  //  21     : Radio
  //  22     : Instrument Panel
  //  23     : Windshield
  //  24     : Seats
  //  25     : Suspension
  //  26     : Brakes
  //  27     : Wheels
  //  28     : Rear Window
  internal static class VinDB
  {
    public const int VinLength = 29;

    // 1. Country
    private static readonly Dictionary<char, string> Countries = new Dictionary<char, string>
    {
      { 'U', "Corris Britain" }
    };

    // 2. Assembly Plant
    private static readonly Dictionary<char, string> AssemblyPlants = new Dictionary<char, string>
    {
      { 'A', "Dagenham" },
      { 'B', "Manchester" },
      { 'C', "Saarlouis" },
      { 'K', "Rheine" }
    };

    // 3. Model
    private static readonly Dictionary<char, string> Models = new Dictionary<char, string>
    {
      { 'B', "Rivett" }
    };

    // 4. Body Type
    private static readonly Dictionary<char, string> BodyTypes = new Dictionary<char, string>
    {
      { 'B', "2D Pillared Sedan" }
    };

    // 5. Version
    private static readonly Dictionary<char, string> Versions = new Dictionary<char, string>
    {
      { 'D', "L" },
      { 'E', "LX" },
      { 'G', "SLX" },
      { 'P', "GT" }
    };

    // 6. Year
    private static readonly Dictionary<char, string> Years = new Dictionary<char, string>
    {
      { 'L', "1971" },
      { 'M', "1972" },
      { 'N', "1973" },
      { 'P', "1974 (Facelift)" },
      { 'R', "1975" },
      { 'S', "1976" }
    };

    // 7. Month
    private static readonly Dictionary<char, string> Months = new Dictionary<char, string>
    {
      { 'C', "January" },
      { 'K', "February" },
      { 'D', "March" },
      { 'E', "April" },
      { 'L', "May" },
      { 'Y', "June" },
      { 'S', "July" },
      { 'T', "August" },
      { 'J', "September" },
      { 'U', "October" },
      { 'M', "November" },
      { 'P', "December" }
    };

    // 9. Drive
    private static readonly Dictionary<char, string> Drives = new Dictionary<char, string>
    {
      { '1', "RWD" }
    };

    // 10. Engine (2-char key)
    private static readonly Dictionary<string, string> Engines = new Dictionary<string, string>
    {
      { "NA", "Standard 2.0" },
      { "NE", "High Performance 2.0" }
    };

    // 11. Gearbox
    private static readonly Dictionary<char, string> Gearboxes = new Dictionary<char, string>
    {
      { '7', "3-spd Automatic" },
      { 'B', "4-spd Manual" }
    };

    // 12. Axle Ratio
    private static readonly Dictionary<char, string> AxleRatios = new Dictionary<char, string>
    {
      { 'S', "3.44" },
      { 'B', "3.75" },
      { 'C', "3.89" },
      { 'N', "4.11" },
      { 'E', "4.44" }
    };

    // 13. Axle Lock
    private static readonly Dictionary<char, string> AxleLocks = new Dictionary<char, string>
    {
      { 'A', "Open" },
      { 'B', "LSD" }
    };

    // 14. Body Colour
    private static readonly Dictionary<char, string> BodyColours = new Dictionary<char, string>
    {
      { 'A', "Dark Grey" },
      { 'B', "Nature White" },
      { 'C', "Sand" },
      { 'D', "Asphalt Grey" },
      { 'E', "Blue" },
      { 'F', "Sun Yellow" },
      { 'G', "Dark Navy" },
      { 'H', "Royal Red" },
      { 'I', "Brown" },
      { 'J', "Red" },
      { 'K', "Electric Green" },
      { 'L', "White Pearl" },
      { 'M', "Spring Green" },
      { 'R', "Purple" },
      { 'T', "Yellow" },
      { 'U', "Sky Blue" },
      { 'V', "Orange" },
      { 'X', "Navy Blue" },
      { 'Y', "Special" }
    };

    // 15. Vinyl Roof
    private static readonly Dictionary<char, string> VinylRoofs = new Dictionary<char, string>
    {
      { '-', "Paint" },
      { 'A', "Black" },
      { 'B', "White" },
      { 'C', "Tan" },
      { 'K', "Light Brown" },
      { 'M', "Dark Brown" }
    };

    // 16. Interior Trim
    private static readonly Dictionary<char, string> InteriorTrims = new Dictionary<char, string>
    {
      { 'N', "Red" },
      { 'A', "Black" },
      { 'K', "Tan" },
      { 'F', "Blue" },
      { 'Y', "Special" }
    };

    // 17. Radio
    private static readonly Dictionary<char, string> Radios = new Dictionary<char, string>
    {
      { '-', "Radio Delete" },
      { 'J', "Radio" }
    };

    // 18. Instrument Panel
    private static readonly Dictionary<char, string> InstrumentPanels = new Dictionary<char, string>
    {
      { '-', "Standard" },
      { 'G', "Clock" },
      { 'M', "Tachometer" }
    };

    // 19. Windshield
    private static readonly Dictionary<char, string> Windshields = new Dictionary<char, string>
    {
      { '1', "Clear" },
      { '2', "Tinted" },
      { 'F', "Sunstrip" }
    };

    // 20. Seats
    private static readonly Dictionary<char, string> SeatTypes = new Dictionary<char, string>
    {
      { '8', "Standard" },
      { 'B', "Bucket Style" }
    };

    // 21. Suspension
    private static readonly Dictionary<char, string> Suspensions = new Dictionary<char, string>
    {
      { 'A', "Standard" },
      { 'B', "Standard + Stiffened" },
      { '4', "Lowered" },
      { 'M', "Lowered + Stiffened" }
    };

    // 22. Brakes
    private static readonly Dictionary<char, string> BrakeTypes = new Dictionary<char, string>
    {
      { '-', "Standard" },
      { 'B', "Power Brakes" }
    };

    // 23. Wheels
    private static readonly Dictionary<char, string> WheelTypes = new Dictionary<char, string>
    {
      { 'A', "13\" Steel" },
      { 'B', "13\" Steel + Hubcaps" },
      { '4', "14\" Sport" },
      { 'M', "14\" Steel / 14\" Octo" }
    };

    // 24. Rear Window
    private static readonly Dictionary<char, string> RearWindows = new Dictionary<char, string>
    {
      { '-', "Standard" },
      { 'B', "Heated" },
      { 'M', "Standard + Window Grille" }
    };

    private static string Lookup(Dictionary<char, string> dict, char key)
    {
      return dict.TryGetValue(key, out string value) ? value : null;
    }

    private static string Lookup(Dictionary<string, string> dict, string key)
    {
      return dict.TryGetValue(key, out string value) ? value : null;
    }

    private static bool AllValid(params string[] values)
    {
      return System.Array.TrueForAll(values, v => v != null);
    }

    public static VinInfo Parse(string vin)
    {
      if (vin == null || vin.Length != VinLength) return null;

      vin = vin.ToUpper();

      string country = Lookup(Countries, vin[0]);
      string assemblyPlant = Lookup(AssemblyPlants, vin[1]);
      string model = Lookup(Models, vin[2]);
      string bodyType = Lookup(BodyTypes, vin[3]);
      string version = Lookup(Versions, vin[4]);
      string year = Lookup(Years, vin[5]);
      string month = Lookup(Months, vin[6]);
      string serialNumber = vin.Substring(7, 5);
      string drive = Lookup(Drives, vin[12]);
      string engine = Lookup(Engines, vin.Substring(13, 2));
      string gearbox = Lookup(Gearboxes, vin[15]);
      string axleRatio = Lookup(AxleRatios, vin[16]);
      string axleLock = Lookup(AxleLocks, vin[17]);
      string bodyColour = Lookup(BodyColours, vin[18]);
      string vinylRoof = Lookup(VinylRoofs, vin[19]);
      string interiorTrim = Lookup(InteriorTrims, vin[20]);
      string radio = Lookup(Radios, vin[21]);
      string instrumentPanel = Lookup(InstrumentPanels, vin[22]);
      string windshield = Lookup(Windshields, vin[23]);
      string seats = Lookup(SeatTypes, vin[24]);
      string suspension = Lookup(Suspensions, vin[25]);
      string brakes = Lookup(BrakeTypes, vin[26]);
      string wheels = Lookup(WheelTypes, vin[27]);
      string rearWindow = Lookup(RearWindows, vin[28]);

      if (!AllValid(country, assemblyPlant, model, bodyType, version, year, month,
                    drive, engine, gearbox, axleRatio, axleLock, bodyColour, vinylRoof,
                    interiorTrim, radio, instrumentPanel, windshield, seats, suspension,
                    brakes, wheels, rearWindow))
        return null;

      return new VinInfo(
        vin, country, assemblyPlant, model, bodyType, version, year, month, serialNumber,
        drive, engine, gearbox, axleRatio, axleLock, bodyColour, vinylRoof, interiorTrim,
        radio, instrumentPanel, windshield, seats, suspension, brakes, wheels, rearWindow
      );
    }
  }
}