using PureHDF;
using System.Runtime.InteropServices;
using System;

public class HDFHelperNew
{
    public static int GetSize<T>()
    {
        var tsize = 0;
        if( typeof(T) == typeof(int) ) 
        {
            tsize = sizeof(int);
        }
        else if( typeof(T) == typeof(float) )
        {
            tsize = sizeof(float);
        }
        else if( typeof(T) == typeof(double) )
        {
            tsize = sizeof(double);
        }
        else if( typeof(T) == typeof(uint))
        {
            tsize = sizeof(uint);
        }
        return tsize;
    }
    public static T[,] ToMatrix<T>(Span<T> memory, int nrows, int ncols)
    {
        var doubleMatrix = new T[nrows, ncols];

        for(int i = 0; i < nrows; i++)
        {
            for(int j = 0; j < ncols; j++)
            {
                doubleMatrix[i,j] = memory[i*ncols + j];
            }
        
        }
        return doubleMatrix;
    }

    public static UInt32[] GetDims<T>(IH5Group group, string dsname)
    {
        var dataset = (PureHDF.VOL.Native.NativeDataset)group.Dataset(dsname);
        var nrows = (UInt32)dataset.Space.Dimensions[0]; 
        var ncols = (UInt32)1;
        if (dataset.Space.Dimensions.Length > 1)
        {
            ncols = (UInt32)dataset.Space.Dimensions[1];
        }
        var tsize = new UInt32[2];
        tsize[0] = nrows;
        tsize[1] = ncols;
        return tsize;
    }

    public static Memory<byte> ReadData<T>(IH5Group group, string dsname)
    {
        var dataset = (PureHDF.VOL.Native.NativeDataset)group.Dataset(dsname);
        var dims = GetDims<T>(group, dsname);
        var nrows = dims[0];
        var ncols = dims[1];
        var tsize = GetSize<T>();
        var length = (int)(nrows * ncols * tsize);
        using (var memoryOwner = System.Buffers.MemoryPool<byte>.Shared.Rent(minBufferSize: length))
        {
        var memory = memoryOwner.Memory.Slice(0, length);
        dataset.Read(buffer: memory);
        return memory;
        }
    }

    public static float[,] ReadMatrixFloat(IH5Group group, string dsname)
    {
        var dims = GetDims<float>(group, dsname);
        var nrows = (int)dims[0];
        var ncols = (int)dims[1];
        var memory = ReadData<float>(group,dsname);
        var floatData = MemoryMarshal.Cast<byte, float>(memory.Span);
        var floatMatrix = ToMatrix<float>(floatData, nrows, ncols);
        return floatMatrix;
    }

    public static double[,] ReadMatrixDouble(IH5Group group, string dsname)
    {
        var dims = GetDims<double>(group, dsname);
        var nrows = (int)dims[0];
        var ncols = (int)dims[1];
        var memory = ReadData<double>(group,dsname);
        var floatData = MemoryMarshal.Cast<byte, double>(memory.Span);
        var floatMatrix = ToMatrix<double>(floatData, nrows, ncols);
        return floatMatrix;
    }

    public static int[,] ReadMatrixInt(IH5Group group, string dsname)
    {
        var dims = GetDims<int>(group, dsname);
        var nrows = (int)dims[0];
        var ncols = (int)dims[1];
        var memory = ReadData<int>(group,dsname);
        var floatData = MemoryMarshal.Cast<byte, int>(memory.Span);
        var floatMatrix = ToMatrix<int>(floatData, nrows, ncols);
        return floatMatrix;
    }

    public static uint[,] ReadMatrixUInt(IH5Group group, string dsname)
    {
        var dims = GetDims<uint>(group, dsname);
        var nrows = (int)dims[0];
        var ncols = (int)dims[1];
        Console.Write($"From ReadMatrixUInt: dsname = {dsname}, nrows = {nrows}, ncols = {ncols}");
        var memory = ReadData<uint>(group,dsname);
        var floatData = MemoryMarshal.Cast<byte, uint>(memory.Span);
        var floatMatrix = ToMatrix<uint>(floatData, nrows, ncols);
        return floatMatrix;
    }

}
