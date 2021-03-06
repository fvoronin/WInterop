﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Microsoft.Win32.SafeHandles;
using System;
using Tests.Support;
using WInterop.DirectoryManagement;
using WInterop.ErrorHandling;
using WInterop.ErrorHandling.Types;
using WInterop.FileManagement;
using WInterop.FileManagement.Types;
using WInterop.Support;
using WInterop;
using Xunit;

namespace Tests.FileManagement
{
    public partial class FileManagementBehaviors
    {
        [Theory,
            // Basic dot space handling
            InlineData(@"C:\", @"C:\"),
            InlineData(@"C:\ ", @"C:\"),
            InlineData(@"C:\.", @"C:\"),
            InlineData(@"C:\..", @"C:\"),
            InlineData(@"C:\...", @"C:\"),
            InlineData(@"C:\ .", @"C:\"),
            InlineData(@"C:\ ..", @"C:\"),
            InlineData(@"C:\ ...", @"C:\"),
            InlineData(@"C:\. ", @"C:\"),
            InlineData(@"C:\.. ", @"C:\"),
            InlineData(@"C:\... ", @"C:\"),
            InlineData(@"C:\.\", @"C:\"),
            InlineData(@"C:\..\", @"C:\"),
            InlineData(@"C:\...\", @"C:\...\"),
            InlineData(@"C:\ \", @"C:\ \"),
            InlineData(@"C:\ .\", @"C:\ \"),
            InlineData(@"C:\ ..\", @"C:\ ..\"),
            InlineData(@"C:\ ...\", @"C:\ ...\"),
            InlineData(@"C:\. \", @"C:\. \"),
            InlineData(@"C:\.. \", @"C:\.. \"),
            InlineData(@"C:\... \", @"C:\... \"),
            InlineData(@"C:\A \", @"C:\A \"),
            InlineData(@"C:\A \B", @"C:\A \B"),

            // Same as above with prefix
            InlineData(@"\\?\C:\", @"\\?\C:\"),
            InlineData(@"\\?\C:\ ", @"\\?\C:\"),
            InlineData(@"\\?\C:\.", @"\\?\C:"),           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\..", @"\\?\"),            // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...", @"\\?\C:\"),
            InlineData(@"\\?\C:\ .", @"\\?\C:\"),
            InlineData(@"\\?\C:\ ..", @"\\?\C:\"),
            InlineData(@"\\?\C:\ ...", @"\\?\C:\"),
            InlineData(@"\\?\C:\. ", @"\\?\C:\"),
            InlineData(@"\\?\C:\.. ", @"\\?\C:\"),
            InlineData(@"\\?\C:\... ", @"\\?\C:\"),
            InlineData(@"\\?\C:\.\", @"\\?\C:\"),
            InlineData(@"\\?\C:\..\", @"\\?\"),           // Changes behavior, without \\?\, returns C:\
            InlineData(@"\\?\C:\...\", @"\\?\C:\...\"),

            // How deep can we go with prefix
            InlineData(@"\\?\C:\..\..", @"\\?\"),
            InlineData(@"\\?\C:\..\..\..", @"\\?\"),

            // Pipe tests
            InlineData(@"\\.\pipe", @"\\.\pipe"),
            InlineData(@"\\.\pipe\", @"\\.\pipe\"),
            InlineData(@"\\?\pipe", @"\\?\pipe"),
            InlineData(@"\\?\pipe\", @"\\?\pipe\"),

            // Basic dot space handling with UNCs
            InlineData(@"\\Server\Share\", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\ ", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\.", @"\\Server\Share"),                      // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\..", @"\\Server\Share"),                     // UNCs can eat trailing separator
            InlineData(@"\\Server\Share\...", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\ .", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\ ..", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\ ...", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\. ", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\.. ", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\... ", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\.\", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\..\", @"\\Server\Share\"),
            InlineData(@"\\Server\Share\...\", @"\\Server\Share\...\"),

            // Slash direction makes no difference
            InlineData(@"//Server\Share\", @"\\Server\Share\"),
            InlineData(@"//Server\Share\ ", @"\\Server\Share\"),
            InlineData(@"//Server\Share\.", @"\\Server\Share"),                      // UNCs can eat trailing separator
            InlineData(@"//Server\Share\..", @"\\Server\Share"),                     // UNCs can eat trailing separator
            InlineData(@"//Server\Share\...", @"\\Server\Share\"),
            InlineData(@"//Server\Share\ .", @"\\Server\Share\"),
            InlineData(@"//Server\Share\ ..", @"\\Server\Share\"),
            InlineData(@"//Server\Share\ ...", @"\\Server\Share\"),
            InlineData(@"//Server\Share\. ", @"\\Server\Share\"),
            InlineData(@"//Server\Share\.. ", @"\\Server\Share\"),
            InlineData(@"//Server\Share\... ", @"\\Server\Share\"),
            InlineData(@"//Server\Share\.\", @"\\Server\Share\"),
            InlineData(@"//Server\Share\..\", @"\\Server\Share\"),
            InlineData(@"//Server\Share\...\", @"\\Server\Share\...\"),

            // Slash count breaks rooting
            InlineData(@"\\\Server\Share\", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\ ", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\.", @"\\\Server\Share"),                     // UNCs can eat trailing separator
            InlineData(@"\\\Server\Share\..", @"\\\Server"),                          // Paths without 2 initial slashes will not root the share
            InlineData(@"\\\Server\Share\...", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\ .", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\ ..", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\ ...", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\. ", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\.. ", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\... ", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\.\", @"\\\Server\Share\"),
            InlineData(@"\\\Server\Share\..\", @"\\\Server\"),                       // Paths without 2 initial slashes will not root the share
            InlineData(@"\\\Server\Share\...\", @"\\\Server\Share\...\"),

            // Inital slash count is always kept
            InlineData(@"\\\\Server\Share\", @"\\\Server\Share\"),
            InlineData(@"\\\\\Server\Share\", @"\\\Server\Share\"),

            // Extended paths root to \\?\
            InlineData(@"\\?\UNC\Server\Share\", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\ ", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\.", @"\\?\UNC\Server\Share"),
            InlineData(@"\\?\UNC\Server\Share\..", @"\\?\UNC\Server"),               // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\ .", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\ ..", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\ ...", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\. ", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\.. ", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\... ", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\.\", @"\\?\UNC\Server\Share\"),
            InlineData(@"\\?\UNC\Server\Share\..\", @"\\?\UNC\Server\"),             // Extended UNCs can eat into Server\Share
            InlineData(@"\\?\UNC\Server\Share\...\", @"\\?\UNC\Server\Share\...\"),

            // How deep can we go with prefix
            InlineData(@"\\?\UNC\Server\Share\..\..", @"\\?\UNC"),
            InlineData(@"\\?\UNC\Server\Share\..\..\..", @"\\?\"),
            InlineData(@"\\?\UNC\Server\Share\..\..\..\..", @"\\?\"),

            // Root slash behavior
            InlineData(@"C:/", @"C:\"),
            InlineData(@"C:/..", @"C:\"),
            InlineData(@"//Server/Share", @"\\Server\Share"),
            InlineData(@"//Server/Share/..", @"\\Server\Share"),
            InlineData(@"//Server//Share", @"\\Server\Share"),
            InlineData(@"//Server//Share/..", @"\\Server\"),                         // Double slash shares normalize but don't root correctly
            InlineData(@"//Server\\Share/..", @"\\Server\"),
            InlineData(@"//?/", @"\\?\"),
            // InlineData(@"\??\", @"D:\??\")                                       // \??\ will return the current directory's drive if passed to GetFullPathName
            // InlineData(@"/??/", @"D:\??\")
            InlineData(@"//./", @"\\.\"),

            // Legacy device behavior
            InlineData(@"CON", @"\\.\CON"),
            InlineData(@"CON:Alt", @"\\.\CON"),
            InlineData(@"LPT9", @"\\.\LPT9"),
            InlineData(@"C:\CON", @"\\.\CON"),
            InlineData(@"\\.\C:\CON", @"\\.\C:\CON"),

            InlineData(@"C:\A\B\.\..\C", @"C:\A\C")
            ]
        public void ValidateKnownFixedBehaviors(string value, string expected)
        {
            FileMethods.GetFullPathName(value).Should().Be(expected, $"source was {value}");
        }

        [Fact]
        public void FindFirstFileBehaviors()
        {
            using (var cleaner = new TestFileCleaner())
            {
                IntPtr result = FileMethods.Imports.FindFirstFileW(cleaner.TempFolder, out WIN32_FIND_DATA findData);
                try
                {
                    IsValid(result).Should().BeTrue("root location exists");
                }
                finally
                {
                    if (IsValid(result))
                        FileMethods.Imports.FindClose(result);
                }

                result = FileMethods.Imports.FindFirstFileW(cleaner.GetTestPath(), out findData);
                WindowsError error = Errors.GetLastError();

                try
                {
                    IsValid(result).Should().BeFalse("non-existant file");
                    error.Should().Be(WindowsError.ERROR_FILE_NOT_FOUND);
                }
                finally
                {
                    if (IsValid(result))
                        FileMethods.Imports.FindClose(result);
                }

                result = FileMethods.Imports.FindFirstFileW(Paths.Combine(cleaner.GetTestPath(), "NotHere"), out findData);
                error = Errors.GetLastError();

                try
                {
                    IsValid(result).Should().BeFalse("non-existant subdir");
                    error.Should().Be(WindowsError.ERROR_PATH_NOT_FOUND);
                }
                finally
                {
                    if (IsValid(result))
                        FileMethods.Imports.FindClose(result);
                }
            }

            bool IsValid(IntPtr value)
            {
                return value != IntPtr.Zero && value != (IntPtr)(-1);
            }
        }

        [Fact]
        public void GetFileAttributesBehavior_Basic()
        {
            using (var cleaner = new TestFileCleaner())
            {
                bool success = FileMethods.Imports.GetFileAttributesExW(cleaner.TempFolder,
                    GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out WIN32_FILE_ATTRIBUTE_DATA attributeData);
                success.Should().BeTrue("root location exists");
                success = FileMethods.Imports.GetFileAttributesExW(cleaner.GetTestPath(),
                    GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out attributeData);
                WindowsError error = Errors.GetLastError();
                success.Should().BeFalse("non-existant file");
                error.Should().Be(WindowsError.ERROR_FILE_NOT_FOUND);
                success = FileMethods.Imports.GetFileAttributesExW(Paths.Combine(cleaner.GetTestPath(), "NotHere"),
                    GET_FILEEX_INFO_LEVELS.GetFileExInfoStandard, out attributeData);
                error = Errors.GetLastError();
                success.Should().BeFalse("non-existant subdir");
                error.Should().Be(WindowsError.ERROR_PATH_NOT_FOUND);
            }
        }

        [Fact]
        public void GetFileAttributesBehavior_DeletedFile()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string path = cleaner.CreateTestFile(nameof(GetFileAttributesBehavior_DeletedFile));
                using (var handle = FileMethods.CreateFile(path, CreationDisposition.OpenExisting, shareMode: ShareModes.All))
                {
                    handle.IsInvalid.Should().BeFalse();
                    FileMethods.FileExists(path).Should().BeTrue();
                    FileMethods.DeleteFile(path);

                    // With the file deleted and the handle still open the file will still physically exist.
                    // Trying to access the file via a handle at this point will fail with access denied.

                    Action action = () => FileMethods.FileExists(path);
                    action.ShouldThrow<UnauthorizedAccessException>();

                    action = () => FileMethods.CreateFile(path, CreationDisposition.OpenExisting, shareMode: ShareModes.All,
                        desiredAccess: DesiredAccess.ReadAttributes);
                    action.ShouldThrow<UnauthorizedAccessException>();

                    // Find file will work at this point.
                    IntPtr findHandle = FileMethods.Imports.FindFirstFileW(path, out WIN32_FIND_DATA findData);
                    findHandle.Should().NotBe(IntPtr.Zero);
                    try
                    {
                        findData.cFileName.CreateString().Should().Be(Paths.GetLastSegment(path));
                    }
                    finally
                    {
                        FileMethods.Imports.FindClose(findHandle);
                    }
                }
            }
        }

        [Fact]
        public void GetFullPathNameLongPathBehaviors()
        {
            // ERROR_FILENAME_EXCED_RANGE (206)
            // GetFullPathName will fail if the passed in patch is longer than short.MaxValue - 2, even if the path will normalize below that value.
            // FileMethods.GetFullPathName(PathGenerator.CreatePathOfLength(@"C:\..\..\..\..", short.MaxValue - 2));

            // ERROR_INVALID_NAME (123)
            // GetFullPathName will fail if the passed in path normalizes over short.MaxValue - 2
            // FileMethods.GetFullPathName(new string('a', short.MaxValue - 2));

            FileMethods.GetFullPathName(PathGenerator.CreatePathOfLength(FileMethods.GetTempPath(), short.MaxValue - 2));

            // Works
            // NativeMethods.FileManagement.GetFullPathName(PathGenerator.CreatePathOfLength(@"C:\", short.MaxValue - 2));
        }

        [Fact]
        public void LockedFileDirectoryDeletion()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directory = cleaner.GetTestPath();
                DirectoryMethods.CreateDirectory(directory);
                FileMethods.DirectoryExists(directory).Should().BeTrue();
                string file = cleaner.CreateTestFile(nameof(LockedFileDirectoryDeletion), directory);
                using (var handle = FileMethods.CreateFile(file, CreationDisposition.OpenExisting, DesiredAccess.GenericRead, ShareModes.ReadWrite | ShareModes.Delete))
                {
                    handle.IsInvalid.Should().BeFalse();

                    // Mark the file for deletion
                    FileMethods.DeleteFile(file);

                    // RemoveDirectory API call will throw
                    Action action = () => DirectoryMethods.RemoveDirectory(directory);
                    action.ShouldThrow<WInteropIOException>().And.HResult.Should().Be((int)ErrorMacros.HRESULT_FROM_WIN32(WindowsError.ERROR_DIR_NOT_EMPTY));

                    // Opening the directory for deletion will succeed, but have no impact
                    using (var directoryHandle = FileMethods.CreateFile(
                        directory,
                        CreationDisposition.OpenExisting,
                        DesiredAccess.ListDirectory | DesiredAccess.Delete,
                        ShareModes.ReadWrite | ShareModes.Delete,
                        FileAttributes.None,
                        FileFlags.BackupSemantics | FileFlags.DeleteOnClose))
                    {
                        directoryHandle.IsInvalid.Should().BeFalse();
                    }
                }

                // File will be gone now that the handle is closed
                FileMethods.FileExists(file).Should().BeFalse();

                // But the directory will still exist as it doesn't respect DeleteOnClose with an open handle when it is closed
                FileMethods.DirectoryExists(directory).Should().BeTrue();

                // Create a handle to the directory again with DeleteOnClose and it will actually delete the directory
                using (var directoryHandle = FileMethods.CreateFile(
                    directory,
                    CreationDisposition.OpenExisting,
                    DesiredAccess.ListDirectory | DesiredAccess.Delete,
                    ShareModes.ReadWrite | ShareModes.Delete,
                    FileAttributes.None,
                    FileFlags.BackupSemantics | FileFlags.DeleteOnClose))
                {
                    directoryHandle.IsInvalid.Should().BeFalse();
                }
                FileMethods.DirectoryExists(directory).Should().BeFalse();
            }
        }


        [Fact]
        public void LockedFileDirectoryDeletion2()
        {
            using (var cleaner = new TestFileCleaner())
            {
                string directory = cleaner.GetTestPath();
                DirectoryMethods.CreateDirectory(directory);
                FileMethods.DirectoryExists(directory).Should().BeTrue();
                string file = cleaner.CreateTestFile(nameof(LockedFileDirectoryDeletion2), directory);

                SafeFileHandle directoryHandle = null;
                using (var handle = FileMethods.CreateFile(file, CreationDisposition.OpenExisting, DesiredAccess.GenericRead, ShareModes.ReadWrite | ShareModes.Delete))
                {
                    handle.IsInvalid.Should().BeFalse();

                    // Mark the file for deletion
                    FileMethods.DeleteFile(file);

                    // Open the directory handle
                    directoryHandle = FileMethods.CreateFile(
                        directory,
                        CreationDisposition.OpenExisting,
                        DesiredAccess.ListDirectory | DesiredAccess.Delete,
                        ShareModes.ReadWrite | ShareModes.Delete,
                        FileAttributes.None,
                        FileFlags.BackupSemantics | FileFlags.DeleteOnClose);
                }

                try
                {
                    // File will be gone now that the handle is closed
                    FileMethods.FileExists(file).Should().BeFalse();

                    directoryHandle.Close();

                    // The directory will not exist as the open handle was closed before it was closed
                    FileMethods.DirectoryExists(directory).Should().BeFalse();

                }
                finally
                {
                    directoryHandle?.Close();
                }
            }
        }
    }
}
