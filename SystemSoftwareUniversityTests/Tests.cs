﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using SystemSoftwareUniversity;
using SystemSoftwareUniversity.Domain;
using Xunit;
using Xunit.Abstractions;

namespace SystemSoftwareUniversityTests
{
    public class Tests
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFileSystemCustom _fs;
        private readonly byte[] _loremBytes;
        private readonly FileSystemProvider _fileSystemProvider;
        private const string FsName = "TestFs";

        public Tests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            using var file = File.Open("lorem.txt", FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(file);
            _loremBytes = reader.ReadBytes(Convert.ToInt32(file.Length));
            _fileSystemProvider = new FileSystemProvider();
            _fs = _fileSystemProvider.CreateNewFileSystem(FsName, 100);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(512)]
        [InlineData(2000)]
        [InlineData(756)]
        [InlineData(1234)]
        [InlineData(6723)]
        [InlineData(5432)]
        [InlineData(1452)]
        [InlineData(529)]
        [InlineData(4318)]
        [InlineData(4406)]
        public void WithOffsetInDescriptorAndMap(int offset)
        {
            _fs.CreateFile($"test{offset}");
            _fs.Truncate($"test{offset}", Convert.ToUInt16(_loremBytes.Length));
            var fd = _fs.OpenFile($"test{offset}");
            var dataToWrite = _loremBytes.Skip(offset).ToArray();
            _fs.WriteToFile(dataToWrite, fd, offset, Convert.ToUInt16(_loremBytes.Length - offset));

            _fs.CloseFile(fd);
            fd = _fs.OpenFile($"test{offset}");
            var res = _fs.ReadFile(fd, offset, Convert.ToUInt16(_loremBytes.Length - offset));
            _fs.CloseFile(fd);
            var text1 = Encoding.Default.GetString(dataToWrite);
            var text2 = Encoding.Default.GetString(res);

            Assert.True(dataToWrite.SequenceEqual(res));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(7)]
        [InlineData(10)]
        public void MultipleFiles(int filesCount)
        {
            var random = new Random();
            var counter = 0;
            var fileParams = Enumerable
                .Repeat(0, filesCount)
                .Select(_ => new
                {
                    offset = Convert.ToInt32(random.NextDouble() * 4000),
                    value = Convert.ToUInt16(random.NextDouble() * 4000),
                    filename = $"file-{counter++}"
                })
                .ToList();
            var files = fileParams.Select(p => _loremBytes.Skip(p.offset).Take(p.value).ToArray()).ToList();
            fileParams.Zip(files).ToList().ForEach(f =>
            {
                var (fileParams, file) = f;
                _fs.CreateFile(fileParams.filename);
                _fs.Truncate(fileParams.filename, Convert.ToUInt16(fileParams.offset + fileParams.value));
                var fd = _fs.OpenFile(fileParams.filename);
                _fs.WriteToFile(file, fd, fileParams.offset, Convert.ToUInt16(file.Length));
                _fs.CloseFile(fd);
            });

            var result = fileParams.Zip(files).ToList().Select(f =>
            {
                var (fileParams, file) = f;
                var fd = _fs.OpenFile(fileParams.filename);
                var readFile = _fs.ReadFile(fd, fileParams.offset, Convert.ToUInt16(file.Length));
                var result = file.SequenceEqual(readFile);
                _fs.CloseFile(fd);
                return result;
            });

            if (!result.All(r => r is true)) Console.WriteLine();

            Assert.True(result.All(r => r is true));
        }

        [Theory]
        [InlineData(597, 913)]
        public void TestLink(int offset, ushort value)
        {
            var file = _loremBytes.Skip(offset).Take(value).ToArray();
            _fs.CreateFile("test");
            _fs.Truncate("test", Convert.ToUInt16(offset + value));
            var fd = _fs.OpenFile("test");
            _fs.WriteToFile(file, fd, offset, Convert.ToUInt16(file.Length));
            _fs.CloseFile(fd);
            _fs.LinkFile("test", "testLink");
            fd = _fs.OpenFile("testLink");
            var res = _fs.ReadFile(fd, offset, Convert.ToUInt16(file.Length));
            var fileText = Encoding.Default.GetString(file);
            var resText = Encoding.Default.GetString(res);
            Assert.True(res.SequenceEqual(file));
        }

        [Theory]
        [InlineData(597, 913)]
        public void TestMount(int offset, ushort value)
        {
            var file = _loremBytes.Skip(offset).Take(value).ToArray();
            _fs.CreateFile("test");
            _fs.Truncate("test", Convert.ToUInt16(offset + value));
            var fd = _fs.OpenFile("test");
            _fs.WriteToFile(file, fd, offset, Convert.ToUInt16(file.Length));
            _fs.CloseFile(fd);
            _fs.Dispose();
            var fs = _fileSystemProvider.MountExistingFileSystem(FsName);
            fd = fs.OpenFile("test");
            var res = fs.ReadFile(fd, offset, Convert.ToUInt16(file.Length));
            var fileText = Encoding.Default.GetString(file);
            var resText = Encoding.Default.GetString(res);
            Assert.True(res.SequenceEqual(file));
        }

        [Fact]
        public void FStatTest()
        {
            var offset = 597;
            ushort value = 913;
            var file = _loremBytes.Skip(offset).Take(value).ToArray();
            _fs.CreateFile("test");
            _fs.Truncate("test", Convert.ToUInt16(offset + value));
            var fd = _fs.OpenFile("test");
            _fs.WriteToFile(file, fd, offset, Convert.ToUInt16(file.Length));
            _fs.CloseFile(fd);
            var descriptor = _fs.GetFileDescriptor(1);
            var json = JsonSerializer.Serialize(
                descriptor,
                new JsonSerializerOptions {WriteIndented = true});
            _testOutputHelper.WriteLine(json);
            Assert.True(descriptor.Id == 1 &&
                        descriptor.References == 1 &&
                        descriptor.FileDescriptorType == FileDescriptorType.File);
        }

        [Fact]
        public void LsTest()
        {
            _fs.CreateFile("file1");
            _fs.CreateFile("file2");
            var ls = _fs.DirectoryList();
            Assert.True(ls[0].Name == "file1" &&
                        ls[0].IsValid &&
                        ls[1].Name == "file2" &&
                        ls[1].IsValid);
        }

        // for debug
        [Fact]
        public void Testing()
        {
            var offset = 597;
            ushort value = 913;
            var file = _loremBytes.Skip(offset).Take(value).ToArray();
            var fileText = Encoding.Default.GetString(file);
            _fs.CreateFile("test");
            _fs.Truncate("test", Convert.ToUInt16(offset + value));
            var fd = _fs.OpenFile("test");
            _fs.WriteToFile(file, fd, offset, Convert.ToUInt16(file.Length));
            var res = _fs.ReadFile(fd, offset, Convert.ToUInt16(file.Length));
            var resText = Encoding.Default.GetString(res);
            Assert.True(res.SequenceEqual(file));
        }
    }
}