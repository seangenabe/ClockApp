Imports System.Globalization
Imports System.Threading
Imports MetroLog
Imports Microsoft.Toolkit.Uwp.Notifications
Imports Windows.Data.Xml.Dom
Imports Windows.Storage
Imports Windows.UI.Notifications

Public NotInheritable Class Updater

    Private log As ILogger = LogManagerFactory.DefaultLogManager.GetLogger(Of Updater)()

    Public Function UpdateLoop() As IAsyncAction
        Return AsyncInfo.Run(AddressOf UpdateLoop)
    End Function


    Public Sub UpdateBatch()
        Dim now = DateTimeOffset.Now
        Dim res0 = CreateTileNotification(now)
        Dim notification0 = res0.Notification
        Dim diff0 = res0.Diff
        Dim updater = TileUpdateManager.CreateTileUpdaterForApplication()

        ' Schedule other notifications
        For _minute = 0 To 15
            Dim baseTime = now + diff0 + TimeSpan.FromMinutes(_minute)
            Dim res = CreateScheduledTileNotification(baseTime)
            log.Debug($"Registering scheduled notification; dt={res.Notification.DeliveryTime:o} ex={res.Notification.ExpirationTime:o}")
            updater.AddToSchedule(res.Notification)
        Next

        ' Send base notification (done later to account for synchronous CPU execution lag)
        log.Debug($"Sending notification; ex={res0.Notification.ExpirationTime:o}")
        updater.Update(notification0)
    End Sub

    Private Async Function UpdateLoop(cancel As CancellationToken) As Task
        Do While True
            cancel.ThrowIfCancellationRequested()
            Dim diff = UpdateTile()
            Await Task.Delay(diff)
        Loop
    End Function

    Private Function UpdateTile() As TimeSpan
        Dim result = CreateTileNotification(DateTimeOffset.Now)
        Dim notification = result.Notification
        Dim diff = result.Diff

        Dim updater = TileUpdateManager.CreateTileUpdaterForApplication()
        updater.Update(notification)

        Return diff
    End Function

#Region "CreateScheduledTileNotification"

    Private Class CreateScheduledTileNotificationResult
        Property Notification As ScheduledTileNotification
        Property Diff As TimeSpan
    End Class

    Private Function CreateScheduledTileNotification(baseTime As DateTimeOffset) As CreateScheduledTileNotificationResult
        Dim res = CreateTileXml(baseTime)
        Dim notification = New ScheduledTileNotification(res.Xml, baseTime)
        notification.Tag = "1"
        notification.ExpirationTime = res.NextMinute
        Return New CreateScheduledTileNotificationResult() With {.Notification = notification, .Diff = res.Diff}
    End Function

#End Region

#Region "CreateTileNotification"

    Private Class CreateTileNotificationResult
        Property Notification As TileNotification
        Property Diff As TimeSpan
    End Class

    Private Function CreateTileNotification(baseTime As DateTimeOffset) As CreateTileNotificationResult
        Dim res = CreateTileXml(baseTime)
        Dim notification = New TileNotification(res.Xml)
        notification.Tag = "1"
        notification.ExpirationTime = res.NextMinute
        Return New CreateTileNotificationResult() With {.Notification = notification, .Diff = res.Diff}
    End Function

#End Region

#Region "CreateTileXml"

    Private Class CreateTileXmlResult
        Property Xml As XmlDocument
        Property Diff As TimeSpan
        Property NextMinute As DateTimeOffset
    End Class

    Private Function CreateTileXml(baseTime As DateTimeOffset) As CreateTileXmlResult
        ' Get next minute
        Dim remainder = baseTime.Ticks Mod TimeSpan.TicksPerMinute
        Dim diff = TimeSpan.TicksPerMinute - remainder
        Dim nextMinute = New DateTimeOffset(baseTime.Ticks + diff, baseTime.Offset)

        Dim updater = TileUpdateManager.CreateTileUpdaterForApplication()
        updater.Clear()
        updater.EnableNotificationQueue(False)

        Dim tile = New TileContent()
        Dim visual = New TileVisual()
        Dim binding = New TileBinding()
        Dim content = New TileBindingContentAdaptive()
        Dim text = New AdaptiveText()
        text.Text = baseTime.ToLocalTime().ToString("t", CultureInfo.CurrentCulture)
        text.HintStyle = GetTileStyle()
        text.HintAlign = AdaptiveTextAlign.Center
        content.Children.Add(text)
        content.TextStacking = TileTextStacking.Center
        binding.Content = content
        visual.TileMedium = binding
        visual.TileWide = binding
        tile.Visual = visual

        Return New CreateTileXmlResult() With {.Xml = tile.GetXml(), .Diff = TimeSpan.FromTicks(diff), .NextMinute = nextMinute}
    End Function

#End Region

    Private Function GetTileStyle() As AdaptiveTextStyle
        Select Case TileFontSize
            Case 0
                Return AdaptiveTextStyle.HeaderNumeral
            Case 1
                Return AdaptiveTextStyle.SubheaderNumeral
        End Select
        Return AdaptiveTextStyle.TitleNumeral
    End Function

    Private Property TileFontSize As Integer
        Get
            Return CInt(ApplicationData.Current.RoamingSettings.Values.ItemOrDefault("TileFontSize"))
        End Get
        Set(value As Integer)
            ApplicationData.Current.RoamingSettings.Values.Item("TileFontSize") = value
        End Set
    End Property

End Class
