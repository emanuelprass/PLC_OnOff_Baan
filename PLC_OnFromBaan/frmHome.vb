Imports System.IO
Imports Oracle.ManagedDataAccess.Client

Public Class frmHome
    Dim rows As Integer = 0
    Dim col As Integer = 0
    Dim oc As OracleCommand
    Dim sql As String
    Dim ord As OracleDataReader
    Public con As String = ""
    Public conLHP As String = ""
    Dim generateTime As String = ""
    Public plant As String = ""
    Public pathInbound As String = ""
    Public pathOutbound As String = ""
    Dim statusRun As Integer = 0

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
         
    End Sub

    Private Sub GetData()
        Dim Connection As New OracleConnection(con)

        Try
            oc = New OracleCommand
            If Connection.State = ConnectionState.Closed Then
                Connection.Open()
            End If
            oc.CommandText = "SELECT T$CWOC$PLC as WORKCENTER,T$LAST$PLC as LAST_PROCESS,T$STRBAAN as BAAN_ON,T$STARTPLC as PLC_ON, " &
                                "T$REMARK1 as TAKT_TIME, '01/01/1970' as GENERATE FROM BAANDB.tgsinh007100 WHERE T$PLANT = '" & plant & "'"
            oc.CommandType = CommandType.Text
            oc.Connection = Connection
            ord = oc.ExecuteReader()

            If ord.HasRows = False Then
                MsgBox("No collected data")
            End If

            Dim lv As ListView = Me.ListView1
            Dim rownum As Integer = 0
            Dim jumlah_baris_query As Integer = 0
            Dim jumlah_baris_lv As Integer = lv.Items.Count

            While ord.Read
                jumlah_baris_query = jumlah_baris_query + 1
            End While
            'lv.Items.Clear()

            If jumlah_baris_query = 0 Then
                lv.Items.Clear()
            Else
                If jumlah_baris_lv > jumlah_baris_query Then
                    Dim i As Integer = jumlah_baris_query
                    For i = jumlah_baris_lv To jumlah_baris_query Step -1
                        lv.Items(i - 1).Remove()
                    Next
                End If
            End If

            'ord.Close()

            ord = oc.ExecuteReader()
            Dim ColsOfData As Integer = ord.FieldCount

            While ord.Read

                If lv.Items.Count <= rownum Then
                    Dim item As ListViewItem = New ListViewItem
                    item.Text = ord.GetValue(0).ToString()
                    item.SubItems.Add(ord.GetValue(1).ToString())
                    For i As Integer = 2 To 3
                        If ord.GetValue(i).ToString() = "1" Then
                            item.SubItems.Add("True")
                        Else
                            item.SubItems.Add("False")
                        End If
                    Next

                    item.SubItems.Add(ord.GetValue(4).ToString())
                    item.SubItems.Add(CDate(DateTime.Now))
                    item.SubItems.Add("".ToString())
                    lv.Items.Add(item)
                Else
                    Dim statBaanBef As String = ""
                    Dim statPlcBef As String = ""

                    For i As Integer = 2 To 3
                        If lv.Items(rownum).SubItems(i).Text = "True" Then
                            If i = 2 Then
                                statBaanBef = "1"
                            Else
                                statPlcBef = "1"
                            End If
                        Else
                            If i = 2 Then
                                statBaanBef = "2"
                            Else
                                statPlcBef = "2"
                            End If
                        End If
                    Next
                    Dim statBaanAfr As String = ord.GetValue(2).ToString()
                    Dim statPlcAfr As String = ord.GetValue(3).ToString()
                    Dim beda As String = ""

                    If statBaanBef <> statBaanAfr Then
                        beda = "YES"
                    End If

                    lv.Items(rownum).Text = ord.GetValue(0).ToString()
                    lv.Items(rownum).SubItems(1).Text = ord.GetValue(1).ToString()
                    For i As Integer = 2 To 3
                        If ord.GetValue(i).ToString() = "1" Then
                            lv.Items(rownum).SubItems(i).Text = "True"
                        Else
                            lv.Items(rownum).SubItems(i).Text = "False"
                        End If
                    Next
                    lv.Items(rownum).SubItems(4).Text = ord.GetValue(4).ToString()
                    If ord.GetValue(2).ToString() = "1" And ord.GetValue(3).ToString() = "1" Then
                        Dim last As String = CDate(lv.Items(rownum).SubItems(5).Text).AddMinutes(lv.Items(rownum).SubItems(4).Text)
                        lv.Items(rownum).SubItems(6).Text = last
                        'Check generate
                        If CStr(CDate(DateTime.Now)) = lv.Items(rownum).SubItems(6).Text Then
                            GenerateTextFile(lv.Items(rownum).Text, lv.Items(rownum).SubItems(6).Text, statBaanBef, statBaanAfr, lv.Items(rownum).SubItems(5).Text)
                            lv.Items(rownum).SubItems(5).Text = CDate(DateTime.Now)
                            updateRowGenerate(CDate(lv.Items(rownum).SubItems(6).Text), CStr(lv.Items(rownum).SubItems(0).Text))
                        End If
                        'ElseIf ord.GetValue(2).ToString() <> "1" Then
                    Else
                        lv.Items(rownum).SubItems(5).Text = CDate(DateTime.Now)
                        lv.Items(rownum).SubItems(6).Text = ""
                    End If

                    CheckTextPLC(lv.Items(rownum).Text, CDate(DateTime.Now).ToString("ddMMyyyy"))

                    If beda = "YES" Then
                        If statBaanBef = "2" Then
                            If statBaanAfr = "1" Then
                                TextFileCreated(lv.Items(rownum).Text, "ON", DateTime.Now, "NO")
                            End If
                        End If
                    End If

                    
                End If

                rownum = rownum + 1
            End While
        Catch ex As Exception
            'MsgBox(ex.ToString)
        Finally
            'ord.Close()
            Connection.Close()
        End Try
    End Sub

    Private Sub GenerateTextFile(ByRef wcenter As String, ByRef gen As String, ByRef statBaanBef As String, ByRef statBaanAfr As String, ByRef lastexec As String)
        Dim connection As New OracleConnection(conLHP)
        Dim statusUpdate As String = ""
        Dim daTable As New DataTable

        Try
            If statBaanAfr = "1" Then
                connection.Open()
                Dim strSql As String = "SELECT distinct T$CWOC FROM LHPIGP WHERE T$CWOC = '" & wcenter & "' AND " &
                                        "to_char(T$TRDT + 7/24, 'DD-MM-YYYY HH24:MI') BETWEEN '" & lastexec & "' AND " &
                                        "'" & gen & "'"

                oc = New OracleCommand(strSql, connection)
                oc.CommandText = strSql
                ord = oc.ExecuteReader()

                If ord.HasRows = False Then
                    Dim conn As New OracleConnection(con)
                    conn.Open()
                    Dim sqlQuery As String = "UPDATE BAANDB.tgsinh007100 SET T$STRBAAN = 2 WHERE T$CWOC$PLC = '" & wcenter & "' "
                    oc = New OracleCommand(sqlQuery, conn)
                    oc.CommandText = sqlQuery
                    ord = oc.ExecuteReader()
                    conn.Close()
                    Dim BaanNow As String = "2"

                    If statBaanBef <> BaanNow Or statBaanAfr <> BaanNow Then
                        TextFileCreated(wcenter, "OFF", DateTime.Now, "NO")
                        'TextFileCreated(wcenter, "OFF", CDate(DateTime.Now).ToString("dd-MM-yyyy HH':'mm':'ss"), "YES")
                    End If
                End If
            End If
        Catch ex As Exception
            'MsgBox(ex.Message)
        Finally
            'ord.Close()
            connection.Close()
        End Try

    End Sub

    Private Sub TextFileCreated(ByRef wcenter As String, ByRef baanOnOff As String, ByRef gen As DateTime, ByRef statusUp As String)
        If Directory.Exists("" & pathInbound & "") = False Then
            Directory.CreateDirectory("" & pathInbound & "")
        End If

        Dim fileNames As String = "IGP.PLC_" & wcenter & "_" & gen.ToString("ddMMyyyy") & "_" & gen.ToString("HHmmss") & ".txt"
        Dim sFile As String = "" & pathInbound & "\" & fileNames & ""
        Dim count As Integer = 1
        Dim fileNameOnly As String = Path.GetFileNameWithoutExtension(sFile)
        Dim extension As String = Path.GetExtension(sFile)
        Dim path__1 As String = Path.GetDirectoryName(sFile)
        Dim newFullPath As String = sFile

        If statusUp = "NO" Then
            While File.Exists(newFullPath)
                Dim tempFileName As String = String.Format("{0}({1})", fileNameOnly, System.Math.Max(System.Threading.Interlocked.Increment(count), count - 1))
                newFullPath = Path.Combine(path__1, tempFileName & extension)
            End While

            Using f As New IO.StreamWriter(newFullPath, True)
                Dim col As String = ""
                Dim row As String = ""
                Dim x As Integer = 0
                row = "" & gen.ToString("ddMMyyyy HH:mm:ss") & "|" & wcenter & "|" & baanOnOff & ""
                f.WriteLine(row)
            End Using
        End If

        If statusUp = "YES" Then
            If Directory.Exists("" & pathInbound & "\Processed") = False Then
                Directory.CreateDirectory("" & pathInbound & "\Processed")
            End If
            My.Computer.FileSystem.MoveFile(sFile, path__1 + "\Processed\" + fileNames)
        End If
    End Sub

    Private Sub updateRowGenerate(dateTme As DateTime, wcenter As String)
        Dim conn As New OracleConnection(con)
        If conn.State = ConnectionState.Closed Then
            conn.Open()
        End If

        Dim sqlQuery As String = "UPDATE BAANDB.tgsinh007100 SET T$CREATEDT = TO_DATE('" & dateTme & "', 'DD/MM/YYYY hh:mi:ss' ) WHERE T$CWOC$PLC = '" & wcenter & "' AND T$PLANT = " & plant & ""
        oc = New OracleCommand(sqlQuery, conn)
        oc.CommandText = sqlQuery

        Try
            ord = oc.ExecuteReader()
        Catch ex As Exception
            'Nothing
        Finally
            conn.Close()
        End Try

    End Sub

    Private Sub CheckTextPLC(ByRef wcenter As String, ByRef gen As String)
        Try
            If Directory.Exists("" & pathOutbound & "") = False Then
                Directory.CreateDirectory("" & pathOutbound & "")
            Else
                Dim dirs As String() = Directory.GetFiles("" & pathOutbound & "")
            End If

            Dim conn As New OracleConnection(con)
            conn.Open()

            For Each foundFile As String In My.Computer.FileSystem.GetFiles(
                "" & pathOutbound & "",
                FileIO.SearchOption.SearchTopLevelOnly, "IGP.PLC_" & wcenter & "_" & gen & "*.txt")

                Dim fileReader As String
                fileReader = My.Computer.FileSystem.ReadAllText(foundFile, System.Text.Encoding.Default)

                Dim findString As String = "|"
                Dim startPoint As Integer = fileReader.IndexOf(findString.ToString) + 1
                Dim OnOff As String = fileReader.Substring(startPoint)

                Dim findString2 As String = "|"
                Dim startPoint2 As Integer = OnOff.IndexOf(findString2.ToString) + 1
                Dim OnOff2 As String = OnOff.Substring(startPoint2, 2)
                Dim statusPLC As String = ""

                If OnOff2 <> "ON" Then
                    Dim sqlQuery As String = "UPDATE BAANDB.tgsinh007100 SET T$STARTPLC = 2, T$STRBAAN = 2  WHERE T$CWOC$PLC = '" & wcenter & "' "
                    oc = New OracleCommand(sqlQuery, conn)
                    oc.CommandText = sqlQuery
                    ord = oc.ExecuteReader()
                Else
                    Dim sqlQuery As String = "UPDATE BAANDB.tgsinh007100 SET T$STARTPLC = 1 WHERE T$CWOC$PLC = '" & wcenter & "' "
                    oc = New OracleCommand(sqlQuery, conn)
                    oc.CommandText = sqlQuery
                    ord = oc.ExecuteReader()
                End If

                Dim fileName As String = Path.GetFileNameWithoutExtension(foundFile)

                If Directory.Exists("" & pathOutbound & "\Processed") = False Then
                    Directory.CreateDirectory("" & pathOutbound & "\Processed")
                End If
                My.Computer.FileSystem.MoveFile(foundFile, "" & pathOutbound & "\Processed\" + fileName + ".txt")
            Next
            conn.Close()

        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Me.Text = "BAAN PLC"
        GetData()
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(sender As Object, e As MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        ShowInTaskbar = True
        Me.WindowState = FormWindowState.Normal
        NotifyIcon1.Visible = False
    End Sub

    Private Sub frmHome_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Minimized Then
            NotifyIcon1.Visible = True
            ShowInTaskbar = False
        End If
    End Sub

    Private Sub PlantToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PlantToolStripMenuItem.Click
        frmSettingPlant.ShowDialog()        
    End Sub

    Private Sub PathToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles PathToolStripMenuItem.Click
        frmSettingPath.ShowDialog()
    End Sub

    Private Sub btnRun_Click(sender As Object, e As EventArgs) Handles btnRun.Click
        If statusRun = 0 Then
            If con <> "" And conLHP <> "" And plant <> "" And pathInbound <> "" And pathOutbound <> "" Then
                statusRun = 1
                GetData()
                Timer1.Enabled = True
                btnRun.Text = "Running"
            Else
                MsgBox("Please check your setting first!")
            End If            
        ElseIf statusRun = 1 Then
            btnRun.Text = "Run"
            statusRun = 0
            Timer1.Enabled = False
        End If
    End Sub

    Private Sub DatabaseToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DatabaseToolStripMenuItem.Click
        frmDatabase.ShowDialog()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub
End Class
