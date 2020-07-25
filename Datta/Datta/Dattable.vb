Imports System.Linq.Expressions
Imports System.IO

Public MustInherit Class Dattable(Of TSource)

    Private Dattabase As Dattabase

    Private ReadOnly Property Internals As DattaInternals
        Get
            Return Dattabase
        End Get
    End Property

    Protected Sub New(Dattabase As Dattabase)
        Me.Dattabase = Dattabase
    End Sub

    Function ClassName() As String
        Return GetType(TSource).Name
    End Function

    Function NextId() As Integer
        Dim CN As String = ClassName()
        Dim ClassNo As Integer = Me.Dattabase.Header.BuildClassNo(CN) ' Necessary to build DattaClasses record if is a first record of that type
        Dim DC = Me.Dattabase.Header.DattaClasses.SingleOrDefault(Function(x) x.ClassName = CN)

        Dim Ret As Integer = DC.LastId + 1
        DC.LastId = Ret
        Return Ret
    End Function

    Sub Add(ParamArray Objects() As Object)
        For Each O In Objects
            Dim NewVersion As Integer = Me.GetVersion(O) + 1

            If NewVersion = 1 Then
                Dim NewObject = New DattaRecordNew(Me.Dattabase, O, NewVersion)
                Me.Internals.Objects.Add(NewObject)
            Else
                Dim ModifiedObject = New DattaRecordModified(Me.Dattabase, O, NewVersion)
                Me.Internals.Objects.Add(ModifiedObject)
            End If

        Next
    End Sub

    Sub Remove(ParamArray Objects() As Object)
        For Each O In Objects
            Dim NewVersion As Integer = Me.GetVersion(O) + 1
            Dim NewObject = New DattaRecordRemoved(Me.Dattabase, O, NewVersion)

            Me.Internals.Objects.Add(NewObject)
        Next
    End Sub

    Function Where(Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing) As DattaFetch(Of TSource)
        Return New DattaFetch(Of TSource)(Me.Dattabase, Predicate)
    End Function

    Function ToList(Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing) As List(Of TSource)
        Using DattaFetch = Where(Predicate)
            Return DattaFetch.ToList
        End Using
    End Function

    Function Take(Count As Integer, Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing) As List(Of TSource)
        Using DattaFetch = Where(Predicate)
            Return DattaFetch.ToList(Count)
        End Using
    End Function

    Function FirstOrDefault(Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing) As TSource
        Using DattaFetch = Where(Predicate)
            Return DattaFetch.FetchNext
        End Using
    End Function

    Function First(Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing) As TSource
        Dim Obj As TSource = FirstOrDefault(Predicate)

        If Obj Is Nothing Then
            Throw New InvalidOperationException()
        End If

        Return Obj
    End Function

    Function [Single](Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing, Optional ByRef RawRecord As DattaRecordExisting = Nothing) As TSource
        Using DattaFetch As New DattaFetch(Of TSource)(Me.Dattabase, Predicate)
            Dim First As TSource = DattaFetch.FetchNext(RawRecord)

            If First IsNot Nothing Then
                Dim Second As TSource = DattaFetch.FetchNext

                If Second IsNot Nothing Then
                    Throw New InvalidOperationException("source has more than one element")
                Else
                    Return First
                End If
            Else
                Throw New ArgumentNullException("source is Nothing")
            End If
        End Using
    End Function

    Function SingleOrDefault(Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing, Optional ByRef RawRecord As DattaRecordExisting = Nothing) As TSource
        Using DattaFetch As New DattaFetch(Of TSource)(Me.Dattabase, Predicate)
            Dim First As TSource = DattaFetch.FetchNext(RawRecord)

            If First IsNot Nothing Then
                Dim Second As TSource = DattaFetch.FetchNext

                If Second IsNot Nothing Then
                    Throw New InvalidOperationException("source has more than one element")
                Else
                    Return First
                End If
            Else
                Return Nothing
            End If
        End Using
    End Function

    Function Any(Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing) As Boolean
        Using DattaFetch As New DattaFetch(Of TSource)(Me.Dattabase, Predicate)
            Dim First As TSource = DattaFetch.FetchNext

            If First IsNot Nothing Then
                Return True
            Else
                Return False
            End If
        End Using
    End Function

    Function GetSpecificVersion(Id As Int32, Version As Short) As DattaRecordExisting
        Dim Ret = DattaFetch(Of TSource).GetSpecificVersion(Me.Dattabase, Id, Version)
        Return Ret
    End Function

    Function GetPreviousVersion(O As TSource) As DattaRecordExisting
        Dim OId As Int32 = Me.Internals.GetObjectId(O)
        Dim ActualVersion As Short = GetVersion(O)

        Dim Obj = GetSpecificVersion(OId, ActualVersion - 1)
        Return Obj
    End Function

    Function GetVersionHistory(Id As Int32) As List(Of DattaRecordExisting)
        Dim Ret As New List(Of DattaRecordExisting)

        Dim ActualObject As TSource = DattaFetch(Of TSource).GetLiveObject(Me.Dattabase, Id)
        Dim ActualVersion As Short = GetVersion(ActualObject)

        For i = 1 To ActualVersion
            Ret.Add(GetSpecificVersion(Id, i))
        Next

        Return Ret
    End Function

    Function GetVersion(O As TSource) As Short
        Dim OId As Int32 = Me.Internals.GetObjectId(O)
        Dim OClassNo As Short = Me.Dattabase.Header.BuildClassNo(O)

        Dim R = Me.Internals.Objects.SingleOrDefault(Function(x) x.ClassNo = OClassNo And x.Id = OId)

        If R IsNot Nothing Then
            Return R.Version
        Else
            Dim ActualObject = Me.GetLiveObject(OId)

            Dim R2 = Me.Internals.Objects.SingleOrDefault(Function(x) x.ClassNo = OClassNo And x.Id = OId)

            If R2 IsNot Nothing Then
                Return R2.Version
            Else
                Dim RemovedDattaRecord = Me.GetRemovedDattaRecord(OId)

                If RemovedDattaRecord IsNot Nothing Then
                    Return RemovedDattaRecord.Version
                Else
                    Return 0 'There is no version of that object
                End If
            End If
        End If
    End Function

    Function GetRemovedDattaRecord(Id As Int32) As DattaRecordExisting
        Dim Ret = DattaFetch(Of TSource).GetRemovedDattaRecord(Me.Dattabase, Id)
        Return Ret
    End Function

    Function GetLiveObject(Id As Int32, Optional ByRef RawRecord As DattaRecordExisting = Nothing) As TSource
        Dim Ret = DattaFetch(Of TSource).GetLiveObject(Me.Dattabase, Id, RawRecord)
        Return Ret
    End Function

End Class
