Imports System.IO

Public Class DattaHeader
    Private Const MB = 1024 * 1024
    Friend Const HeaderSize As Long = 1 * MB

    Property DattaVersion As Integer
    Property DattaClasses As List(Of DattaClassRecord)
    Property DattaUsers As List(Of DattaUser)

    Private Dattabase As Dattabase

    Sub New()

    End Sub

    Sub New(Dattabase As Dattabase, DattaVersion As Integer)
        Me.Dattabase = Dattabase

        DattaVersion = DattaVersion
        DattaClasses = New List(Of DattaClassRecord)
        DattaUsers = New List(Of DattaUser)
    End Sub

    Function GetClassName(ClassNo As Integer) As String
        Return DattaClasses.SingleOrDefault(Function(x) x.ClassNo = ClassNo).ClassName
    End Function

    Function BuildClassNo(O As Object)
        Dim S As String = O.GetType.Name
        Return BuildClassNo(S)
    End Function

    Function BuildClassNo(S As String)
        Dim C = DattaClasses.SingleOrDefault(Function(x) x.ClassName = S)

        If C IsNot Nothing Then
            Return C.ClassNo
        Else
            Dim MaxNo As Integer

            If DattaClasses.Count > 0 Then
                MaxNo = DattaClasses.Max(Function(x) x.ClassNo)
            Else
                MaxNo = 0
            End If

            Dim NewC As New DattaClassRecord(MaxNo + 1, S)
            DattaClasses.Add(NewC)
            Return NewC.ClassNo
        End If

    End Function

    Function GetDattaHeaderBytes() As Byte()
        Dim Json As String = Serializers.ToJson(Me)
        Dim Buff As Byte() = System.Text.Encoding.UTF8.GetBytes(Json)

        If Buff.Length > HeaderSize - 4 Then
            Throw New DattaHeaderSizeExceedException
        End If

        ReDim Preserve Buff(HeaderSize - 1)
        Buff(HeaderSize - 4) = 13
        Buff(HeaderSize - 3) = 10
        Buff(HeaderSize - 2) = 13
        Buff(HeaderSize - 1) = 10

        Return Buff
    End Function

    Shared Function GetDattaHeaderValue(Datta As DattaInternals) As DattaHeader
        Using Fs = Datta.GetReadOnlyFileStream
            Return GetDattaHeaderValue(Fs)
        End Using
    End Function

    Shared Function GetDattaHeaderValue(Stream As Stream) As DattaHeader
        Dim Buff(HeaderSize - 1) As Byte : Stream.Read(Buff, 0, HeaderSize)
        Dim Json As String = System.Text.Encoding.UTF8.GetString(Buff)

        Dim Ret As DattaHeader = Serializers.FromJson(Json, GetType(DattaHeader))

        Return Ret
    End Function

End Class
