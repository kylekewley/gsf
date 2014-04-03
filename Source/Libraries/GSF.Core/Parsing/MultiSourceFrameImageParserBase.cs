﻿//******************************************************************************************************
//  MultiSourceFrameImageParserBase.cs - Gbtc
//
//  Copyright © 2012, Grid Protection Alliance.  All Rights Reserved.
//
//  Licensed to the Grid Protection Alliance (GPA) under one or more contributor license agreements. See
//  the NOTICE file distributed with this work for additional information regarding copyright ownership.
//  The GPA licenses this file to you under the Eclipse Public License -v 1.0 (the "License"); you may
//  not use this file except in compliance with the License. You may obtain a copy of the License at:
//
//      http://www.opensource.org/licenses/eclipse-1.0.php
//
//  Unless agreed to in writing, the subject software distributed under the License is distributed on an
//  "AS-IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. Refer to the
//  License for the specific language governing permissions and limitations.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  12/03/2008 - J. Ritchie Carroll
//       Generated original version of source code.
//  06/19/2009 - Pinal C. Patel
//       Parsed output is now being delivered in a new collection instead of reusing a single collection.
//  08/07/2009 - Josh L. Patterson
//       Edited Comments.
//  09/14/2009 - Stephen C. Wills
//       Added new header and license agreement.
//  11/01/2012 - J. Ritchie Carroll
//       Updated to use concurrent dictionaries and new recreate parsed output list.
//  12/14/2012 - Starlynn Danyelle Gilliam
//       Modified Header.
//
//******************************************************************************************************

#pragma warning disable 0809

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using GSF.Collections;

namespace GSF.Parsing
{
    /// <summary>
    /// This class defines a basic implementation of parsing functionality suitable for automating the parsing of multiple
    /// binary data streams, each represented as frames with common headers and returning the parsed data via an event.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This parser is designed as a write-only stream such that data can come from any source.
    /// </para>
    /// <para>
    /// This class is more specific than the <see cref="BinaryImageParserBase"/> in that it can automate the parsing of a
    /// particular protocol that is formatted as a series of frames that have a common method of identification.
    /// Automation of type creation occurs by loading implementations of common types that implement the
    /// <see cref="ISupportSourceIdentifiableFrameImage{TSourceIdentifier,TTypeIdentifier}"/> interface. The common method of
    /// identification is handled by creating a class derived from the <see cref="ICommonHeader{TTypeIdentifier}"/> which primarily
    /// includes a TypeID property, but also should include any state information needed to parse a particular frame if necessary.
    /// Derived classes override the <see cref="FrameImageParserBase{TTypeIdentifier, TOutputType}.ParseCommonHeader"/>
    /// method in order to parse the <see cref="ICommonHeader{TTypeIdentifier}"/> from a provided binary image.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSourceIdentifier">Type of identifier for the data source.</typeparam>
    /// <typeparam name="TTypeIdentifier">Type of identifier used to distinguish output types.</typeparam>
    /// <typeparam name="TOutputType">Type of the interface or class used to represent outputs.</typeparam>
    public abstract class MultiSourceFrameImageParserBase<TSourceIdentifier, TTypeIdentifier, TOutputType> : FrameImageParserBase<TTypeIdentifier, TOutputType> where TOutputType : ISupportSourceIdentifiableFrameImage<TSourceIdentifier, TTypeIdentifier>
    {
        #region [ Members ]

        // Nested Types

        /// <summary>
        /// Represents a source identifiable buffer.
        /// </summary>
        /// <remarks>
        /// This class implements <see cref="ISupportLifecycle"/> such that it will support
        /// automatic object pool handling, e.g., returning object to pool when disposed.
        /// </remarks>
        public class SourceIdentifiableBuffer : ISupportLifecycle
        {
            #region [ Members ]

            // Events

            /// <summary>
            /// Occurs when <see cref="SourceIdentifiableBuffer"/> is disposed.
            /// </summary>
            public event EventHandler Disposed;

            // Fields

            /// <summary>
            /// Defines the source identifier of the <see cref="Buffer"/>.
            /// </summary>
            public TSourceIdentifier Source;

            /// <summary>
            /// Defines the buffer which is identifiable by its associated <see cref="Source"/>.
            /// </summary>
            public byte[] Buffer;

            private int m_count;
            private bool m_disposed;

            #endregion

            #region [ Constructors ]

            /// <summary>
            /// Releases the unmanaged resources before the <see cref="SourceIdentifiableBuffer"/> object is reclaimed by <see cref="GC"/>.
            /// </summary>
            ~SourceIdentifiableBuffer()
            {
                Dispose(false);
            }

            #endregion

            #region [ Properties ]

            /// <summary>
            /// Gets or sets enabled state of <see cref="SourceIdentifiableBuffer"/>.
            /// </summary>
            public bool Enabled
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets valid number of bytes within the <see cref="Buffer"/>.
            /// </summary>
            /// <remarks>   
            /// This property will automatically initialize buffer. Set to zero to release buffer. 
            /// </remarks>
            public int Count
            {
                get
                {
                    return m_count;
                }
                set
                {
                    m_count = value;

                    if ((object)Buffer != null)
                        BufferPool.ReturnBuffer(Buffer);

                    if (m_count > 0)
                        Buffer = BufferPool.TakeBuffer(m_count);
                    else
                        Buffer = null;
                }
            }

            #endregion

            #region [ Methods ]

            /// <summary>
            /// Releases all the resources used by the <see cref="SourceIdentifiableBuffer"/> object.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases the unmanaged resources used by the <see cref="SourceIdentifiableBuffer"/> object and optionally releases the managed resources.
            /// </summary>
            /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
            protected virtual void Dispose(bool disposing)
            {
                if (!m_disposed)
                {
                    try
                    {
                        // Return any existing buffer to the pool
                        Count = 0;
                    }
                    finally
                    {
                        m_disposed = true;  // Prevent duplicate dispose.

                        if (Disposed != null)
                            Disposed(this, EventArgs.Empty);
                    }
                }
            }

            /// <summary>
            /// Initializes (or reinitializes) <see cref="SourceIdentifiableBuffer"/> state.
            /// </summary>
            public void Initialize()
            {
                // Un-dispose class instance if it is being reused
                if (m_disposed)
                {
                    m_disposed = false;
                    GC.ReRegisterForFinalize(this);
                }
            }

            #endregion
        }

        // Events

        /// <summary>
        /// Occurs when a data image is deserialized successfully to one or more of the output types that the data image represents.
        /// </summary>
        /// <remarks>
        /// <see cref="EventArgs{T1,T2}.Argument1"/> is the ID of the source for the data image.<br/>
        /// <see cref="EventArgs{T1,T2}.Argument2"/> is a list of objects deserialized from the data image.
        /// </remarks>
        [Description("Occurs when a data image is deserialized successfully to one or more object of the Type that the data image was for.")]
        public event EventHandler<EventArgs<TSourceIdentifier, IList<TOutputType>>> SourceDataParsed;

        // Fields
        private readonly AsyncDoubleBufferedQueue<SourceIdentifiableBuffer> m_bufferQueue;
        private readonly ConcurrentDictionary<TSourceIdentifier, bool> m_sourceInitialized;
        private readonly ConcurrentDictionary<TSourceIdentifier, byte[]> m_unparsedBuffers;
        private readonly ConcurrentDictionary<TSourceIdentifier, List<TOutputType>> m_parsedSourceData;
        private TSourceIdentifier m_source;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Creates a new instance of the <see cref="MultiSourceFrameImageParserBase{TSourceIdentifier,TTypeIdentifier,TOutputType}"/> class.
        /// </summary>
        protected MultiSourceFrameImageParserBase()
        {
            m_bufferQueue = new AsyncDoubleBufferedQueue<SourceIdentifiableBuffer>();
            m_bufferQueue.ProcessItemsFunction = ParseQueuedBuffers;
            m_bufferQueue.ProcessException += ProcessExceptionHandler;

            if (ProtocolUsesSyncBytes)
                m_sourceInitialized = new ConcurrentDictionary<TSourceIdentifier, bool>();

            m_unparsedBuffers = new ConcurrentDictionary<TSourceIdentifier, byte[]>();
            m_parsedSourceData = new ConcurrentDictionary<TSourceIdentifier, List<TOutputType>>();
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Gets the total number of buffers that are currently queued for processing, if any.
        /// </summary>
        public virtual int QueuedBuffers
        {
            get
            {
                return m_bufferQueue.Count;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Start the streaming data parser.
        /// </summary>
        public override void Start()
        {
            base.Start();

            if ((object)m_sourceInitialized != null)
                m_sourceInitialized.Clear();

            m_unparsedBuffers.Clear();
            m_parsedSourceData.Clear();
        }

        /// <summary>
        /// Starts the data parser given the specified type implementations.
        /// </summary>
        /// <param name="implementations">Output type implementations to establish for the parser.</param>
        public override void Start(IEnumerable<Type> implementations)
        {
            base.Start(implementations);

            if ((object)m_sourceInitialized != null)
                m_sourceInitialized.Clear();

            m_unparsedBuffers.Clear();
            m_parsedSourceData.Clear();
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">Identifier of the data source.</param>
        /// <param name="buffer">An array of bytes to queue for parsing</param>
        public virtual void Parse(TSourceIdentifier source, byte[] buffer)
        {
            if ((object)buffer == null)
                throw new ArgumentNullException("buffer");

            Parse(source, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Queues a sequence of bytes, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">Identifier of the data source.</param>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the queue.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <remarks>
        /// This method associates a buffer with its data source identifier.
        /// </remarks>
        public virtual void Parse(TSourceIdentifier source, byte[] buffer, int offset, int count)
        {
            SourceIdentifiableBuffer identifiableBuffer = null;

            buffer.ValidateParameters(offset, count);

            try
            {
                if (count > 0)
                {
                    // Get an identifiable buffer object
                    identifiableBuffer = ReusableObjectPool<SourceIdentifiableBuffer>.Default.TakeObject();
                    identifiableBuffer.Source = source;
                    identifiableBuffer.Count = count;

                    // Copy buffer data for processing (destination buffer pre-allocated from buffer pool)
                    Buffer.BlockCopy(buffer, offset, identifiableBuffer.Buffer, 0, count);

                    // Add buffer to the queue for parsing
                    m_bufferQueue.Enqueue(new SourceIdentifiableBuffer[] { identifiableBuffer });
                }
            }
            catch
            {
                // Return source buffer to object pool if we failed to queue it for processing
                if ((object)identifiableBuffer != null)
                    identifiableBuffer.Dispose();

                throw;
            }
        }

        /// <summary>
        /// Queues the object implementing the <see cref="ISupportBinaryImage"/> interface, from the specified data source, onto the stream for parsing.
        /// </summary>
        /// <param name="source">Identifier of the data source.</param>
        /// <param name="image">Object to be parsed that implements the <see cref="ISupportBinaryImage"/> interface.</param>
        /// <remarks>
        /// This method takes the binary image from <see cref="ISupportBinaryImage"/> and writes the buffer to the <see cref="BinaryImageParserBase"/> stream for parsing.
        /// </remarks>
        public virtual void Parse(TSourceIdentifier source, ISupportBinaryImage image)
        {
            Parse(source, image.BinaryImage());
        }

        /// <summary>
        /// Clears the internal buffer of unparsed data received from the specified <paramref name="source"/>.
        /// </summary>
        /// <param name="source">Identifier of the data source.</param>
        /// <remarks>
        /// This method can be used to ensure that partial data received from the <paramref name="source"/> is not kept in memory indefinitely.
        /// </remarks>
        public virtual void PurgeBuffer(TSourceIdentifier source)
        {
            byte[] buffer;
            m_unparsedBuffers.TryRemove(source, out buffer);
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,ISupportBinaryImage)"/> method instead to make sure data source source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This method should not be called directly.</exception>
        /// <param name="image">A <see cref="ISupportBinaryImage"/>.</param>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("MultiSourceFrameImageParserBase requires consumers call Parse overload that takes data source identifier as an argument", true)]
        public override void Parse(ISupportBinaryImage image)
        {
            throw new NotImplementedException("This method should not be called directly, call the Parse(TSourceIdentifier,ISupportBinaryImage) method to queue data for parsing instead.");
        }

        /// <summary>
        /// Not implemented. Consumers should call the <see cref="Parse(TSourceIdentifier,byte[],int,int)"/> method instead to make sure data source source ID gets tracked with data buffer.
        /// </summary>
        /// <exception cref="NotImplementedException">This method should not be called directly.</exception>
        /// <param name="buffer">A <see cref="Byte"/> array.</param>
        /// <param name="count">An <see cref="Int32"/> for the offset.</param>
        /// <param name="offset">An <see cref="Int32"/> for the count.</param>
        [EditorBrowsable(EditorBrowsableState.Never), Obsolete("MultiSourceFrameImageParserBase requires consumers call Parse overload that takes data source identifier as an argument", true)]
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException("This method should not be called directly, call the Parse(TSourceIdentifier,byte[],int,int) method to queue data for parsing instead.");
        }

        // This method is used by the internal <see cref="ProcessQueue{T}"/> to process all queued data buffers.
        private void ParseQueuedBuffers(IList<SourceIdentifiableBuffer> buffers)
        {
            if (Enabled)
            {
                try
                {
                    // Process all queued data buffers...
                    foreach (SourceIdentifiableBuffer buffer in buffers)
                    {
                        // Track current buffer source
                        m_source = buffer.Source;

                        // Check to see if this data source has been initialized
                        if ((object)m_sourceInitialized != null)
                            StreamInitialized = m_sourceInitialized.GetOrAdd(m_source, true);

                        // Restore any unparsed buffers for this data source, if any
                        UnparsedBuffer = m_unparsedBuffers.GetOrAdd(m_source, (byte[])null);

                        // Start parsing sequence for this buffer - this will begin publication of new parsed outputs
                        base.Write(buffer.Buffer, 0, buffer.Count);

                        // Track last unparsed buffer for this data source
                        m_unparsedBuffers[m_source] = UnparsedBuffer;
                    }
                }
                finally
                {
                    // If user has attached to SourceDataParsed event, expose list of parsed data per source
                    if ((object)SourceDataParsed != null)
                    {
                        foreach (KeyValuePair<TSourceIdentifier, List<TOutputType>> parsedData in m_parsedSourceData)
                        {
                            OnSourceDataParsed(parsedData.Key, parsedData.Value);
                        }

                        // Clear parsed data dictionary for next pass
                        m_parsedSourceData.Clear();
                    }

                }
            }

            // Dispose of source buffers - no rush
            ThreadPool.QueueUserWorkItem(DisposeSourceBuffers, buffers);
        }

        // Handle disposing of source buffers (this will return items to the reusable object pool)
        private void DisposeSourceBuffers(object state)
        {
            SourceIdentifiableBuffer[] buffers = state as SourceIdentifiableBuffer[];

            if ((object)buffers != null)
            {
                foreach (SourceIdentifiableBuffer buffer in buffers)
                {
                    if ((object)buffer != null)
                        buffer.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="FrameImageParserBase{TTypeIdentifier, TOutputType}.DataParsed"/> event.
        /// </summary>
        /// <param name="output">The object that was deserialized from binary image.</param>
        protected override void OnDataParsed(TOutputType output)
        {
            // Make sure output gets associated with its data source prior to publication
            output.Source = m_source;

            base.OnDataParsed(output);

            // If user has attached to SourceDataParsed event, track parsed data per source
            if ((object)SourceDataParsed != null)
            {
                List<TOutputType> sourceData = m_parsedSourceData.GetOrAdd(output.Source, id => new List<TOutputType>());
                sourceData.Add(output);
            }
        }

        /// <summary>
        /// Raises the <see cref="SourceDataParsed"/> event.
        /// </summary>
        /// <param name="source">Identifier for the data source.</param>
        /// <param name="output">The objects that were deserialized from binary images from the <paramref name="source"/> data stream.</param>
        protected virtual void OnSourceDataParsed(TSourceIdentifier source, IList<TOutputType> output)
        {
            if ((object)SourceDataParsed != null)
                SourceDataParsed(this, new EventArgs<TSourceIdentifier, IList<TOutputType>>(source, output));
        }

        /// <summary>
        /// Raises the <see cref="BinaryImageParserBase.DataDiscarded"/> event.
        /// </summary>
        /// <param name="buffer">Source buffer that contains output that failed to parse.</param>
        protected override void OnDataDiscarded(byte[] buffer)
        {
            // If an error occurs during parsing from a data source, we reset its initialization state
            if ((object)m_sourceInitialized != null)
                m_sourceInitialized[m_source] = false;

            base.OnDataDiscarded(buffer);
        }

        // We just bubble any exceptions captured in process queue out to parsing exception event...
        private void ProcessExceptionHandler(object sender, EventArgs<Exception> e)
        {
            OnParsingException(e.Argument);
        }

        #endregion
    }
}
