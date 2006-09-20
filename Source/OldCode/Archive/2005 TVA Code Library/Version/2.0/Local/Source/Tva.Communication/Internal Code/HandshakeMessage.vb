' 07-06-06

<Serializable()> _
Friend Class HandshakeMessage

    Private m_id As Guid
    Private m_passphrase As String

    Public Sub New(ByVal id As Guid, ByVal passphrase As String)
        Me.ID = id
        Me.Passphrase = passphrase
    End Sub

    Public Property ID() As Guid
        Get
            Return m_id
        End Get
        Set(ByVal value As Guid)
            m_id = value
        End Set
    End Property

    Public Property Passphrase() As String
        Get
            Return m_passphrase
        End Get
        Set(ByVal value As String)
            m_passphrase = value
        End Set
    End Property

End Class
