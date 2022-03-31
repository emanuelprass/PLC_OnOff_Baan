Public Class frmSettingPlant
    Public plant As String = ""

    Private Sub frmSettingPlant_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        plant = txtPlant.Text.ToString()
        If txtPlant.Text.Length > 3 Then
            MsgBox("Please input less than 3 characters")
        Else
            frmHome.plant = plant
            MsgBox("Successfully save data")
            Close()
        End If    

    End Sub

    'Private Sub txtPlant_KeyPress(sender As Object, e As KeyPressEventArgs) Handles txtPlant.KeyPress
    '    If Not Char.IsDigit(e.KeyChar) And Not Char.IsControl(e.KeyChar) Then
    '        e.Handled = True
    '    End If
    'End Sub

    Private Sub txtPlant_TextChanged(sender As Object, e As EventArgs) Handles txtPlant.TextChanged
        If txtPlant.Text.Length > 3 Then
            MsgBox("Please input less than 3 characters")
        End If
    End Sub
End Class