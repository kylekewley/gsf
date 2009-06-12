//*******************************************************************************************************
//  ArchiveFileAllocationTable.cs
//  Copyright � 2009 - TVA, all rights reserved - Gbtc
//
//  Build Environment: C#, Visual Studio 2008
//  Primary Developer: Pinal C. Patel
//      Office: INFO SVCS APP DEV, CHATTANOOGA - MR BK-C
//       Phone: 423/751-3024
//       Email: pcpatel@tva.gov
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  02/18/2007 - Pinal C. Patel
//       Generated original version of source code.
//  01/23/2008 - Pinal C. Patel
//       Added thread safety to all FindDataBlock() methods.
//       Recoded RequestDataBlock() method to include the logic to use previously used partially filled 
//       data blocks first.
//  03/31/2008 - Pinal C. Patel
//       Removed intervaled persisting of FAT since FAT is persisted when new block is requested.
//       Recoded RequestDataBlock() method to speed up the block request process based on the block index 
//       suggestion provided from the state information of the point.
//  07/14/2008 - Pinal C. Patel
//       Added overload to GetDataBlock() method that takes a block index.
//  04/21/2009 - Pinal C. Patel
//       Converted to C#.
//
//*******************************************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TVA.Interop;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// Represents the File Allocation Table of an <see cref="ArchiveFile"/>.
    /// </summary>
    /// <seealso cref="ArchiveFile"/>.
    /// <seealso cref="ArchiveDataBlock"/>
    /// <seealso cref="ArchiveDataBlockPointer"/>
    public class ArchiveFileAllocationTable : ISupportBinaryImage
    {
        #region [ Members ]

        // Constants
        private const int ArrayDescriptorLength = 10;

        // Fields
        private TimeTag m_fileStartTime;
        private TimeTag m_fileEndTime;
        private int m_dataPointsReceived;
        private int m_dataPointsArchived;
        private int m_dataBlockSize;
        private int m_dataBlockCount;
        private List<ArchiveDataBlockPointer> m_dataBlockPointers;
        private ArchiveFile m_parent;
        private int m_searchHistorianId;     // <=|
        private TimeTag m_searchStartTime;  // <=| Used for finding data block pointer in m_dataBlockPointers
        private TimeTag m_searchEndTime;    // <=|

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="ArchiveFileAllocationTable"/> class.
        /// </summary>
        /// <param name="parent">An <see cref="ArchiveFile"/> object.</param>
        internal ArchiveFileAllocationTable(ArchiveFile parent)
        {
            m_parent = parent;
            m_dataBlockPointers = new List<ArchiveDataBlockPointer>();

            if (m_parent.FileData.Length == 0)
            {
                // File is brand new.
                m_fileStartTime = TimeTag.MinValue;
                m_fileEndTime = TimeTag.MinValue;
                m_dataBlockSize = m_parent.DataBlockSize;
                m_dataBlockCount = ArchiveFile.MaximumDataBlocks(m_parent.FileSize, m_parent.DataBlockSize);

                for (int i = 0; i < m_dataBlockCount; i++)
                {
                    m_dataBlockPointers.Add(new ArchiveDataBlockPointer(m_parent, i));
                }
            }
            else
            {
                // File was created previously.
                byte[] fixedFatData = new byte[FixedBinaryLength];
                m_parent.FileData.Seek(-fixedFatData.Length, SeekOrigin.End);
                m_parent.FileData.Read(fixedFatData, 0, fixedFatData.Length);
                FileStartTime = new TimeTag(EndianOrder.LittleEndian.ToDouble(fixedFatData, 0));
                FileEndTime = new TimeTag(EndianOrder.LittleEndian.ToDouble(fixedFatData, 8));
                DataPointsReceived = EndianOrder.LittleEndian.ToInt32(fixedFatData, 16);
                DataPointsArchived = EndianOrder.LittleEndian.ToInt32(fixedFatData, 20);
                DataBlockSize = EndianOrder.LittleEndian.ToInt32(fixedFatData, 24);
                DataBlockCount = EndianOrder.LittleEndian.ToInt32(fixedFatData, 28);

                byte[] variableFatData = new byte[m_dataBlockCount * ArchiveDataBlockPointer.ByteCount];
                m_parent.FileData.Seek(-(variableFatData.Length + FixedBinaryLength), SeekOrigin.End);
                m_parent.FileData.Read(variableFatData, 0, variableFatData.Length);
                for (int i = 0; i < m_dataBlockCount; i++)
                {
                    m_dataBlockPointers.Add(new ArchiveDataBlockPointer(m_parent, i, variableFatData, i * ArchiveDataBlockPointer.ByteCount, variableFatData.Length));
                }
            }
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of the oldest <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not between 01/01/1995 and 01/19/2063.</exception>
        public TimeTag FileStartTime
        {
            get
            {
                return m_fileStartTime;
            }
            set
            {
                if (value < TimeTag.MinValue || value > TimeTag.MaxValue)
                    throw new ArgumentException("Value must between 01/01/1995 and 01/19/2063.");

                m_fileStartTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TimeTag"/> of the newest <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not between 01/01/1995 and 01/19/2063.</exception>
        public TimeTag FileEndTime
        {
            get
            {
                return m_fileEndTime;
            }
            set
            {
                if (value < TimeTag.MinValue || value > TimeTag.MaxValue)
                    throw new ArgumentException("Value must between 01/01/1995 and 01/19/2063.");

                m_fileEndTime = value;
            }
        }

        /// <summary>
        /// Gets or sets the number <see cref="ArchiveData"/> points received by the <see cref="ArchiveFile"/> for archival.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not positive or zero.</exception>
        public int DataPointsReceived
        {
            get
            {
                return m_dataPointsReceived;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive or zero.");

                m_dataPointsReceived = value;
            }
        }

        /// <summary>
        /// Gets or sets the number <see cref="ArchiveData"/> points archived by the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Value being set is not positive or zero.</exception>
        public int DataPointsArchived
        {
            get
            {
                return m_dataPointsArchived;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Value must be positive or zero.");

                m_dataPointsArchived = value;
            }
        }

        /// <summary>
        /// Gets the size (in KB) of a single <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlockSize
        {
            get
            {
                return m_dataBlockSize;
            }
            private set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_dataBlockSize = value;
            }
        }

        /// <summary>
        /// Gets the total number of <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlockCount
        {
            get
            {
                return m_dataBlockCount;
            }
            private set
            {
                if (value < 1)
                    throw new ArgumentException("Value must be positive.");

                m_dataBlockCount = value;
            }
        }

        /// <summary>
        /// Gets the number of used <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlocksUsed
        {
            get
            {
                return m_dataBlockCount - DataBlocksAvailable;
            }
        }

        /// <summary>
        /// Gets the number of unused <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        public int DataBlocksAvailable
        {
            get
            {
                ArchiveDataBlock unusedDataBlock = FindDataBlock(-1);
                if (unusedDataBlock != null)
                    return m_dataBlockCount - unusedDataBlock.Index;
                else
                    return 0;
            }
        }

        /// <summary>
        /// Gets the <see cref="ArchiveDataBlockPointer"/>s to the <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/>.
        /// </summary>
        /// <remarks>
        /// WARNING: <see cref="DataBlockPointers"/> is thread unsafe. Synchronized access is required.
        /// </remarks>
        public IList<ArchiveDataBlockPointer> DataBlockPointers
        {
            get
            {
                return m_dataBlockPointers.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return VariableBinaryLength + FixedBinaryLength;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="ArchiveFileAllocationTable"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[BinaryLength];

                Array.Copy(VariableBinaryImage, 0, image, 0, VariableBinaryLength);
                Array.Copy(FixedBinaryImage, 0, image, VariableBinaryLength, FixedBinaryLength);

                return image;
            }
        }

        private long DataBinaryLength
        {
            get
            {
                return (m_dataBlockCount * (m_dataBlockSize * 1024));
            }
        }

        private int FixedBinaryLength
        {
            get
            {
                return 32;
            }
        }

        private int VariableBinaryLength
        {
            get
            {
                // We add the extra bytes for the array descriptor that required for reading the file from VB.
                return (ArrayDescriptorLength + (m_dataBlockCount * ArchiveDataBlockPointer.ByteCount));
            }
        }

        private byte[] FixedBinaryImage
        {
            get
            {
                byte[] fixedImage = new byte[FixedBinaryLength];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_fileStartTime.Value), 0, fixedImage, 0, 8);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_fileEndTime.Value), 0, fixedImage, 8, 8);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataPointsReceived), 0, fixedImage, 16, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataPointsArchived), 0, fixedImage, 20, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataBlockSize), 0, fixedImage, 24, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_dataBlockCount), 0, fixedImage, 28, 4);

                return fixedImage;
            }
        }

        private byte[] VariableBinaryImage
        {
            get
            {
                byte[] variableImage = new byte[VariableBinaryLength];
                VBArrayDescriptor arrayDescriptor = VBArrayDescriptor.OneBasedOneDimensionalArray(m_dataBlockCount);

                Array.Copy(arrayDescriptor.BinaryImage, 0, variableImage, 0, arrayDescriptor.BinaryLength);
                lock (m_dataBlockPointers)
                {
                    for (int i = 0; i < m_dataBlockPointers.Count; i++)
                    {
                        Array.Copy(m_dataBlockPointers[i].BinaryImage, 0, variableImage, (i * ArchiveDataBlockPointer.ByteCount) + arrayDescriptor.BinaryLength, ArchiveDataBlockPointer.ByteCount);
                    }
                }

                return variableImage;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Saves the <see cref="ArchiveFileAllocationTable"/> data to the <see cref="ArchiveFile"/>.
        /// </summary>
        public void Save()
        {
            // Leave space for data blocks.
            lock (m_parent.FileData)
            {
                m_parent.FileData.Seek(DataBinaryLength, SeekOrigin.Begin);
                m_parent.FileData.Write(BinaryImage, 0, BinaryLength);
                if (!m_parent.CacheWrites)
                    m_parent.FileData.Flush();
            }
        }

        /// <summary>
        /// Extends the <see cref="ArchiveFile"/> by one <see cref="ArchiveDataBlock"/>.
        /// </summary>
        public void Extend()
        {
            Extend(1);
        }

        /// <summary>
        /// Extends the <see cref="ArchiveFile"/> by the specified number of <see cref="ArchiveDataBlock"/>s.
        /// </summary>
        /// <param name="dataBlocksToAdd">Number of <see cref="ArchiveDataBlock"/>s to add to the <see cref="ArchiveFile"/>.</param>
        public void Extend(int dataBlocksToAdd)
        {
            // Extend the FAT and persist it to the disk.
            ArchiveDataBlockPointer blockPointer;
            lock (m_dataBlockPointers)
            {
                for (int i = 1; i <= dataBlocksToAdd; i++)
                {
                    blockPointer = new ArchiveDataBlockPointer(m_parent, m_dataBlockPointers.Count);
                    blockPointer.DataBlock.Reset();

                    m_dataBlockPointers.Add(blockPointer);
                }
            }
            m_dataBlockCount += dataBlocksToAdd;
            Save();
        }

        /// <summary>
        /// Returns an <see cref="ArchiveDataBlock"/> for writting <see cref="ArchiveData"/> points for the specified <paramref name="historianId"/>.
        /// </summary>
        /// <param name="historianId">Historian identifier for which the <see cref="ArchiveDataBlock"/> is being requested.</param>
        /// <param name="dataTime"><see cref="TimeTag"/> of the <see cref="ArchiveData"/> point to be written to the <see cref="ArchiveDataBlock"/>.</param>
        /// <param name="blockIndex"><see cref="ArchiveDataBlock.Index"/> of the <see cref="ArchiveDataBlock"/> last used for writting <see cref="ArchiveData"/> points for the <paramref name="historianId"/>.</param>
        /// <returns><see cref="ArchiveDataBlock"/> object if available; otherwise null if all <see cref="ArchiveDataBlock"/>s have been allocated.</returns>
        public ArchiveDataBlock RequestDataBlock(int historianId, TimeTag dataTime, int blockIndex)
        {
            ArchiveDataBlock dataBlock = null;
            ArchiveDataBlockPointer dataBlockPointer = null;
            if (blockIndex >= 0 && blockIndex < m_dataBlockCount)
            {
                // The specified block index is valid so we retrieve the corresponding data block.
                lock (m_dataBlockPointers)
                {
                    dataBlockPointer = m_dataBlockPointers[blockIndex];
                }
                if (dataBlockPointer.HistorianId == -1 || dataBlockPointer.HistorianId == historianId)
                {
                    // The data block is either unallocated or allocated to the specified historian identifier.
                    dataBlock = dataBlockPointer.DataBlock;
                    if (dataBlockPointer.HistorianId == -1 && dataBlock.SlotsUsed > 0)
                    {
                        // Reset the data block since it's marked as unallocated but has data in it.
                        dataBlock.Reset();
                    }
                }
                else
                {
                    // Invalid block index was specified.
                    throw (new InvalidOperationException(string.Format("Invalid block index {0} specified for Historian Id {1} - Block used by Historian Id {2} for data from {3}.", blockIndex, historianId, dataBlockPointer.HistorianId, dataBlockPointer.StartTime)));
                }
            }
            else if (blockIndex < 0)
            {
                // A negative block index is specified indicating a search must be performed for the data block.
                dataBlock = FindLastDataBlock(historianId);
                if (dataBlock != null && dataBlock.SlotsAvailable == 0)
                {
                    // We found a previously used data block for the specified historian identifier but it's full.
                    dataBlock = null;
                }

                if (dataBlock == null)
                {
                    // We try to find the first unallocated data block and use it if one is found.
                    dataBlock = FindDataBlock(-1);

                    if (dataBlock == null)
                    {
                        // Extend the file by one data block for allocation only if the request is for historic writes.
                        if (m_parent.FileType == ArchiveFileType.Historic)
                        {
                            Extend();
                            dataBlock = m_dataBlockPointers[m_dataBlockPointers.Count - 1].DataBlock;
                        }
                    }
                    else
                    {
                        // Reset the unallocated data block if there is data in it.
                        if (dataBlock.SlotsUsed > 0)
                        {
                            dataBlock.Reset();
                        }
                    }
                }

                // Get the pointer to the data block so that its information can be updated if necessary.
                if (dataBlock != null)
                {
                    lock (m_dataBlockPointers)
                    {
                        dataBlockPointer = m_dataBlockPointers[dataBlock.Index];
                    }
                }
            }

            if (dataBlock != null && dataBlock.SlotsUsed == 0)
            {
                // We must update the data block pointer since we're allocating a new data block.
                dataBlockPointer.HistorianId = historianId;
                dataBlockPointer.StartTime = dataTime;
                if (m_fileStartTime == TimeTag.MinValue)
                    m_fileStartTime = dataTime;

                lock (m_parent.FileData)
                {
                    // We'll write information about the just allocated data block to the file.
                    m_parent.FileData.Seek(DataBinaryLength + ArrayDescriptorLength + (dataBlockPointer.Index * ArchiveDataBlockPointer.ByteCount), SeekOrigin.Begin);
                    m_parent.FileData.Write(dataBlockPointer.BinaryImage, 0, ArchiveDataBlockPointer.ByteCount);
                    // We'll also write the fixed part of the FAT data that resides at the end.
                    m_parent.FileData.Seek(-FixedBinaryLength, SeekOrigin.End);
                    m_parent.FileData.Write(FixedBinaryImage, 0, FixedBinaryLength);
                    if (!m_parent.CacheWrites)
                        m_parent.FileData.Flush();
                }
            }

            return dataBlock;
        }

        /// <summary>
        /// Returns the first <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/> for the specified <paramref name="historianId"/>.
        /// </summary>
        /// <param name="historianId">Historian identifier whose <see cref="ArchiveDataBlock"/> is to be retrieved.</param>
        /// <returns><see cref="ArchiveDataBlock"/> object if a match is found; otherwise null.</returns>
        public ArchiveDataBlock FindDataBlock(int historianId)
        {
            ArchiveDataBlockPointer pointer = null;
            lock (m_dataBlockPointers)
            {
                // Setup the search criteria to find the first data block pointer for the specified id.
                m_searchHistorianId = historianId;
                m_searchStartTime = TimeTag.MinValue;
                m_searchEndTime = TimeTag.MaxValue;

                pointer = m_dataBlockPointers.Find(FindDataBlockPointer);
            }

            if (pointer == null)
                return null;
            else
                return pointer.DataBlock;
        }

        /// <summary>
        /// Returns the last <see cref="ArchiveDataBlock"/> in the <see cref="ArchiveFile"/> for the specified <paramref name="historianId"/>.
        /// </summary>
        /// <param name="historianId">Historian identifier.</param>
        /// <returns><see cref="ArchiveDataBlock"/> object if a match is found; otherwise null.</returns>
        public ArchiveDataBlock FindLastDataBlock(int historianId)
        {
            ArchiveDataBlockPointer pointer = null;
            lock (m_dataBlockPointers)
            {
                // Setup the search criteria to find the last data block pointer for the specified id.
                m_searchHistorianId = historianId;
                m_searchStartTime = TimeTag.MinValue;
                m_searchEndTime = TimeTag.MaxValue;

                pointer = m_dataBlockPointers.FindLast(FindDataBlockPointer);
            }

            if (pointer == null)
                return null;
            else
                return pointer.DataBlock;
        }

        /// <summary>
        /// Returns all <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/> for the specified <paramref name="historianId"/>.
        /// </summary>
        /// <param name="historianId">Historian identifier.</param>
        /// <returns>A collection of <see cref="ArchiveDataBlock"/>s.</returns>
        public IList<ArchiveDataBlock> FindDataBlocks(int historianId)
        {
            return FindDataBlocks(historianId, TimeTag.MinValue);
        }

        /// <summary>
        /// Returns all <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/> for the specified <paramref name="historianId"/> with <see cref="ArchiveData"/> points later than the specified <paramref name="startTime"/>.
        /// </summary>
        /// <param name="historianId">Historian identifier.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/>.</param>
        /// <returns>A collection of <see cref="ArchiveDataBlock"/>s.</returns>
        public IList<ArchiveDataBlock> FindDataBlocks(int historianId, TimeTag startTime)
        {
            return FindDataBlocks(historianId, startTime, TimeTag.MaxValue);
        }

        /// <summary>
        /// Returns all <see cref="ArchiveDataBlock"/>s in the <see cref="ArchiveFile"/> for the specified <paramref name="historianId"/> with <see cref="ArchiveData"/> points between the specified <paramref name="startTime"/> and <paramref name="endTime"/>.
        /// </summary>
        /// <param name="historianId">Historian identifier.</param>
        /// <param name="startTime">Start <see cref="TimeTag"/>.</param>
        /// <param name="endTime">End <see cref="TimeTag"/>.</param>
        /// <returns>A collection of <see cref="ArchiveDataBlock"/>s.</returns>
        public IList<ArchiveDataBlock> FindDataBlocks(int historianId, TimeTag startTime, TimeTag endTime)
        {
            List<ArchiveDataBlockPointer> blockPointers = null;
            lock (m_dataBlockPointers)
            {
                // Setup the search criteria to find all data block pointers for the specified point id
                // that fall between the specified start and end time.
                m_searchHistorianId = historianId;
                m_searchStartTime = (startTime != null ? startTime : TimeTag.MinValue);
                m_searchEndTime = (endTime != null ? endTime : TimeTag.MaxValue);

                blockPointers = m_dataBlockPointers.FindAll(FindDataBlockPointer);
            }

            // Build a list of data blocks that correspond to the found data block pointers.
            List<ArchiveDataBlock> blocks = new List<ArchiveDataBlock>();
            for (int i = 0; i < blockPointers.Count; i++)
            {
                blocks.Add(blockPointers[i].DataBlock);
            }

            return blocks;
        }

        /// <summary>
        /// Initializes <see cref="ArchiveFileAllocationTable"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="ArchiveFileAllocationTable"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="ArchiveFileAllocationTable"/>.</returns>
        /// <exception cref="NotSupportedException">Always</exception>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Finds <see cref="ArchiveDataBlockPointer"/> that match the search criteria that is determined by member variables.
        /// </summary>
        private bool FindDataBlockPointer(ArchiveDataBlockPointer dataBlockPointer)
        {
            if (dataBlockPointer != null)
                // Note: The StartTime value of the pointer is ignored if m_searchStartTime = TimeTag.MinValue and
                //       m_searchEndTime = TimeTag.MaxValue. In this case only the PointID value is compared. This
                //       comes in handy when the first or last pointer is to be found from the list of pointers for
                //       a point ID in addition to all the pointer for a point ID.
                return ((dataBlockPointer.HistorianId == m_searchHistorianId) &&
                        (m_searchStartTime == TimeTag.MinValue || dataBlockPointer.StartTime >= m_searchStartTime) &&
                        (m_searchEndTime == TimeTag.MaxValue || dataBlockPointer.StartTime <= m_searchEndTime));
            else
                return false;
        }

        #endregion
    }
}