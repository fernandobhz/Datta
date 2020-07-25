Imports System.IO
Imports System.Reflection

Public Class DattaRecordNew
    Inherits DattaRecord

    Private Obj As Object

    Sub New(Dattabase As Dattabase, Obj As Object, Version As Short)
        Me.Dattabase = Dattabase
        Me.ObjectType = Obj.GetType
        Me.Obj = Obj

        Me.Id = Me.Internals.GetObjectId(Obj)
        Me.Version = Version
        Me.ClassNo = Dattabase.Header.BuildClassNo(Obj)
        Me.IsLastVersion = True
        Me.State = DattaState.Added
    End Sub

    Overrides ReadOnly Property InfoStr As String
        Get
            Return GetReadableHeader() & vbCrLf
        End Get
    End Property

    Overrides ReadOnly Property InfoBytes As Byte()
        Get
            Return System.Text.Encoding.UTF8.GetBytes(Me.InfoStr)
        End Get
    End Property

    Overrides ReadOnly Property InfoLen As Short
        Get
            Return Me.InfoBytes.Length
        End Get
    End Property



    Overrides ReadOnly Property JsonStr As String
        Get
            Return Serializers.ToJson(Obj)
        End Get
    End Property

    Overrides ReadOnly Property JsonBytes As Byte()
        Get
            Return System.Text.Encoding.UTF8.GetBytes(Me.JsonStr)
        End Get
    End Property

    Overrides ReadOnly Property JsonLen As Integer
        Get
            Return Me.JsonBytes.Length
        End Get
    End Property

End Class
