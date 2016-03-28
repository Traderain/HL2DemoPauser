using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Half_Life2_GH
{
    class Memory
    {

        public static Int32 ReadInt32(Process targetProcess, IntPtr memoryAddress)
        {
            int bytesRead;
            return BitConverter.ToInt32(Read(targetProcess, memoryAddress, sizeof(Int32), out bytesRead), 0);
        }

        public static string ReadString(Process targetProcess, IntPtr memoryAddress,int length)
        {
            int bytesRead;
            return Encoding.ASCII.GetString(Read(targetProcess, memoryAddress,(uint)length, out bytesRead));
        }

        public static float ReadFloat(Process targetProcess, IntPtr memoryAddress)
        {
            int bytesRead;
            return BitConverter.ToSingle(Read(targetProcess, memoryAddress, sizeof(float), out bytesRead), 0);
        }

        private static Byte[] Read(Process targetProcess, IntPtr memoryAddress, uint bytesToRead, out int bytesRead)
        {
            byte[] byteBuffer = new byte[bytesToRead];
            IntPtr bytesReadPtr;
            ReadProcessMemory(targetProcess.Handle, memoryAddress, byteBuffer,
                bytesToRead, out bytesReadPtr);
            bytesRead = bytesReadPtr.ToInt32();
            return byteBuffer; //Return an array of bytes we convert ourselves
        }

        public static void WriteInt32(Process targetProcess, IntPtr memoryAddress, Int32 value)
        {
            int bytesWritten;
            Write(targetProcess, memoryAddress, BitConverter.GetBytes(value), out bytesWritten);
        }

        public static void WriteFloat(Process targetProcess, IntPtr memoryAddress, float value)
        {
            int bytesWritten;
            Write(targetProcess, memoryAddress, BitConverter.GetBytes(value), out bytesWritten);
        }

        private static void Write(Process targetProcess, IntPtr memoryAddress, byte[] bytesToWrite, out int bytesWritten)
        {
            IntPtr bytesWrittenPtr;
            WriteProcessMemory(targetProcess.Handle, memoryAddress, bytesToWrite, (uint)bytesToWrite.Length, out bytesWrittenPtr);

            bytesWritten = bytesWrittenPtr.ToInt32();
        }

        public static int CalculatePointer(Process targetProcess, int baseAddress, int[] offsets)
        {
            int ptrCount = offsets.Length - 1;
            byte[] byteBuffer = new byte[4];
            int temporaryAddress = 0;
            IntPtr bytesReadPtr;

            baseAddress += (Int32)targetProcess.MainModule.BaseAddress;

            if (ptrCount == 0)
                temporaryAddress = baseAddress;

            for (int i = 0; i <= ptrCount; i++)
            {
                if (i == ptrCount) //Last case (finished)
                {
                    ReadProcessMemory(targetProcess.Handle, (IntPtr)temporaryAddress, byteBuffer, 4, out bytesReadPtr);
                    temporaryAddress = BitConverter.ToInt32(byteBuffer, 0) + offsets[i];
                }
                else if (i == 0) //First case (base)
                {
                    ReadProcessMemory(targetProcess.Handle, (IntPtr)baseAddress, byteBuffer, 4, out bytesReadPtr);
                    temporaryAddress = BitConverter.ToInt32(byteBuffer, 0) + offsets[0];
                }
                else //Normal offset
                {
                    ReadProcessMemory(targetProcess.Handle, (IntPtr)temporaryAddress, byteBuffer, 4, out bytesReadPtr);
                    temporaryAddress = BitConverter.ToInt32(byteBuffer, 0) + offsets[i];
                }
            }
            return temporaryAddress;
        }

        [DllImport("kernel32.dll")]
        private static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress,
            [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);
    }
}