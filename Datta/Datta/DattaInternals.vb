Public Interface DattaInternals
    Property Objects As List(Of DattaRecord)
    Function GetObjectId(O As Object) As Int32
    Function GetReadOnlyFileStream() As IO.FileStream
    Property DBKey As Byte()
End Interface
