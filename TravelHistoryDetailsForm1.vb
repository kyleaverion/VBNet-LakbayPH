Friend Class TravelHistoryDetailsForm
    Private tripId As Integer

    Public Sub New(tripId As Integer)
        Me.tripId = tripId
    End Sub

    Friend Sub ShowDialog()
        Throw New NotImplementedException()
    End Sub
End Class
