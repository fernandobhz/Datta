Imports System.IO
Imports System.Text

Module TestesPerformance

    Sub TesteBase64()
        '1,92 segundos
        Dim OriginalBuff() As Byte = {&H11, &H11, &H11, &H11, &H0, &H55, &H55, &H66, &H66, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H13, &H10}
        Dim B64 As String = Convert.ToBase64String(OriginalBuff)

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO * 100

            Dim Buff() As Byte = Convert.FromBase64String(B64)

            Dim Id As Integer = BitConverter.ToInt32(Buff, 0)
            Dim Active As Boolean = BitConverter.ToBoolean(Buff, 4)
            Dim Version As Short = BitConverter.ToInt16(Buff, 5)
            Dim Length As Short = BitConverter.ToInt16(Buff, 7)

            'Debug.Print(String.Format("{0} {1} {2} {3}", Id, Active, Version, Length))
        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub

    Sub TesteBinario()
        '0,37s - 10M
        '20x mais rápido do que HexString
        Dim Buff() As Byte = {&H11, &H11, &H11, &H11, &H0, &H55, &H55, &H66, &H66, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H0, &H13, &H10}

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO * 100
            Dim Id As Integer = BitConverter.ToInt32(Buff, 0)
            Dim Active As Boolean = BitConverter.ToBoolean(Buff, 4)
            Dim Version As Short = BitConverter.ToInt16(Buff, 5)
            Dim Length As Short = BitConverter.ToInt16(Buff, 7)

            'Debug.Print(String.Format("{0} {1} {2} {3}", Id, Active, Version, Length))
        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub

    Sub TesteURILikeHeader()
        '5.7s - 10M

        Dim Record As String = "Datta://1.2@Cliente.3(102);".Substring(8)

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO * 1

            Dim Parts() As String = Record.Split(".", ":", "@", "(", ")")

        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)


    End Sub

    Sub TesteURILikeHeader_2()

        Dim Record As String = "Datta://1.2@Cliente.3(102);".Substring(8)

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO * 1
            Dim PI As Integer
            Dim PF As Integer

            PI = 0
            PF = Record.IndexOf(".", PI)
            Dim Id As String = Record.Substring(PI, PF - PI)

            PI = PF + 1
            PF = Record.IndexOf("@", PI)
            Dim Version As String = Record.Substring(PI, PF - PI)


            PI = PF + 1
            PF = Record.IndexOf(".", PI)
            Dim Type As String = Record.Substring(PI, PF - PI)


            PI = PF + 1
            PF = Record.IndexOf("(", PI)
            Dim Schema As String = Record.Substring(PI, PF - PI)


            PI = PF + 1
            PF = Record.IndexOf(")", PI)
            Dim Length As String = Record.Substring(PI, PF - PI)


        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub


    Sub TesteHexString()
        '7,7s - 10M
        Dim Record As String = "1111111100555566660000000000000000001310"

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO * 10
            Dim IdHex As String = Record.Substring(0, 8)
            Dim ActiveHex As String = Record.Substring(8, 2)
            Dim VersionHex As String = Record.Substring(10, 4)
            Dim LengthHex As String = Record.Substring(14, 4)

            Dim Id As Integer = Integer.Parse(IdHex, Globalization.NumberStyles.HexNumber)
            Dim Active As Boolean = Integer.Parse(ActiveHex, Globalization.NumberStyles.HexNumber)
            Dim Version As Short = Integer.Parse(VersionHex, Globalization.NumberStyles.HexNumber)
            Dim Length As Short = Integer.Parse(LengthHex, Globalization.NumberStyles.HexNumber)

            'Debug.Print(String.Format("{0} {1} {2} {3}", Id, Active, Version, Length))
        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub

    Sub TesteHexStringToBuffer()
        '3,8s - 10M
        Dim Record As String = "1111111100555566660000000000000000001310"

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO * 10

            Dim Buff() As Byte = HexStringToBuffer(Record)

            Dim Id As Integer = BitConverter.ToInt32(Buff, 0)
            Dim Active As Boolean = BitConverter.ToBoolean(Buff, 4)
            Dim Version As Short = BitConverter.ToInt16(Buff, 5)
            Dim Length As Short = BitConverter.ToInt16(Buff, 7)

            'Debug.Print(String.Format("{0} {1} {2} {3}", Id, Active, Version, Length))
        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub

    Sub TesteLineByLine()
        Dim S = "1" & vbCrLf & "Fernando" & vbCrLf & vbCrLf

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO
            Dim Ms As New MemoryStream(System.Text.Encoding.UTF8.GetBytes(S))
            Dim SR As New StreamReader(Ms)

            Dim Id As String = SR.ReadLine
            Dim Nome As String = SR.ReadLine

            Dim C As New Cliente(Id, Nome)
        Next

        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub

    Sub TesteJson()
        Dim C1 As New Cliente(1, "Fernando")
        Dim S1 As String = Serializers.ToJson(C1)

        Dim T1 = Date.Now

        For i = 0 To HUM_MILHAO

            Dim C As Cliente = Serializers.FromJson(S1, GetType(Cliente))

            'Debug.Print(String.Format("{0} {1} {2} {3}", Id, Active, Version, Length))
        Next


        Dim Diff = (Date.Now - T1).TotalMilliseconds / 1000

        MsgBox(Diff)

    End Sub

    Function HexStringToByteArray_15secs(S As String) As Byte()
        'Too slow.. 15 seconds vs 3,5 seconds of my implementation - 10M
        If S.Length Mod 2 = 1 Then
            Throw New Exception("HexString must be even")
        End If

        Dim Len As Integer = S.Length / 2

        Dim Buff(Len - 1) As Byte

        Dim j As Integer = 0

        For i = 0 To Len - 1 Step 2

            Buff(j) = Byte.Parse(S(i) & S(i + 1), Globalization.NumberStyles.HexNumber)

            j += 1
        Next

        Return Buff

    End Function

    Public Function StringToByteArrayFastest_14secs(hex As String) As Byte()
        If hex.Length Mod 2 = 1 Then
            Throw New Exception("HexString must be even")
        End If

        Dim Buff(hex.Length - 1) As Byte

        Dim j As Integer = 0

        For i = 0 To hex.Length - 1 Step 2

            Dim i2 As Integer = i + 1

            Dim Asc1 As Integer = AscW(hex(i))
            Dim Asc2 As Integer = AscW(hex(i2))

            Dim B1 As Integer = Asc1 - (If(Asc1 < 58, 48, 55))
            Dim B2 As Integer = Asc2 - (If(Asc2 < 58, 48, 55))

            B1 = B1 << 4

            Dim B As Byte = B1 + B2

            Buff(j) = B1

            j += 1
        Next

        Return Buff
    End Function

    Sub WriteCode()
        Dim Code As New StringBuilder

        For x = 0 To 15

            For xx = 0 To 15

                Dim ss As String = String.Empty

                Select Case x
                    Case 10 : ss = ss & "A"
                    Case 11 : ss = ss & "B"
                    Case 12 : ss = ss & "C"
                    Case 13 : ss = ss & "D"
                    Case 14 : ss = ss & "E"
                    Case 15 : ss = ss & "F"
                    Case Else
                        ss = ss & x
                End Select

                Select Case xx
                    Case 10 : ss = ss & "A"
                    Case 11 : ss = ss & "B"
                    Case 12 : ss = ss & "C"
                    Case 13 : ss = ss & "D"
                    Case 14 : ss = ss & "E"
                    Case 15 : ss = ss & "F"
                    Case Else
                        ss = ss & xx
                End Select


                Code.AppendLine(String.Format("Case ""{0}"" : Buff(j) = &H{0}", ss))

            Next

        Next

        Clipboard.SetText(Code.ToString)
        MsgBox("Code on clipboard")
    End Sub

    Function HexStringToBuffer2(S As String) As Byte()
        'Very Very slow... 7 secs to do 1M... when with two cases... takes 0.5 secs
        If S.Length Mod 2 = 1 Then
            Throw New Exception("HexString must be even")
        End If

        Dim Len As Integer = S.Length / 2

        Dim Buff(Len - 1) As Byte

        Dim j As Integer = 0

        For i = 0 To Len - 1 Step 2

            Dim X As String = S(i) & S(i + 1)

            Select Case X
                Case "00" : Buff(j) = &H0
                Case "01" : Buff(j) = &H1
                Case "02" : Buff(j) = &H2
                Case "03" : Buff(j) = &H3
                Case "04" : Buff(j) = &H4
                Case "05" : Buff(j) = &H5
                Case "06" : Buff(j) = &H6
                Case "07" : Buff(j) = &H7
                Case "08" : Buff(j) = &H8
                Case "09" : Buff(j) = &H9
                Case "0A" : Buff(j) = &HA
                Case "0B" : Buff(j) = &HB
                Case "0C" : Buff(j) = &HC
                Case "0D" : Buff(j) = &HD
                Case "0E" : Buff(j) = &HE
                Case "0F" : Buff(j) = &HF
                Case "10" : Buff(j) = &H10
                Case "11" : Buff(j) = &H11
                Case "12" : Buff(j) = &H12
                Case "13" : Buff(j) = &H13
                Case "14" : Buff(j) = &H14
                Case "15" : Buff(j) = &H15
                Case "16" : Buff(j) = &H16
                Case "17" : Buff(j) = &H17
                Case "18" : Buff(j) = &H18
                Case "19" : Buff(j) = &H19
                Case "1A" : Buff(j) = &H1A
                Case "1B" : Buff(j) = &H1B
                Case "1C" : Buff(j) = &H1C
                Case "1D" : Buff(j) = &H1D
                Case "1E" : Buff(j) = &H1E
                Case "1F" : Buff(j) = &H1F
                Case "20" : Buff(j) = &H20
                Case "21" : Buff(j) = &H21
                Case "22" : Buff(j) = &H22
                Case "23" : Buff(j) = &H23
                Case "24" : Buff(j) = &H24
                Case "25" : Buff(j) = &H25
                Case "26" : Buff(j) = &H26
                Case "27" : Buff(j) = &H27
                Case "28" : Buff(j) = &H28
                Case "29" : Buff(j) = &H29
                Case "2A" : Buff(j) = &H2A
                Case "2B" : Buff(j) = &H2B
                Case "2C" : Buff(j) = &H2C
                Case "2D" : Buff(j) = &H2D
                Case "2E" : Buff(j) = &H2E
                Case "2F" : Buff(j) = &H2F
                Case "30" : Buff(j) = &H30
                Case "31" : Buff(j) = &H31
                Case "32" : Buff(j) = &H32
                Case "33" : Buff(j) = &H33
                Case "34" : Buff(j) = &H34
                Case "35" : Buff(j) = &H35
                Case "36" : Buff(j) = &H36
                Case "37" : Buff(j) = &H37
                Case "38" : Buff(j) = &H38
                Case "39" : Buff(j) = &H39
                Case "3A" : Buff(j) = &H3A
                Case "3B" : Buff(j) = &H3B
                Case "3C" : Buff(j) = &H3C
                Case "3D" : Buff(j) = &H3D
                Case "3E" : Buff(j) = &H3E
                Case "3F" : Buff(j) = &H3F
                Case "40" : Buff(j) = &H40
                Case "41" : Buff(j) = &H41
                Case "42" : Buff(j) = &H42
                Case "43" : Buff(j) = &H43
                Case "44" : Buff(j) = &H44
                Case "45" : Buff(j) = &H45
                Case "46" : Buff(j) = &H46
                Case "47" : Buff(j) = &H47
                Case "48" : Buff(j) = &H48
                Case "49" : Buff(j) = &H49
                Case "4A" : Buff(j) = &H4A
                Case "4B" : Buff(j) = &H4B
                Case "4C" : Buff(j) = &H4C
                Case "4D" : Buff(j) = &H4D
                Case "4E" : Buff(j) = &H4E
                Case "4F" : Buff(j) = &H4F
                Case "50" : Buff(j) = &H50
                Case "51" : Buff(j) = &H51
                Case "52" : Buff(j) = &H52
                Case "53" : Buff(j) = &H53
                Case "54" : Buff(j) = &H54
                Case "55" : Buff(j) = &H55
                Case "56" : Buff(j) = &H56
                Case "57" : Buff(j) = &H57
                Case "58" : Buff(j) = &H58
                Case "59" : Buff(j) = &H59
                Case "5A" : Buff(j) = &H5A
                Case "5B" : Buff(j) = &H5B
                Case "5C" : Buff(j) = &H5C
                Case "5D" : Buff(j) = &H5D
                Case "5E" : Buff(j) = &H5E
                Case "5F" : Buff(j) = &H5F
                Case "60" : Buff(j) = &H60
                Case "61" : Buff(j) = &H61
                Case "62" : Buff(j) = &H62
                Case "63" : Buff(j) = &H63
                Case "64" : Buff(j) = &H64
                Case "65" : Buff(j) = &H65
                Case "66" : Buff(j) = &H66
                Case "67" : Buff(j) = &H67
                Case "68" : Buff(j) = &H68
                Case "69" : Buff(j) = &H69
                Case "6A" : Buff(j) = &H6A
                Case "6B" : Buff(j) = &H6B
                Case "6C" : Buff(j) = &H6C
                Case "6D" : Buff(j) = &H6D
                Case "6E" : Buff(j) = &H6E
                Case "6F" : Buff(j) = &H6F
                Case "70" : Buff(j) = &H70
                Case "71" : Buff(j) = &H71
                Case "72" : Buff(j) = &H72
                Case "73" : Buff(j) = &H73
                Case "74" : Buff(j) = &H74
                Case "75" : Buff(j) = &H75
                Case "76" : Buff(j) = &H76
                Case "77" : Buff(j) = &H77
                Case "78" : Buff(j) = &H78
                Case "79" : Buff(j) = &H79
                Case "7A" : Buff(j) = &H7A
                Case "7B" : Buff(j) = &H7B
                Case "7C" : Buff(j) = &H7C
                Case "7D" : Buff(j) = &H7D
                Case "7E" : Buff(j) = &H7E
                Case "7F" : Buff(j) = &H7F
                Case "80" : Buff(j) = &H80
                Case "81" : Buff(j) = &H81
                Case "82" : Buff(j) = &H82
                Case "83" : Buff(j) = &H83
                Case "84" : Buff(j) = &H84
                Case "85" : Buff(j) = &H85
                Case "86" : Buff(j) = &H86
                Case "87" : Buff(j) = &H87
                Case "88" : Buff(j) = &H88
                Case "89" : Buff(j) = &H89
                Case "8A" : Buff(j) = &H8A
                Case "8B" : Buff(j) = &H8B
                Case "8C" : Buff(j) = &H8C
                Case "8D" : Buff(j) = &H8D
                Case "8E" : Buff(j) = &H8E
                Case "8F" : Buff(j) = &H8F
                Case "90" : Buff(j) = &H90
                Case "91" : Buff(j) = &H91
                Case "92" : Buff(j) = &H92
                Case "93" : Buff(j) = &H93
                Case "94" : Buff(j) = &H94
                Case "95" : Buff(j) = &H95
                Case "96" : Buff(j) = &H96
                Case "97" : Buff(j) = &H97
                Case "98" : Buff(j) = &H98
                Case "99" : Buff(j) = &H99
                Case "9A" : Buff(j) = &H9A
                Case "9B" : Buff(j) = &H9B
                Case "9C" : Buff(j) = &H9C
                Case "9D" : Buff(j) = &H9D
                Case "9E" : Buff(j) = &H9E
                Case "9F" : Buff(j) = &H9F
                Case "A0" : Buff(j) = &HA0
                Case "A1" : Buff(j) = &HA1
                Case "A2" : Buff(j) = &HA2
                Case "A3" : Buff(j) = &HA3
                Case "A4" : Buff(j) = &HA4
                Case "A5" : Buff(j) = &HA5
                Case "A6" : Buff(j) = &HA6
                Case "A7" : Buff(j) = &HA7
                Case "A8" : Buff(j) = &HA8
                Case "A9" : Buff(j) = &HA9
                Case "AA" : Buff(j) = &HAA
                Case "AB" : Buff(j) = &HAB
                Case "AC" : Buff(j) = &HAC
                Case "AD" : Buff(j) = &HAD
                Case "AE" : Buff(j) = &HAE
                Case "AF" : Buff(j) = &HAF
                Case "B0" : Buff(j) = &HB0
                Case "B1" : Buff(j) = &HB1
                Case "B2" : Buff(j) = &HB2
                Case "B3" : Buff(j) = &HB3
                Case "B4" : Buff(j) = &HB4
                Case "B5" : Buff(j) = &HB5
                Case "B6" : Buff(j) = &HB6
                Case "B7" : Buff(j) = &HB7
                Case "B8" : Buff(j) = &HB8
                Case "B9" : Buff(j) = &HB9
                Case "BA" : Buff(j) = &HBA
                Case "BB" : Buff(j) = &HBB
                Case "BC" : Buff(j) = &HBC
                Case "BD" : Buff(j) = &HBD
                Case "BE" : Buff(j) = &HBE
                Case "BF" : Buff(j) = &HBF
                Case "C0" : Buff(j) = &HC0
                Case "C1" : Buff(j) = &HC1
                Case "C2" : Buff(j) = &HC2
                Case "C3" : Buff(j) = &HC3
                Case "C4" : Buff(j) = &HC4
                Case "C5" : Buff(j) = &HC5
                Case "C6" : Buff(j) = &HC6
                Case "C7" : Buff(j) = &HC7
                Case "C8" : Buff(j) = &HC8
                Case "C9" : Buff(j) = &HC9
                Case "CA" : Buff(j) = &HCA
                Case "CB" : Buff(j) = &HCB
                Case "CC" : Buff(j) = &HCC
                Case "CD" : Buff(j) = &HCD
                Case "CE" : Buff(j) = &HCE
                Case "CF" : Buff(j) = &HCF
                Case "D0" : Buff(j) = &HD0
                Case "D1" : Buff(j) = &HD1
                Case "D2" : Buff(j) = &HD2
                Case "D3" : Buff(j) = &HD3
                Case "D4" : Buff(j) = &HD4
                Case "D5" : Buff(j) = &HD5
                Case "D6" : Buff(j) = &HD6
                Case "D7" : Buff(j) = &HD7
                Case "D8" : Buff(j) = &HD8
                Case "D9" : Buff(j) = &HD9
                Case "DA" : Buff(j) = &HDA
                Case "DB" : Buff(j) = &HDB
                Case "DC" : Buff(j) = &HDC
                Case "DD" : Buff(j) = &HDD
                Case "DE" : Buff(j) = &HDE
                Case "DF" : Buff(j) = &HDF
                Case "E0" : Buff(j) = &HE0
                Case "E1" : Buff(j) = &HE1
                Case "E2" : Buff(j) = &HE2
                Case "E3" : Buff(j) = &HE3
                Case "E4" : Buff(j) = &HE4
                Case "E5" : Buff(j) = &HE5
                Case "E6" : Buff(j) = &HE6
                Case "E7" : Buff(j) = &HE7
                Case "E8" : Buff(j) = &HE8
                Case "E9" : Buff(j) = &HE9
                Case "EA" : Buff(j) = &HEA
                Case "EB" : Buff(j) = &HEB
                Case "EC" : Buff(j) = &HEC
                Case "ED" : Buff(j) = &HED
                Case "EE" : Buff(j) = &HEE
                Case "EF" : Buff(j) = &HEF
                Case "F0" : Buff(j) = &HF0
                Case "F1" : Buff(j) = &HF1
                Case "F2" : Buff(j) = &HF2
                Case "F3" : Buff(j) = &HF3
                Case "F4" : Buff(j) = &HF4
                Case "F5" : Buff(j) = &HF5
                Case "F6" : Buff(j) = &HF6
                Case "F7" : Buff(j) = &HF7
                Case "F8" : Buff(j) = &HF8
                Case "F9" : Buff(j) = &HF9
                Case "FA" : Buff(j) = &HFA
                Case "FB" : Buff(j) = &HFB
                Case "FC" : Buff(j) = &HFC
                Case "FD" : Buff(j) = &HFD
                Case "FE" : Buff(j) = &HFE
                Case "FF" : Buff(j) = &HFF
            End Select

            j += 1
        Next

        Return Buff

    End Function

    Function HexStringToBuffer(S As String) As Byte()
        If S.Length Mod 2 = 1 Then
            Throw New Exception("HexString must be even")
        End If

        Dim Len As Integer = S.Length / 2

        Dim Buff(Len - 1) As Byte

        Dim j As Integer = 0

        For i = 0 To Len - 1 Step 2

            Select Case S(i)
                Case "0"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H0
                        Case "1"
                            Buff(j) = &H1
                        Case "2"
                            Buff(j) = &H2
                        Case "3"
                            Buff(j) = &H3
                        Case "4"
                            Buff(j) = &H4
                        Case "5"
                            Buff(j) = &H5
                        Case "6"
                            Buff(j) = &H6
                        Case "7"
                            Buff(j) = &H7
                        Case "8"
                            Buff(j) = &H8
                        Case "9"
                            Buff(j) = &H9
                        Case "A"
                            Buff(j) = &HA
                        Case "B"
                            Buff(j) = &HB
                        Case "C"
                            Buff(j) = &HC
                        Case "D"
                            Buff(j) = &HD
                        Case "E"
                            Buff(j) = &HE
                        Case "F"
                            Buff(j) = &HF
                    End Select
                Case "1"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H10
                        Case "1"
                            Buff(j) = &H11
                        Case "2"
                            Buff(j) = &H12
                        Case "3"
                            Buff(j) = &H13
                        Case "4"
                            Buff(j) = &H14
                        Case "5"
                            Buff(j) = &H15
                        Case "6"
                            Buff(j) = &H16
                        Case "7"
                            Buff(j) = &H17
                        Case "8"
                            Buff(j) = &H18
                        Case "9"
                            Buff(j) = &H19
                        Case "A"
                            Buff(j) = &H1A
                        Case "B"
                            Buff(j) = &H1B
                        Case "C"
                            Buff(j) = &H1C
                        Case "D"
                            Buff(j) = &H1D
                        Case "E"
                            Buff(j) = &H1E
                        Case "F"
                            Buff(j) = &H1F
                    End Select
                Case "2"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H20
                        Case "1"
                            Buff(j) = &H21
                        Case "2"
                            Buff(j) = &H22
                        Case "3"
                            Buff(j) = &H23
                        Case "4"
                            Buff(j) = &H24
                        Case "5"
                            Buff(j) = &H25
                        Case "6"
                            Buff(j) = &H26
                        Case "7"
                            Buff(j) = &H27
                        Case "8"
                            Buff(j) = &H28
                        Case "9"
                            Buff(j) = &H29
                        Case "A"
                            Buff(j) = &H2A
                        Case "B"
                            Buff(j) = &H2B
                        Case "C"
                            Buff(j) = &H2C
                        Case "D"
                            Buff(j) = &H2D
                        Case "E"
                            Buff(j) = &H2E
                        Case "F"
                            Buff(j) = &H2F
                    End Select
                Case "3"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H30
                        Case "1"
                            Buff(j) = &H31
                        Case "2"
                            Buff(j) = &H32
                        Case "3"
                            Buff(j) = &H33
                        Case "4"
                            Buff(j) = &H34
                        Case "5"
                            Buff(j) = &H35
                        Case "6"
                            Buff(j) = &H36
                        Case "7"
                            Buff(j) = &H37
                        Case "8"
                            Buff(j) = &H38
                        Case "9"
                            Buff(j) = &H39
                        Case "A"
                            Buff(j) = &H3A
                        Case "B"
                            Buff(j) = &H3B
                        Case "C"
                            Buff(j) = &H3C
                        Case "D"
                            Buff(j) = &H3D
                        Case "E"
                            Buff(j) = &H3E
                        Case "F"
                            Buff(j) = &H3F
                    End Select
                Case "4"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H40
                        Case "1"
                            Buff(j) = &H41
                        Case "2"
                            Buff(j) = &H42
                        Case "3"
                            Buff(j) = &H43
                        Case "4"
                            Buff(j) = &H44
                        Case "5"
                            Buff(j) = &H45
                        Case "6"
                            Buff(j) = &H46
                        Case "7"
                            Buff(j) = &H47
                        Case "8"
                            Buff(j) = &H48
                        Case "9"
                            Buff(j) = &H49
                        Case "A"
                            Buff(j) = &H4A
                        Case "B"
                            Buff(j) = &H4B
                        Case "C"
                            Buff(j) = &H4C
                        Case "D"
                            Buff(j) = &H4D
                        Case "E"
                            Buff(j) = &H4E
                        Case "F"
                            Buff(j) = &H4F
                    End Select
                Case "5"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H50
                        Case "1"
                            Buff(j) = &H51
                        Case "2"
                            Buff(j) = &H52
                        Case "3"
                            Buff(j) = &H53
                        Case "4"
                            Buff(j) = &H54
                        Case "5"
                            Buff(j) = &H55
                        Case "6"
                            Buff(j) = &H56
                        Case "7"
                            Buff(j) = &H57
                        Case "8"
                            Buff(j) = &H58
                        Case "9"
                            Buff(j) = &H59
                        Case "A"
                            Buff(j) = &H5A
                        Case "B"
                            Buff(j) = &H5B
                        Case "C"
                            Buff(j) = &H5C
                        Case "D"
                            Buff(j) = &H5D
                        Case "E"
                            Buff(j) = &H5E
                        Case "F"
                            Buff(j) = &H5F
                    End Select
                Case "6"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H60
                        Case "1"
                            Buff(j) = &H61
                        Case "2"
                            Buff(j) = &H62
                        Case "3"
                            Buff(j) = &H63
                        Case "4"
                            Buff(j) = &H64
                        Case "5"
                            Buff(j) = &H65
                        Case "6"
                            Buff(j) = &H66
                        Case "7"
                            Buff(j) = &H67
                        Case "8"
                            Buff(j) = &H68
                        Case "9"
                            Buff(j) = &H69
                        Case "A"
                            Buff(j) = &H6A
                        Case "B"
                            Buff(j) = &H6B
                        Case "C"
                            Buff(j) = &H6C
                        Case "D"
                            Buff(j) = &H6D
                        Case "E"
                            Buff(j) = &H6E
                        Case "F"
                            Buff(j) = &H6F
                    End Select
                Case "7"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H70
                        Case "1"
                            Buff(j) = &H71
                        Case "2"
                            Buff(j) = &H72
                        Case "3"
                            Buff(j) = &H73
                        Case "4"
                            Buff(j) = &H74
                        Case "5"
                            Buff(j) = &H75
                        Case "6"
                            Buff(j) = &H76
                        Case "7"
                            Buff(j) = &H77
                        Case "8"
                            Buff(j) = &H78
                        Case "9"
                            Buff(j) = &H79
                        Case "A"
                            Buff(j) = &H7A
                        Case "B"
                            Buff(j) = &H7B
                        Case "C"
                            Buff(j) = &H7C
                        Case "D"
                            Buff(j) = &H7D
                        Case "E"
                            Buff(j) = &H7E
                        Case "F"
                            Buff(j) = &H7F
                    End Select
                Case "8"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H80
                        Case "1"
                            Buff(j) = &H81
                        Case "2"
                            Buff(j) = &H82
                        Case "3"
                            Buff(j) = &H83
                        Case "4"
                            Buff(j) = &H84
                        Case "5"
                            Buff(j) = &H85
                        Case "6"
                            Buff(j) = &H86
                        Case "7"
                            Buff(j) = &H87
                        Case "8"
                            Buff(j) = &H88
                        Case "9"
                            Buff(j) = &H89
                        Case "A"
                            Buff(j) = &H8A
                        Case "B"
                            Buff(j) = &H8B
                        Case "C"
                            Buff(j) = &H8C
                        Case "D"
                            Buff(j) = &H8D
                        Case "E"
                            Buff(j) = &H8E
                        Case "F"
                            Buff(j) = &H8F
                    End Select
                Case "9"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &H90
                        Case "1"
                            Buff(j) = &H91
                        Case "2"
                            Buff(j) = &H92
                        Case "3"
                            Buff(j) = &H93
                        Case "4"
                            Buff(j) = &H94
                        Case "5"
                            Buff(j) = &H95
                        Case "6"
                            Buff(j) = &H96
                        Case "7"
                            Buff(j) = &H97
                        Case "8"
                            Buff(j) = &H98
                        Case "9"
                            Buff(j) = &H99
                        Case "A"
                            Buff(j) = &H9A
                        Case "B"
                            Buff(j) = &H9B
                        Case "C"
                            Buff(j) = &H9C
                        Case "D"
                            Buff(j) = &H9D
                        Case "E"
                            Buff(j) = &H9E
                        Case "F"
                            Buff(j) = &H9F
                    End Select
                Case "A"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &HA0
                        Case "1"
                            Buff(j) = &HA1
                        Case "2"
                            Buff(j) = &HA2
                        Case "3"
                            Buff(j) = &HA3
                        Case "4"
                            Buff(j) = &HA4
                        Case "5"
                            Buff(j) = &HA5
                        Case "6"
                            Buff(j) = &HA6
                        Case "7"
                            Buff(j) = &HA7
                        Case "8"
                            Buff(j) = &HA8
                        Case "9"
                            Buff(j) = &HA9
                        Case "A"
                            Buff(j) = &HAA
                        Case "B"
                            Buff(j) = &HAB
                        Case "C"
                            Buff(j) = &HAC
                        Case "D"
                            Buff(j) = &HAD
                        Case "E"
                            Buff(j) = &HAE
                        Case "F"
                            Buff(j) = &HAF
                    End Select
                Case "B"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &HB0
                        Case "1"
                            Buff(j) = &HB1
                        Case "2"
                            Buff(j) = &HB2
                        Case "3"
                            Buff(j) = &HB3
                        Case "4"
                            Buff(j) = &HB4
                        Case "5"
                            Buff(j) = &HB5
                        Case "6"
                            Buff(j) = &HB6
                        Case "7"
                            Buff(j) = &HB7
                        Case "8"
                            Buff(j) = &HB8
                        Case "9"
                            Buff(j) = &HB9
                        Case "A"
                            Buff(j) = &HBA
                        Case "B"
                            Buff(j) = &HBB
                        Case "C"
                            Buff(j) = &HBC
                        Case "D"
                            Buff(j) = &HBD
                        Case "E"
                            Buff(j) = &HBE
                        Case "F"
                            Buff(j) = &HBF
                    End Select
                Case "C"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &HC0
                        Case "1"
                            Buff(j) = &HC1
                        Case "2"
                            Buff(j) = &HC2
                        Case "3"
                            Buff(j) = &HC3
                        Case "4"
                            Buff(j) = &HC4
                        Case "5"
                            Buff(j) = &HC5
                        Case "6"
                            Buff(j) = &HC6
                        Case "7"
                            Buff(j) = &HC7
                        Case "8"
                            Buff(j) = &HC8
                        Case "9"
                            Buff(j) = &HC9
                        Case "A"
                            Buff(j) = &HCA
                        Case "B"
                            Buff(j) = &HCB
                        Case "C"
                            Buff(j) = &HCC
                        Case "D"
                            Buff(j) = &HCD
                        Case "E"
                            Buff(j) = &HCE
                        Case "F"
                            Buff(j) = &HCF
                    End Select
                Case "D"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &HD0
                        Case "1"
                            Buff(j) = &HD1
                        Case "2"
                            Buff(j) = &HD2
                        Case "3"
                            Buff(j) = &HD3
                        Case "4"
                            Buff(j) = &HD4
                        Case "5"
                            Buff(j) = &HD5
                        Case "6"
                            Buff(j) = &HD6
                        Case "7"
                            Buff(j) = &HD7
                        Case "8"
                            Buff(j) = &HD8
                        Case "9"
                            Buff(j) = &HD9
                        Case "A"
                            Buff(j) = &HDA
                        Case "B"
                            Buff(j) = &HDB
                        Case "C"
                            Buff(j) = &HDC
                        Case "D"
                            Buff(j) = &HDD
                        Case "E"
                            Buff(j) = &HDE
                        Case "F"
                            Buff(j) = &HDF
                    End Select
                Case "E"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &HE0
                        Case "1"
                            Buff(j) = &HE1
                        Case "2"
                            Buff(j) = &HE2
                        Case "3"
                            Buff(j) = &HE3
                        Case "4"
                            Buff(j) = &HE4
                        Case "5"
                            Buff(j) = &HE5
                        Case "6"
                            Buff(j) = &HE6
                        Case "7"
                            Buff(j) = &HE7
                        Case "8"
                            Buff(j) = &HE8
                        Case "9"
                            Buff(j) = &HE9
                        Case "A"
                            Buff(j) = &HEA
                        Case "B"
                            Buff(j) = &HEB
                        Case "C"
                            Buff(j) = &HEC
                        Case "D"
                            Buff(j) = &HED
                        Case "E"
                            Buff(j) = &HEE
                        Case "F"
                            Buff(j) = &HEF
                    End Select
                Case "F"
                    Select Case S(i + 1)
                        Case "0"
                            Buff(j) = &HF0
                        Case "1"
                            Buff(j) = &HF1
                        Case "2"
                            Buff(j) = &HF2
                        Case "3"
                            Buff(j) = &HF3
                        Case "4"
                            Buff(j) = &HF4
                        Case "5"
                            Buff(j) = &HF5
                        Case "6"
                            Buff(j) = &HF6
                        Case "7"
                            Buff(j) = &HF7
                        Case "8"
                            Buff(j) = &HF8
                        Case "9"
                            Buff(j) = &HF9
                        Case "A"
                            Buff(j) = &HFA
                        Case "B"
                            Buff(j) = &HFB
                        Case "C"
                            Buff(j) = &HFC
                        Case "D"
                            Buff(j) = &HFD
                        Case "E"
                            Buff(j) = &HFE
                        Case "F"
                            Buff(j) = &HFF
                    End Select
            End Select

            j += 1
        Next

        Return Buff

    End Function

End Module