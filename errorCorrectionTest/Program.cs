using System;
using System.Collections.Generic;
using System.Linq;



static void PrintBits(string label, IEnumerable<bool> bits)
{
    Console.Write($"{label}: ");
    foreach (var bit in bits)
        Console.Write(bit ? '1' : '0');
    Console.WriteLine();
}

static bool BoolArrayEquals(bool[] a, bool[] b)
{
    if (ReferenceEquals(a, b))
        return true;

    if (a == null || b == null || a.Length != b.Length)
        return false;

    for (int i = 0; i < a.Length; i++)
    {
        if (a[i] != b[i])
            return false;
    }

    return true;
}


for (int i = 0; i < 1000; i++)
{
    bool[] input = BoolArrayGenerator.GenerateRandomBoolArray(58, 70);

    bool[] encoded = Hamming3126.Encode(input);

    // Simulate an error
    // encoded[2] = !encoded[2];
    // encoded[57] = !encoded[57];

    bool[] decoded = Hamming3126.Decode(encoded);

    if (!BoolArrayEquals(input, decoded))
    {
        PrintBits("Original Input", input);
        PrintBits("Encoded Hamming(31,26)", encoded);
        PrintBits("Decoded Output", decoded);
        Console.WriteLine($"========================");
    }    
    
}






public static class BoolArrayGenerator
{
    private static readonly Random random = new Random();

    public static bool[] GenerateRandomBoolArray(int minLength, int maxLength)
    {
        if (minLength < 0 || maxLength < minLength)
            throw new ArgumentException("Invalid min/max length values.");

        int length = random.Next(minLength, maxLength + 1);
        bool[] result = new bool[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = random.Next(2) == 1;
        }

        return result;
    }
}



public static class Hamming3126
{
    private const int DataBits = 26;
    private const int CodeBits = 31;
    private static readonly int[] ParityPositions = { 0, 1, 3, 7, 15 }; // 1-based: 1,2,4,8,16 (0-based indexing)

    public static bool[] Encode(bool[] data)
    {

        // Format: 5 bits representing padding length + n bits of data + m bits of padding to make total multiple of 26bits long

        var result = new List<bool>();
        var dataToEncode = new List<bool>();

        // Calculate the padding required based on the data length + 5 bits to indicate padding length
        int paddingLength = (DataBits - ((data.Length + 5) % DataBits)) % DataBits;
        // Console.WriteLine($"Padding Length {paddingLength}");

        // Create padding bits
        bool[] paddingMetadata = new bool[5];
        for (int j = 0; j < 5; j++)
            paddingMetadata[j] = (paddingLength & (1 << j)) != 0;

        // Add padding metadata and padding together
        dataToEncode.AddRange(paddingMetadata);
        dataToEncode.AddRange(data);
        if (paddingLength > 0)
        {
            dataToEncode.AddRange(new bool[paddingLength]);
        }

        // Encode each block
        for (int i = 0; i < dataToEncode.Count; i += 26)
        {
            bool[] block = new bool[DataBits];
            Array.Copy(dataToEncode.ToArray(), i, block, 0, DataBits);

            result.AddRange(EncodeBlock(block));
        }

        return result.ToArray();
    }

    public static bool[] Decode(bool[] code)
    {

        if (code.Length % CodeBits != 0)
        {
            return null;
        }

        var decodedBlocks = new List<bool>();

        // Decode blocks
        int totalBlocks = code.Length / CodeBits;
        for (int i = 0; i < totalBlocks; i++)
        {
            bool[] block = new bool[CodeBits];
            Array.Copy(code, i * CodeBits, block, 0, CodeBits);
            bool[] decoded = DecodeBlock(block);
            decodedBlocks.AddRange(decoded);
        }

        static void PrintBits(string label, IEnumerable<bool> bits)
        {
            Console.Write($"{label}: ");
            foreach (var bit in bits)
                Console.Write(bit ? '1' : '0');
            Console.WriteLine();
        }

        // Remove padding
        // Calculate padding length
        int paddingLength = 0;
        for (int j = 0; j < 5; j++)
            if (decodedBlocks[j])
                paddingLength |= (1 << j);
        // Copy data not related to padding
        bool[] originalMessage = new bool[decodedBlocks.Count - 5 - paddingLength];
        Array.Copy(decodedBlocks.ToArray(), 5, originalMessage, 0, originalMessage.Length);

        return originalMessage;
        
    }

    public static bool[] EncodeBlock(bool[] dataBlock)
    {
        bool[] codeword = new bool[CodeBits];
        int dataIdx = 0;

        // Fill in data bits (skip parity positions)
        for (int i = 0; i < CodeBits; i++)
        {
            if (Array.IndexOf(ParityPositions, i) == -1)
            {
                codeword[i] = dataBlock[dataIdx++];
            }
        }

        // Calculate parity bits
        for (int i = 0; i < ParityPositions.Length; i++)
        {
            int p = ParityPositions[i];
            bool parity = false;
            for (int j = 0; j < CodeBits; j++)
            {
                if (j == p) continue;
                if (((j + 1) & (p + 1)) != 0)
                    parity ^= codeword[j];
            }
            codeword[p] = parity;
        }

        return codeword;
    }

    public static bool[] DecodeBlock(bool[] codeword)
    {
        int syndrome = 0;

        // Calculate syndrome
        for (int i = 0; i < ParityPositions.Length; i++)
        {
            int p = ParityPositions[i];
            bool parity = false;

            for (int j = 0; j < CodeBits; j++)
            {
                if (j == p) continue;
                if (((j + 1) & (p + 1)) != 0)
                    parity ^= codeword[j];
            }

            if (parity != codeword[p])
                syndrome |= (1 << i);
        }

        // Correct single-bit error
        if (syndrome != 0 && syndrome <= CodeBits)
        {
            codeword[syndrome - 1] ^= true;
        }

        // Extract data bits (skip parity positions)
        bool[] data = new bool[DataBits];
        int dataIdx = 0;
        for (int i = 0; i < CodeBits; i++)
        {
            if (Array.IndexOf(ParityPositions, i) == -1)
                data[dataIdx++] = codeword[i];
        }

        return data;
    }
}




/// REALY OLD

// static bool[] EncodeWithPaddingHeader(bool[] input)
// {
//     int padding = (22 - (input.Length % 22)) % 22;
//     Console.WriteLine($"Padding: {padding} bits");

//     // Step 1: create 26-bit metadata block
//     bool[] metadata = new bool[26];
//     metadata[0] = (padding & 1) != 0;
//     metadata[1] = (padding & 2) != 0;
//     metadata[2] = false;
//     metadata[3] = false;
//     // rest already false (default)

//     List<bool> fullData = new List<bool>();
//     fullData.AddRange(input);
//     for (int i = 0; i < padding; i++)
//         fullData.Add(false);

//     List<bool> encoded = new List<bool>();

//     // Step 2: Encode metadata block first
//     encoded.AddRange(Encode31(metadata));

//     // Step 3: Encode remaining data blocks
//     for (int i = 0; i < fullData.Count; i += 26)
//     {
//         var block = fullData.Skip(i).Take(26).ToList();
//         while (block.Count < 26)
//             block.Add(false); // pad last block if needed
//         encoded.AddRange(Encode31(block.ToArray()));
//     }

//     return encoded.ToArray();
// }

// static bool[] DecodeWithPaddingHeader(bool[] encoded)
// {
//     if (encoded.Length % 31 != 0)
//         throw new ArgumentException("Encoded data must be multiple of 31 bits.");

//     int numBlocks = encoded.Length / 31;

//     // Step 1: Decode the first metadata block
//     bool[] metadataBlock = encoded.Take(31).ToArray();
//     bool[] decodedMetadata = Decode31(metadataBlock);

//     int padding = (decodedMetadata[0] ? 1 : 0) | (decodedMetadata[1] ? 2 : 0);
//     Console.WriteLine($"Detected padding: {padding} bits");

//     List<bool> decodedData = new List<bool>();

//     // Step 2: Decode remaining blocks
//     for (int i = 1; i < numBlocks; i++)
//     {
//         bool[] block = encoded.Skip(i * 31).Take(31).ToArray();
//         decodedData.AddRange(Decode31(block));
//     }

//     // Step 3: Remove padding bits
//     if (padding > 0)
//         decodedData.RemoveRange(decodedData.Count - padding, padding);

//     return decodedData.ToArray();
// }

// static bool[] Encode31(bool[] data)
// {
//     if (data.Length != 26)
//         throw new ArgumentException("Data block must be 26 bits.");

//     bool[] codeword = new bool[31];
//     int dataIndex = 0;

//     // Place data into non-parity positions (1-based index not power of two)
//     for (int i = 1; i <= 31; i++)
//     {
//         if (!IsPowerOfTwo(i))
//             codeword[i - 1] = data[dataIndex++];
//     }

//     // Calculate parity bits at positions: 1, 2, 4, 8, 16 (1-based)
//     for (int p = 1; p <= 16; p <<= 1)
//     {
//         bool parity = false;
//         for (int i = 1; i <= 31; i++)
//         {
//             if ((i & p) != 0)
//                 parity ^= codeword[i - 1];
//         }
//         codeword[p - 1] = parity;
//     }

//     return codeword;
// }

// static bool[] Decode31(bool[] codeword)
// {
//     if (codeword.Length != 31)
//         throw new ArgumentException("Codeword must be 31 bits.");

//     int errorPosition = 0;

//     // Calculate syndrome (error position)
//     for (int p = 1; p <= 16; p <<= 1)
//     {
//         bool parity = false;
//         for (int i = 1; i <= 31; i++)
//         {
//             if ((i & p) != 0)
//                 parity ^= codeword[i - 1];
//         }
//         if (parity)
//             errorPosition += p;
//     }

//     // Correct error if detected
//     if (errorPosition != 0 && errorPosition <= 31)
//     {
//         Console.WriteLine($"Error detected at bit {errorPosition} (corrected)");
//         codeword[errorPosition - 1] = !codeword[errorPosition - 1];
//     }

//     // Extract data from non-parity positions
//     List<bool> data = new List<bool>();
//     for (int i = 1; i <= 31; i++)
//     {
//         if (!IsPowerOfTwo(i))
//             data.Add(codeword[i - 1]);
//     }

//     return data.ToArray();
// }

// static bool IsPowerOfTwo(int x) => (x & (x - 1)) == 0;

// static void PrintBits(string label, IEnumerable<bool> bits)
// {
//     Console.Write($"{label}: ");
//     foreach (var bit in bits)
//         Console.Write(bit ? '1' : '0');
//     Console.WriteLine();
// }


// bool[] input = { true, false, true, true, true, true, false, false, true, true, false, false, true, false, true, false, false, true, true, true, true, false, true, true, true, true, true };
// Console.WriteLine($"Input Length: {input.Length}");
// PrintBits("Original Input", input);

// bool[] encoded = EncodeWithPaddingHeader(input);
// PrintBits("Encoded Hamming(31,26)", encoded);

// // Simulate an error
// // encoded[9] = !encoded[9];
// // encoded[45] = !encoded[45];
// // encoded[12] = !encoded[12];
// PrintBits("Corrupted", encoded);

// bool[] decoded = DecodeWithPaddingHeader(encoded);
// PrintBits("Decoded Output", decoded);

// Console.WriteLine($"Arrays Equal: {BoolArrayEquals(input, decoded)}");