Imports System.IO
Imports System.Data
Imports System.Data.SqlClient
Imports System.Xml

Public Class cl_base
    Inherits System.Web.UI.Page

    Protected contentOfsequoiaCon As String
    Protected contentOfaccountId As String
    Protected contentOfaccountGUID As String
    Protected contentOfthemeCode As String
    Protected contentOfthemeFolder As String
    Protected contentOfdbODBC As String
    Protected contentOfsqDB As String
    Protected contentOfsigninPage As String
    Protected contentOffrontPage As String
    Protected wordofWindowOnLoad As String = ""
    Protected contentOfScripts As String
    Protected contentofError As String = ""
    Protected isOnDelegation As Boolean = False

    Function runSQLwithResult(ByVal sqlstr As String, Optional ByVal sqlconstr As String = "") As String
        Dim result As String

        ' If the connection string is null, usse a default.
        Dim myConnectionString As String = sqlconstr
        If sqlconstr = "" Then myConnectionString = contentOfdbODBC
        If myConnectionString = "" And sqlconstr = "" Then
            'SignOff()
            Return ""
            Exit Function
        End If

        Dim myConnection As New SqlConnection(myConnectionString)
        Dim myInsertQuery As String = sqlstr
        Dim myCommand As New SqlCommand(myInsertQuery)
        Try
            Dim Reader As SqlClient.SqlDataReader

            myCommand.Connection = myConnection
            myConnection.Open()

            Reader = myCommand.ExecuteReader()

            Reader.Read()
            If Reader.HasRows Then
                result = Reader.GetValue(0).ToString
            Else
                result = ""
            End If

        Catch ex As SqlException
            contentofError = ex.Message & "<br>"
            Return ""
        Catch ex As Exception

            contentofError = ex.Message & "<br>"
            Return ""
        Finally
            myCommand.Connection.Close()
            myConnection.Close()
        End Try
        Return result
    End Function
    Public Function getXML(ByVal sqlstr As String, Optional ByVal sqlconstr As String = "") As String
        Dim result As String

        Dim myConnectionString As String
        ' If the connection string is null, usse a default.
        myConnectionString = contentOfdbODBC
        If myConnectionString = "" And sqlconstr = "" Then
            Return Nothing
            Exit Function
        End If


        If sqlconstr <> "" Then
            myConnectionString = sqlconstr
        End If

        Dim myConnection As New SqlConnection(myConnectionString)
        Dim myInsertQuery As String = sqlstr

        Dim myCommand As New SqlCommand(myInsertQuery)
        Try
            'Dim Reader As SqlClient.SqlDataReader
            myCommand.Connection = myConnection
            myConnection.Open()
            myCommand.CommandTimeout = 600
            Dim r As XmlReader = myCommand.ExecuteXmlReader()

            result = r.ReadOuterXml()

        Catch ex As SqlException
            'Response.Write(ex.Message)
            contentofError = ex.Message & "<br>"
            Return Nothing
        Catch ex As Exception
            'Response.Write(ex.Message)
            contentofError = ex.Message & "<br>"
            Return Nothing
        Finally
            myCommand.Connection.Close()
            myConnection.Close()
        End Try
        'GC.Collect()
        Return result

    End Function

    Public Function SelectSqlSrvRows(ByVal query As String, ByVal Optional sqlconstr As String = "") As DataSet

        Dim myConnectionString As String = sqlconstr
        If sqlconstr = "" Then myConnectionString = contentOfdbODBC

        Dim conn As New SqlConnection(myConnectionString)
        Dim adapter As New SqlDataAdapter
        Dim dataSet As New DataSet
        Try
            adapter.SelectCommand = New SqlCommand(query, conn)
            adapter.SelectCommand.CommandTimeout = 0
            adapter.Fill(dataSet)

        Catch ex As SqlException
            contentofError = query & ex.Message & "<br>"
        Catch ex As Exception
            contentofError = query & ex.Message & "<br>"
        Finally
            conn.Close()

        End Try
        adapter = Nothing
        'GC.Collect()
        Return dataSet

    End Function
    Sub loadAccount()
        Dim appSettings As NameValueCollection = ConfigurationManager.AppSettings
        'dynamic account
        contentOfsequoiaCon = appSettings.Item("sequoia")
        Session("sequoia") = contentOfsequoiaCon

        Dim x = Request.Url.Authority & Request.ApplicationPath
        If x.Substring(Len(x) - 1, 1) = "/" Then x = x.Substring(0, Len(x) - 1)

        Dim sqlstr = "select accountid from acctinfo a inner join acct b on a.accountguid=b.accountguid where infokey='address' and infovalue like '%" & x & "%'"
        contentOfaccountId = runSQLwithResult(sqlstr, Session("sequoia"))

        sqlstr = "exec core.info_acct '" & contentOfaccountId & "', 'ODBC'"
        contentOfdbODBC = runSQLwithResult(sqlstr, contentOfsequoiaCon)

        sqlstr = "exec core.info_acct '" & contentOfaccountId & "', 'themeCode'"
        contentOfthemeFolder = runSQLwithResult(sqlstr, contentOfsequoiaCon)

        sqlstr = "exec core.info_acct '" & contentOfaccountId & "', 'frontPage'"
        contentOffrontPage = runSQLwithResult(sqlstr, contentOfsequoiaCon)

        sqlstr = "exec core.info_acct '" & contentOfaccountId & "', 'signinPage'"
        contentOfsigninPage = runSQLwithResult(sqlstr, contentOfsequoiaCon)

        'dynamic account
        Dim ret As String = contentOfsequoiaCon.Replace(" ", "")
        ret = ret.Substring(ret.Replace(" ", "").IndexOf("atalog") + 7)
        ret = ret.Substring(0, ret.IndexOf(";"))
        contentOfsqDB = ret
    End Sub
    Sub writeLog(logMessage As String)
        Dim path = Server.MapPath("~/")
        path = path.Substring(0, Len(path) - 5) & "log\"
        Dim logFilepath = path & DateTime.Now().Year & "\" & Right("0" & DateTime.Now().Month, 2) & "\" & Right("0" & DateTime.Now().Day, 2) & ".txt"
        Dim logPath = path & DateTime.Now().Year & "\" & Right("0" & DateTime.Now().Month, 2) & "\"

        If (Not System.IO.Directory.Exists(logPath)) Then
            System.IO.Directory.CreateDirectory(logPath)
        End If
        Try
            Using w As StreamWriter = File.AppendText(logFilepath)
                w.Write(vbCrLf + "Log Entry : ")
                w.WriteLine("{0} {1}: " + vbCrLf + "{2}", DateTime.Now.ToLongTimeString(), DateTime.Now.ToLongDateString(), logMessage)
            End Using

        Catch ex As Exception

        End Try
    End Sub
    Function getQueryVar(key As String) As String
        Dim r = ""
        'replace this:' " ( ) ; , | < > - \ + & $ @
        If Not IsNothing(Request.QueryString(key)) Then
            If key.ToLower = "sqlfilter" Then
                r = Request.QueryString(key).Replace("--", "").Replace("+", "").Replace(";", "").Replace("<", "").Replace(">", "")
            Else
                r = Request.QueryString(key).Replace(" Then ", "").Replace("'", "").Replace("--", "").Replace("+", "").Replace(";", "").Replace("""", "").Replace("<", "").Replace(">", "")
                If Request.QueryString(key) <> r Then
                    writeLog(key & "from :" & Request.QueryString(key) & "to :" & r)
                End If
                'Else
                '    writeLog(key & ":" & "nothing")
            End If
        End If

        Return r
    End Function
    Function GetClientIPAddress() As String
        Dim context As System.Web.HttpContext = System.Web.HttpContext.Current
        Dim sIPAddress As String = context.Request.ServerVariables("HTTP_X_FORWARDED_FOR")
        If String.IsNullOrEmpty(sIPAddress) Then
            Return context.Request.ServerVariables("REMOTE_ADDR")
        Else
            Dim ipArray As String() = sIPAddress.Split(New [Char]() {","c})
            Return ipArray(0)
        End If
    End Function
    Function GetServerIPAddress() As String
        Return Request.ServerVariables("LOCAL_ADDR")
    End Function
End Class
