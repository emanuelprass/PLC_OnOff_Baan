Public Class frmSettingPath
    Public pathInbound As String = ""
    Public pathOutbound As String = ""

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        pathInbound = txtInboundPath.Text
        pathOutbound = txtOutboundPath.Text
        If pathInbound <> "" And pathOutbound <> "" Then
            frmHome.pathInbound = pathInbound
            frmHome.pathOutbound = pathOutbound
            MsgBox("Successfully saved data")
            Close()
        Else
            MsgBox("Please enter path")
        End If
    End Sub
End Class