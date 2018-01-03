Imports Windows.ApplicationModel.Background

Public Module Tasks

    Public Function UnregisterAll() As IAsyncAction
        Dim tasks = From s In {NotifierTask.TaskString, AppEntryTask.TaskString} Select Unregister(s).AsTask()
        Return Task.WhenAll(tasks).AsAsyncAction()
    End Function

    Public Function Unregister(taskString As String) As IAsyncAction
        Return AsyncInfo.Run(
            Async Function() As Task
                Dim backgroundAccessStatus = Await BackgroundExecutionManager.RequestAccessAsync()
                Select Case backgroundAccessStatus
                    Case BackgroundAccessStatus.AllowedSubjectToSystemPolicy, BackgroundAccessStatus.AlwaysAllowed
                        For Each task In BackgroundTaskRegistration.AllTasks
                            If task.Value.Name = taskString Then
                                task.Value.Unregister(True)
                            End If
                        Next
                End Select
            End Function)
    End Function

    Public Function Register(trigger As IBackgroundTrigger, taskString As String, entryPoint As String) As IAsyncOperation(Of BackgroundTaskRegistration)
        Return AsyncInfo.Run(
            Async Function(cancel) As Task(Of BackgroundTaskRegistration)
                ' Register new background task
                Dim backgroundAccessStatus = Await BackgroundExecutionManager.RequestAccessAsync()
                Select Case backgroundAccessStatus
                    Case BackgroundAccessStatus.AllowedSubjectToSystemPolicy, BackgroundAccessStatus.AlwaysAllowed
                        Dim taskBuilder As New BackgroundTaskBuilder With {
                            .Name = taskString,
                            .TaskEntryPoint = entryPoint
                        }
                        taskBuilder.SetTrigger(trigger)
                        Return taskBuilder.Register()
                End Select
                Return Nothing
            End Function
        )
    End Function

End Module
