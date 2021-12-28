namespace SystemSoftwareUniversity
{
    public interface IFileSystemProvider
    {
        IFileSystemCustom CreateNewFileSystem(string fsName, ushort descriptorsCount);

        IFileSystemCustom MountExistingFileSystem(string fsName);
    }
}