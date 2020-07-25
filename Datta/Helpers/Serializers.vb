Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Newtonsoft.Json
Imports System.Text
Imports System.IO

Friend Class Serializers

#Region "Xml"
    Shared Function ToXml(O As Object) As String
        Return ToXml(O, O.GetType)
    End Function

    Shared Function ToXml(O As Object, T As Type) As String
        Dim Xml As New XmlSerializer(T)
        Dim MS As New MemoryStream

        Xml.Serialize(MS, O)

        Return MS.ToString
    End Function

    Shared Function FromXml(S As String, T As System.Type) As Object
        Dim Xml As New XmlSerializer(T)
        Dim MS As New MemoryStream(S)

        Return Xml.Deserialize(MS)
    End Function
#End Region

#Region "Binary"
    Friend Shared Function ToBinary(O As Object) As Byte()
        Dim Bin As New BinaryFormatter
        Using MS As New MemoryStream
            Bin.Serialize(MS, O)
            Return MS.ToArray
        End Using
    End Function

    Friend Shared Function FromBinary(Buff As Byte()) As Object
        Dim Bin As New BinaryFormatter
        Using MS As New MemoryStream(Buff)
            Return Bin.Deserialize(MS)
        End Using
    End Function

    Friend Shared Function BinaryClone(O As Object) As Object
        Return FromBinary(ToBinary(O))
    End Function

#End Region

#Region "Json.net"
    Shared Function ToJson(O As Object, Optional BinaryCloneBefore As Boolean = False, Optional Idented As Boolean = True) As String
        Dim JsonSettings As New Newtonsoft.Json.JsonSerializerSettings
        JsonSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore

        If BinaryCloneBefore Then
            O = BinaryClone(O)
        End If

        If Idented Then
            Return Newtonsoft.Json.JsonConvert.SerializeObject(O, Newtonsoft.Json.Formatting.Indented, JsonSettings)
        Else
            Return Newtonsoft.Json.JsonConvert.SerializeObject(O, Newtonsoft.Json.Formatting.None, JsonSettings)
        End If
    End Function

    Shared Function FromJson(S As String, T As System.Type) As Object
        Return JsonConvert.DeserializeObject(S, T)
    End Function

    Shared Function FromJson(S As String) As Object
        Return JsonConvert.DeserializeObject(S)
    End Function
#End Region

End Class

Module SerializersExtensions

    <Extension()>
    Function BinaryClone(O As Object) As Object
        Return Serializers.BinaryClone(O)
    End Function

    <Extension()>
    Function ToJson(O As Object) As String
        Return Serializers.ToJson(O)
    End Function

End Module