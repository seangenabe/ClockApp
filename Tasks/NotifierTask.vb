Imports MetroLog
Imports Windows.ApplicationModel.Background

Public NotInheritable Class NotifierTask
    Implements IBackgroundTask

    Public Shared ReadOnly Property TaskString As String = NameOf(NotifierTask)
    Public Shared ReadOnly Property EntryPointString As String = GetType(NotifierTask).FullName
    Private log As ILogger = LogManagerFactory.DefaultLogManager.GetLogger(Of NotifierTask)

    Shared Sub New()
        InitializeLogs()
    End Sub

    Public Sub Run(taskInstance As IBackgroundTaskInstance) Implements IBackgroundTask.Run
        Dim updater As New Updater()
        log.Debug("Start update batch.")
        updater.UpdateBatch()
        log.Debug("END background task.")
    End Sub

End Class
