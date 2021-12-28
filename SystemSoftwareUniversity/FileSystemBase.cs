﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SystemSoftwareUniversity.Domain.Models;

namespace SystemSoftwareUniversity
{
    public abstract class FileSystemBase : IFileSystemCustom, IDisposable
        {
        protected readonly FileStream FsFileStream;

        protected FileSystemBase(FileStream fsFileStream)
            {
            FsFileStream = fsFileStream;
            }

        public abstract Block GetBlock(int index);

        public abstract void SetBlock(int index, Block block);

        public abstract FileMap GetFileMap(int index);

        public abstract void SetFileMap(int index, FileMap fileMap);

        public abstract FileDescriptor GetFileDescriptor(ushort id);

        public abstract void SetFileDescriptor(ushort id, FileDescriptor fileDescriptor);

        public abstract BitArray GetBitmask(int offset, int bytes = 16);

        public abstract void SetBitmask(int offset, BitArray bitArray);

        public abstract void SetBitFree(int index);

        public abstract void UnsetBitFree(int index);
        public abstract void CreateFile(string filename, ushort cwd = 0);

        public abstract void WriteToFile(byte[] file, int fd, int offset, ushort size);

        public abstract byte[] ReadFile(int fd, int offset, ushort size);

        public abstract void UnlinkFile(string filename, ushort cwd = 0);

        public abstract void LinkFile(string existingFileName, string linkName, ushort cwd = 0);

        public abstract FileDescriptor Truncate(string filename, ushort size, ushort cwd = 0);

        public abstract List<DirectoryEntry> DirectoryList(ushort directoryDescriptorId = 0);

        public abstract int OpenFile(string filename, ushort cwd = 0);

        public abstract void CloseFile(int fd);
        public abstract void MakeDirectory(string directoryName, ushort cwd = 0);

        public abstract void RemoveDirectory(string directoryName, ushort cwd = 0);

        public abstract void CreateSymlink(string path, string payload, ushort cwd = 0);

        public abstract FileDescriptor LookUp(string path,
            ushort cwd = 0,
            bool resolveSymlink = true,
            int symlinkMaxCount = FileSystemSettings.MaxSymlinkInOneLookup);

        public void Dispose()
            {
            FsFileStream?.Dispose();
            }
        }
    }