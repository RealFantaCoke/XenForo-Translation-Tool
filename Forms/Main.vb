Imports System.Text.RegularExpressions
Imports FastColoredTextBoxNS
Imports System.IO
Imports System.IO.Compression
Imports System.Text

Public Class Main

    Dim rtb As New RichTextBox
    Dim file As String
    Dim state As Integer

    Public UserProfile As String = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
    Public AppData As String = UserProfile & "\AppData\Roaming\"
    Public Bin As String = AppData + "LeakMania"

    Public title As String = "LeakMania | XenForo Translation Tool - v1.1"

    Private Sub Main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = title
        If IO.Directory.Exists(Bin) = False Then
            IO.Directory.CreateDirectory(Bin)
        End If
    End Sub

    Private Sub LoadFileToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem1.Click
        Dim fd As New OpenFileDialog
        Dim title As String
        fd.Title = "Please select the language file..."
        fd.Filter = "Select Files (*.xml)|*.xml|All Files (*.*)|*.*"
        fd.FilterIndex = 1
        fd.RestoreDirectory = True
        If fd.ShowDialog = DialogResult.OK Then
            Dim text As String
            file = fd.FileName
            rtb.Text = IO.File.ReadAllText(file)
            For Each str As String In rtb.Lines
                If str.Contains("<phrase title=""") And str.Contains("<![CDATA[") Then
                    title = GetBetween(str, "<phrase title=""", """ addon_id=")
                    text = text + title + ": " + GetBetween(str, "<![CDATA[", "]]>") + vbNewLine
                End If
                If Not title = "" Then
                    StateTitleLabel.Text = "Loading: " + title
                End If
                System.Threading.Thread.Sleep(1)
                Application.DoEvents()
            Next
            StateTitleLabel.Text = ""
            FastColoredTextBox1.Text = text
        End If
    End Sub

    Private Sub SaveToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ToolStripMenuItem2.Click
        state = 0
        Dim list As New List(Of String)
        Dim rich As New RichTextBox
        For Each str1 As String In rtb.Lines
            If str1.Contains("<phrase title=""") And str1.Contains("<![CDATA[") Then
                Dim title As String = GetBetween(str1, "<phrase title=""", """ addon_id=")
                rich.Text = FastColoredTextBox1.Text
                For Each str2 As String In rich.Lines
                    Dim cdata = str2.Replace(title & ": ", "")
                    If str2.Contains(title) Then
                        Try
                            Dim replaced = str1.Replace(GetBetween(str1, "<![CDATA[", "]]>"), cdata)
                            list.Add(replaced)
                        Catch ex As Exception

                        End Try
                    End If
                Next
                state += 1
                If Not title = "" Then
                    StateTitleLabel.Text = "(" + state.ToString + "/" + FastColoredTextBox1.LinesCount.ToString + ") Saving: " + title
                End If
                System.Threading.Thread.Sleep(1)
                Application.DoEvents()
            Else
                list.Add(str1)
            End If
        Next
        StateTitleLabel.Text = ""
        Dim newfile = file.Replace(".xml", "-NEW.xml")
        If IO.File.Exists(newfile) = True Then
            IO.File.Delete(newfile)
        End If
        Using sw = New StreamWriter(newfile)
            For Each str As String In list
                sw.WriteLine(str)
            Next
        End Using
    End Sub

    Dim separator As String = "#====================================#"

    Private Sub LoadToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles LoadToolStripMenuItem.Click
        Try
            Dim fd As New OpenFileDialog
            fd.Title = "Please select the project file..."
            fd.Filter = "Select Files (*.lmproj)|*.lmproj|All Files (*.*)|*.*"
            fd.FilterIndex = 1
            fd.RestoreDirectory = True
            If fd.ShowDialog = DialogResult.OK Then
                If Not rtb.Text = "" Then
                    FastColoredTextBox1.Text = IO.File.ReadAllText(fd.FileName)
                Else
                    MsgBox("You must load the XenForo XML language file to do this !")
                End If
            End If
        Catch ex As Exception
            MsgBox(ErrorToString)
        End Try
    End Sub

    Private Sub SaveToolStripMenuItem_Click_1(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click
        Dim fd As New SaveFileDialog
        fd.Title = "Please choose a directory..."
        fd.Filter = "Select Files (*.lmproj)|*.lmproj|All Files (*.*)|*.*"
        fd.FilterIndex = 1
        fd.RestoreDirectory = True
        If fd.ShowDialog = DialogResult.OK Then
            Using sw = New StreamWriter(fd.FileName)
                Dim rich As New RichTextBox
                rich.Text = FastColoredTextBox1.Text
                For Each str As String In rich.Lines
                    sw.WriteLine(str)
                Next
            End Using
        End If
    End Sub

#Region "Text Highlighting"

    Private StyleBold As FontStyle = FontStyle.Bold
    Private StyleItalic As FontStyle = FontStyle.Italic
    Private StyleRegular As FontStyle = FontStyle.Regular
    Private GrayItalic As New TextStyle(Brushes.Gray, Nothing, StyleItalic)
    Private OrangeBold As New TextStyle(Brushes.OrangeRed, Nothing, StyleBold)
    Private DarkBlueRegular As New TextStyle(Brushes.DarkBlue, Nothing, StyleRegular)

    Private Sub FastColoredTextBox1_TextChanged(ByVal sender As Object, ByVal e As TextChangedEventArgs) Handles FastColoredTextBox1.TextChanged
        e.ChangedRange.ClearStyle(OrangeBold, DarkBlueRegular, GrayItalic)
        e.ChangedRange.SetStyle(OrangeBold, ":")
        e.ChangedRange.SetStyle(DarkBlueRegular, ".*:")
        e.ChangedRange.SetStyle(GrayItalic, ": .*")
    End Sub

#End Region

    Public Function GetBetween(ByVal str As String, ByVal ex1 As String, ByVal ex2 As String) As String
        Dim istart As Integer = InStr(str, ex1)
        If istart > 0 Then
            Dim istop As Integer = InStr(istart, str, ex2)
            If istop > 0 Then
                Try
                    Dim value As String = str.Substring(istart + Len(ex1) - 1, istop - istart - Len(ex1))
                    Return value
                Catch ex As Exception
                    Return ""
                End Try
            End If
        End If
        Return ""
    End Function

End Class
