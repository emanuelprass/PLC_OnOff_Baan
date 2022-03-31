Public Class frmDatabase
    Public dbWorkcenter As String = ""
    Public dbLHPIgp As String = ""

    Private Sub frmDatabase_Load(sender As Object, e As EventArgs) Handles Me.Load
        If txtDbWorkcenter.Text = "" Then
            'txtDbWorkcenter.Text = "Data Source=""(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.14.99.220)(PORT=1521))(CONNECT_DATA=(SERVER=dedicated)(SERVICE_NAME=baan)))"";Persist Security Info=True;User ID=baan;Password=Komponen1;Unicode=True"
            txtDbWorkcenter.Text = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.14.99.220)(PORT=1521))(CONNECT_DATA=(SERVER=dedicated)(SERVICE_NAME=baan)));Persist Security Info=True;password=Komponen1;user ID=baan;"
            txtDbWorkcenter.PasswordChar = "*"
        End If

        If txtdbLHP.Text = "" Then
            'txtdbLHP.Text = "Data Source=""(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.14.99.220)(PORT=1521))(CONNECT_DATA=(SERVER=dedicated)(SERVICE_NAME=baan)))"";Persist Security Info=True;User ID=baandb;Password=Komponen1;Unicode=True"
            txtdbLHP.Text = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=10.14.99.220)(PORT=1521))(CONNECT_DATA=(SERVER=dedicated)(SERVICE_NAME=baan)));Persist Security Info=True;password=Komponen1;user ID=baandb;"
            txtdbLHP.PasswordChar = "*"
        End If
    End Sub

    Private Sub Panel5_Paint(sender As Object, e As PaintEventArgs) Handles Panel5.Paint

    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        dbWorkcenter = txtDbWorkcenter.Text
        dbLHPIgp = txtdbLHP.Text
        If dbWorkcenter <> "" And dbLHPIgp <> "" Then
            frmHome.con = dbWorkcenter
            frmHome.conLHP = dbLHPIgp
            MsgBox("Succesfully saved data")
            Close()
        Else
            MsgBox("Please entry data")
        End If
    End Sub

End Class