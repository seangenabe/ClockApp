Imports MetroLog
Imports Tasks
Imports Windows.ApplicationModel.Background
Imports Windows.Storage

''' <summary>
''' An empty page that can be used on its own or navigated to within a Frame.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page

    Private log As ILogger = LogManagerFactory.DefaultLogManager.GetLogger(Of MainPage)

    Shared Sub New()
        InitializeLogs()
    End Sub

    Protected Overrides Async Sub OnNavigatedTo(e As NavigationEventArgs)
        tileFontSizeComboBox.SelectedIndex = TileFontSize

        Await UnregisterAll()

        Dim appTrigger As New ApplicationTrigger()
        log.Trace("Registering immediate task.")
        Await Register(appTrigger, AppEntryTask.TaskString, AppEntryTask.EntryPointString)
        log.Trace("Calling immediate task.")
        Await appTrigger.RequestAsync()

        Dim timeTrigger As New TimeTrigger(15, False)
        log.Trace("Registering timed task.")
        Await Register(timeTrigger, NotifierTask.TaskString, NotifierTask.EntryPointString)
    End Sub

    Private Property TileFontSize As Integer
        Get
            Return CInt(ApplicationData.Current.RoamingSettings.Values.ItemOrDefault("TileFontSize"))
        End Get
        Set(value As Integer)
            ApplicationData.Current.RoamingSettings.Values.Item("TileFontSize") = value
        End Set
    End Property

    Private Sub tileFontSizeComboBox_SelectionChanged(sender As Object, e As SelectionChangedEventArgs)
        TileFontSize = tileFontSizeComboBox.SelectedIndex
    End Sub

End Class
