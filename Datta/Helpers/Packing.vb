Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO.Compression
Imports System.Threading

Friend Class Packing

    Shared Function GenerateRSA_AESKey(ByVal RSAPublicKey As String, ByRef outAESKey As Byte()) As Byte()

        Dim AES As New AesCryptoServiceProvider
        outAESKey = AES.Key

        Dim RSA As New RSACryptoServiceProvider
        RSA.FromXmlString(RSAPublicKey)

        Dim RSA_AESKey As Byte() = RSA.Encrypt(outAESKey, False)

        Return RSA_AESKey

    End Function

    Shared Sub ParseRSA_AESKey(RSA_AESKey As Byte(), ByVal RSAPrivateKey As String, ByRef outAESKey As Byte())

        Dim RSA As New RSACryptoServiceProvider
        RSA.FromXmlString(RSAPrivateKey)

        outAESKey = RSA.Decrypt(RSA_AESKey, False)

    End Sub

    Shared Function GenerateAESKey() As Byte()
        Using AES As New AesCryptoServiceProvider
            Return AES.Key
        End Using
    End Function

    Shared Sub GenerateRSAKey(ByRef RSAPublicKey As String, ByRef RSAPrivateKey As String)

        Using RSA As New RSACryptoServiceProvider(2048)
            RSAPublicKey = RSA.ToXmlString(includePrivateParameters:=False)
            RSAPrivateKey = RSA.ToXmlString(includePrivateParameters:=True)
        End Using

    End Sub


    Shared Function PackString(inStr As String) As Byte()
        Dim AESKey(31) As Byte
        Return PackString(inStr, AESKey)
    End Function

    Shared Function PackString(inStr As String, ByVal AESKey As String) As Byte()
        Dim inBuff As Byte() = System.Text.Encoding.UTF8.GetBytes(inStr)
        Dim AesKeyBytes() As Byte = System.Text.Encoding.ASCII.GetBytes(AESKey)
        Return Pack(inBuff, AesKeyBytes)
    End Function

    Shared Function PackString(inStr As String, ByVal AESKey As Byte()) As Byte()
        Dim inBuff As Byte() = System.Text.Encoding.ASCII.GetBytes(inStr)
        Return Pack(inBuff, AESKey)
    End Function

    Shared Function Pack(inBuff As Byte()) As Byte()
        Dim AESKey(31) As Byte
        Return Pack(inBuff, AESKey)
    End Function

    Shared Function Pack(inBuff As Byte(), ByVal AESKey As String) As Byte()
        Return Pack(inBuff, System.Text.Encoding.ASCII.GetBytes(AESKey))
    End Function

    Shared Function Pack(inBuff As Byte(), ByVal AESKey As Byte()) As Byte()

        Dim inMS As New MemoryStream(inBuff)
        Dim outMS As New MemoryStream

        Pack(inMS, outMS, AESKey, Nothing)

        Return outMS.ToArray

    End Function


    Shared Sub Pack(ByRef inStream As Stream, ByRef outStream As Stream, ByRef Progress As Action(Of Integer))
        Dim AESKey(31) As Byte
        Pack(inStream, outStream, AESKey, Progress)
    End Sub

    Shared Sub Pack(ByRef inStream As Stream, ByRef outStream As Stream, ByVal AESKey As String, Progress As Action(Of Integer))
        Pack(inStream, outStream, System.Text.Encoding.ASCII.GetBytes(AESKey), Progress)
    End Sub


    Shared Sub Pack(ByRef inStream As Stream, ByRef outStream As Stream, ByVal AESKey As Byte(), Progress As Action(Of Integer))

        If AESKey.Length > 32 Then
            Throw New ArgumentOutOfRangeException("AESKey", "AESKey is 32 chars max length")
        ElseIf AESKey.Length < 32 Then
            ReDim Preserve AESKey(31)
        End If

        Dim AES As New AesCryptoServiceProvider
        AES.Key = AESKey

        outStream.Write(AES.IV, 0, 16)

        Const BuffSize As Integer = 1024 * 64
        Dim Buff(BuffSize - 1) As Byte

        Using cryptoStream As New CryptoStream(outStream, AES.CreateEncryptor, CryptoStreamMode.Write)
            Using deflateStream As New DeflateStream(cryptoStream, CompressionMode.Compress)

                Dim bytesRead As Integer

                Do
                    bytesRead = inStream.Read(Buff, 0, BuffSize)
                    deflateStream.Write(Buff, 0, bytesRead)

                    If Progress IsNot Nothing Then
                        Progress.Invoke(inStream.Position / inStream.Length * 100)
                    End If

                Loop Until bytesRead = 0

            End Using
        End Using
    End Sub




    Shared Function UnpackString(inBuff As Byte()) As String
        Dim AESKey(31) As Byte
        Return UnpackString(inBuff, AESKey)
    End Function

    Shared Function UnpackString(inBuff As Byte(), ByVal AESKey As String) As String
        Return UnpackString(inBuff, System.Text.Encoding.ASCII.GetBytes(AESKey))
    End Function

    Shared Function UnpackString(inBuff As Byte(), ByVal AESKey As Byte()) As String
        Dim B As Byte() = Unpack(inBuff, AESKey)
        Return System.Text.Encoding.UTF8.GetString(B)
    End Function


    Shared Function Unpack(inBuff As Byte()) As Byte()
        Dim AESKey(31) As Byte
        Return Unpack(inBuff, AESKey)
    End Function

    Shared Function Unpack(inBuff As Byte(), ByVal AESKey As String) As Byte()
        Return Unpack(inBuff, System.Text.Encoding.ASCII.GetBytes(AESKey))
    End Function

    Shared Function Unpack(inBuff As Byte(), ByVal AESKey As Byte()) As Byte()

        Dim inMS As New MemoryStream(inBuff)
        Dim outMS As New MemoryStream

        Unpack(inMS, outMS, AESKey, Nothing)

        Return outMS.ToArray

    End Function

    Shared Sub Unpack(ByRef inStream As Stream, ByRef outStream As Stream, Progress As Action(Of Integer))
        Dim AESKey(31) As Byte
        Unpack(inStream, outStream, AESKey, Progress)
    End Sub

    Shared Sub Unpack(ByRef inStream As Stream, ByRef outStream As Stream, AESKey As String, Progress As Action(Of Integer))
        Unpack(inStream, outStream, System.Text.Encoding.ASCII.GetBytes(AESKey), Progress)
    End Sub


    Shared Sub Unpack(ByRef inStream As Stream, ByRef outStream As Stream, AESKey As Byte(), Progress As Action(Of Integer))

        If AESKey.Length > 32 Then
            Throw New ArgumentOutOfRangeException("AESKey", "AESKey is 32 chars max length")
        ElseIf AESKey.Length < 32 Then
            ReDim Preserve AESKey(31)
        End If

        Dim Iv(15) As Byte
        inStream.Read(Iv, 0, 16)

        Dim AES As New AesCryptoServiceProvider
        AES.Key = AESKey
        AES.IV = Iv

        Const BuffSize As Integer = 1024 * 64
        Dim Buff(BuffSize - 1) As Byte

        Using cryptoStream As New CryptoStream(inStream, AES.CreateDecryptor, CryptoStreamMode.Read)
            Using inflateStream As New DeflateStream(cryptoStream, CompressionMode.Decompress)

                Dim bytesRead As Integer

                Do
                    bytesRead = inflateStream.Read(Buff, 0, BuffSize)
                    outStream.Write(Buff, 0, bytesRead)

                    If Progress IsNot Nothing Then
                        Progress.Invoke(inStream.Position / inStream.Length * 100)
                    End If

                Loop Until bytesRead = 0

            End Using
        End Using
    End Sub

End Class
