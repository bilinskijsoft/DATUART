Imports System.Text.RegularExpressions

Public Class Form1
    ''' <summary>
    ''' Структура пункта гоосования
    ''' </summary>
    Public Structure PoolVariantData
        Dim Name As String
        Dim Value As Integer
        Dim Percent As Integer
    End Structure

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        SerialPort1.Open()

        If TextBox2.Text = "" Then
            MsgBox("Введите ссылку!")
            Exit Sub
        End If
        Me.Text = "DATUART - Working..."
        Timer1.Enabled = True

        TextBox1.Enabled = False
        TextBox2.Enabled = False
        ComboBox1.Enabled = False
        ComboBox2.Enabled = False

    End Sub

    ''' <summary>
    ''' Поиск вариантов голосования 
    ''' </summary>
    ''' <param name="HTMl">HTML код</param>
    ''' <returns>Возвращает колекцию совпадений поиска вариантов голосования</returns>
    Function getPoolVariants(HTMl As String) As MatchCollection
        Return Regex.Matches(HTMl, "<div class=""poll-variant-container"" data-optionid="".*"">\n.*")
    End Function

    ''' <summary>
    ''' Поучаем данные с варианта голосования
    ''' </summary>
    ''' <param name="HTML">HTML код</param>
    ''' <returns>Возвращает структуру данных</returns>
    Function getDataFromVariant(HTML As String) As PoolVariantData
        Dim result As PoolVariantData = New PoolVariantData
        Dim m1 As MatchCollection = Regex.Matches(HTML, "(?!>)([а-яА-ЯёЁa-zA-Z0-9])+(?=<)")

        result.Name = m1.Item(0).ToString
        result.Value = Int(m1.Item(1).ToString)
        result.Percent = Int(m1.Item(2).ToString)

        Return result
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        SerialPort1.Close()
        Me.Text = "DATUART"
        Timer1.Enabled = False
        TextBox1.Enabled = True
        TextBox2.Enabled = True
        ComboBox1.Enabled = True
        ComboBox2.Enabled = True
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim Ports = IO.Ports.SerialPort.GetPortNames()
        SerialPort1.Encoding = System.Text.Encoding.GetEncoding(1251)
        For Each port In Ports
            ComboBox2.Items.Add(port)
        Next
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        Try
            SerialPort1.BaudRate = Int(ComboBox1.Text)
        Catch ex As Exception
            MsgBox(ex.Message.ToString(), MsgBoxStyle.Critical, "ERROR")
        End Try

    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged
        Try
            SerialPort1.PortName = ComboBox2.Text
        Catch ex As Exception
            MsgBox(ex.Message.ToString(), MsgBoxStyle.Critical, "ERROR")
        End Try
    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged
        If TextBox1.Text <> "" Then
            Timer1.Interval = TextBox1.Text * 1000
        End If
    End Sub

    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged
        If TextBox2.Text <> "" Then
            Try
                WebBrowser1.Url = New Uri(TextBox2.Text)
            Catch ex As Exception

            End Try

        End If
    End Sub

    Private Sub Timer1_Tick(sender As Object, e As EventArgs) Handles Timer1.Tick
        Dim elements As HtmlElementCollection
        elements = WebBrowser1.Document.GetElementsByTagName("HTML")
        Dim htmlElement = elements(0)
        Dim pageSource = htmlElement.OuterHtml

        Dim Pool As MatchCollection
        Pool = getPoolVariants(pageSource)

        Dim poolVariantData As PoolVariantData

        list.Items.Clear()

        For Each poolVariant In Pool
            poolVariantData = getDataFromVariant(poolVariant.ToString)

            Try
                SerialPort1.Write(poolVariantData.Name & "-" & poolVariantData.Value & ";" + vbCrLf)
            Catch ex As Exception
                MsgBox(ex.Message.ToString, MsgBoxStyle.Critical, "ERROR")
            End Try


            list.Items.Add(poolVariantData.Name)
            list.Items.Item(list.Items.Count - 1).SubItems.Add(poolVariantData.Value)
            list.Items.Item(list.Items.Count - 1).SubItems.Add(poolVariantData.Percent)
        Next
        WebBrowser1.Refresh()
    End Sub
End Class
