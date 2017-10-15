﻿Imports System.IO
Imports System.Text
Imports Fiddler

Public Class C_BattlegroundsProxy
    Inherits C_Proxy
    Dim battlegroundsURL As String = "front.battlegroundsgame.com"
    Dim proxyIP As String = "127.0.0.1"

    Public Overrides Sub ServerStarting()
        C_HostsFile.redirectSite(battlegroundsURL, proxyIP)
    End Sub

    Public Overrides Sub ServerClosing()
        C_HostsFile.unblockSiteRedirect("front.battlegroundsgame.com")
    End Sub

    ''' <summary>
    ''' Check HTTP requests to look for requests to the battlegrounds page
    ''' </summary>
    Overrides Sub FiddlerBeforeRequestHandler(ByVal tSession As Session)
        If tSession.url.Contains(battlegroundsURL + "/index.html") Then
            tSession.bBufferResponse = True

            ' Unblock the battlegrounds page so the response can be retrieved
            C_HostsFile.unblockSiteRedirect(battlegroundsURL)
        End If
    End Sub

    ''' <summary>
    ''' Returns a new edited battlegrounds page to the client
    ''' </summary>
    Overrides Sub FiddlerBeforeResponseHandler(ByVal tSession As Session)
        If Not tSession.bBufferResponse Then Exit Sub

        Console.WriteLine("Whitelisted URL response from " + tSession.fullUrl + "!")

        ' Decode the response
        tSession.utilDecodeResponse()

        ' Calculate the redirect URL to locate and download the main menu page (unblocking the page first)
        C_HostsFile.unblockSiteRedirect(battlegroundsURL)
        Dim menuHTML As String = C_WebMethods.fetchSiteHTML(GetBattlegroundsRedirectURL(Encoding.ASCII.GetString(tSession.ResponseBody)))

        ' Inject theme into webpage and convert back to transmittable bytes
        tSession.utilSetResponseBody(InstallThemeIntoPage(menuHTML))

        ' Re-block the battlegrounds page to catch future requests & responses
        C_HostsFile.redirectSite(battlegroundsURL, proxyIP)
    End Sub

    ''' <summary>
    ''' Locates the redirect URL and token inside the script at front.battlegroundsgame.com
    ''' </summary>
    Private Function GetBattlegroundsRedirectURL(ByVal redirectScript As String) As String
        ' This is what a redirect script from front.battlegroundsgame.com normally looks like:
        '<script>
        '    var url = window.location.search;
        '    url = url.replace("?", '');
        '    Location.href ='https://prod-live-front.playbattlegrounds.com/2017.09.14-2017.09.13-351/index-steam.2017.09.14-2017.09.13-351.html?'+url;
        '</script>

        ' Loop through the script to find the line that contains the next URL and extract the address
        For Each line As String In redirectScript.ToLower.Split(Environment.NewLine)
            If line.Contains("location.href='") Then
                Dim battlegroundsRedirectURL As String = line.Trim()

                ' Remove spaces and semi colons from code
                battlegroundsRedirectURL = battlegroundsRedirectURL.Replace(" ", "").Replace(";", "")

                ' Remove string beginning and end surrounding URL
                battlegroundsRedirectURL = battlegroundsRedirectURL.Replace("location.href='", "")
                battlegroundsRedirectURL = battlegroundsRedirectURL.Replace("'+url", "")

                Return battlegroundsRedirectURL
            End If
        Next

        Return Nothing
    End Function


    Private Function InstallThemeIntoPage(ByVal pageHTML As String) As String
        ' Tweak page for debugging
        pageHTML = pageHTML.Replace("oncontextmenu=""return false;""", "") 'Enable context menu in webbrowsers (blocked in-game)
        pageHTML = pageHTML.Replace("engine.hideOverlay();", "") 'Enable developer overlay in webbrowsers (blocked in-game)


        ' Apply user settings
        If True Then pageHTML = C_WebMethods.addStylesheetToHTML(".intro{visibility:hidden !important}", pageHTML)

        '' Hide original page UI
        ''menuHTML = C_WebMethods.addStylesheetToHTML(".con-connected{visibility:hidden !important}", menuHTML)



        '' Inject theme scripts.js into document head
        'Dim themeScriptsDir As String = rootPath + "scripts.js"
        'If File.Exists(themeScriptsDir) Then
        '    pageHTML = C_WebMethods.addScriptToHTML(File.ReadAllText(themeScriptsDir), pageHTML)
        'End If

        '' Check if any required files are missing
        'Dim themeHTMLDir As String = rootPath + "index.html"
        'Dim themeStylesheetDir As String = rootPath + "stylesheet.css"
        'Select Case True
        '    Case Not File.Exists(themeHTMLDir)
        '        MsgBox("The theme you have selected does not contain the index.html file!", MsgBoxStyle.Critical, "ERROR!")
        '        Application.Exit()
        '    Case Not File.Exists(themeStylesheetDir)
        '        MsgBox("The theme you have selected does not contain a stylesheet.css file!", MsgBoxStyle.Critical, "ERROR!")
        '        Application.Exit()
        'End Select

        '' Load theme code into vars
        'Dim themeHTMLCode As String = File.ReadAllText(themeHTMLDir)
        'Dim stylesheet As String = File.ReadAllText(themeStylesheetDir)

        '' Prepare theme code for injection via javascript string
        'themeHTMLCode = themeHTMLCode.Replace("""", "\""") ' Escape all double quotes in the HTML for JS compatibility
        'themeHTMLCode = themeHTMLCode.Replace(vbCr, "").Replace(vbLf, "") ' Remove all newLine characters
        'stylesheet = stylesheet.Replace("""", "\""") ' Escape all double quotes in the HTML for JS compatibility
        'stylesheet = stylesheet.Replace(vbCr, "").Replace(vbLf, "") ' Remove all newLine characters

        '' Append processed theme HTML&CSS into themeInjector variable
        'Dim themeInjectorScript As String = File.ReadAllText(resourcesDirectory + "themeInjector.js")
        'themeInjectorScript = themeInjectorScript.Replace("BT_HTMLHERE", themeHTMLCode)
        'themeInjectorScript = themeInjectorScript.Replace("BT_CSSHERE", stylesheet)


        '' Append themeInjector & dev tools into page end
        'If ImADevAndIWantAllTheMenuHTML Then pageHTML = C_WebMethods.addScriptToHTML(File.ReadAllText(resourcesDirectory + "fetchCode.js"), pageHTML)
        'pageHTML = C_WebMethods.addScriptToHTML(themeInjectorScript, pageHTML)

        '' Fix all references to local files in code
        'pageHTML = pageHTML.Replace("battlegroundsThemer_ROOT", "http://" + redirectIP.ToString)

        ''Write completed HTML to desktop for debugging
        'File.WriteAllText("C:\Users\fredd\Desktop\compiledDocument.html", pageHTML)

        Return pageHTML
    End Function
End Class
