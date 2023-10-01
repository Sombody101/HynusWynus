using Spectre.Console;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Utils;

namespace HynusWynus.Classes;

internal static class ProcessManager
{
    public static Process[]? Processes { get; private set; }

    [DllImport("dbghelp.dll", SetLastError = true)]
    private static extern bool MiniDumpWriteDump(IntPtr hProcess, uint processId, SafeHandle hFile, uint dumpType,
    IntPtr expParam, IntPtr userStreamParam, IntPtr callbackParam);

    // Print all processes in a table
    public static void PrintProcesses(ProcFilter pf, string searchKey = "")
    {
        RefreshProcessList(pf, searchKey);

        if (Processes is null || Processes.Length is 0)
        {
            Logging.LogError($"Unable to find any processes with the parameter '{searchKey}'");
            return;
        }

        var table = new Table { Border = TableBorder.Rounded };
        table.BorderColor(Color.White);
        AnsiConsole.Live(table).Start(ctx =>
        {
            int cellSpace = Processes.Select(proc => proc.ProcessName).ToArray().GetLongestLength() + 10;
            var load = DivideArray(Processes, Console.BufferWidth / cellSpace);

            string[] output = new string[load[0].Length];
            int count = 0;
            int line = 0;

            for (int round = 0; round < load.Count; round++)
            {
                int prePad = load[round].Length.ToString().Length + 5;
                int pidLen = Processes.Select(proc => proc.Id.ToString()).ToArray().GetLongestLength();
                int siLen = Processes.Select(proc => proc.SessionId.ToString()).ToArray().GetLongestLength();

                cellSpace = load[round].Select(proc => proc.ProcessName).ToArray().GetLongestLength() + prePad + 25; // Add 25 to make up for markup

                if (pf == ProcFilter.ShowAll)
                    cellSpace += pidLen + siLen - 5;

                foreach (string item in load[round].Select(proc => proc.ProcessName).ToArray())
                {
                    output[line] += $"[white][magenta]({((count + 1).ToString() + ")").PadRight(prePad)}[/]{item}[/]".PadRight(cellSpace);
                    
                    if (pf.HasFlag(ProcFilter.IndexingPID) || pf.HasFlag(ProcFilter.ShowAll))
                        output[line] += $"[white]{Processes[count].Id.ToString().PadRight(pidLen + 1)}[/]";

                    if (pf.HasFlag(ProcFilter.IndexingSI) || pf.HasFlag(ProcFilter.ShowAll))
                        output[line] += $"[white]{Processes[count].SessionId.ToString().PadRight(siLen + 1)}[/]";

                    line++;
                    count++;

                    if (line >= output.Length)
                        line = 0;
                }
            }

            table.AddColumns(new TableColumn($"[yellow]Currently running processes {(pf == ProcFilter.ShowAll ? "" : $"with the same SI as {Process.GetCurrentProcess().SessionId}")}[/]" +
                $" ({Processes.Length})\n[yellow]Enter the number to the corresponding process you would like to dump " +
                $"{(pf != ProcFilter.ShowAll ? "(use [magenta]procs -a[/] to show PID and other information)" :
                $"(Any processes with a PID higher than [magenta]{Process.GetCurrentProcess().SessionId}[/] cannot be dumped)\n" +
                $"[green]Format:[/] ProcessName [magenta]PID[/] [darkmagenta]SI[/]")}[/]"));

            foreach (string item in output.Shrink())
                table.AddRow(item);
        });
    }

    public static void Dump(int procIndex, bool procIsIndex = true)
    {
        if (Processes is null)
        {
            Logging.LogError("Please run '[cyan]procs[/]' to get a list of processes to dump, or find their PID and re-run the [cyan]dump[/] command with the switch \"PID\"");
            return;
        }

        bool status;
        Process? proc = null;
        string Path = "";

        try
        {
            if (procIsIndex && procIndex > Processes.Length)
            {
                Logging.LogError($"The value [yellow]{procIndex + 1}[/] is larger than the amount of processes currently loaded in the process list. Check the value and try again.");
                return;
            }

            proc = procIsIndex ? Processes[procIndex] : Process.GetProcesses().Where(_proc => _proc.Id == procIndex).ToArray()[0];

            if (proc is null)
            {
                Logging.LogError((procIsIndex ? $"Unable to find a process with the PID of [yellow]{procIndex}[/]." :
                    "Unable to find process with the index of [yellow]{procID}[/] in process list") + " Check if it has closed.");
                return;
            }
            else
                Logging.Log($"[purple]{proc.ProcessName}[/] found (PID: {procIndex}). Attempting dump...");

            Path = SettingsManager.Settings.DumpFileOutputDirectory + $"\\{proc.ProcessName}.dmp";

            using (FileStream fs = new(Path, FileMode.Create, FileAccess.ReadWrite, FileShare.Write))
            {
                status = MiniDumpWriteDump(proc.Handle, (uint)proc.Id, fs.SafeFileHandle, 2, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            if (status)
            {
                if (new FileInfo(Path).Length is 0)
                {
                    Logging.LogError($"Failed to dump [purple]{proc.ProcessName}[/] : Dump returned true, but dump file is empty");
                    File.Delete(Path);
                }
                else
                    Logging.Log($"[purple]{proc.ProcessName}[/] process dumped successfully and saved at {Path}");
            }
            else
            {
                Logging.LogError($"Failed to dump [purple]{proc.ProcessName}[/] (ExceptionCode: {Marshal.GetExceptionCode()})");
                File.Delete(Path);
            }
        }
        catch (Win32Exception ex)
        {
            if (ex.Message.Contains("Unauth"))
            {
                Logging.WriteException($"Cannot dump [purple]{proc?.ProcessName}[/]", ex);
            }
        }
        catch (Exception ex)
        {
            Logging.WriteException($"Failed to extract [purple]{proc?.ProcessName}[/]", ex);
            File.Delete(Path);
        }
    }

    // Refactor sorting "algorithm" to make slightly faster when dealing with lots of data
    public static void RefreshProcessList(ProcFilter pf, string searchKey = "")
    {
        var proclist = Process.GetProcesses();
        var output = new List<Process>();
        var SI = Process.GetCurrentProcess().SessionId;

        foreach (var proc in proclist)
        {
            // please help me ;-;
            // i hate this
            if (pf == ProcFilter.ShowAll)
                output.Add(proc);
            else if (pf == ProcFilter.None)
            {
                if (proc.SessionId == SI)
                    output.Add(proc);
            }
            else if (pf.HasFlag(ProcFilter.Equals))
            {
                if (pf.HasFlag(ProcFilter.IndexingName))
                {
                    if (proc.ProcessName == searchKey)
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingPID))
                {
                    if (proc.Id.ToString() == searchKey)
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingSI))
                {
                    if (proc.SessionId.ToString() == searchKey)
                        output.Add(proc);
                }
                else if (proc.ProcessName == searchKey)
                    output.Add(proc);
            }
            else if (pf.HasFlag(ProcFilter.StartsWith) && !pf.HasFlag(ProcFilter.EndsWith))
            {
                if (pf.HasFlag(ProcFilter.IndexingName))
                {
                    if (proc.ProcessName.StartsWith(searchKey))
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingPID))
                {
                    if (proc.Id.ToString().StartsWith(searchKey))
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingSI))
                {
                    if (proc.SessionId.ToString().StartsWith(searchKey))
                        output.Add(proc);
                }
                else if (proc.ProcessName.StartsWith(searchKey))
                    output.Add(proc);
            }
            else if (pf.HasFlag(ProcFilter.EndsWith) && !pf.HasFlag(ProcFilter.StartsWith))
            {
                if (pf.HasFlag(ProcFilter.IndexingName))
                {
                    if (proc.ProcessName.EndsWith(searchKey))
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingPID))
                {
                    if (proc.Id.ToString().EndsWith(searchKey))
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingSI))
                {
                    if (proc.SessionId.ToString().EndsWith(searchKey))
                        output.Add(proc);
                }
                else if (proc.ProcessName.EndsWith(searchKey))
                    output.Add(proc);
            }
            else if (pf.HasFlag(ProcFilter.StartsWith | ProcFilter.EndsWith))
            {
                if (pf.HasFlag(ProcFilter.IndexingName))
                {
                    if (proc.ProcessName.Contains(searchKey))
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingPID))
                {
                    if (proc.Id.ToString().Contains(searchKey))
                        output.Add(proc);
                }
                else if (pf.HasFlag(ProcFilter.IndexingSI))
                {
                    if (proc.SessionId.ToString().Contains(searchKey))
                        output.Add(proc);
                }
                else if (proc.ProcessName.Contains(searchKey))
                    output.Add(proc);
            }
        }

        Processes = output.ToArray();
    }

    private static List<Process[]> DivideArray(Process[] array, int numberOfArrays)
    {
        int elementsPerArray = (int)Math.Ceiling((double)array.Length / numberOfArrays);
        List<Process[]> dividedArrays = new();

        for (int i = 0; i < array.Length; i += elementsPerArray)
        {
            Process[] subArray = array.Skip(i).Take(elementsPerArray).ToArray();
            dividedArrays.Add(subArray);
        }

        return dividedArrays;
    }
}

[Flags]
public enum ProcFilter
{
    None = 0,
    ShowAll = 1,
    Equals = 2,
    StartsWith = 4,
    EndsWith = 8,
    IndexingPID = 16,
    IndexingSI = 32,
    IndexingName = 64
}