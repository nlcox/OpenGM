﻿namespace OpenGM;
public class Buffer
{
    public byte[] Data = null!;
    public BufferType Type;
    public int Alignment;
    public int AlignmentOffset;
    public int BufferIndex;
    public int UsedSize;
    public int Size;

    public void CalculateNextAlignmentOffset()
    {
        AlignmentOffset = (AlignmentOffset + Size) % Alignment;
    }

    public void UpdateUsedSize(int size = -1, bool reset = false)
    {
        var newSize = size;
        if (newSize == -1)
        {
            newSize = BufferIndex;
        }

        if (reset)
        {
            UsedSize = newSize;
        }
        else
        {
            UsedSize = CustomMath.Max(UsedSize, newSize);
            UsedSize = CustomMath.Max(UsedSize, Size);
        }
    }

    // Based on https://github.com/YoYoGames/GameMaker-HTML5/blob/95d8f5643efbdb74ffce2bfae4b82bc3426b2b54/scripts/yyBuffer.js#L1807-L1830
    public int Seek(BufferSeek basePosition, int offset)
    {
        // Actual wrapping of position is handled when reading from the data.
        // Wrapping does not apply when going backwards, only forwards.

        switch (basePosition)
        {
            case BufferSeek.SeekStart:
                if (offset < 0)
                {
                    offset = 0;
                }

                BufferIndex = offset;
                break;
            case BufferSeek.SeekRelative:
                BufferIndex += offset;

                if (BufferIndex < 0)
                {
                    BufferIndex = 0;
                }

                break;
            case BufferSeek.SeekEnd:
                // Shouldnt all this be the other way around?
                // The other two cases check for negative seek positions, but this one doesnt.
                // A positive offset moves the seek position BACKWARDS now? wtf gamemaker

                BufferIndex = Size - offset;
                if (BufferIndex > Size)
                {
                    BufferIndex = Size;
                }

                break;
        }

        return BufferIndex;
    }

    public void Resize(int newSize)
    {
        var newData = new byte[newSize];

        for (var i = 0; i < newSize; i++)
        {
            if (i < Data.Length)
            {
                newData[i] = Data[i];
            }
        }

        Data = newData;
        Size = newSize;
        UpdateUsedSize();
    }

    public void Poke(BufferDataType type, int offset, object value)
    {
        if (offset < 0)
        {
            return;
        }

        var size = BufferManager.BufferDataTypeToSize(type);

        if (Type != BufferType.Wrap)
        {
            if (offset > (Size - size))
            {
                return; // // can't write off the end of the buffer
            }
        }
        else
        {
            while (offset >= Size)
            {
                offset -= Size;
            }
        }

        switch (type)
        {
            case BufferDataType.buffer_bool:
                Data[offset] = (byte)((bool)value ? 1 : 0);
                UpdateUsedSize(offset + 1);
                break;
            case BufferDataType.buffer_u8:
                Data[offset] = (byte)value;
                UpdateUsedSize(offset + 1);
                break;
            case BufferDataType.buffer_s8:
                Data[offset] = (byte)value;
                UpdateUsedSize(offset + 1);
                break;
            case BufferDataType.buffer_u16:
                throw new NotImplementedException();
            case BufferDataType.buffer_s16:
                throw new NotImplementedException();
            case BufferDataType.buffer_s32:
                throw new NotImplementedException();
            case BufferDataType.buffer_u32:
                throw new NotImplementedException();
            case BufferDataType.buffer_f32:
                throw new NotImplementedException();
            case BufferDataType.buffer_f64:
                throw new NotImplementedException();
            case BufferDataType.buffer_u64:
                throw new NotImplementedException();
            case BufferDataType.buffer_string:
            case BufferDataType.buffer_text:
                throw new NotImplementedException();
        }

        UpdateUsedSize(offset + size);
    }

    public string MD5(int offset, int size)
    {
        if (Size == 0)
        {
            return "";
        }

        if (size < 0)
        {
            size = Size;
        }

        if (Type == BufferType.Wrap)
        {
            while (offset < 0)
            {
                offset += Size;
            }

            while (offset >= Size)
            {
                offset -= Size;
            }
        }
        else
        {
            if (offset < 0)
            {
                offset = 0;
            }

            if (offset >= Size)
            {
                offset = Size - 1;
            }

            if ((offset + size) >= Size)
            {
                size = Size - offset;
            }
        }

        if (size > Size - offset)
        {
            return "";
        }

        var dataToHash = Data.Skip(offset).Take(size).ToArray();
        var md5 = System.Security.Cryptography.MD5.HashData(dataToHash);
        return Convert.ToHexString(md5);
    }
}

public enum BufferType
{
    Fixed,
    Grow,
    Wrap,
    Fast
}

public enum BufferSeek
{
    SeekStart,
    SeekRelative,
    SeekEnd
}

public enum BufferDataType
{
    None,
    buffer_u8,
    buffer_s8,
    buffer_u16,
    buffer_s16,
    buffer_u32,
    buffer_s32,
    buffer_f16,
    buffer_f32,
    buffer_f64,
    buffer_bool,
    buffer_string,
    buffer_u64,
    buffer_text
}
