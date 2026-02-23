using System.Collections;
using System.ComponentModel;
using I386API;
using MSCLoader;
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
      DisplayingInfo,
    }


    private ProgramState state;
    private string inputBuffer;
    private string lastErrorMessage;
    private string connectionStatusBuffer;
    private VinInfo currentVinInfo;
    private Coroutine coroutine;
    private float uploadProgress, downloadProgress;
    string author, version;
    TextMesh bootSequenceTextMesh;

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
            state = ProgramState.DisplayingInfo;
            connectionStatusBuffer = "";
          }
          break;

        case ProgramState.DisplayingInfo:
          InfoScreen(currentVinInfo);
          if (I386.GetKeyDown(KeyCode.Return))
          {
            state = ProgramState.WaitingForInput;
            inputBuffer = "";
            currentVinInfo = null;
          }
          break;
      }

      return false;
    }

    private string PrependTabs(int count, string text)
    {
      return new string('\t', count) + text;
    }

    private void PrintHeader()
    {
      I386.POS_WriteNewLine(PrependTabs(4, "/======================================\\"));
      I386.POS_WriteNewLine(PrependTabs(4, "||   ____ ___  ____  ____  ___ ____   ||"));
      I386.POS_WriteNewLine(PrependTabs(4, "||  / ___/ _ \\|  _ \\|  _ \\|_ _/ ___|  ||"));
      I386.POS_WriteNewLine(PrependTabs(4, "|| | |  | | | | |_) | |_) || |\\___ \\  ||"));
      I386.POS_WriteNewLine(PrependTabs(4, "|| | |__| |_| |  _ <|  _ < | | ___) | ||"));
      I386.POS_WriteNewLine(PrependTabs(4, "||  \\____\\___/|_| \\_\\_| \\_\\___|____/  ||"));
      I386.POS_WriteNewLine(PrependTabs(4, "\\======================================/"));
      I386.POS_WriteNewLine(PrependTabs(4, $"        Rivett VIN Decoder v{version}"));
      I386.POS_WriteNewLine(PrependTabs(4, $"               by {author}"));
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
      if (state == ProgramState.ConnectionError)
      {
        I386.POS_NewLine();
        I386.POS_WriteNewLine(PrependTabs(4, "CONNECTION ERROR: Press [ENTER] to retry."));
      }
      else if (state == ProgramState.VinVerificationFailed)
      {
        I386.POS_NewLine();
        I386.POS_WriteNewLine(PrependTabs(2, "ERROR: Invalid VIN number, press [ENTER] to type another VIN."));
      }
      else if (state == ProgramState.DownloadSucessful)
      {
        I386.POS_NewLine();
        I386.POS_WriteNewLine(PrependTabs(2, "Download finished! Press [ENTER] to view VIN information."));
      }
    }

    private void InfoScreen(VinInfo info)
    {
      I386.POS_ClearScreen();
      I386.POS_WriteNewLine($"VIN: {info.Vin.ToUpper()}");
      I386.POS_WriteNewLine(new string('-', 58));
      I386.POS_WriteNewLine(FormatRow("COUNTRY:", info.Country, "AXLE LOCK:", info.AxleLock));
      I386.POS_WriteNewLine(FormatRow("PLANT:", info.AssemblyPlant, "BODY COLOUR:", info.BodyColour));
      I386.POS_WriteNewLine(FormatRow("MODEL:", info.Model, "VINYL ROOF:", info.VinylRoof));
      I386.POS_WriteNewLine(FormatRow("BODY TYPE:", info.BodyType, "INT. TRIM:", info.InteriorTrim));
      I386.POS_WriteNewLine(FormatRow("VERSION:", info.Version, "RADIO:", info.Radio));
      I386.POS_WriteNewLine(FormatRow("YEAR:", info.Year, "INSTR. PANEL:", info.InstrumentPanel));
      I386.POS_WriteNewLine(FormatRow("MONTH:", info.Month, "WINDSHIELD:", info.Windshield));
      I386.POS_WriteNewLine(FormatRow("SERIAL:", info.SerialNumber, "SEATS:", info.Seats));
      I386.POS_WriteNewLine(FormatRow("DRIVE:", info.Drive, "SUSPENSION:", info.Suspension));
      I386.POS_WriteNewLine(FormatRow("ENGINE:", info.Engine, "BRAKES:", info.Brakes));
      I386.POS_WriteNewLine(FormatRow("GEARBOX:", info.Gearbox, "WHEELS:", info.Wheels));
      I386.POS_WriteNewLine(FormatRow("AXLE RATIO:", info.AxleRatio, "REAR WINDOW:", info.RearWindow));
      I386.POS_NewLine();
      I386.POS_WriteNewLine(PrependTabs(5, "Press [ENTER] to input a new VIN."));
    }

    private static string FormatRow(string label1, string val1, string label2, string val2)
    {
      return $"{label1,-12} {val1,-20}  {label2,-14} {val2}";
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
      for (int i = 0; i < Random.Range(4, 7); i++)
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
      connectionStatusBuffer += "Uploading VIN... [{0:F2}%]\n";
      for (int bytesUploaded = 0; bytesUploaded < VinDB.VinLength; bytesUploaded++)
      {
        if (!I386.ModemConnected || !I386.PhoneBillPaid)
        {
          state = ProgramState.ConnectionError;
          yield break;
        }

        yield return new WaitForSeconds(I386.GetDownloadTime(1)); // Simulate upload time
        uploadProgress = (float)(bytesUploaded + 1) / VinDB.VinLength;
      }

      bool isValid = ParseVin(inputBuffer, out currentVinInfo);
      if (!isValid)
      {
        state = ProgramState.VinVerificationFailed;
        connectionStatusBuffer += "RESPONSE (400): VIN verification failed.\n";
        yield break;
      }
    }

    private IEnumerator DownloadInfoAsync()
    {
      downloadProgress = 0f;
      connectionStatusBuffer += "RESPONSE (200): VIN verified successfully.\n";
      connectionStatusBuffer += "Downloading VIN information... [{1:F2}%]\n";
      int bytesToDownload = currentVinInfo.GetSizeInBytes();
      for (int bytesDownloaded = 0; bytesDownloaded < bytesToDownload; bytesDownloaded++)
      {
        if (!I386.ModemConnected || !I386.PhoneBillPaid)
        {
          state = ProgramState.ConnectionError;
          yield break;
        }

        yield return new WaitForSeconds(I386.GetDownloadTime(1)); // Simulate download time
        downloadProgress = (float)(bytesDownloaded + 1) / bytesToDownload;
      }

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