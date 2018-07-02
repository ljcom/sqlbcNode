Imports System.Data
Imports cryptoSQL

Partial Class api_Default
    Inherits cl_base

    Private Sub api_Default_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim mode = getQueryVar("mode")
        Dim message = "", result = ""

        If Session("Sequoia") = "" Then
            loadSession()
        End If

        Dim appSettings As NameValueCollection = ConfigurationManager.AppSettings
        Dim pwd = appSettings("password")

        If GetServerIPAddress() <> GetClientIPAddress() Then
            runSQLwithResult("node_addNode '" & GetClientIPAddress() & "'", Session("Sequoia"))
        End If
        Select Case mode.ToLower()
            Case "start"
                message = loadSession(pwd)

            Case "shakehand"
                Dim sqlstr = "select ','+nodeAddress from node where isnull(nodeqt,'')<>'' for xml path('')"
                Dim yourNodes = runSQLwithResult(sqlstr, Session("Sequoia"))
                Dim myNodes = ""
                yourNodes = Session("wellknownNode") & yourNodes
                Dim n = yourNodes.Split(",")
                For Each x In n
                    If x <> "" Then
                        myNodes = myNodes & "<node>" & x & "</node>"
                    End If
                Next
                'Session("nodeList") = yourNodes
                Dim qt = Session("qt")
                sqlstr = "select eNonce from node where nodeaddress like '%" & GetClientIPAddress() & "%'"
                Dim eNonce As String = runSQLwithResult(sqlstr, Session("Sequoia"))

                result = "<nodes>" & myNodes & "</nodes><qt>" & qt & "</qt><eNonce>" & eNonce & "</eNonce><ip>" & GetClientIPAddress() & "</ip>"
            Case "reqheader"
                Dim header = IIf(IsNothing(Request.Form("header")), "", Request.Form("header"))
                header = header.Replace("%26lt;", "<").Replace("%26gt;", ">").Replace("%26", "&").Replace("&lt;", "<").Replace("&gt;", ">").replace("%2b", "+")
                writeLog(header)
                Dim sqlstr = "exec [node_serveReqHeader] '" & header & "'"

                result = runSQLwithResult(sqlstr, Session("Sequoia"))

                If result = "" Then
                    message = "Incorrect-Data"
                End If
            Case "reqblock"
                Dim blockId = getQueryVar("blockId")
                writeLog("blockId=" & blockId)
                Dim sqlstr = "exec [node_serveReqBlock] " & blockId & ""

                result = getXML(sqlstr, Session("Sequoia"))

                If result = "" Then
                    message = "Incorrect-Data"
                End If
            Case "reqsuspect"
                If message = "" Then
                    message = "Incorrect-Data"
                End If
            Case "reqtrx"
                Dim trxList = IIf(IsNothing(Request.Form("trxList")), "", Request.Form("trxList"))
                trxList = trxList.Replace("%26lt;", "<").Replace("%26gt;", ">").Replace("%26", "&").Replace("&lt;", "<").Replace("&gt;", ">").replace("%2b", "+")
                writeLog("trxlist=" & trxList)
                Dim sqlstr = "exec [node_serveReqTrx] '" & trxList & "', 'live', '" & Session("myED") & "', '" & pwd & "'"

                result = getXML(sqlstr, Session("Sequoia"))

                If result = "" Then
                    message = "Incorrect-Data"
                End If
            Case "confirmtrx"
                Dim trx = IIf(IsNothing(Request.Form("trx")), "", Request.Form("trx"))
                trx = trx.Replace("%26lt;", "<").Replace("%26gt;", ">").Replace("%26", "&").Replace("&lt;", "<").Replace("&gt;", ">").replace("%2b", "+")
                writeLog(trx)
                Dim sqlstr = "exec [node_serveConfirmTrx] '" & trx & "', 'live', '" & Session("myED") & "', '" & pwd & "'"

                result = getXML(sqlstr, Session("Sequoia"))

                If result = "" Then
                    message = "Incorrect-Data"
                End If

            Case "submittrx"
                Dim trx = IIf(IsNothing(Request.Form("trx")), "", Request.Form("trx"))
                Dim ip=getQueryVar("ip")
                trx = trx.Replace("%26lt;", "<").Replace("%26gt;", ">").Replace("%26", "&").Replace("&lt;", "<").Replace("&gt;", ">").replace("%2b", "+")
                writeLog(trx)
                Dim ss2 = "", eNonce = ""
                Dim sqlstr = "select eNonce, SS2 from node where nodeaddress like '%" & ip & "%'"
                Dim ds As DataSet = SelectSqlSrvRows(sqlstr, Session("Sequoia"))
                If ds.Tables.Count() > 0 Then
                    eNonce = ds.Tables(0).Rows(0).Item(0)
                    ss2 = ds.Tables(0).Rows(0).Item(1)
                End If
                If trx.indexOf("eNonce") >= 0 Then
                    sqlstr = "exec [node_getSubmitTrx] '" & trx & "', 'live', '" & Session("myED") & "', '" & pwd & "', '" & ss2 & "'"
                    message = runSQLwithResult(sqlstr, Session("Sequoia"))
                Else
                    message = "request-timeout"
                End If


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
