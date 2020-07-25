Imports System.IO
Imports System.Reflection

Public Class DattaRecordOld
    Inherits DattaRecord

    Private Sub New()
    End Sub

    Overrides Function GetStoredObject() As Object
        Dim Ret = Serializers.FromJson(Me.JsonStr, Me.ObjectType)
        Return Ret
    End Function

    Shared Function FromStream(Dattabase As Dattabase, ObjectType As Type, Stream As Stream) As DattaRecord
        Dim Buff(FixedBinaryHeaderSize - 1) As Byte
        Stream.Read(Buff, 0, FixedBinaryHeaderSize)

        Dim R As New DattaRecordOld()
        R.ObjectType = ObjectType
        R.Dattabase = Dattabase
        R.ClassNo = BitConverter.ToInt16(Buff, 0)
        R.IsLastVersion = BitConverter.ToBoolean(Buff, 2)

        R.Id = BitConverter.ToInt32(Buff, 3)
        R.Version = BitConverter.ToInt16(Buff, 7)

        Dim InfoLen = BitConverter.ToInt16(Buff, 9)
        Dim JsonLen = BitConverter.ToInt16(Buff, 11)

        ReDim R._InfoBytes(InfoLen - 1)
        Stream.Read(R._InfoBytes, 0, InfoLen)

        ReDim R._JsonBytes(JsonLen - 1)
        Stream.Read(R._JsonBytes, 0, JsonLen)

        Return R
    End Function

    Private _InfoBytes As Byte()
    Public Overrides ReadOnly Property InfoBytes As Byte()
        Get
            Return _InfoBytes
        End Get
    End Property

    Public Overrides ReadOnly Property InfoLen As Short
        Get
            Return _InfoBytes.Length
        End Get
    End Property

    Public Overrides ReadOnly Property InfoStr As String
        Get
            If String.IsNullOrEmpty(_InfoStr) Then
                _InfoStr = System.Text.Encoding.UTF8.GetString(_InfoBytes)
            End If

            Return _InfoStr
        End Get
    End Property

    Private _JsonBytes As Byte()
    Public Overrides ReadOnly Property JsonBytes As Byte()
        Get
            Return _JsonBytes
        End Get
    End Property

    Public Overrides ReadOnly Property JsonLen As Short
        Get
            Return _JsonBytes.Length
        End Get
    End Property

    Public Overrides ReadOnly Property JsonStr As String
        Get
            If String.IsNullOrEmpty(_JsonStr) Then
                _JsonStr = System.Text.Encoding.UTF8.GetString(_JsonBytes)
            End If

            Return _JsonStr
        End Get
    End Property

End Class
