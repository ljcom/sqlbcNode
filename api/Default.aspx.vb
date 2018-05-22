
Imports System.Data

Partial Class api_Default
    Inherits cl_base

    Private Sub api_Default_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim mode = getQueryVar("mode")
        Dim message = "", result = ""
        If Session("Sequoia") = "" Then
            message = "Start-Required"
        End If

        Select Case mode.ToLower()
            Case "start"
                Dim pwd = getQueryVar("pwd")

                Dim appSettings As NameValueCollection = ConfigurationManager.AppSettings
                Session("nodeList") = appSettings("wellknownNode")
                Session("Sequoia") = appSettings("Sequoia")
                Session("myNode") = appSettings("myNode")
                Session("myEd") = appSettings("myEd")

                Dim sqlstr = "declare @hostGUID uniqueidentifier; exec login 'live', '" & Session("myEd") & "', '" & pwd & "', @issilent=0"
                Dim ds As DataSet = SelectSqlSrvRows(sqlstr, Session("Sequoia"))
                If ds.Tables.Count > 0 Then
                    Session("d") = ds.Tables(0).Rows(0).Item("d")
                    Session("ct") = ds.Tables(0).Rows(0).Item("ct")
                    Session("qt") = ds.Tables(0).Rows(0).Item("qt")
                End If
                Dim hostguid = runSQLwithResult(sqlstr, Session("Sequoia"))
                If GetServerIPAddress() = GetClientIPAddress() Then
                    If hostguid <> "" Then
                        Session("ed") = getQueryVar("ed")
                        'Session("pwd") = getQueryVar("pwd")
                        message = "Start-Success"
                        'result = "<hostguid>" & hostguid & "</hostguid>"
                        Session("hostGUID") = hostguid
                    Else
                        message = "Start-Failed"
                    End If
                Else
                    message = "Start-Invalid"
                End If
            Case "shakehand"
                Dim yourNodes = getQueryVar("nodeList").Split(",")
                Dim myNodes = Session("nodeList")
                For Each x In yourNodes
                    If myNodes.ToString.IndexOf(x) = 0 And x <> "" Then
                        myNodes = myNodes & ";" & x
                    End If
                Next
                Session("nodeList") = myNodes
                Dim qt = Session("qt")
                result = "<nodes>" & myNodes & "</nodes><qt>" & qt & "</qt>"
            Case "reqheader"

            Case "reqblock"
            Case "reqsuspect"
            Case "sendnewtrx"
            Case "sendnewblock"
        End Select
        Response.ContentType = "text/xml"
        Response.Write("<?xml version=""1.0"" encoding=""utf-8""?>")
        If message <> "" Then
            message = "<message>" & message & "</message>"
        End If
        Response.Write("<sqroot>" & result & message & "</sqroot>")
    End Sub
End Class
