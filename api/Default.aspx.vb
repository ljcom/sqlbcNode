
Imports System.Data

Partial Class api_Default
    Inherits cl_base

    Private Sub api_Default_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim mode = getQueryVar("mode")
        Dim message = "", result = ""
        If Session("Sequoia") = "" Then
            If loadSession() <> "Start-Success" Then
                message = "Start-Required"
            End If
        End If

        Select Case mode.ToLower()
            Case "start"
                Dim pwd = getQueryVar("pwd")
                message = loadSession(pwd)

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
            Case "submittrx"
                Dim appSettings As NameValueCollection = ConfigurationManager.AppSettings
                Dim pwd = appSettings("password")

                Dim trx = IIf(IsNothing(Request.Form("trx")), "", Request.Form("trx"))
                trx = trx.Replace("%26lt;", "<").Replace("%26gt;", ">").Replace("%26", "&").Replace("&lt;", "<").Replace("&gt;", ">").replace("%2b", "+")
                writeLog(trx)
                Dim sqlstr = "exec [node_getSubmitTrx] '" & trx & "'; exec node_fillNewBlock 'live', '" & Session("myED") & "', '" & pwd & "'"

                message = runSQLwithResult(sqlstr, Session("Sequoia"))

                If message = "" Then
                    message = "Incorrect-Data"
                End If
            Case "sendnewblock"
            Case Else
                message = "Invalid-Command"
        End Select
        Response.ContentType = "text/xml"
        Response.Write("<?xml version=""1.0"" encoding=""utf-8""?>")
        If message <> "" Then
            message = "<message>" & message & "</message>"
        End If
        Response.Write("<sqroot>" & result & message & "</sqroot>")
    End Sub
End Class
