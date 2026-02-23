using MSCLoader;
using UnityEngine;
using I386API;

namespace MP386VINDecoder
{
  public class MP386VINDecoder : Mod
  {
    public override string ID => "MP386VINDecoder"; // Your (unique) mod ID 
    public override string Name => "386 VIN Decoder"; // Your mod name
    public override string Author => "casper-3"; // Name of the Author (your name)
    public override string Version => "1.0.0"; // Version
    public override string Description => "Query Rivett VINs using your 386 computer."; // Short description of your mod
    public override Game SupportedGames => Game.MyWinterCar; //Supported Games
    public override void ModSetup()
    {
      SetupFunction(Setup.OnLoad, Mod_OnLoad);
    }

    private void Mod_OnLoad()
    {
      // Called once, when mod is loading after game is fully loaded
      AssetBundle ab = LoadAssets.LoadBundle(this, "mp386vindecoder.unity3d");

      Vin vin = new Vin(Author, Version);
      Command.Create("vin", vin.CommandEnter, vin.CommandUpdate);
      Diskette diskette = Diskette.Create("vin", new Vector3(-1529.327f, 3.225054f, 1261.304f), new Vector3(270f, 88.48043f, 0f));
      Texture2D texture = ab.LoadAsset<Texture2D>("diskette");
      diskette.SetTexture(texture);

      ab.Unload(false);
    }
  }
}