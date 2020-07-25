Public Class DattaClassRecord
    Sub New()
    End Sub

    Sub New(ClassNo As Short, TypeName As String)
        Me.ClassNo = ClassNo
        Me.ClassName = TypeName
    End Sub

    Property ClassNo As Short
    Property ClassName As String
    Property LastId As Integer

End Class
