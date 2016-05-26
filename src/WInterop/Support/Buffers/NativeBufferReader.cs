﻿// ------------------------
//    WInterop Framework
// ------------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace WInterop.Support.Buffers
{
    /// <summary>
    /// Checked helpers for reading data from a NativeBuffer. Use StreamBuffer for more complicated read operations.
    /// </summary>
    /// <remarks>
    /// Didn't use extension methods to avoid dirtying the usage of derived classes. We don't want to encourage accidentally
    /// grabbing strings for StringBuffer using these methods, for example.
    /// </remarks>
    public class NativeBufferReader
    {
        // Windows Data Alignment on IPF, x86, and x64
        // https://msdn.microsoft.com/en-us/library/aa290049.aspx

        private NativeBuffer _buffer;
        private ulong _byteOffset;

        public NativeBufferReader(NativeBuffer buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            _buffer = buffer;
        }

        /// <summary>
        /// Get/set the offset in bytes
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if setting an offset greater than the capacity.</exception>
        public ulong ByteOffset
        {
            get
            {
                return _byteOffset;
            }
            set
            {
                if (value > _buffer.ByteCapacity) throw new ArgumentOutOfRangeException(nameof(ByteOffset));
                _byteOffset = value;
            }
        }

        /// <summary>
        /// Read a string of the given amount of characters from the buffer. Advances the reader offset.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the count of characters would go past the end of the buffer.</exception>
        unsafe public string ReadString(int charCount)
        {
            if ((uint)charCount * sizeof(char) > _buffer.ByteCapacity - _byteOffset) throw new ArgumentOutOfRangeException(nameof(charCount));

            if (charCount == 0) return string.Empty;

            string value = new string((char*)((byte*)_buffer.DangerousGetHandle() + _byteOffset), startIndex: 0, length: charCount);
            _byteOffset += (ulong)(charCount * sizeof(char));
            return value;
        }

        /// <summary>
        /// Read a ushort at the current offset. Advances the reader offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading a ushort would go past the end of the buffer.</exception>
        public ushort ReadUshort()
        {
            return (ushort)ReadShort();
        }

        /// <summary>
        /// Read a short at the current offset. Advances the reader offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading a short would go past the end of the buffer.</exception>
        unsafe public short ReadShort()
        {
            if (_buffer.ByteCapacity < sizeof(short) || _byteOffset > _buffer.ByteCapacity - sizeof(short)) throw new EndOfStreamException();

            byte* address = (byte*)_buffer.DangerousGetHandle() + _byteOffset;
            _byteOffset += sizeof(short);

            if (((short)address & (sizeof(short) - 1)) == 0)
            {
                // aligned read
                return *((short*)address);
            }
            else
            {
                // unaligned read
                return (short)(*address | *(address + 1) << 8);
            }
        }

        /// <summary>
        /// Read a uint at the current offset. Advances the reader offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading a uint would go past the end of the buffer.</exception>
        public uint ReadUint()
        {
            return (uint)ReadInt();
        }

        /// <summary>
        /// Read an int at the current offset. Advances the reader offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading an int would go past the end of the buffer.</exception>
        unsafe public int ReadInt()
        {
            if (_buffer.ByteCapacity < sizeof(int) || _byteOffset > _buffer.ByteCapacity - sizeof(int)) throw new EndOfStreamException();

            byte* address = (byte*)_buffer.DangerousGetHandle() + _byteOffset;
            _byteOffset += sizeof(int);

            if (((int)address & (sizeof(int) - 1)) == 0)
            {
                // aligned read
                return *((int*)address);
            }
            else
            {
                // unaligned read
                return *address | *(address + 1) << 8 | *(address + 2) << 16 | *(address + 3) << 24;
            }
        }

        /// <summary>
        /// Read a ulong at the current offset. Advances the reader offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading a ulong would go past the end of the buffer.</exception>
        public ulong ReadUlong()
        {
            return (ulong)ReadLong();
        }

        /// <summary>
        /// Read a long at the current offset. Advances the reader offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading a long would go past the end of the buffer.</exception>
        unsafe public long ReadLong()
        {
            if (_buffer.ByteCapacity < sizeof(long) || _byteOffset > _buffer.ByteCapacity - sizeof(long)) throw new System.IO.EndOfStreamException();

            byte* address = (byte*)_buffer.DangerousGetHandle() + _byteOffset;
            _byteOffset += sizeof(long);

            if (((long)address & (sizeof(long) - 1)) == 0)
            {
                // aligned read
                return *((long*)address);
            }
            else
            {
                // unaligned read
                int lo = *address | *(address + 1) << 8 | *(address + 2) << 16 | *(address + 3) << 24;
                int hi = *(address + 4) | *(address + 5) << 8 | *(address + 6) << 16 | *(address + 7) << 24;
                return ((long)hi << 32) | (uint)lo;
            }
        }

        /// <summary>
        /// Get a pointer at the current offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading a pointer would go past the end of the buffer.</exception>
        public IntPtr ReadIntPtr()
        {
            if (Support.Environment.Is64BitProcess)
            {
                return (IntPtr)ReadUlong();
            }
            else
            {
                return (IntPtr)ReadUint();
            }
        }

        /// <summary>
        /// Read the given struct type at the current offset.
        /// </summary>
        /// <exception cref="EndOfStreamException">Thrown if reading the given struct would go past the end of the buffer.</exception>
        public T ReadStruct<T>() where T : struct
        {
            ulong sizeOfStruct = (ulong)Marshal.SizeOf<T>();
            if (_buffer.ByteCapacity < sizeOfStruct || _byteOffset > _buffer.ByteCapacity - sizeOfStruct) throw new System.IO.EndOfStreamException();

            T value = Marshal.PtrToStructure<T>(_buffer.DangerousGetHandle() + checked((int)_byteOffset));
            _byteOffset += sizeOfStruct;
            return value;
        }
    }
}