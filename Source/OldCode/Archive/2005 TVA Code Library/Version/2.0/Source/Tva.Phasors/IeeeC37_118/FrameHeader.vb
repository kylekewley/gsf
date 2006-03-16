'*******************************************************************************************************
'  FrameHeader.vb - IEEE C37.118 Shared frame header functions
'  Copyright � 2005 - TVA, all rights reserved - Gbtc
'
'  Build Environment: VB.NET, Visual Studio 2003
'  Primary Developer: James R Carroll, System Analyst [TVA]
'      Office: COO - TRNS/PWR ELEC SYS O, CHATTANOOGA, TN - MR 2W-C
'       Phone: 423/751-2827
'       Email: jrcarrol@tva.gov
'
'  Code Modification History:
'  -----------------------------------------------------------------------------------------------------
'  11/12/2004 - James R Carroll
'       Initial version of source generated
'
'*******************************************************************************************************

Imports Tva.DateTime
Imports Tva.DateTime.Common
Imports Tva.Interop
Imports Tva.Interop.Bit

Namespace IeeeC37_118

    ' This class generates and parses a frame header specfic to C37.118
    <CLSCompliant(False)> _
    Public NotInheritable Class FrameHeader

#Region " Internal Frame Header Instance Class "

        ' This class is used to temporarily hold parsed frame header
        Private Class FrameHeaderInstance

            Implements IFrameHeader

            Private m_frameType As FrameType
            Private m_version As Byte
            Private m_frameLength As Int16
            Private m_idCode As UInt16
            Private m_ticks As Long
            Private m_timeQualityFlags As Int32

            Public ReadOnly Property This() As IFrameHeader
                Get
                    Return Me
                End Get
            End Property

            Public Property FrameType() As FrameType Implements IFrameHeader.FrameType
                Get
                    Return m_frameType
                End Get
                Set(ByVal value As FrameType)
                    m_frameType = value
                End Set
            End Property

            Public Property Version() As Byte Implements IFrameHeader.Version
                Get
                    Return m_version
                End Get
                Set(ByVal value As Byte)
                    m_version = value
                End Set
            End Property

            Public Property FrameLength() As Short Implements IFrameHeader.FrameLength
                Get
                    Return m_frameLength
                End Get
                Set(ByVal value As Short)
                    m_frameLength = value
                End Set
            End Property

            Public Property IDCode() As UInt16 Implements IFrameHeader.IDCode
                Get
                    Return m_idCode
                End Get
                Set(ByVal value As UInt16)
                    m_idCode = value
                End Set
            End Property

            Public Property Ticks() As Long Implements IFrameHeader.Ticks
                Get
                    Return m_ticks
                End Get
                Set(ByVal value As Long)
                    m_ticks = value
                End Set
            End Property

            Private Property InternalTimeQualityFlags() As Int32 Implements IFrameHeader.InternalTimeQualityFlags
                Get
                    Return m_timeQualityFlags
                End Get
                Set(ByVal value As Int32)
                    m_timeQualityFlags = value
                End Set
            End Property

            Public ReadOnly Property SecondOfCentury() As UInt32 Implements IFrameHeader.SecondOfCentury
                Get
                    Return FrameHeader.SecondOfCentury(Me)
                End Get
            End Property

            Public ReadOnly Property FractionOfSecond() As Int32 Implements IFrameHeader.FractionOfSecond
                Get
                    Return FrameHeader.FractionOfSecond(Me)
                End Get
            End Property

            Public Property TimeQualityFlags() As TimeQualityFlags Implements IFrameHeader.TimeQualityFlags
                Get
                    Return FrameHeader.TimeQualityFlags(Me)
                End Get
                Set(ByVal value As TimeQualityFlags)
                    FrameHeader.TimeQualityFlags(Me) = value
                End Set
            End Property

            Public Property TimeQualityIndicatorCode() As TimeQualityIndicatorCode Implements IFrameHeader.TimeQualityIndicatorCode
                Get
                    Return FrameHeader.TimeQualityIndicatorCode(Me)
                End Get
                Set(ByVal value As TimeQualityIndicatorCode)
                    FrameHeader.TimeQualityIndicatorCode(Me) = value
                End Set
            End Property

            Public Property TimeBase() As Int32 Implements IFrameHeader.TimeBase
                Get
                    Return -1
                End Get
                Set(ByVal value As Int32)
                    ' Nothing to do...
                End Set
            End Property

        End Class

#End Region

        Public Const BinaryLength As Int16 = 14
        Public Const TimeQualityFlagsMask As Integer = Bit31 Or Bit30 Or Bit29 Or Bit28 Or Bit27 Or Bit26 Or Bit25 Or Bit24

        Private Sub New()

            ' This class contains only global functions and is not meant to be instantiated

        End Sub

        Public Shared Function ParseBinaryImage(ByVal configurationFrame As ConfigurationFrame, ByVal binaryImage As Byte(), ByVal startIndex As Integer) As IFrameHeader

            If binaryImage(startIndex) <> Common.SyncByte Then Throw New InvalidOperationException("Bad Data Stream: Expected sync byte &HAA as first byte in C37.118 frame, got " & binaryImage(startIndex).ToString("x"c).PadLeft(2, "0"c))

            With New FrameHeaderInstance
                ' Strip out frame type and version information...
                .FrameType = (binaryImage(startIndex + 1) And Not FrameType.VersionNumberMask)
                .Version = (binaryImage(startIndex + 1) And FrameType.VersionNumberMask)

                .FrameLength = EndianOrder.BigEndian.ToInt16(binaryImage, startIndex + 2)
                .IDCode = EndianOrder.BigEndian.ToUInt16(binaryImage, startIndex + 4)

                Dim secondOfCentury As Double = EndianOrder.BigEndian.ToUInt32(binaryImage, startIndex + 6)
                Dim fractionOfSecond As Int32 = EndianOrder.BigEndian.ToInt32(binaryImage, startIndex + 10)

                If configurationFrame Is Nothing OrElse .FrameType = FrameType.ConfigurationFrame1 OrElse .FrameType = FrameType.ConfigurationFrame2 Then
                    ' Without timebase, the best timestamp you can get is down to the whole second
                    .Ticks = (New UnixTimeTag(secondOfCentury)).ToDateTime.Ticks
                Else
                    .Ticks = (New UnixTimeTag(secondOfCentury + (fractionOfSecond And Not TimeQualityFlagsMask) / configurationFrame.TimeBase)).ToDateTime.Ticks
                End If

                .TimeQualityFlags = fractionOfSecond And TimeQualityFlagsMask

                Return .This
            End With

        End Function

        Public Shared Function BinaryImage(ByVal frameHeader As IFrameHeader) As Byte()

            Dim buffer As Byte() = Array.CreateInstance(GetType(Byte), BinaryLength)

            With frameHeader
                buffer(0) = Common.SyncByte
                buffer(1) = .FrameType Or .Version
                EndianOrder.BigEndian.CopyBytes(.FrameLength, buffer, 2)
                EndianOrder.BigEndian.CopyBytes(.IDCode, buffer, 4)
                EndianOrder.BigEndian.CopyBytes(.SecondOfCentury, buffer, 6)
                EndianOrder.BigEndian.CopyBytes(.FractionOfSecond Or .TimeQualityFlags, buffer, 10)
            End With

            Return buffer

        End Function

        Public Shared Sub Clone(ByVal sourceFrameHeader As IFrameHeader, ByVal destinationFrameHeader As IFrameHeader)

            With destinationFrameHeader
                .FrameType = sourceFrameHeader.FrameType
                .Version = sourceFrameHeader.Version
                .FrameLength = sourceFrameHeader.FrameLength
                .IDCode = sourceFrameHeader.IDCode
                .Ticks = sourceFrameHeader.Ticks
                .TimeBase = sourceFrameHeader.TimeBase
                .InternalTimeQualityFlags = sourceFrameHeader.InternalTimeQualityFlags
            End With

        End Sub

        Public Shared WriteOnly Property Version(ByVal frameHeader As IFrameHeader) As Byte
            Set(ByVal value As Byte)
                frameHeader.Version = value And FrameType.VersionNumberMask
            End Set
        End Property

        Public Shared ReadOnly Property SecondOfCentury(ByVal frameHeader As IFrameHeader) As UInt32
            Get
                Return System.Math.Floor(TimeTag(frameHeader).Value)
            End Get
        End Property

        Public Shared ReadOnly Property FractionOfSecond(ByVal frameHeader As IFrameHeader) As Int32
            Get
                Dim seconds As Double = TimeTag(frameHeader).Value
                Return (seconds - System.Math.Floor(seconds)) * frameHeader.TimeBase
            End Get
        End Property

        Public Shared ReadOnly Property TimeTag(ByVal frameHeader As IFrameHeader) As UnixTimeTag
            Get
                Return New UnixTimeTag(frameHeader.Ticks)
            End Get
        End Property

        Public Shared Property TimeQualityFlags(ByVal frameHeader As IFrameHeader) As IeeeC37_118.TimeQualityFlags
            Get
                Return frameHeader.InternalTimeQualityFlags And Not TimeQualityFlags.TimeQualityIndicatorMask
            End Get
            Set(ByVal value As IeeeC37_118.TimeQualityFlags)
                With frameHeader
                    .InternalTimeQualityFlags = (.InternalTimeQualityFlags And IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorMask) Or value
                End With
            End Set
        End Property

        Public Shared Property TimeQualityIndicatorCode(ByVal frameHeader As IFrameHeader) As IeeeC37_118.TimeQualityIndicatorCode
            Get
                Return frameHeader.InternalTimeQualityFlags And IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorMask
            End Get
            Set(ByVal value As IeeeC37_118.TimeQualityIndicatorCode)
                With frameHeader
                    .InternalTimeQualityFlags = (.InternalTimeQualityFlags And Not IeeeC37_118.TimeQualityFlags.TimeQualityIndicatorMask) Or value
                End With
            End Set
        End Property

    End Class

End Namespace
