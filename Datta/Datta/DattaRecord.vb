Imports System.IO
Imports System.Reflection

Public MustInherit Class DattaRecord

    Property Id As Int32
    Property Version As Short
    Property ClassNo As Short
    Property IsLastVersion As Boolean
    Property State As DattaState


    MustOverride ReadOnly Property InfoStr As String
    MustOverride ReadOnly Property InfoBytes As Byte()
    MustOverride ReadOnly Property InfoLen As Short

    MustOverride ReadOnly Property JsonStr As String
    MustOverride ReadOnly Property JsonBytes As Byte()
    MustOverride ReadOnly Property JsonLen As Integer

    Protected Const FixedBinaryHeaderSize = 30
    Protected Dattabase As Dattabase
    Protected ObjectType As Type

    Protected ReadOnly Property Internals As DattaInternals
        Get
            Return Me.Dattabase
        End Get
    End Property

    ReadOnly Property ClassName As String
        Get
            Return ObjectType.Name
        End Get
    End Property

    ReadOnly Property IsAlive As Boolean
        Get
            If Me.State = DattaState.Removed Then
                Return False
            Else
                Return Me.IsLastVersion
                'If Me.IsLastVersion Then
                '    Return True
                'Else
                '    Return False
                'End If
            End If
        End Get
    End Property

    'MustOverride Function GetStoredObject() As Object

    Protected Function GetReadableHeader()
        Dim Ret = String.Format("{0}://{1}:{2}@{3}:{4}", Me.State.ToString, Me.Id, Me.Version, Me.ObjectType.Name, Me.ClassNo)
        Return Ret
    End Function


    Function GetBytes() As Byte()
        Dim DBKey As Byte() = Me.Internals.DBKey

        Dim BodyBytes As Byte()
        Dim BodyLen As Integer

        If DBKey IsNot Nothing Then
            BodyBytes = Packing.Pack(Me.JsonBytes, DBKey)
            BodyLen = BodyBytes.Length
        Else
            BodyBytes = Me.JsonBytes
            BodyLen = Me.JsonLen
        End If


        If BodyLen <> BodyBytes.Length Then
            Throw New Exception("BodyLen is different of BodyBytes.Length")
        End If

        If InfoLen <> InfoBytes.Length Then
            Throw New Exception("InfoLen is different of InfoBytes.Length")
        End If


        Using Ms As New MemoryStream
            Ms.WriteByte(If(Me.IsLastVersion, 1, 0))
            Ms.WriteByte(Me.State)
            Ms.Write(BitConverter.GetBytes(Me.ClassNo), 0, 2)
            Ms.Write(BitConverter.GetBytes(Me.Id), 0, 4)
            Ms.Write(BitConverter.GetBytes(Me.Version), 0, 2)
            Ms.Write(BitConverter.GetBytes(Me.InfoLen), 0, 2)
            Ms.Write(BitConverter.GetBytes(BodyLen), 0, 4)

            For i = 1 To 12
                Ms.WriteByte(32)
            Next

            Ms.WriteByte(13)
            Ms.WriteByte(10)

            If Ms.Length <> FixedBinaryHeaderSize Then
                Throw New InvalidBinaryHeaderSizeException
            End If

            Ms.Write(Me.InfoBytes, 0, Me.InfoBytes.Length)
            Ms.Write(BodyBytes, 0, BodyLen)
            Ms.WriteByte(13)
            Ms.WriteByte(10)
            Ms.WriteByte(13)
            Ms.WriteByte(10)


            Dim Ret = Ms.ToArray

            Return Ret
        End Using

    End Function
End Class

