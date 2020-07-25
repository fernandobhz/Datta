Imports Datta

Module Main

    Private Username As String = "Admin"
    Private Password As String = ""

    'Verificar se passou senha, se sim e não estiver criptografado o que fazer??? throw exception???


    Sub Main()

        'UsuarioFRGAdicionar()
        'Exit Sub

        'ListaPeloFRG()
        'Exit Sub

        'Lista()
        'Exit Sub

        Adiciona()
        'Lista()

        Modifica()
        'Lista()

        Remove()
        'Lista()

        Adiciona()
        Lista()

    End Sub

    Sub UsuarioFRGAdicionar()
        Using ctx As New Entites(Username, Password)
            ctx.AddDattaUser("frg", "102030")
            ctx.SaveChanges()
        End Using
    End Sub

    Sub ListaPeloFRG()
        Using ctx As New Entites("frg", "102030")
            For Each Xyz In ctx.Xyz.ToList
                MsgBox(Xyz.X & vbCrLf & Xyz.Y & vbCrLf & Xyz.Z)
            Next
        End Using

        MsgBox("Listado todos com sucesso")
    End Sub

    Sub Lista()
        Using ctx As New Entites(Username, Password)
            For Each Xyz In ctx.Xyz.ToList
                MsgBox(Xyz.X & vbCrLf & Xyz.Y & vbCrLf & Xyz.Z)
            Next
        End Using

        MsgBox("Listado todos com sucesso")
    End Sub

    Sub Adiciona()

        Using ctx As New Entites(Username, Password)
            ctx.Xyz.Add(New Xyz With {.X = 1, .Y = "First Record", .Z = "Stuart"})
            ctx.SaveChanges()
        End Using

    End Sub

    Sub Modifica()
        Using ctx As New Entites(Username, Password)
            Dim Stuart = ctx.Xyz.Single(Function(x) x.Z = "Stuart")

            Stuart.Z += " Little"
            ctx.SaveChanges()
        End Using
    End Sub

    Sub Remove()
        Using ctx As New Entites(Username, Password)
            Dim Stuart = ctx.Xyz.Single(Function(x) x.X = 1)
            ctx.Xyz.Remove(Stuart)
            ctx.SaveChanges()
        End Using
    End Sub


End Module

Class Entites
    Inherits Dattabase

    Sub New(Username As String, Password As String)
        MyBase.New(Username, Password)
    End Sub

    Property Xyz As Dattable(Of Xyz)
End Class

Class Xyz
    <Key>
    Property X As Integer
    Property Y As String
    Property Z As String
End Class
