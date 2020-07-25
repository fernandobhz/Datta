Imports System.IO

Friend Class Streams

    Public Shared Function ReadUntilCRLF(Stream As Stream) As Byte()
        Return ReadUntil(Stream, {13, 10})
    End Function

    Public Shared Function ReadUntilDoubleCRLF(Stream As Stream) As Byte()
        Return ReadUntil(Stream, {13, 10, 13, 10})
    End Function

    Public Shared Function ReadUntil(Stream As Stream, UntilBlock As Byte()) As Byte()

        Dim CompareBlock(UntilBlock.Length - 1) As Byte

        Using MS As New MemoryStream

            Dim j As Integer = 0

            Do
                Dim B As Integer = Stream.ReadByte

                If B < 0 Then
                    Return MS.ToArray
                End If

                MS.WriteByte(B)

                CompareBlock(0) = CompareBlock(1)
                CompareBlock(1) = B


                Dim Flag As Boolean = True

                For i = 0 To UntilBlock.Length - 1
                    If CompareBlock(i) <> UntilBlock(i) Then
                        Flag = False
                        Exit For
                    End If
                Next

                If Flag Then
                    Return MS.ToArray
                End If
            Loop

        End Using

    End Function

End Class
