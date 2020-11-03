using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace AdventOfCode
{
    public class MainWindowViewModel : BaseViewModel
    {
        public IEnumerable<DayAttribute> Days { get; set; }
        public DayAttribute SelectedDay { get; set; }
        public string InputText { get; set; }
        public string OutputText { get; set; }
        public string TestName { get; set; }

        public MainWindowViewModel()
        {
            RunPartOneCommand = new RelayCommand(x => RunDay(1), x => CanRun());
            RunPartTwoCommand = new RelayCommand(x => RunDay(2), x => CanRun());
            CreateTestCommand = new RelayCommand(x => CreateTest(), x => CanCreateTest());

            Days = GetDayAttributes().ToList();
            SelectedDay = GetDefaultDay();
        }

        public ICommand RunPartOneCommand { get; set; }
        public ICommand RunPartTwoCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }

        private DayAttribute GetDefaultDay()
        {
            return Days.FirstOrDefault(d => d.Year == AdventConfig.DefaultYear && d.Day == AdventConfig.DefaultDay);
        }

        private bool CanRun()
        {
            return SelectedDay != null;
        }

        private bool CanCreateTest()
        {
            return !string.IsNullOrWhiteSpace(TestName);
        }

        private void CreateTest()
        {
            //if (!IsTestNameUnique(TestName, SelectedDay.Year, SelectedDay.Day))
            //{
            //    LogMessage($"Create Test Failed, a test with then name {TestName} already exists for {SelectedDay.Year}.{SelectedDay.Day}");
            //}


        }

        private void RunDay(int part)
        {
            StartNewLog(SelectedDay.Year, SelectedDay.Day);

            var input = GetInput(SelectedDay);

            LogMessage($"====== Input ======");
            LogMessage(input);
            LogMessage($"======= Running Day {SelectedDay.Year}.{SelectedDay.Day} Part {part} =======");

            var day = GetDayInstance(SelectedDay);

            // TODO: disable buttons and show spinner

            var dayThread = new DayThreadHarness(input, day, part, DayComplete);
            var thread = new Thread(new ThreadStart(dayThread.Run));
            thread.Start();
        }

        private void DayComplete(string result, long elapsed)
        {
            LogMessage($"========= DONE (Elapsed: {elapsed}ms) =========");
            LogMessage($"ANSWER: {result}");

            Clipboard.SetText(result);
        }

        private class DayThreadHarness
        {
            private string _input;
            private BaseDay _day;
            private int _part;
            private Action<string, long> _dayCompleteCallback;

            public DayThreadHarness(string input, BaseDay day, int part, Action<string, long> dayCompleteCallback)
            {
                _input = input;
                _day = day;
                _part = part;
                _dayCompleteCallback = dayCompleteCallback;
            }

            public void Run()
            {
                string result;
                var start = Stopwatch.StartNew();

                if (_part == 1)
                {
                    result = _day.PartOne(_input);
                }
                else
                {
                    result = _day.PartTwo(_input);
                }

                var end = start.ElapsedMilliseconds;

                Application.Current.Dispatcher.Invoke(() => _dayCompleteCallback(result, end));
            }
        }

        private string GetInput(DayAttribute day)
        {
            if (InputText?.Trim().Length > 0)
            {
                return InputText;
            }

            Directory.CreateDirectory(AdventConfig.InputFileFolder);

            var inputFileName = $"{day.ToString()}.txt";
            var inputFile = Path.Combine(AdventConfig.InputFileFolder, inputFileName);

            if (!File.Exists(inputFile))
            {
                DownloadInput(day, inputFile);
            }

            InputText = File.ReadAllText(inputFile);
            NotifyChange(nameof(InputText));

            return InputText;
        }

        private void DownloadInput(DayAttribute day, string inputFile)
        {
            var url = $"https://adventofcode.com/{day.Year}/day/{day.Day}/input";

            if (!File.Exists(AdventConfig.SessionCookieFile))
            {
                throw new ArgumentException($"Cannot find Session Cookie file, please check AdventConfig.cs [{AdventConfig.SessionCookieFile}]");
            }

            var sessionCookie = File.ReadAllText(AdventConfig.SessionCookieFile);

            using var client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Cookie, sessionCookie);
            client.DownloadFile(url, inputFile);
        }

        private BaseDay GetDayInstance(DayAttribute day)
        {
            var dayType = GetDayType(day);

            var instance = (BaseDay)Activator.CreateInstance(dayType);
            instance.Log = LogMessage;

            return instance;
        }

        private IEnumerable<DayAttribute> GetDayAttributes()
        {
            return GetDayTypes().Select(t => t.GetCustomAttribute<DayAttribute>())
                                .OrderBy(a => a.Year)
                                .ThenBy(a => a.Day);
        }

        private IEnumerable<Type> GetDayTypes()
        {
            return Assembly.GetExecutingAssembly()
                           .ExportedTypes
                           .Where(t => t.IsSubclassOf(typeof(BaseDay)))
                           .Where(t => t.CustomAttributes.Any(a => a.AttributeType == typeof(DayAttribute)));
        }

        private Type GetDayType(DayAttribute day)
        {
            return GetDayTypes().Single(t => t.GetCustomAttribute<DayAttribute>().Year == day.Year &&
                                             t.GetCustomAttribute<DayAttribute>().Day == day.Day);
        }

        private void LogMessage(string message)
        {
            var timestamp = $"[{DateTime.Now.ToString("hh:mm:ss")}] ";
            var logMsg = message.Replace("\n", $"\n{timestamp}");
            logMsg = $"{timestamp}{logMsg}";

            if (!string.IsNullOrWhiteSpace(OutputText))
            {
                logMsg = Environment.NewLine + logMsg;
            }

            OutputText += logMsg;

            if (AdventConfig.LogFilesEnabled)
            {
                _logWriter.Write(logMsg);
            }

            NotifyChange(nameof(OutputText));
        }

        private StreamWriter _logWriter;

        private void StartNewLog(int year, int day)
        {
            if (AdventConfig.LogFilesEnabled)
            {
                var fileName = $"{DateTime.Now.ToString("yyyyMMddHHmmssffff")}-{year}.{day}.log";
                var path = Path.Combine(AdventConfig.LogFileFolder, fileName);

                if (_logWriter != null)
                {
                    _logWriter.Close();
                }

                Directory.CreateDirectory(AdventConfig.LogFileFolder);
                _logWriter = File.CreateText(path);
                _logWriter.AutoFlush = true;
            }

            OutputText = string.Empty;
            NotifyChange(nameof(OutputText));
        }
    }
}
