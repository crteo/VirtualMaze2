using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// MonoBehaviour that affects VirtualMaze globally.
/// </summary>
public class GameController : MonoBehaviour {
    //UPDATE THESE WITH EACH COMPILATION
    public static readonly int versionNum = 4;
    public static readonly string versionInfo = "20171221 Taxi Continuous v" + versionNum;
    public static readonly string pportInfo = "v" + versionNum;

    [SerializeField]
    private ScreenSaver saver = null;

    private bool generationComplete = false;

    private string SessionPattern = "[Ss]ession[0-9]{2}";
    private string DayPattern = "[0-9]{8}";

    private string eyelinkMatFile = $"{Path.DirectorySeparatorChar}eyelink.mat";
    
    // Modified to support both .mat and .txt session files
    private string unityfileMatFile = $"{Path.DirectorySeparatorChar}unityfile.mat";
  
    
    private string resultFile = $"{Path.DirectorySeparatorChar}unityfile_eyelink_new.csv";

    private static GameController _instance;
    public static GameController instance {
        get {
            if (_instance == null) {
                _instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
                if (_instance == null) {
                    Debug.LogError("need at least one GameController");
                }
            }
            return _instance;
        }
    }

    private void Update() {
        ProcessKeyPress();
    }

    //framerate dependent
    private readonly int pressDelay = 10;
    private int counter = 10;

    private void ProcessKeyPress() {
        if (!Application.isBatchMode && Input.GetKey(KeyCode.Escape)) {
            if (counter > 0) {
                counter--;
            }
            else {
                Application.Quit();
            }
        }

        if (Input.GetKeyUp(KeyCode.Escape)) {
            counter = pressDelay;
        }
    }

    private void Start() {
        //Online sources says that if vSyncCount != 0, targetFrameRate will be ignored.
        if (!Application.isEditor) {
            Application.targetFrameRate = 30;
            /* if display is 60hz, Unity will run at 30hz */
            QualitySettings.vSyncCount = 1;
        }

        if (Application.isBatchMode) {
            BatchModeLogger logger = new BatchModeLogger(PresentWorkingDirectory);

            string[] args = Environment.GetCommandLineArgs();
            bool isSessionList = false;

            int numofLengthBins = BinMapper.DEFAULT_NUM_BIN_LENGTH;
            int radius = BinWallManager.Default_Radius;
            int density = BinWallManager.Default_Density;
            string sessionListPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                Debug.LogError($"ARG {i}: {args[i]}");
                switch (args[i].ToLower())
                {
                    case "-sessionlist":
                        isSessionList = true;
                        Debug.LogError($"Session List detected!");
                        Debug.LogError($"{args[i + 1]}");
                        sessionListPath = args[i + 1];
                        break;

                    case "-numOfLengthBins":
                        if (int.TryParse(args[i + 1], out numofLengthBins))
                        {
                            Debug.LogError($"Setting number of length bins to : {numofLengthBins}");
                        }
                        else
                        {
                            Debug.LogError($"Unable to parse {args[i + 1]} to integer, using  {numofLengthBins} as default");
                        }
                        break;
                    case "-density":
                        if (int.TryParse(args[i + 1], out density))
                        {
                            Debug.LogError($"Setting density to : {density}");
                        }
                        else
                        {
                            Debug.LogError($"Unable to parse {args[i + 1]} to integer, using  {density} as default");
                        }
                        break;
                    case "-radius":
                        if (int.TryParse(args[i + 1], out radius))
                        {
                            Debug.LogError($"Setting radius to : {radius}");
                        }
                        else
                        {
                            Debug.LogError($"Unable to parse {args[i + 1]} to integer, using  {radius} as default");
                        }
                        break;
                }
       
            }

            Queue<DirectoryInfo> dirQ = new Queue<DirectoryInfo>();

            if (!isSessionList)
            {
                PwdMode(logger, dirQ);
                Debug.Log("isSessionList: False");
            }
            else
            {
                SessionListMode(logger, sessionListPath, dirQ);
            }
            Debug.LogError($"Present Working Directory: {PresentWorkingDirectory}");
            Debug.LogError($"Running in {(isSessionList ? "Session List" : "PWD")} Mode");
            Debug.LogError($"Directory queue count after population: {dirQ.Count}"); 
            BinWallManager.ReconfigureGazeOffsetCache(radius, density);
            ProcessExperimentQueue(dirQ, logger, numofLengthBins);
        }
    }

    private void SessionListMode(BatchModeLogger logger, string listPath, Queue<DirectoryInfo> dirQ) {
        using (StreamReader reader = new StreamReader(listPath)) {

            while (reader.Peek() > 0) {
                DirectoryInfo dir = new DirectoryInfo(reader.ReadLine());
                dirQ.Enqueue(dir);
            }
        }
    }

    private void PwdMode(BatchModeLogger logger, Queue<DirectoryInfo> dirQ) {
        DirectoryInfo pwd = new DirectoryInfo(PresentWorkingDirectory);
        Debug.LogError($"PWD Directory: {pwd.FullName}");
        Debug.LogError($"PWD exists: {pwd.Exists}");
        Debug.LogError($"PWD name: '{pwd.Name}' | IsDay: {IsDayDir(pwd)} | IsSession: {IsSessionDir(pwd)}");

        // List all subdirectories
        if (pwd.Exists) {
            DirectoryInfo[] subDirs = pwd.GetDirectories();
            Debug.LogError($"Found {subDirs.Length} subdirectories in PWD:");
            foreach (DirectoryInfo subDir in subDirs) {
                Debug.LogError($"  '{subDir.Name}' | IsDay: {IsDayDir(subDir)} | IsSession: {IsSessionDir(subDir)}");
            }
        }
        dirQ.Enqueue(pwd);
    }

    private void ProcessExperimentQueue(Queue<DirectoryInfo> dirQ, BatchModeLogger logger, int numOfBinsForFloorLength) {
        Queue<string> sessionQ = new Queue<string>();

        while (dirQ.Count > 0)
        {
            DirectoryInfo dir = dirQ.Dequeue();
            Debug.LogError($"Processing: '{dir.Name}' | Exists: {dir.Exists} | IsDay: {IsDayDir(dir)} | IsSession: {IsSessionDir(dir)}");
            if (IsDayDir(dir))
            {
                IEnumerable<string> subDirs = Directory.EnumerateDirectories(dir.FullName, "*", SearchOption.TopDirectoryOnly);
                foreach (string subDir in subDirs)
                {
                    if (IsSessionDir(new DirectoryInfo(subDir)))
                    {
                        logger.Print($"Queuing {subDir}");
                        sessionQ.Enqueue(subDir);
                    }
                }
            }
            else if (IsSessionDir(dir))
            {
                logger.Print($"Queuing {dir}");
                Debug.LogError("IsSessionDir!");
                sessionQ.Enqueue(dir.FullName);
            }
        }
        Debug.LogError($"Regex patterns - Day: '{DayPattern}' | Session: '{SessionPattern}'");
        

        BinMapper mapper = new DoubleTeeBinMapper(numOfBinsForFloorLength);

        if (sessionQ.Count > 0) {
            logger.Print($"{sessionQ.Count} sessions to be processed");
            Debug.LogError($"{sessionQ.Count} sessions to be processed");
            ProcessSession(sessionQ, logger, mapper);
        }
        else {
            logger.Print("No Session directories found! Exiting");
            Debug.LogError($"No Session directories found! Exiting");
            logger.Dispose();
            Application.Quit();
        }
    }

    /// <summary>
    /// Determines the appropriate session file path based on what's available in the directory.
    /// Prioritizes .txt files over .mat files to match GUI behavior.
    /// </summary>
    /// <param name="sessionDir">Directory path containing session files</param>
    /// <returns>Full path to the session file, or null if none found</returns>
    private string GetSessionFilePath(string sessionDir) {
        // Look for any session*.txt file
        // Assume that session*.txt is stored in RawData*.txt, and sessions.list provided to VirtualMaze2 contains the path to session*.txt files
        string[] txtFiles = Directory.GetFiles(sessionDir, "session*.txt");
        
        if (txtFiles.Length > 0) {
            Debug.LogError($"Found .txt session file: {txtFiles[0]}");
            return txtFiles[0];
        }
        string parentDir = Path.GetDirectoryName(sessionDir); // need to navigate out by one level
        string matPath = parentDir + unityfileMatFile;
        if (File.Exists(matPath)) {
            Debug.LogError($"Found .mat session file: {matPath}");
            return matPath;
        }
        
        Debug.LogError($"No session file found in {sessionDir}");
        return null;
    }

    private async void ProcessSession(Queue<string> sessions, BatchModeLogger logger, BinMapper mapper) {
        string path;
        int total = sessions.Count;
        int count = 1, notifyAliveCount = 0;

        while (sessions.Count > 0) {
            path = sessions.Dequeue();
            logger.Print($"Starting({count}/{total}): {path}");
            Debug.LogError($"Starting({count}/{total}): {path}");
            
            // Use the new method to determine session file path
            string sessionFilePath = GetSessionFilePath(path);
            if (sessionFilePath == null) {
                logger.Print($"Failed: No valid session file found in {path}");
                count++;
                continue;
            }
            
            string eyelinkFilePath = path + eyelinkMatFile;
            
            logger.Print($"Path: {path}");
            logger.Print($"Session file: {sessionFilePath}");
            logger.Print($"Eyelink file: {eyelinkFilePath}");

            StartCoroutine(ProcessWrapper(sessionFilePath, eyelinkFilePath, path, mapper));
            while (!generationComplete) {
                await Task.Delay(10000); //10 second notify-alive message

                notifyAliveCount++;
                notifyAliveCount %= 6; //only print message every 60 seconds
                if (notifyAliveCount == 0) {
                    logger.Print($"{saver.progressBar.value * 100}%: Data Generation is still running. {DateTime.Now.ToString()}");
                }
            }
            if (File.Exists(path + resultFile)) {
                logger.Print($"Success: {path + resultFile}");
            }
            else {
                logger.Print($"File.Exists(path: {path} + resultFile: {resultFile}): {File.Exists(path + resultFile)}");
                logger.Print($"Failed: {path + resultFile}. Add to the command \"-logfile <log file location>.txt\" to debug");
            }
            count++;
        }
        logger.Print("BatchMode Complete! Exiting VirtualMaze.");
        logger.Dispose();
        Application.Quit();
    }

    private string PresentWorkingDirectory { get => Path.GetFullPath("."); }

    private bool IsDayDir(DirectoryInfo dirInfo) {
        return Regex.IsMatch(dirInfo.Name, DayPattern);
    }

    private bool IsSessionDir(DirectoryInfo dirInfo) {
        return Regex.IsMatch(dirInfo.Name, SessionPattern);
    }

    private IEnumerator ProcessWrapper(string sessionPath, string edfPath, string toFolderPath, BinMapper mapper) {
        Debug.LogError("ProcessWrapper Running");
        Debug.LogError($"session: {sessionPath}");
        Debug.LogError($"edf: {edfPath}");
        Debug.LogError($"toFolder: {toFolderPath}");

        generationComplete = false;
        try {
            yield return saver.ProcessSessionDataTask(sessionPath, edfPath, toFolderPath, mapper);
        }
        finally { //so that the batchmode app will quit or move on the the next session
            generationComplete = true;
            Debug.LogError("Generation Complete");
        }
    }
}