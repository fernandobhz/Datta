Imports System.IO
Imports System.Linq.Expressions

Public Class DattaFetch(Of TSource)
    Implements IDisposable

    Private Dattabase As Dattabase
    Private Condition As Func(Of TSource, Boolean)

    Private ClassName As String
    Private ClassNo As Integer

    Private Fs As FileStream

    ReadOnly Property Internals As DattaInternals
        Get
            Return Dattabase
        End Get
    End Property

    Sub New(Dattabase As Dattabase, Optional Predicate As Expression(Of Func(Of TSource, Boolean)) = Nothing)
        Me.Dattabase = Dattabase
        Me.Condition = If(Predicate IsNot Nothing, Predicate.Compile, Nothing)

        Me.ClassName = GetType(TSource).Name
        Me.ClassNo = Dattabase.Header.BuildClassNo(ClassName)

        Me.Fs = Internals.GetReadOnlyFileStream
        Fs.Seek(DattaHeader.HeaderSize, IO.SeekOrigin.Begin)
    End Sub

    Sub New(Dattabase As Dattabase, Optional Condition As Func(Of TSource, Boolean) = Nothing)
        Me.Dattabase = Dattabase
        Me.Condition = Condition

        Me.ClassName = GetType(TSource).Name

        Me.Fs = Internals.GetReadOnlyFileStream
        Fs.Seek(DattaHeader.HeaderSize, IO.SeekOrigin.Begin)
    End Sub

    Function FetchNext(Optional ByRef RawRecord As DattaRecordExisting = Nothing) As TSource
        Do
            If Fs.Position = Fs.Length Then
                Return Nothing
            End If

            Dim R As DattaRecordExisting = DattaRecordExisting.FromStream(Me.Dattabase, GetType(TSource), Fs)

            If R.ClassNo = Me.ClassNo And R.IsAlive Then

                Dim Obj As TSource = R.DeliveryObject

                If Condition IsNot Nothing Then
                    If Condition(Obj) Then
                        RawRecord = R

                        'xpto-dup
                        Dim Existing = Internals.Objects.SingleOrDefault(Function(x) x.ClassNo = Me.ClassNo And x.Id = R.Id)

                        If Existing IsNot Nothing Then
                            Internals.Objects.Remove(Existing)
                        End If
                        'xpto-dup

                        Internals.Objects.Add(R)
                        Return Obj
                    Else
                        Continue Do
                    End If
                Else
                    RawRecord = R
                    Internals.Objects.Add(R)
                    Return Obj
                End If

            End If
        Loop
    End Function

    Function ToList(Optional LimitResult As Integer = 0) As List(Of TSource)
        Dim Ret As New List(Of TSource)

        Do
            Dim O As Object = FetchNext()

            If O IsNot Nothing Then

                'xpto-dup
                Dim DuplicateLastVersion = Ret.SingleOrDefault(Function(x As Object) x.Id = O.Id)

                If DuplicateLastVersion IsNot Nothing Then
                    Ret.Remove(DuplicateLastVersion)
                End If
                'xpto-dup

                Ret.Add(O)

                If Ret.Count = LimitResult Then
                    Return Ret
                End If
            Else
                Return Ret
            End If
        Loop

    End Function

    Shared Function GetSpecificVersion(Dattabase As Dattabase, Id As Int32, Version As Short) As DattaRecordExisting
        Dim Internals As DattaInternals = Dattabase
        Dim ClassName = GetType(TSource).Name
        Dim ClassNo = Dattabase.Header.BuildClassNo(ClassName)

        Dim Fs = Internals.GetReadOnlyFileStream
        Fs.Seek(DattaHeader.HeaderSize, IO.SeekOrigin.Begin)

        Do
            If Fs.Position = Fs.Length Then
                Return Nothing
            End If

            Dim R As DattaRecordExisting = DattaRecordExisting.FromStream(Dattabase, GetType(TSource), Fs)

            If R.ClassNo = ClassNo And R.Id = Id And R.Version = Version Then
                Return R
            End If
        Loop
    End Function

    Shared Function GetRemovedDattaRecord(Dattabase As Dattabase, Id As Int32) As DattaRecordExisting
        Dim Internals As DattaInternals = Dattabase
        Dim ClassName = GetType(TSource).Name
        Dim ClassNo = Dattabase.Header.BuildClassNo(ClassName)

        Dim Fs As FileStream

        Try
            Fs = Internals.GetReadOnlyFileStream
        Catch ex As FileNotFoundException
            Return Nothing
        End Try

        Fs.Seek(DattaHeader.HeaderSize, IO.SeekOrigin.Begin)

        Dim RemovedDattaRecord As DattaRecordExisting = Nothing

        Do
            If Fs.Position = Fs.Length Then
                Return RemovedDattaRecord
            End If

            Dim R As DattaRecordExisting = DattaRecordExisting.FromStream(Dattabase, GetType(TSource), Fs)

            If R.ClassNo = ClassNo And R.Id = Id And R.State = DattaState.Removed Then
                RemovedDattaRecord = R
            End If
        Loop

    End Function

    Shared Function GetLiveObject(Dattabase As Dattabase, Id As Int32, Optional ByRef RawRecord As DattaRecordExisting = Nothing) As TSource
        Dim Internals As DattaInternals = Dattabase
        Dim ClassName = GetType(TSource).Name
        Dim ClassNo = Dattabase.Header.BuildClassNo(ClassName)

        Dim Fs As FileStream

        Try
            Fs = Internals.GetReadOnlyFileStream
        Catch ex As FileNotFoundException
            Return Nothing
        End Try

        Fs.Seek(DattaHeader.HeaderSize, IO.SeekOrigin.Begin)

        Do
            If Fs.Position = Fs.Length Then
                Return Nothing
            End If

            Dim R As DattaRecordExisting = DattaRecordExisting.FromStream(Dattabase, GetType(TSource), Fs)

            If R.ClassNo = ClassNo And R.Id = Id And R.IsAlive Then
                Internals.Objects.Add(R)

                RawRecord = R
                Dim Obj As TSource = R.DeliveryObject
                Return Obj
            End If
        Loop
    End Function

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Fs.Dispose()
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        Me.disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub
#End Region

End Class
