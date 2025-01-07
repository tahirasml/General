
''' <summary>
''' Extracts the .rpt resources from assembly
''' </summary>
''' <remarks></remarks>
Public Module Resource

    Public Sub extractInternalResource(ByVal p_resourceName As String, ByVal p_path As String)
        Dim imgStream As System.IO.Stream
        Dim oAssem As System.Reflection.Assembly
        Dim extension As String = String.Empty

        If p_resourceName.Length > 4 Then
            extension = p_resourceName.Substring(p_resourceName.Length - 4, 4)
        End If

        oAssem = System.Reflection.Assembly.GetAssembly(GetType(Resource))
        imgStream = oAssem.GetManifestResourceStream("TDXL0100." & p_resourceName)
        If imgStream Is Nothing Then
            p_resourceName = p_resourceName.Replace(".rpt", ".RPT")
            imgStream = oAssem.GetManifestResourceStream("TDXL0100." & p_resourceName)
        End If
        Try
            Select Case extension.ToLower
                Case ".rpt"
                    Dim filStream As IO.FileStream
                    filStream = New IO.FileStream(p_path, IO.FileMode.Create, IO.FileAccess.Write)

                    Dim i As Integer = 0
                    i = imgStream.ReadByte()
                    While (i > -1)
                        filStream.WriteByte(Convert.ToByte(i))
                        i = imgStream.ReadByte()
                    End While

                    filStream.Flush()
                    filStream.Close()
            End Select
            imgStream.Close()

        Catch ex As Exception
            Throw
        End Try
    End Sub

End Module
