using System.Runtime.InteropServices;

namespace HynusWynus;

class DllManager
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, out int lpThreadId);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    private const uint MEM_COMMIT = 0x1000;
    private const uint PAGE_EXECUTE_READWRITE = 0x40;
    private const uint PROCESS_ALL_ACCESS = 0x1F0FFF;

    public static ThreadInfo StartManager(int processId, string dllPath)
    {
        IntPtr processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
        byte[] dllBytes = File.ReadAllBytes(dllPath);
        IntPtr allocatedMemory = VirtualAllocEx(processHandle, IntPtr.Zero, dllBytes.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
        WriteProcessMemory(processHandle, allocatedMemory, dllBytes, dllBytes.Length, out IntPtr bytesWritten);
        IntPtr loadLibraryAddr = GetProcAddress(LoadLibrary("kernel32.dll"), "LoadLibraryA");
        CreateRemoteThread(processHandle, IntPtr.Zero, 0, loadLibraryAddr, allocatedMemory, 0, out int threadId);
        CloseHandle(processHandle);
        return new ThreadInfo(threadId, bytesWritten);
    }

    public class ThreadInfo
    {
        public ThreadInfo(int ThreadID, IntPtr BytesWritten) { threadId = ThreadID; bytesWritten = BytesWritten; }
        public int threadId;
        public IntPtr bytesWritten;
    }
}