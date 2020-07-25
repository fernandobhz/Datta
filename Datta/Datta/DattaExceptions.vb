Public Class DattaException
    Inherits Exception

End Class

Public Class InvalidBinaryHeaderSizeException
    Inherits DattaException
End Class

Public Class DattaAlreadyExistsException
    Inherits DattaException

    Property O As Object

    Sub New(O As Object)
        Me.O = O
    End Sub

End Class

Public Class DattaUnexpectedFileFormatException
    Inherits DattaException

    Property DetailedError As String

    Sub New()

    End Sub

    Sub New(DetailedError As String)
        Me.DetailedError = DetailedError
    End Sub
End Class

Public Class DattaObjectKeyMissingException
    Inherits DattaException
End Class

Public Class DattaObjectKeyInvalidTypeException
    Inherits DattaException
End Class

Public Class DattaHeaderSizeExceedException
    Inherits DattaException

End Class

Public Class DattaUserNotFoundException
    Inherits DattaException
End Class

Public Class DattabaseIsNotEncryptedException
    Inherits DattaException

End Class