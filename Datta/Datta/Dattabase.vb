Imports System.IO
Imports System.Reflection

Public MustInherit Class Dattabase
    Implements DattaInternals, IDisposable

    Private Path As String

    Function DattaFilePath() As String
        Return Path
    End Function

    Friend Header As DattaHeader
    'Private Dattables As List(Of Dattable(Of Object))

    Private Property Objects As List(Of DattaRecord) Implements DattaInternals.Objects
    Private Property DBKey As Byte() Implements DattaInternals.DBKey

    Private Function BuildPath() As String
        Dim ApplicationDataFolder As String = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        Dim DattaDefaultFolder As String = IO.Path.Combine(ApplicationDataFolder, "Datta")

        If Not My.Computer.FileSystem.DirectoryExists(DattaDefaultFolder) Then
            My.Computer.FileSystem.CreateDirectory(DattaDefaultFolder)
        End If

        Dim Path = IO.Path.Combine(DattaDefaultFolder, Application.ProductName & ".datta")
        Return Path
    End Function

    Sub New()
        Dim Path As String = BuildPath()
        Me.Initialize(Path, "Admin", "")
    End Sub

    Sub New(Username As String, Password As String)
        Dim Path As String = BuildPath()

        Me.Initialize(Path, Username, Password)
    End Sub

    Sub New(Path As String, Username As String, Password As String)
        Me.Initialize(Path, Username, Password)
    End Sub

    Private CurrentDattaVersion = 2

    Sub Initialize(Path As String, Username As String, Password As String)
        Me.Path = Path

        'Intializing Objects Lists
        Objects = New List(Of DattaRecord)

        'Initializing DattaHeader
        If My.Computer.FileSystem.FileExists(Path) Then
            Header = DattaHeader.GetDattaHeaderValue(Me)

            Dim DattaUser = Header.DattaUsers.SingleOrDefault(Function(x) x.Username = Username)

            If DattaUser Is Nothing Then
                Throw New DattaUserNotFoundException
            End If

            If DattaUser.ProtectedDBKey IsNot Nothing Then
                Me.DBKey = Packing.Unpack(DattaUser.ProtectedDBKey, Password)
            End If

            If Header.DattaVersion = 1 Then
                'JsonLen at 12: short >> integer
                Header.DattaVersion = 2
            End If
        Else
            Dim DattaUser As New DattaUser()
            DattaUser.Username = Username

            If Not String.IsNullOrEmpty(Password) Then
                Me.DBKey = Packing.GenerateAESKey
                DattaUser.ProtectedDBKey = Packing.Pack(DBKey, Password)
            End If

            Header = New DattaHeader(Me, CurrentDattaVersion)
            Header.DattaUsers.Add(DattaUser)

            SaveChanges()
        End If

        'Initializing Dattables
        Dim Props = MyClass.GetType.GetProperties()

        For Each P In Props
            Dim PropertyGenericTypeDefinition = P.PropertyType.GetGenericTypeDefinition

            Dim CompareGenericTypeDefinition = GetType(Dattable(Of ))

            If PropertyGenericTypeDefinition = CompareGenericTypeDefinition Then
                Dim Dattable1Type = GetType(Dattable1(Of ))
                Dim GenericDattable1Type = Dattable1Type.MakeGenericType(P.PropertyType.GetGenericArguments)

                Dim NewD = Activator.CreateInstance(GenericDattable1Type, Me)
                P.SetValue(Me, NewD)
            End If
        Next

    End Sub

    Public Function AllDattables() As List(Of Object)
        Dim ret As New List(Of Object)

        Dim Props = MyClass.GetType.GetProperties()

        For Each P In Props
            Dim PropertyGenericTypeDefinition = P.PropertyType.GetGenericTypeDefinition

            Dim CompareGenericTypeDefinition = GetType(Dattable(Of ))

            If PropertyGenericTypeDefinition = CompareGenericTypeDefinition Then
                ret.Add(P.GetValue(Me))
            End If
        Next

        Return ret
    End Function

    Public Sub AddDattaUser(Username As String, Password As String)
        If DBKey.Length = 0 Then
            Throw New DattabaseIsNotEncryptedException
        End If

        If String.IsNullOrEmpty(Username) Then
            Throw New ArgumentNullException(Username)
        End If

        If String.IsNullOrEmpty(Password) Then
            Throw New ArgumentNullException(Password)
        End If

        Dim DattaUser As New DattaUser()
        DattaUser.Username = Username
        DattaUser.ProtectedDBKey = Packing.Pack(DBKey, Password)
        Header.DattaUsers.Add(DattaUser)
    End Sub

    Public Sub RemoveDattaUser(Username As String)
        Dim DattaUser As DattaUser = Header.DattaUsers.SingleOrDefault(Function(x) x.Username = Username)

        If DattaUser Is Nothing Then
            Throw New DattaUserNotFoundException
        End If

        Header.DattaUsers.Remove(DattaUser)
    End Sub

    Private Function GetReadOnlyFileStream() As FileStream Implements DattaInternals.GetReadOnlyFileStream
        If Not My.Computer.FileSystem.FileExists(Me.Path) Then
            Throw New FileNotFoundException
        End If

        Return New FileStream(Me.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
    End Function

    Sub SaveChanges()
        'Header
        Using Fs As New FileStream(Me.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)
            Dim HeaderBytes = Header.GetDattaHeaderBytes
            Fs.Seek(0, SeekOrigin.Begin)
            Fs.Write(HeaderBytes, 0, HeaderBytes.Length)
        End Using


        'Body
        Using Fs As New FileStream(Me.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)
            'Existing Objects Modified
            For Each R In Me.Objects.OfType(Of DattaRecordExisting)()
                Dim NewR = R.BuildNextDattaRecord

                If NewR IsNot Nothing Then
                    If NewR.Version = 1 Then
                        'Don't allow remove object that not in database
                        Continue For
                    End If

                    If Me.Objects.OfType(Of DattaRecordModified).SingleOrDefault(Function(x) x.ClassNo = NewR.ClassNo And x.Id = NewR.Id And x.Version = NewR.Version) IsNot Nothing Then
                        'That happens when 
                        '   1) the structe of object has change
                        '   2) user submit a new modified version of existing object, but like lik a new object
                        '   3) the BuildNextDattaRecord will call haschanges
                        '   4) haschanges will return true, because the structure changes but no data changes
                        '   5) no data changes, because that existing object was retrieved only to get a version of him.
                        'To reproduce
                        '   1) Create a new type
                        '   2) Add some new object, and save
                        '   3) Change object structure adding some new boolean not null property for example
                        '   4) Change and existing object
                        '   5) Save changes

                        Continue For
                    End If


                    R.ChangeLastVersinToFalse(Fs)

                    Dim Buff = NewR.GetBytes
                    Fs.Seek(0, SeekOrigin.End)
                    Fs.Write(Buff, 0, Buff.Length)
                End If
            Next

            'New/Modified Objects... thats means... objects that alredy exists in .datta file, but user request a db.xyz.add(new...), so that is an "new" object but.. it's already exists
            For Each R In Me.Objects.OfType(Of DattaRecordModified)()
                Dim ExistR As DattaRecordExisting = Me.Objects.SingleOrDefault(Function(x) x.ClassNo = R.ClassNo And x.Id = R.Id And x.Version = R.Version - 1)
                If ExistR IsNot Nothing Then
                    ExistR.ChangeLastVersinToFalse(Fs)
                End If


                Dim Buff = R.GetBytes
                Fs.Seek(0, SeekOrigin.End)
                Fs.Write(Buff, 0, Buff.Length)
            Next

            'Existing Objects Removed
            For Each R In Me.Objects.OfType(Of DattaRecordRemoved)()
                Dim ExistR As DattaRecordExisting = Me.Objects.Single(Function(x) x.ClassNo = R.ClassNo And x.Id = R.Id And x.Version = R.Version - 1)
                ExistR.ChangeLastVersinToFalse(Fs)

                Dim Buff = R.GetBytes
                Fs.Seek(0, SeekOrigin.End)
                Fs.Write(Buff, 0, Buff.Length)
            Next

            'New Objects
            For Each R In Me.Objects.OfType(Of DattaRecordNew)()
                If R.Version > 1 Then
                    Dim ExistR As DattaRecordExisting = Me.Objects.Single(Function(x) x.ClassNo = R.ClassNo And x.Id = R.Id And x.Version = R.Version - 1)
                    ExistR.ChangeLastVersinToFalse(Fs)
                End If

                Dim Buff = R.GetBytes
                Fs.Seek(0, SeekOrigin.End)
                Fs.Write(Buff, 0, Buff.Length)
            Next
        End Using

    End Sub


    Private Function GetObjectKey(O As Object) As PropertyInfo
        Try

            Dim Ps = O.GetType.GetProperties.Where(Function(x) x.GetCustomAttribute(GetType(KeyAttribute)) IsNot Nothing)

            If Ps.Count > 1 Then
                Throw New Exception("This version of Datta is not allowed use multiple keys in object (doble primary keys)")
            End If

            Return Ps.Single

        Catch ex As Exception
        End Try

        Try
            Return O.GetType.GetProperty("Id")
        Catch ex As Exception
        End Try

        Return Nothing
    End Function

    Private Function GetObjectId(O As Object) As Int32 Implements DattaInternals.GetObjectId
        Dim P = GetObjectKey(O)

        If P Is Nothing Then
            Throw New DattaObjectKeyMissingException
        End If

        If P.PropertyType <> GetType(Integer) Then
            Throw New DattaObjectKeyInvalidTypeException
        End If

        Dim R = P.GetValue(O)

        Return R
    End Function

    'Real Class that will replace the user datta-property denifition
    Private Class Dattable1(Of TSource)
        Inherits Dattable(Of TSource)

        Sub New(Dattabase As Dattabase)
            MyBase.New(Dattabase)
        End Sub
    End Class

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not Me.disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
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
