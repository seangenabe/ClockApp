Imports MetroLog
Imports MetroLog.Targets

Public Module Util

    Sub New()
        InitializeLogs()
    End Sub

    Private areLogsInitialized As Boolean = False

    Public Sub InitializeLogs()
        If areLogsInitialized Then Return

        Dim config = LogManagerFactory.DefaultConfiguration
        config.AddTarget(LogLevel.Trace, LogLevel.Fatal, New JsonPostTarget(1, New Uri("http://127.0.0.1:49001/")))
        config.AddTarget(LogLevel.Trace, LogLevel.Fatal, New StreamingFileTarget())

        areLogsInitialized = True
    End Sub

    <Extension>
    Public Function ItemOrDefault(x As IPropertySet, key As String) As Object
        Try
            Return x.Item(key)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

End Module
