using System.Collections;
using HutongGames.PlayMaker;
using I386API;
using UnityEngine;

namespace MP386VINDecoder
{
  internal class Vin
  {
    enum ProgramState
    {
      WaitingForInput,
      Connecting,
      UploadingVIN,
      VinVerificationFailed,
      DownloadingInfo,
      ConnectionError,
      DownloadSucessful,
      AnimatingInfo,
      DisplayingInfo,
    }

    private const int ScreenWidth = 80;
    private const int VINInfoRows = 12;
    private ProgramState state;
    private string inputBuffer;
    private string lastErrorMessage;
    private string connectionStatusBuffer, vinInfoBuffer;
    private VinInfo currentVinInfo;
    private Coroutine coroutine;
    private float uploadProgress, downloadProgress;
    string author, version;
    TextMesh bootSequenceTextMesh;
    FsmBool playerComputer;

    public Vin(string author, string version)
    {
      this.author = author;
      this.version = version;
    }

    internal bool CommandEnter()
    {
      if (I386.Args.Length != 1)
      {
        I386.POS_WriteNewLine("ERROR: No arguments needed.");
        return true;
      }
      // Clear POS boot sequence text
      bootSequenceTextMesh = GameObject.Find("COMPUTER").transform.Find("SYSTEM/POS/Text").GetComponent<TextMesh>();
      bootSequenceTextMesh.text = "";

      // Is player sat in front of the computer?
      playerComputer = PlayMakerGlobals.Instance.Variables.FindFsmBool("PlayerComputer");

      state = ProgramState.WaitingForInput;
      inputBuffer = "";
      lastErrorMessage = "";
      currentVinInfo = null;

      return false;
    }

    internal bool CommandUpdate()
    {
      if (I386.GetKey(KeyCode.LeftControl) && I386.GetKeyDown(KeyCode.C))
      {
        if (coroutine != null)
          I386.StopCoroutine(coroutine);
        I386.POS_NewLine();
        coroutine = null;
        return true; // exit
      }

      switch (state)
      {
        case ProgramState.WaitingForInput:
          InputScreen();
          if (!playerComputer.Value)
            break;
            
          foreach (char c in Input.inputString)
          {
            if (c == '\b')
            {
              if (inputBuffer.Length > 0)
                inputBuffer = inputBuffer.Substring(0, inputBuffer.Length - 1);
            }
            else if ((c == '\n') || (c == '\r'))
            {
              if (inputBuffer.Length != VinDB.VinLength)
              {
                lastErrorMessage = $"ERROR: VIN must be exactly {VinDB.VinLength} characters.";
                inputBuffer = "";
                break;
              }
              else
              {
                state = ProgramState.Connecting;
                connectionStatusBuffer = "";
                coroutine = I386.StartCoroutine(RequestVinAsync());
              }
            }
            else
            {
              inputBuffer += c;
            }
          }
          break;
        case ProgramState.Connecting:
        case ProgramState.UploadingVIN:
        case ProgramState.DownloadingInfo:
          ConnectionScreen();
          break;
        case ProgramState.ConnectionError:
          ConnectionScreen();
          if (I386.GetKeyDown(KeyCode.Return))
          {
            state = ProgramState.Connecting;
            connectionStatusBuffer = "";
            coroutine = I386.StartCoroutine(RequestVinAsync());
          }
          break;
        case ProgramState.VinVerificationFailed:
          ConnectionScreen();
          if (I386.GetKeyDown(KeyCode.Return))
          {
            state = ProgramState.WaitingForInput;
            inputBuffer = "";
            lastErrorMessage = "";
          }
          break;
        case ProgramState.DownloadSucessful:
          ConnectionScreen();
          if (I386.GetKeyDown(KeyCode.Return))
          {
            state = ProgramState.AnimatingInfo;
            connectionStatusBuffer = "";
            vinInfoBuffer = new string('\n', VINInfoRows - 1);
            coroutine = I386.StartCoroutine(AnimateInfoScreenAsync(currentVinInfo));
          }
          break;

        case ProgramState.AnimatingInfo:
          InfoScreen(currentVinInfo);
          break;

        case ProgramState.DisplayingInfo:
          InfoScreen(currentVinInfo);
          if (I386.GetKeyDown(KeyCode.Return))
          {
            state = ProgramState.WaitingForInput;
            inputBuffer = "";
            vinInfoBuffer = "";
            currentVinInfo = null;
          }
          break;
      }

      return false;
    }

    private string CenterText(string text, int width)
    {
      if (text.Length >= width)
        return text;

      int leftPadding = (width - text.Length) / 2;
      int rightPadding = width - text.Length - leftPadding;

      return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }

    private void PrintHeader()
    {
      I386.POS_WriteNewLine(CenterText("/======================================\\", ScreenWidth));
      I386.POS_WriteNewLine(CenterText("||   ____ ___  ____  ____  ___ ____   ||", ScreenWidth));
      I386.POS_WriteNewLine(CenterText("||  / ___/ _ \\|  _ \\|  _ \\|_ _/ ___|  ||", ScreenWidth));
      I386.POS_WriteNewLine(CenterText("|| | |  | | | | |_) | |_) || |\\___ \\  ||", ScreenWidth));
      I386.POS_WriteNewLine(CenterText("|| | |__| |_| |  _ <|  _ < | | ___) | ||", ScreenWidth));
      I386.POS_WriteNewLine(CenterText("||  \\____\\___/|_| \\_\\_| \\_\\___|____/  ||", ScreenWidth));
      I386.POS_WriteNewLine(CenterText("\\======================================/", ScreenWidth));
      I386.POS_WriteNewLine(CenterText($"Rivett VIN Decoder v{version}", ScreenWidth));
      I386.POS_WriteNewLine(CenterText($"by {author}", ScreenWidth));
    }

    private void InputScreen()
    {
      I386.POS_ClearScreen();
      PrintHeader();
      I386.POS_NewLine();
      I386.POS_WriteNewLine($"VIN> {inputBuffer}");
      I386.POS_WriteNewLine(lastErrorMessage);
    }

    private void ConnectionScreen()
    {
      I386.POS_ClearScreen();
      PrintHeader();
      I386.POS_NewLine();
      I386.POS_WriteNewLine($"VIN: {inputBuffer.ToUpper()}");
      I386.POS_Write(string.Format(connectionStatusBuffer, uploadProgress * 100, downloadProgress * 100));
      I386.POS_NewLine();
      if (state == ProgramState.ConnectionError)
      {
        I386.POS_WriteNewLine(CenterText("CONNECTION ERROR: Press [ENTER] to retry.", ScreenWidth));
      }
      else if (state == ProgramState.VinVerificationFailed)
      {
        I386.POS_WriteNewLine(CenterText("ERROR: Invalid VIN number, press [ENTER] to type another VIN.", ScreenWidth));
      }
      else if (state == ProgramState.DownloadSucessful)
      {
        I386.POS_WriteNewLine(CenterText("Download finished! Press [ENTER] to view VIN information.", ScreenWidth));
      }
      else
      {
        I386.POS_NewLine();
      }
    }

    private void InfoScreen(VinInfo info)
    {
      I386.POS_ClearScreen();
      I386.POS_WriteNewLine($"VIN: {info.Vin.ToUpper()}");
      I386.POS_WriteNewLine(new string('-', 60));
      I386.POS_WriteNewLine(vinInfoBuffer);
      if (state == ProgramState.DisplayingInfo)
      {
        I386.POS_NewLine();
        I386.POS_WriteNewLine(CenterText("Press [ENTER] to input a new VIN.", ScreenWidth));
      }
      else
      {
        I386.POS_WriteNewLine("\n");
      }
    }

    private static string FormatRow(string label1, string val1, string label2, string val2)
    {
      return $"{label1,-12} {val1,-20}  {label2,-14} {val2}";
    }

    private IEnumerator AnimateInfoScreenAsync(VinInfo info)
    {
      string[] rows = new string[]
      {
        FormatRow("COUNTRY:", info.Country, "AXLE LOCK:", info.AxleLock),
        FormatRow("PLANT:", info.AssemblyPlant, "BODY COLOUR:", info.BodyColour),
        FormatRow("MODEL:", info.Model, "VINYL ROOF:", info.VinylRoof),
        FormatRow("BODY TYPE:", info.BodyType, "INT. TRIM:", info.InteriorTrim),
        FormatRow("VERSION:", info.Version, "RADIO:", info.Radio),
        FormatRow("YEAR:", info.Year, "INSTR. PANEL:", info.InstrumentPanel),
        FormatRow("MONTH:", info.Month, "WINDSHIELD:", info.Windshield),
        FormatRow("SERIAL:", info.SerialNumber, "SEATS:", info.Seats),
        FormatRow("DRIVE:", info.Drive, "SUSPENSION:", info.Suspension),
        FormatRow("ENGINE:", info.Engine, "BRAKES:", info.Brakes),
        FormatRow("GEARBOX:", info.Gearbox, "WHEELS:", info.Wheels),
        FormatRow("AXLE RATIO:", info.AxleRatio, "REAR WINDOW:", info.RearWindow),
      };

      for (int i = 0; i < rows.Length; i++)
      {
        yield return new WaitForSeconds(0.75f);

        vinInfoBuffer = string.Join("\n", rows, 0, i + 1) + new string('\n', VINInfoRows - i - 1);
      }

      state = ProgramState.DisplayingInfo;
    }

    private IEnumerator RequestVinAsync()
    {
      state = ProgramState.Connecting;
      yield return I386.StartCoroutine(ConnectToDatabaseAsync());

      if (state == ProgramState.ConnectionError)
        yield break;

      state = ProgramState.UploadingVIN;
      yield return I386.StartCoroutine(UploadVinAsync());

      if (state == ProgramState.ConnectionError)
        yield break;

      if (state == ProgramState.VinVerificationFailed)
        yield break;

      state = ProgramState.DownloadingInfo;
      yield return I386.StartCoroutine(DownloadInfoAsync());

      if (state == ProgramState.ConnectionError)
        yield break;
    }

    private IEnumerator ConnectToDatabaseAsync()
    {
      connectionStatusBuffer = "Connecting to Corris Database";
      for (int i = 0; i < UnityEngine.Random.Range(4, 7); i++)
      {
        connectionStatusBuffer += ".";
        yield return new WaitForSeconds(.65f);
      }
      connectionStatusBuffer += "\n";

      if (!I386.ModemConnected || !I386.PhoneBillPaid)
      {
        state = ProgramState.ConnectionError;
        yield break;
      }
    }

    private IEnumerator UploadVinAsync()
    {
      uploadProgress = 0f;
      connectionStatusBuffer += "Uploading VIN... [{0:F0}%]\n";
      for (int bytesUploaded = 0; bytesUploaded < VinDB.VinLength; bytesUploaded++)
      {
        if (!I386.ModemConnected || !I386.PhoneBillPaid)
        {
          yield return new WaitForSeconds(2f);
          state = ProgramState.ConnectionError;
          yield break;
        }

        yield return new WaitForSeconds(I386.GetDownloadTime(1) * UnityEngine.Random.Range(1.2f, 1.6f)); // Simulate upload time
        uploadProgress = (float)(bytesUploaded + 1) / VinDB.VinLength;
      }

      yield return new WaitForSeconds(0.8f);

      bool isValid = ParseVin(inputBuffer, out currentVinInfo);
      if (!isValid)
      {
        state = ProgramState.VinVerificationFailed;
        connectionStatusBuffer += "RESPONSE (400): VIN verification failed.\n";
        yield break;
      }

      connectionStatusBuffer += "RESPONSE (200): VIN verified successfully.\n";
      yield return new WaitForSeconds(0.65f);
    }

    private IEnumerator DownloadInfoAsync()
    {
      connectionStatusBuffer += "Downloading VIN information... [{1:F0}%]\n";

      downloadProgress = 0f;
      int bytesToDownload = currentVinInfo.GetSizeInBytes();
      for (int bytesDownloaded = 0; bytesDownloaded < bytesToDownload; bytesDownloaded++)
      {
        if (!I386.ModemConnected || !I386.PhoneBillPaid)
        {
          yield return new WaitForSeconds(2f);
          state = ProgramState.ConnectionError;
          yield break;
        }

        yield return new WaitForSeconds(I386.GetDownloadTime(1) * UnityEngine.Random.Range(1f, 1.4f)); // Simulate download time
        downloadProgress = (float)(bytesDownloaded + 1) / bytesToDownload;
      }

      yield return new WaitForSeconds(0.65f);

      state = ProgramState.DownloadSucessful;
    }

    private bool ParseVin(string vinNumber, out VinInfo info)
    {
      info = VinDB.Parse(vinNumber);

      if (info == null)
      {
        lastErrorMessage = "ERROR: Invalid VIN.";
        return false;
      }

      return true;
    }
  }
}