using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace AntiRATTool
{
    class Program
    {
        // Color codes for styling
        const string Red = "\u001b[31m";
        const string Green = "\u001b[32m";
        const string Yellow = "\u001b[33m";
        const string Cyan = "\u001b[36m";
        const string Reset = "\u001b[0m";

        // Windows Console Mode Constants
        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        static void Main(string[] args)
        {
            // Enable ANSI color support if running in an appropriate terminal
            EnableANSIColors();

            // Display header and ensure proper start
            DisplayHeader();

            // Ensure administrator rights
            if (!IsAdministrator())
            {
                Console.WriteLine($"{Red}This application must be run as an administrator.{Reset}");
                RestartAsAdmin();
                return;
            }

            // Check for the AntiRat/ProcessHacker2 directory
            string antiRatFolder = @"C:\Program Files\AntiRat";
            string processHackerFolder = Path.Combine(antiRatFolder, "ProcessHacker2");
            string processHackerPath = Path.Combine(processHackerFolder, "ProcessHacker.exe");

            // If the folder or tool does not exist, prompt the user to download
            if (!Directory.Exists(processHackerFolder) || !File.Exists(processHackerPath))
            {
                Console.Write($"{Yellow}Process Hacker is required but not found. Do you agree to download and install it? (yes/no): {Reset}");
                if (Console.ReadLine()?.ToLower() != "yes")
                {
                    Console.WriteLine($"{Red}Required tools not installed. Exiting application.{Reset}");
                    return;
                }

                DownloadAndInstallProcessHacker(antiRatFolder, processHackerFolder);
            }

            // Main loop for program functionality
            while (true)
            {
                Console.Clear();
                DisplayHeader();
                Console.WriteLine($"{Cyan}1. Check Connections{Reset}");
                Console.WriteLine($"{Cyan}2. Process Hacker{Reset}");
                Console.WriteLine($"{Cyan}3. Check Startup{Reset}");
                Console.WriteLine($"{Cyan}4. Exit{Reset}");
                Console.Write($"{Yellow}Choose an option: {Reset}");

                switch (Console.ReadLine())
                {
                    case "1":
                        CheckConnections();
                        break;
                    case "2":
                        RunProcessHacker(processHackerPath);
                        break;
                    case "3":
                        CheckStartup();
                        break;
                    case "4":
                        Console.WriteLine($"{Green}Exiting XYZ AntiRat. Goodbye!{Reset}");
                        return;
                    default:
                        Console.WriteLine($"{Red}Invalid option, try again.{Reset}");
                        break;
                }
                Console.WriteLine($"{Yellow}Press any key to continue...{Reset}");
                Console.ReadKey();
            }
        }

        static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void RestartAsAdmin()
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(startInfo);
            Environment.Exit(0);
        }

        static void EnableANSIColors()
        {
            IntPtr handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (GetConsoleMode(handle, out uint mode))
            {
                SetConsoleMode(handle, mode | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
        }

        static void DownloadAndInstallProcessHacker(string antiRatFolder, string processHackerFolder)
        {
            // Ensure the AntiRat folder exists
            if (!Directory.Exists(antiRatFolder))
            {
                Console.WriteLine($"{Green}Creating AntiRat directory...{Reset}");
                Directory.CreateDirectory(antiRatFolder);
            }

            // Ensure the ProcessHacker2 folder exists
            if (!Directory.Exists(processHackerFolder))
            {
                Console.WriteLine($"{Green}Creating ProcessHacker2 directory...{Reset}");
                Directory.CreateDirectory(processHackerFolder);
            }

            try
            {
                Console.WriteLine($"{Green}Downloading Process Hacker...{Reset}");
                using (var client = new WebClient())
                {
                    // Set the full path for the zip file
                    string zipFilePath = Path.Combine(antiRatFolder, "PH2.zip");

                    // Download Process Hacker
                    client.DownloadFile("https://psychoo.site/files/PH2.zip", zipFilePath);

                    Console.WriteLine($"{Green}Extracting Process Hacker...{Reset}");

                    // Extract the zip file to the target folder
                    ZipFile.ExtractToDirectory(zipFilePath, processHackerFolder);

                    // Delete the zip file after extraction
                    File.Delete(zipFilePath);

                    Console.WriteLine($"{Green}Process Hacker installed successfully.{Reset}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Red}Error during download or extraction: {ex.Message}{Reset}");
            }
        }

        static void CheckConnections()
        {
            Console.WriteLine($"{Yellow}Checking active connections with detailed info...{Reset}");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c netstat -nbf",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }

                using (StreamReader errorReader = process.StandardError)
                {
                    string error = errorReader.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"{Red}Error checking connections: {error}{Reset}");
                    }
                }
            }
        }

        static void RunProcessHacker(string processHackerPath)
        {
            Console.WriteLine($"{Green}Launching Process Hacker...{Reset}");

            // Check if the Process Hacker file exists
            if (File.Exists(processHackerPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = processHackerPath,  // Directly specify the Process Hacker path
                    UseShellExecute = true,        // UseShellExecute is necessary for GUI applications
                    CreateNoWindow = false         // Don't create a new console window
                };

                // Start Process Hacker
                Process.Start(startInfo);
            }
            else
            {
                Console.WriteLine($"{Red}Process Hacker not found at {processHackerPath}. Please check the path or install it.{Reset}");
            }
        }

        static void CheckStartup()
        {
            Console.WriteLine($"{Yellow}Opening startup directories...{Reset}");
            string[] startupDirs = {
                Environment.GetFolderPath(Environment.SpecialFolder.Startup),
                Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup)
            };

            for (int i = 0; i < startupDirs.Length; i++)
            {
                Console.WriteLine($"{i + 1}. {startupDirs[i]}");
            }
            Console.Write($"{Yellow}Choose a folder to open or press Enter to skip: {Reset}");
            string input = Console.ReadLine();

            if (int.TryParse(input, out int choice) && choice > 0 && choice <= startupDirs.Length)
            {
                Process.Start("explorer.exe", startupDirs[choice - 1]);
            }
            else
            {
                Console.WriteLine($"{Red}No valid option chosen.{Reset}");
            }
        }

        static void DisplayHeader()
        {
            Console.WriteLine($"{Green}");
            Console.WriteLine("██╗  ██╗██╗   ██╗███████╗      █████╗ ███╗   ██╗████████╗██╗██████╗ ");
            Console.WriteLine("██║  ██║██║   ██║██╔════╝     ██╔══██╗████╗  ██║╚══██╔══╝██║██╔══██╗");
            Console.WriteLine("███████║██║   ██║███████╗     ███████║██╔██╗ ██║   ██║   ██║██████╔╝");
            Console.WriteLine("██╔══██║██║   ██║╚════██║     ██╔══██║██║╚██╗██║   ██║   ██║██╔═══╝ ");
            Console.WriteLine("██║  ██║╚██████╔╝███████║     ██║  ██║██║ ╚████║   ██║   ██║██║     ");
            Console.WriteLine("╚═╝  ╚═╝ ╚═════╝ ╚══════╝     ╚═╝  ╚═╝╚═╝  ╚═══╝   ╚═╝   ╚═╝╚═╝     ");
            Console.WriteLine($"{Reset}");
            Console.WriteLine($"{Cyan}Welcome to XYZ AntiRat Tool - Protecting Your System{Reset}\n");
        }
    }
}
