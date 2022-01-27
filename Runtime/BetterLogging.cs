using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FortySevenE
{
    public class UnityLogMessage
    {
        public string condition;
        public string stackTrace;
        public LogType type;

        public UnityLogMessage(string condition, string stackTrace, LogType type)
        {
            this.condition = condition;
            this.stackTrace = stackTrace;
            this.type = type;
        }
    }

    public enum LogLevel
    {
        Verbose,
        Info,
        Warning,
        Error
    }

    public class BetterLogging : MonoBehaviour
    {
        [SerializeField] private bool _keepLogFiles = true;
        [SerializeField] private LogLevel _minEditorLogLevel = LogLevel.Verbose;
        [SerializeField] private LogLevel _minBuildLogLevel = LogLevel.Info;

        public static readonly string DateTimeParseFormat = "yyyy_MM_dd_T_HH_mm_ss_fff";
        private static BetterLogging _instance;
        private static object _lock = new object();

        public string CurrentLogFileName { get; private set; }
        public string CurrentLogPath { get; private set; }
        public StreamWriter LoggingFileStream { get; private set; }

        private ConcurrentQueue<UnityLogMessage> _logConcurrentQueue;

        static public void Log(string message, LogLevel logLevel = LogLevel.Info, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var minLogLevel = LogLevel.Info;
            if (_instance != null)
            {
                minLogLevel = Application.isEditor? _instance._minEditorLogLevel: _instance._minBuildLogLevel;
            }
            if (!Application.isPlaying || logLevel >= minLogLevel)
            {
                string prefix = $"[{Path.GetFileNameWithoutExtension(sourceFilePath)}->{memberName}:{sourceLineNumber})]";
                switch(logLevel)
                {
                    case LogLevel.Verbose:
                        goto case LogLevel.Info;
                    case LogLevel.Info:
                        Debug.Log($"{prefix} {message}");
                        break;
                    case LogLevel.Warning:
                        Debug.LogWarning($"{prefix} {message}");
                        break;
                    case LogLevel.Error:
                        Debug.LogError($"{prefix} {message}");
                        break;
                }
            }
        }

        static private BetterLogging GetInstance()
        {
            lock (_lock)
            {
                if (_instance == null)
                {
                    // Search for existing instance.
                    _instance = FindObjectOfType<BetterLogging>();

                    // Create new instance if one doesn't already exist.
                    if (_instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        _instance = singletonObject.AddComponent<BetterLogging>();
                        singletonObject.name = "BetterLogging";
                        Debug.Log("<b>A BetterLogging instance was added to the scene.</b>");
                    }

                    if (_instance != null)
                    {
                        if (Application.isPlaying)
                        {
                            // Make instance persistent.
                            DontDestroyOnLoad(_instance);
                        }
                    }
                }

                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(_instance);
            }
            else
            {
                Debug.LogWarning($"Another instance of BetterLogging detected on GO {name}. Destroyed", gameObject);
                Destroy(this);
            }

            if (_keepLogFiles)
            {
                _logConcurrentQueue = new ConcurrentQueue<UnityLogMessage>();

                CurrentLogFileName = Application.productName + "_" + DateTime.Now.ToString(DateTimeParseFormat) + ".txt";
                string logDirectory = Path.Combine(Application.persistentDataPath, "Logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                CurrentLogPath = Path.Combine(logDirectory, CurrentLogFileName);

                if (!File.Exists(CurrentLogPath))
                {
                    // Create a file to write to.
                    using (StreamWriter sw = File.CreateText(CurrentLogPath))
                    {
                        sw.WriteLine("App Name: " + Application.productName);
                        sw.WriteLine("App Version: " + Application.version);
                        sw.WriteLine("Unity Version: " + Application.unityVersion);
                        sw.WriteLine("CPU: " + SystemInfo.processorType);
                        sw.WriteLine("GPU: " + SystemInfo.graphicsDeviceName);
                        sw.WriteLine("Memory: " + SystemInfo.systemMemorySize);
                        sw.WriteLine(Environment.NewLine);
                    }
                }
            }
        }

        private void OnEnable()
        {
            if (_keepLogFiles)
            {
                Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
            }
        }

        private void OnDisable()
        {
            if (_keepLogFiles)
            {
                Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
            }
        }

        private void LateUpdate()
        {
            if (!_keepLogFiles)
            {
                return;
            }

            if (_logConcurrentQueue.Count > 0)
            {
                using (LoggingFileStream = File.AppendText(CurrentLogPath))
                {
                    UnityLogMessage logMessage = default;
                    while (_logConcurrentQueue.TryDequeue(out logMessage))
                    {
                        LoggingFileStream.Write($"[{DateTime.Now.ToString(DateTimeParseFormat)}] " + logMessage.type.ToString() + Environment.NewLine + logMessage.condition + Environment.NewLine + logMessage.stackTrace + Environment.NewLine);
                    }
                }
            }
        }

        private void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            //Add the log from different threads to the main thread, so it can be write to the files without the locking issue
            _logConcurrentQueue.Enqueue(new UnityLogMessage(condition, stackTrace, type));
        }
    }

}