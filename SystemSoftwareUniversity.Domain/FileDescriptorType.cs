namespace SystemSoftwareUniversity.Domain
{
    public enum FileDescriptorType : byte
    {
        Unused = 0,
        File = 1,
        Directory = 2,
        Symlink = 3
    }
}