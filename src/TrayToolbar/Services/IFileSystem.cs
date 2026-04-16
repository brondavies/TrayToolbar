namespace TrayToolbar.Services;

internal interface IFileSystem
{
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    string ReadAllText(string path);
    void WriteAllText(string path, string contents);
    void DeleteFile(string path);
    void MoveFile(string sourceFileName, string destFileName);
    IEnumerable<string> EnumerateFileSystemEntries(string path);
}