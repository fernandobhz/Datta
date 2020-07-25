Imports System.Security.Cryptography
Imports System.Text
Imports System.IO

Friend Class Hashing

    Friend Shared Function StringToMd5(s As String) As String
        Dim InputBuff() As Byte = ASCIIEncoding.UTF8.GetBytes(s)
        Using MD5 As New MD5CryptoServiceProvider()
            Dim MD5Buff() As Byte = MD5.ComputeHash(InputBuff)
            Dim intTotal As Integer = (CInt(MD5Buff.Length * 2) + CInt((MD5Buff.Length / 8)))
            Dim strRes As StringBuilder = New StringBuilder(intTotal)
            Dim intI As Integer

            For intI = 0 To MD5Buff.Length - 1
                strRes.Append(BitConverter.ToString(MD5Buff, intI, 1))
            Next intI

            Return strRes.ToString().TrimEnd(New Char() {" "c}).ToLower
        End Using
    End Function

    Friend Shared Function StringToMd5Guid(s As String) As Guid
        Dim InputBuff() As Byte = ASCIIEncoding.UTF8.GetBytes(s)

        Using MD5 As New MD5CryptoServiceProvider()
            Dim MD5Buff() As Byte = MD5.ComputeHash(InputBuff)
            Dim G As New Guid(MD5Buff)
            Return G
        End Using
    End Function

    Friend Shared Function FileToMd5(ByVal filePath As String)
        Using MD5 As MD5CryptoServiceProvider = New MD5CryptoServiceProvider
            Using InputFile = New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 64 * 1024)
                MD5.ComputeHash(InputFile)
            End Using

            Dim Hash As Byte() = MD5.Hash
            Dim Buff As StringBuilder = New StringBuilder
            Dim HashByte As Byte

            For Each HashByte In Hash
                Buff.Append(String.Format("{0:x2}", HashByte))
            Next

            Dim MD5String As String
            MD5String = Buff.ToString()

            Return MD5String
        End Using
    End Function

End Class
