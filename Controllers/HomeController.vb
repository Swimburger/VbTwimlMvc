Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading.Tasks
Imports MimeTypes
Imports Twilio.AspNet.Mvc
Imports Twilio.TwiML
Imports Twilio.TwiML.Messaging

Public Class HomeController
    Inherits TwilioController

    Shared httpClient As HttpClient = CreateHttpClient()
    Private Shared Function CreateHttpClient() As HttpClient
        Dim client As New HttpClient
        Dim appSettings As NameValueCollection = ConfigurationManager.AppSettings
        If Boolean.Parse(If(appSettings.Get("TwilioUseBasicAuthForMedia"), False)) Then
            Dim authString = $"{appSettings.Get("TwilioAccountSid")}:{appSettings.Get("TwilioAuthToken")}"
            authString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authString))
            client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Basic", authString)
        End If

        Return client
    End Function

    Async Function Index() As Task(Of TwiMLResult)
        Dim response As New MessagingResponse
        Dim message As New Message

        Dim numMedia = Short.Parse(If(Request.Form.Get("NumMedia"), 0))
        If numMedia = 0 Then
            response.Message("No file received.")
            Return TwiML(response)
        End If

        For mediaIndex As Integer = 0 To numMedia
            Dim mediaUrl = Request.Form.Get($"MediaUrl{mediaIndex}")
            Dim contentType = Request.Form.Get($"MediaContentType{mediaIndex}")
            Dim saveFilePath = Server.MapPath(String.Format(
                "~/App_Data/{0}{1}",
                Path.GetFileName(mediaUrl),
                MimeTypeMap.GetExtension(ContentType)
            ))
            Await DownloadUrlToFileAsync(mediaUrl, saveFilePath)
        Next

        response.Message("File received.")
        Return TwiML(response)
    End Function

    Private Async Function DownloadUrlToFileAsync(mediaUrl As String, saveFilePath As String) As Task
        Dim Response = Await httpClient.GetAsync(mediaUrl)
        Dim httpStream = Await Response.Content.ReadAsStreamAsync()
        Using fileStream As Stream = IO.File.Create(saveFilePath)
            Await httpStream.CopyToAsync(fileStream)
            Await fileStream.FlushAsync()
        End Using
    End Function
End Class
