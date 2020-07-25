Imports System.IO
Imports System.Reflection

Public Class DattaRecordExisting
    Inherits DattaRecord

    Private Sub New()
    End Sub

    Private DeliveredObject As Object

    Function BuildNextDattaRecord() As DattaRecord
        If HasChanges() Then
            If Me.DeliveredObject Is Nothing Then
                Dim Ret = New DattaRecordRemoved(Me.Dattabase, DeliveryObject, Me.Version + 1)
                Return Ret
            Else
                Dim ExistsRemovedObject = Me.Internals.Objects.SingleOrDefault(Function(x) x.ClassNo = Me.ClassNo And x.Id = Me.Id And x.Version = Me.Version + 1 And x.GetType = GetType(DattaRecordRemoved))

                If ExistsRemovedObject IsNot Nothing Then
                    Return Nothing
                End If

                Dim Ret = New DattaRecordModified(Me.Dattabase, DeliveryObject, Me.Version + 1)
                Return Ret
            End If
        Else
            Return Nothing
        End If
    End Function

    Private Function HasChanges() As Boolean
        If Me.DeliveredObject Is Nothing Then
            Return True
        Else

            Dim NewJson As String = Serializers.ToJson(DeliveredObject).Trim
            Dim OldJson As String = Me.JsonStr.Trim

            If NewJson <> OldJson Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

    Function DeliveryObject() As Object
        If Me.DeliveredObject Is Nothing Then
            Me.DeliveredObject = GetOldObject()
        End If

        Return DeliveredObject
    End Function

    Private Function GetOldObject() As Object
        Dim Ret = Serializers.FromJson(Me.JsonStr, Me.ObjectType)
        Return Ret
    End Function

    Private HumanLastVersionPosition As Long
    Private BinaryLastVersionPosition As Long

    Sub ChangeLastVersinToFalse(Fs As FileStream)
        Dim OriginalPosition = Fs.Position

        'Fs.Seek(Me.HumanLastVersionPosition, SeekOrigin.Begin)
        'Fs.WriteByte(45) '45 -

        Me.IsLastVersion = False
        Fs.Seek(Me.BinaryLastVersionPosition, SeekOrigin.Begin)
        Fs.WriteByte(0)

        Fs.Seek(OriginalPosition, SeekOrigin.Begin)
    End Sub

    Shared Function FromStream(Dattabase As Dattabase, ObjectType As Type, Stream As Stream) As DattaRecord
        Dim Internals As DattaInternals = Dattabase
        Dim DBKey As Byte() = Internals.DBKey

        Dim InitialPosition As Long = Stream.Position

        Dim Buff(FixedBinaryHeaderSize - 1) As Byte
        Stream.Read(Buff, 0, FixedBinaryHeaderSize)

        Dim R As New DattaRecordExisting()
        R.ObjectType = ObjectType
        R.Dattabase = Dattabase

        R.IsLastVersion = BitConverter.ToBoolean(Buff, 0)

        Select Case Buff(1)
            Case 1
                R.State = DattaState.Added
            Case 2
                R.State = DattaState.Modified
            Case 3
                R.State = DattaState.Removed
        End Select

        R.ClassNo = BitConverter.ToInt16(Buff, 2)

        R.BinaryLastVersionPosition = InitialPosition

        R.Id = BitConverter.ToInt32(Buff, 4)
        R.Version = BitConverter.ToInt16(Buff, 8)

        Dim InfoLen = BitConverter.ToInt16(Buff, 10)
        Dim JsonLen As Integer

        If Dattabase.Header.DattaVersion = 2 And Buff(14) = 32 And Buff(15) = 32 Then
            'Version 1 uses 16 bits (Short)
            JsonLen = BitConverter.ToInt16(Buff, 12)
        Else
            JsonLen = BitConverter.ToInt32(Buff, 12)
        End If

        ReDim R._InfoBytes(InfoLen - 1)
        Stream.Read(R._InfoBytes, 0, InfoLen)

        If DBKey IsNot Nothing Then
            Dim EncryptedJsonBytes(JsonLen - 1) As Byte
            Stream.Read(EncryptedJsonBytes, 0, JsonLen)

            R._JsonBytes = Packing.Unpack(EncryptedJsonBytes, DBKey)
        Else
            ReDim R._JsonBytes(JsonLen - 1)
            Stream.Read(R._JsonBytes, 0, JsonLen)
        End If

        Stream.ReadByte() 'CR
        Stream.ReadByte() 'LF
        Stream.ReadByte() 'CR
        Stream.ReadByte() 'LF

        Return R
    End Function

    Private _InfoBytes As Byte()
    Public Overrides ReadOnly Property InfoBytes As Byte()
        Get
            Return _InfoBytes
        End Get
    End Property

    Private _InfoStr As String
    Public Overrides ReadOnly Property InfoStr As String
        Get
            If String.IsNullOrEmpty(_InfoStr) Then
                _InfoStr = System.Text.Encoding.UTF8.GetString(_InfoBytes)
            End If

            Return _InfoStr
        End Get
    End Property

    Public Overrides ReadOnly Property InfoLen As Short
        Get
            Return _InfoBytes.Length
        End Get
    End Property


    Private _JsonBytes As Byte()
    Public Overrides ReadOnly Property JsonBytes As Byte()
        Get
            Return _JsonBytes
        End Get
    End Property

    Private _JsonStr As String
    Public Overrides ReadOnly Property JsonStr As String
        Get
            If String.IsNullOrEmpty(_JsonStr) Then
                _JsonStr = System.Text.Encoding.UTF8.GetString(_JsonBytes)
            End If

            Return _JsonStr
        End Get
    End Property

    Public Overrides ReadOnly Property JsonLen As Integer
        Get
            Return _JsonBytes.Length
        End Get
    End Property

End Class
