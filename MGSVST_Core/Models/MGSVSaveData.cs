using MGSVST_Core.Helpers;
using System.Security.Cryptography;
using System.Text;

namespace MGSVST_Core.Models;

public class MGSVSaveData
{
    /// <summary>
    /// Create an empty MGSVSaveData object.
    /// </summary>
    public MGSVSaveData()
    { }

    /// <summary>
    /// A status reporter.
    /// </summary>
    public SimpleStatusReporter Reporter { get; set; } = new();

    /// <summary>
    /// Content of SaveData file.
    /// </summary>
    public byte[] Data { get; set; } = Array.Empty<byte>();
    
    /// <summary>
    /// Key to the currently loaded SaveData file.
    /// </summary>
    public uint Key { get; set; }

    /// <summary>
    /// A FilePath of a currently loaded file.
    /// </summary>
    public string FilePath { get; private set; }

    /// <summary>
    /// A flag which determines if current <see cref="Data"/> is encrypted.
    /// </summary>
    public bool IsEncrypted { get; private set; } = true;
    
    /// <summary>
    /// Loads SaveData file into the current object.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public bool Load(string filePath)
    {
        byte[] bytes;
        try
        {
            bytes = File.ReadAllBytes(filePath);
        }
        catch
        {
            Reporter.Error($"Couldn't load '{Path.GetFileNameWithoutExtension(filePath)}' file.");
            return false;
        }

        Data = bytes;
        IsEncrypted = IsDataEncrypted(Data);
        FilePath = filePath;

        Reporter.Success($"Loaded '{Path.GetFileNameWithoutExtension(filePath)}' file.");
        return true;
    }

    /// <summary>
    /// Loads SaveData file into the current object.
    /// </summary>
    /// <returns></returns>
    private bool Save(bool backupEnabled)
    {
        try
        {
            if (backupEnabled)
            {
                var suffix = IsEncrypted ? "encr" : "decr";
                File.Copy(FilePath, $"{FilePath}.bak{suffix}");
            }
            File.WriteAllBytes(FilePath,Data);
            
        }
        catch
        {
            Reporter.Error("Couldn't save decrypted file.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Specifies whether the SaveData is encrypted.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private static bool IsDataEncrypted(IEnumerable<byte> data) 
        => BitConverter.ToUInt32(data.Skip(16).Take(4).ToArray()) is not (1396921939 or 1380013651);
    private static bool IsDataEncrypted(uint data)
        => data is not (1396921939 or 1380013651);

    /// <summary>
    /// Tries out all possible keys (4,294,967,295) for content in the <see cref="Data"/>.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="progressModel"></param>
    /// <returns></returns>
    public bool BruteforceKey(CancellationToken cancellationToken, ProgressModel progressModel)
    {
        var result = false;

        ParallelOptions po = new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = Environment.ProcessorCount - 1
        };

        uint progress = 0;
        Parallel.For(1, uint.MaxValue, po, (ctr, state) =>
        {
            Interlocked.Increment(ref progress);
            progressModel.TasksDone = progress;
            var i = Convert.ToUInt32(ctr);
            if (!DecryptLite(i)) return;
            if (!Deencrypt(i)) return;
            Key = i;
            result = true;
            state.Break();
        });

        if (result)
        {
            Reporter.Success("Decryption key has been found");
        }
        else
        {
            Reporter.Error("The decryption key could not be found using the current algorithm.");
        }

        return result;
    }

    /// <summary>
    /// Testing a collection of key candidates for content in the <see cref="Data"/>.
    /// </summary>
    /// <param name="keys"></param>
    /// <returns></returns>
    public bool BruteforceKeyLite(uint[] keys)
    {
        var result = false;

        ParallelOptions po = new()
        {
            MaxDegreeOfParallelism = Environment.ProcessorCount - 1
        };

        Parallel.ForEach(keys, po, (key, state) =>
        {
            if (!DecryptLite(key)) return;
            if (!Deencrypt(key, true)) return;
            Key = key;
            result = true;
            state.Break();
        });
        return result;
    }

    /// <summary>
    /// Compares the checksum of the file with the checksum stored in the file.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private static bool ValidateDataMd5(byte[] bytes)
    {
        using var md5 = MD5.Create();
        var checksumCurrent = md5.ComputeHash(bytes.Skip(16).ToArray());
        var checksumStored = bytes.Take(16).ToArray();
        
        return checksumCurrent.AsSpan().SequenceEqual(checksumStored);
    }

    /// <summary>
    /// Compares the checksum of the file with the checksum stored in the file.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    private void SignDataMd5()
    {
        using var md5 = MD5.Create();
        var checksum = md5.ComputeHash(Data.Skip(16).ToArray());
        checksum.CopyTo(Data,0);
    }

    /// <summary>
    /// Tries to generates key for the SaveData using an md5 checksum of the <see cref="fileType"/>.
    /// </summary>
    /// <param name="fileType"></param>
    /// <returns></returns>
    public bool GenerateUsingFileType(string fileType)
    {
        using var md5 = MD5.Create();
        var checksum = md5.ComputeHash(Encoding.ASCII.GetBytes(fileType));
        var key = (uint)( checksum[0] | checksum[1] << 8 | checksum[2] << 16 | checksum[3] << 24);
        
        // test key
        var bytes = Data;
        var result = Deencrypt(key);
        Key = key;
        if (result)
        {
            Reporter.Success("A correct decryption key has been generated.");
        }
        else
        {
            Reporter.Error("A decryption key has been generated, but it doesn't work.");
        }
        return result;
    }

    /// <summary>
    /// Decrypts/Encrypts <see cref="Data"/> using <see cref="Key"/> and custom algorithm.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dry"></param>
    /// <returns></returns>
    private bool Deencrypt(uint key = 0, bool dry = false)
    {
        var bytes = Data;
        if (key == 0) key = Key;
        using MemoryStream ms = new();
        for (int i = 0; i < bytes.Length; i += 4)
        {
            // take 4 bytes
            byte[] batch = bytes.Skip(i).Take(4).ToArray();

            // convert batch array to UInt32
            uint batchUInt32 = BitConverter.ToUInt32(batch);

            // transform
            var hash = key;
            hash <<= 0x0D;
            key ^= hash;
            hash = key;
            hash >>>= 0x07;
            key ^= hash;
            hash = key;
            hash <<= 0x05;
            key ^= hash;
            batchUInt32 ^= key;

            // write xor'ed bytes to MemoryStream
            batch = BitConverter.GetBytes(batchUInt32);
            ms.Write(batch);
        }

        // validate result
        var result = ms.ToArray();
        var test = !IsEncrypted || ValidateDataMd5(result);
        if (test && !dry)
        {
            Data = result;
            IsEncrypted ^= true;
        }
        return test;
    }

    /// <summary>
    /// Decrypts first 20 bytes to check if the encryption key matches.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    private bool DecryptLite(uint key)
    {
        var bytes = Data;
        uint batchUInt32 = 0;
        for (int i = 0; i < 20; i += 4)
        {
            // take 4 bytes
            byte[] batch = bytes.Skip(i).Take(4).ToArray();

            // convert batch array to UInt32
            batchUInt32 = BitConverter.ToUInt32(batch);

            // transform
            var hash = key;
            hash <<= 0x0D;
            key ^= hash;
            hash = key;
            hash >>>= 0x07;
            key ^= hash;
            hash = key;
            hash <<= 0x05;
            key ^= hash;
            batchUInt32 ^= key;
        }
        return !IsDataEncrypted(batchUInt32);
    }

    /// <summary>
    /// Decrypts <see cref="Data"/>see and saves it to a file.
    /// </summary>
    /// <param name="backupEnabled"></param>
    public void Decrypt(bool backupEnabled)
    {
        var result = Deencrypt();
        if (result)
        {
            Reporter.Success("File decrypted successfully.");
            Save(backupEnabled);
        }
        else
        {
            Reporter.Error("File decryption failed.");
        }
    }

    /// <summary>
    /// Encrypts <see cref="Data"/>see and saves it to a file.
    /// </summary>
    /// <param name="backupEnabled"></param>
    public void Encrypt(bool backupEnabled)
    {
        SignDataMd5();
        var result = Deencrypt();
        if (result)
        {
            Reporter.Success("File encrypted successfully.");
            Save(backupEnabled);
        }
        else
        {
            Reporter.Error("File encryption failed.");
        }
    }
}