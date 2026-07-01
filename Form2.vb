Imports System.Drawing
Imports System.Windows.Forms
Imports System.IO
Imports MySqlConnector

Public Class AdventurerHomeForm
    Inherits Form

    Private leftPanel As Panel
    Private rightPanel As Panel
    Private gridPanel As TableLayoutPanel
    Private reviewCards As New List(Of ReviewCard)
    Private addNewCard As Panel
    Private scrollPanel As Panel

    ' Fix: Declare user property
    Private user As MainForm.UserInfo
    Private loggedInUser As MainForm.UserInfo

    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Dim conn As MySqlConnection = New MySqlConnection("Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;")
    Public sql As String
    Public cmd As MySqlCommand

    Public Sub New()
        InitializeComponent()
        SetupUI()
        LoadUserReviews()  ' Changed from LoadSampleData() to LoadUserReviews()
        ' Disable add review when no user is logged in
        DisableAddReviewFeature()
    End Sub

    Public Sub New(user As MainForm.UserInfo, connection As MySqlConnection)
        Me.user = user
        Me.conn = connection
        InitializeComponent()
        SetupUI()
        LoadUserReviews()  ' Load all reviews publicly
        ' Enable add review when user is logged in
        EnableAddReviewFeature()
    End Sub

    Public Sub New(loggedInUser As MainForm.UserInfo)
        Me.user = loggedInUser  ' Use the user property consistently
        Me.loggedInUser = loggedInUser

        ' Create a new connection for this form
        Me.conn = New MySqlConnection("Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;")

        InitializeComponent()
        SetupUI()
        LoadUserReviews()  ' Load all reviews publicly
        EnableAddReviewFeature()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'AdventurerHomeForm
        '
        Me.BackColor = System.Drawing.Color.FromArgb(CType(CType(44, Byte), Integer), CType(CType(62, Byte), Integer), CType(CType(80, Byte), Integer))
        Me.ClientSize = New System.Drawing.Size(1920, 1080)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.MaximizeBox = False
        Me.Name = "AdventurerHomeForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Adventurer's Journal - My Reviews"
        Me.ResumeLayout(False)
        Me.DoubleBuffered = True

    End Sub

    Private Sub SetupUI()
        ' Create main container
        Dim mainContainer As New TableLayoutPanel()
        mainContainer.Dock = DockStyle.Fill
        mainContainer.ColumnCount = 2
        mainContainer.RowCount = 1
        mainContainer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 35))
        mainContainer.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 65))

        ' Setup left panel
        SetupLeftPanel()
        mainContainer.Controls.Add(leftPanel, 0, 0)

        ' Setup right panel
        SetupRightPanel()
        mainContainer.Controls.Add(rightPanel, 1, 0)

        Me.Controls.Add(mainContainer)
    End Sub

    Private Sub SetupLeftPanel()
        leftPanel = New Panel()
        leftPanel.Dock = DockStyle.Fill
        leftPanel.BackColor = Color.FromArgb(52, 73, 94)
        leftPanel.Padding = New Padding(40)

        ' User welcome
        Dim welcomeLabel As New Label()
        If user IsNot Nothing Then
            welcomeLabel.Text = $"Welcome back," & vbCrLf & $"{user.FirstName}!"
        Else
            welcomeLabel.Text = "Welcome," & vbCrLf & "Adventurer!"
        End If
        welcomeLabel.Font = New Font("Arial", 28, FontStyle.Bold)
        welcomeLabel.ForeColor = Color.White
        welcomeLabel.AutoSize = True
        welcomeLabel.Location = New Point(40, 100)

        ' Main quote label
        Dim quoteLabel As New Label()
        quoteLabel.Text = "YOUR ADVENTURE" & vbCrLf & "MEMORIES"
        quoteLabel.Font = New Font("Arial", 24, FontStyle.Bold)
        quoteLabel.ForeColor = Color.FromArgb(52, 152, 219)
        quoteLabel.AutoSize = True
        quoteLabel.Location = New Point(40, 200)

        ' Subtitle
        Dim subtitleLabel As New Label()
        subtitleLabel.Text = "Relive your journeys through the reviews" & vbCrLf & "and memories you've shared. Each card" & vbCrLf & "tells a story of adventure and discovery."
        subtitleLabel.Font = New Font("Arial", 12, FontStyle.Regular)
        subtitleLabel.ForeColor = Color.FromArgb(189, 195, 199)
        subtitleLabel.AutoSize = True
        subtitleLabel.Location = New Point(40, 320)

        ' Stats panel (will be updated after loading reviews)
        Dim statsPanel As New Panel()
        statsPanel.Name = "StatsPanel" ' Add name for easier access
        statsPanel.Location = New Point(40, 420)
        statsPanel.Size = New Size(300, 100)
        statsPanel.BackColor = Color.FromArgb(44, 62, 80)

        Dim reviewCountLabel As New Label()
        reviewCountLabel.Name = "ReviewCountLabel"
        reviewCountLabel.Text = "Total Reviews: 0"
        reviewCountLabel.Font = New Font("Arial", 11, FontStyle.Bold)
        reviewCountLabel.ForeColor = Color.FromArgb(52, 152, 219)
        reviewCountLabel.Location = New Point(15, 15)
        reviewCountLabel.AutoSize = True

        Dim averageRatingLabel As New Label()
        averageRatingLabel.Name = "AverageRatingLabel"
        averageRatingLabel.Text = "Average Rating: 0.0/5.0 ⭐"
        averageRatingLabel.Font = New Font("Arial", 11, FontStyle.Bold)
        averageRatingLabel.ForeColor = Color.FromArgb(241, 196, 15)
        averageRatingLabel.Location = New Point(15, 40)
        averageRatingLabel.AutoSize = True

        statsPanel.Controls.AddRange({reviewCountLabel, averageRatingLabel})

        ' CTA Button
        Dim ctaButton As New Button()
        ctaButton.Name = "CtaButton"
        ctaButton.Text = "ADD NEW REVIEW"
        ctaButton.Size = New Size(200, 50)
        ctaButton.Location = New Point(40, 550)
        ctaButton.BackColor = Color.FromArgb(231, 76, 60)
        ctaButton.ForeColor = Color.White
        ctaButton.Font = New Font("Arial", 11, FontStyle.Bold)
        ctaButton.FlatStyle = FlatStyle.Flat
        ctaButton.FlatAppearance.BorderSize = 0
        ctaButton.Cursor = Cursors.Hand
        AddHandler ctaButton.Click, AddressOf AddNewReview_Click

        ' Close Button
        Dim closeButton As New Button()
        closeButton.Text = "BACK TO MAIN"
        closeButton.Size = New Size(200, 40)
        closeButton.Location = New Point(40, 620)
        closeButton.BackColor = Color.FromArgb(95, 106, 106)
        closeButton.ForeColor = Color.White
        closeButton.Font = New Font("Arial", 10, FontStyle.Regular)
        closeButton.FlatStyle = FlatStyle.Flat
        closeButton.FlatAppearance.BorderSize = 0
        closeButton.Cursor = Cursors.Hand
        AddHandler closeButton.Click, AddressOf CloseForm_Click

        leftPanel.Controls.AddRange({welcomeLabel, quoteLabel, subtitleLabel, statsPanel, ctaButton, closeButton})

    End Sub

    Private Sub SetupRightPanel()
        rightPanel = New Panel()
        rightPanel.Dock = DockStyle.Fill
        rightPanel.BackColor = Color.FromArgb(236, 240, 241)
        rightPanel.Padding = New Padding(20)

        ' Create scrollable panel for reviews
        scrollPanel = New Panel()
        scrollPanel.Dock = DockStyle.Fill
        scrollPanel.AutoScroll = True
        scrollPanel.BackColor = Color.FromArgb(236, 240, 241)

        ' Grid for review cards (dynamic sizing)
        gridPanel = New TableLayoutPanel()
        gridPanel.AutoSize = True
        gridPanel.ColumnCount = 3 ' 3 columns for better layout
        gridPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        gridPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        gridPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 33.33F))
        gridPanel.GrowStyle = TableLayoutPanelGrowStyle.AddRows
        gridPanel.Dock = DockStyle.Top

        ' Add "Add New Review" card
        SetupAddNewCard()
        gridPanel.Controls.Add(addNewCard, 0, 0)

        scrollPanel.Controls.Add(gridPanel)
        rightPanel.Controls.Add(scrollPanel)
    End Sub

    Private Sub SetupAddNewCard()
        addNewCard = New Panel()
        addNewCard.Name = "AddNewCard"
        addNewCard.Margin = New Padding(10)
        addNewCard.BackColor = Color.White
        addNewCard.BorderStyle = BorderStyle.FixedSingle
        addNewCard.Cursor = Cursors.Hand
        addNewCard.Size = New Size(300, 200)

        Dim addLabel As New Label()
        addLabel.Text = "✏️" & vbCrLf & "WRITE NEW REVIEW"
        addLabel.Font = New Font("Arial", 14, FontStyle.Bold)
        addLabel.ForeColor = Color.FromArgb(127, 140, 141)
        addLabel.TextAlign = ContentAlignment.MiddleCenter
        addLabel.Dock = DockStyle.Fill

        addNewCard.Controls.Add(addLabel)
        AddHandler addNewCard.Click, AddressOf AddNewReview_Click
        AddHandler addLabel.Click, AddressOf AddNewReview_Click
    End Sub

    Private Sub EnableAddReviewFeature()
        ' Enable the add new review functionality when user is logged in
        Try
            Dim ctaButton As Button = DirectCast(leftPanel.Controls("CtaButton"), Button)
            If ctaButton IsNot Nothing Then
                ctaButton.Enabled = True
                ctaButton.BackColor = Color.FromArgb(231, 76, 60)
                ctaButton.Text = "ADD NEW REVIEW"
            End If

            If addNewCard IsNot Nothing Then
                addNewCard.Enabled = True
                addNewCard.BackColor = Color.White
                Dim addLabel As Label = DirectCast(addNewCard.Controls(0), Label)
                If addLabel IsNot Nothing Then
                    addLabel.ForeColor = Color.FromArgb(127, 140, 141)
                    addLabel.Text = "✏️" & vbCrLf & "WRITE NEW REVIEW"
                End If
            End If
        Catch ex As Exception
            ' Handle error silently or log to file if needed
        End Try
    End Sub

    Private Sub DisableAddReviewFeature()
        ' Disable the add new review functionality when no user is logged in
        Try
            Dim ctaButton As Button = DirectCast(leftPanel.Controls("CtaButton"), Button)
            If ctaButton IsNot Nothing Then
                ctaButton.Enabled = False
                ctaButton.BackColor = Color.Gray
                ctaButton.Text = "LOGIN TO ADD REVIEW"
            End If

            If addNewCard IsNot Nothing Then
                addNewCard.Enabled = False
                addNewCard.BackColor = Color.FromArgb(200, 200, 200)
                Dim addLabel As Label = DirectCast(addNewCard.Controls(0), Label)
                If addLabel IsNot Nothing Then
                    addLabel.ForeColor = Color.Gray
                    addLabel.Text = "🔒" & vbCrLf & "LOGIN TO ADD REVIEW"
                End If
                ' Remove click events for disabled state
                RemoveHandler addNewCard.Click, AddressOf AddNewReview_Click
                RemoveHandler addLabel.Click, AddressOf AddNewReview_Click
            End If
        Catch ex As Exception
            ' Handle error silently or log to file if needed
        End Try
    End Sub

    Private Sub LoadUserReviews()
        Try
            ' Clear existing reviews first
            reviewCards.Clear()
            gridPanel.Controls.Clear()

            ' Re-add the "Add New Review" card first
            SetupAddNewCard()
            gridPanel.Controls.Add(addNewCard, 0, 0)

            ' Create a fresh connection for this operation
            Using tempConn As New MySqlConnection(connectionString)
                tempConn.Open()

                ' ALWAYS load ALL reviews for public viewing - regardless of login status
                Dim query As String = "SELECT r.ReviewID, r.UserID, r.BookingID, r.Rating, r.Comment, r.ReviewDate, " &
                   "u.FirstName, u.LastName, tp.Title as PackageTitle " &
                   "FROM Reviews r " &
                   "LEFT JOIN Users u ON r.UserID = u.UserID " &
                   "LEFT JOIN Bookings b ON r.BookingID = b.BookingID " &
                   "LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
                   "ORDER BY r.ReviewDate DESC " &
                   "LIMIT 50"  ' Show up to 50 most recent reviews

                Using cmd As New MySqlCommand(query, tempConn)
                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        Dim reviewCount As Integer = 0

                        If Not reader.HasRows Then
                            MessageBox.Show("No reviews found in the database yet. Be the first to write a review!",
                                      "No Reviews", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        End If

                        While reader.Read()
                            reviewCount += 1

                            Dim reviewText As String = If(IsDBNull(reader("Comment")), "No review text provided.", reader("Comment").ToString())
                            Dim rating As Double = If(IsDBNull(reader("Rating")), 0.0, Convert.ToDouble(reader("Rating")))
                            Dim reviewDate As DateTime = If(IsDBNull(reader("ReviewDate")), DateTime.Now, Convert.ToDateTime(reader("ReviewDate")))
                            Dim bookingID As Integer = If(IsDBNull(reader("BookingID")), 0, Convert.ToInt32(reader("BookingID")))
                            Dim reviewerName As String = If(IsDBNull(reader("FirstName")), "Anonymous", reader("FirstName").ToString())
                            Dim packageTitle As String = If(IsDBNull(reader("PackageTitle")), "", reader("PackageTitle").ToString())

                            ' Add last name if available
                            If Not IsDBNull(reader("LastName")) Then
                                reviewerName &= " " & reader("LastName").ToString()
                            End If

                            ' Get destination - use package title if available, otherwise fallback
                            Dim destination As String
                            If Not String.IsNullOrEmpty(packageTitle) Then
                                destination = packageTitle
                            ElseIf bookingID > 0 Then
                                destination = $"Trip #{bookingID}"
                            Else
                                destination = "General Review"
                            End If
                            Dim travelDate As DateTime = reviewDate.AddDays(-7) ' Default travel date

                            ' Create review card with reviewer name
                            CreateReviewCardWithReviewer(reviewText, destination, rating, reviewDate, travelDate, reviewerName)
                        End While

                        If reviewCount = 0 Then
                            ' If no reviews exist at all, show sample data
                            LoadSampleData()
                            Return
                        End If
                    End Using
                End Using

            End Using

        Catch ex As Exception
            MessageBox.Show($"Error loading reviews: {ex.Message}",
                      "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            LoadSampleData() ' Fallback to sample data
        End Try

        ' Update stats after loading
        UpdateStatsPanel()
    End Sub

    Private Function GetDestinationFromBooking(bookingID As Integer) As String
        If bookingID = 0 Then
            Return "General Review"
        End If

        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT tp.Title FROM Bookings b " &
                         "LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
                         "WHERE b.BookingID = @BookingID"

            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@BookingID", bookingID)
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    Return result.ToString()
                Else
                    Return $"Booking #{bookingID}"
                End If
            End Using

        Catch ex As Exception
            Return $"Booking #{bookingID}"
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Function GetTravelDateFromBooking(bookingID As Integer) As DateTime
        If bookingID = 0 Then
            Return DateTime.Now.AddDays(-7) ' Default travel date
        End If

        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT TravelDate FROM Bookings WHERE BookingID = @BookingID"

            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@BookingID", bookingID)
                Dim result As Object = cmd.ExecuteScalar()
                If result IsNot Nothing AndAlso Not IsDBNull(result) Then
                    Return Convert.ToDateTime(result)
                Else
                    Return DateTime.Now.AddDays(-7) ' Default travel date
                End If
            End Using

        Catch ex As Exception
            Return DateTime.Now.AddDays(-7) ' Default travel date
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Sub LoadSampleData()
        ' Clear existing reviews first
        reviewCards.Clear()
        gridPanel.Controls.Clear()

        ' Re-add the "Add New Review" card first
        SetupAddNewCard()
        gridPanel.Controls.Add(addNewCard, 0, 0)

        ' Sample review cards if no database connection
        CreateReviewCard("Amazing experience! The mountain views were breathtaking and the guide was very knowledgeable.", "Mountain Explorer Package", 5.0, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-35))
        CreateReviewCard("Great adventure, though the weather wasn't perfect. Still enjoyed the cultural experience.", "Desert Safari Adventure", 4.0, DateTime.Now.AddDays(-15), DateTime.Now.AddDays(-20))
        CreateReviewCard("Perfect getaway! The beaches were pristine and the accommodation was excellent.", "Coastal Paradise Tour", 4.5, DateTime.Now.AddDays(-7), DateTime.Now.AddDays(-10))
        UpdateStatsPanel()
    End Sub

    Private Sub CreateReviewCard(reviewText As String, destination As String, rating As Double, reviewDate As DateTime, travelDate As DateTime)
        Try
            Dim card As New ReviewCard(reviewText, destination, rating, reviewDate, travelDate)
            reviewCards.Add(card)

            ' Find next available position
            Dim position As Point = GetNextGridPosition()
            gridPanel.Controls.Add(card.Panel, position.X, position.Y)
        Catch ex As Exception
            ' Handle error silently or log to file if needed
        End Try
    End Sub

    Private Sub CreateReviewCardWithReviewer(reviewText As String, destination As String, rating As Double, reviewDate As DateTime, travelDate As DateTime, reviewerName As String)
        Try
            Dim card As New ReviewCardWithReviewer(reviewText, destination, rating, reviewDate, travelDate, reviewerName)
            reviewCards.Add(card)

            ' Find next available position
            Dim position As Point = GetNextGridPosition()
            gridPanel.Controls.Add(card.Panel, position.X, position.Y)
        Catch ex As Exception
            ' Handle error silently or log to file if needed
        End Try
    End Sub

    Private Function GetNextGridPosition() As Point
        Dim totalControls As Integer = gridPanel.Controls.Count
        Dim cols As Integer = gridPanel.ColumnCount

        Dim row As Integer = totalControls \ cols
        Dim col As Integer = totalControls Mod cols

        ' Ensure we have enough rows
        If row >= gridPanel.RowCount Then
            gridPanel.RowCount = row + 1
            gridPanel.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        End If

        Return New Point(col, row)
    End Function

    Private Sub UpdateStatsPanel()
        Try
            ' Find the stats panel by name
            Dim statsPanel As Panel = DirectCast(leftPanel.Controls("StatsPanel"), Panel)
            If statsPanel IsNot Nothing Then
                Dim reviewCountLabel As Label = DirectCast(statsPanel.Controls("ReviewCountLabel"), Label)
                Dim averageRatingLabel As Label = DirectCast(statsPanel.Controls("AverageRatingLabel"), Label)

                If reviewCountLabel IsNot Nothing Then
                    reviewCountLabel.Text = "Total Reviews: " & reviewCards.Count.ToString()
                End If

                If averageRatingLabel IsNot Nothing Then
                    Dim avgRating As Double = If(reviewCards.Count > 0, reviewCards.Average(Function(r) r.Rating), 0)
                    averageRatingLabel.Text = $"Average Rating: {avgRating:F1}/5.0 ⭐"
                End If
            End If
        Catch ex As Exception
            ' Handle error silently or log to file if needed
        End Try
    End Sub

    Private Sub AddNewReview_Click(sender As Object, e As EventArgs)
        If user IsNot Nothing Then
            Try
                ' First check if user has any completed bookings that can be reviewed
                If Not HasCompletedBookingsToReview() Then
                    MessageBox.Show("You don't have any completed trips to review yet. Complete a trip first to share your experience!",
                          "No Trips to Review", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Return
                End If

                ' Use the correct constructor that expects MainForm.UserInfo
                Using reviewDialog As New ReviewDialog(user)
                    Dim result As DialogResult = reviewDialog.ShowDialog(Me)
                    If result = DialogResult.OK Then
                        LoadUserReviews()  ' This should now work properly
                        MessageBox.Show("Review added successfully!", "Review Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End Using
            Catch ex As Exception
                MessageBox.Show($"Error opening review dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Else
            MessageBox.Show("Please login to add a review.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Function HasCompletedBookingsToReview() As Boolean
        If user Is Nothing OrElse conn Is Nothing Then
            Return False
        End If

        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            ' Check if user has completed bookings
            Dim query As String = "SELECT COUNT(*) FROM Bookings WHERE UserID = @UserID AND Status = 'Completed'"
            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@UserID", user.UserID)
                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count > 0
            End Using

        Catch ex As Exception
            Return True ' Default to true to allow review attempt
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Sub CloseForm_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    Private Sub AdventurerHomeForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If user IsNot Nothing Then
            LoadUserReviews()
        End If
    End Sub

    ' Public method to refresh reviews (can be called externally)
    Public Sub RefreshReviews()
        ' Always load all reviews for public viewing, regardless of login status
        LoadUserReviews()
    End Sub

    ' Add a public method to refresh reviews from external forms
    Public Sub RefreshReviewsAfterAdd()
        LoadUserReviews()  ' Load all reviews, not just user reviews
    End Sub
End Class

Public Class ReviewCard
    Public Property Panel As Panel
    Public Property ReviewText As String
    Public Property Destination As String
    Public Property Rating As Double
    Public Property ReviewDate As DateTime
    Public Property TravelDate As DateTime

    Public Sub New(reviewText As String, destination As String, rating As Double, reviewDate As DateTime, travelDate As DateTime)
        Me.ReviewText = reviewText
        Me.Destination = destination
        Me.Rating = rating
        Me.ReviewDate = reviewDate
        Me.TravelDate = travelDate
        CreatePanel()
    End Sub

    Private Sub CreatePanel()
        Panel = New Panel()
        Panel.Margin = New Padding(10)
        Panel.Size = New Size(300, 220)
        Panel.BackColor = GetCardColor()
        Panel.Cursor = Cursors.Hand

        ' Destination label
        Dim destinationLabel As New Label()
        destinationLabel.Text = Destination
        destinationLabel.Font = New Font("Arial", 12, FontStyle.Bold)
        destinationLabel.ForeColor = Color.White
        destinationLabel.Location = New Point(15, 15)
        destinationLabel.Size = New Size(270, 25)
        destinationLabel.AutoEllipsis = True

        ' Rating display
        Dim ratingLabel As New Label()
        Dim stars As String = New String("⭐"c, CInt(Math.Round(Rating)))
        ratingLabel.Text = $"{Rating:F1}/5.0 {stars}"
        ratingLabel.Font = New Font("Arial", 10, FontStyle.Bold)
        ratingLabel.ForeColor = Color.FromArgb(241, 196, 15)
        ratingLabel.Location = New Point(15, 45)
        ratingLabel.AutoSize = True

        ' Review text
        Dim reviewLabel As New Label()
        reviewLabel.Text = If(ReviewText.Length > 120, ReviewText.Substring(0, 117) + "...", ReviewText)
        reviewLabel.Font = New Font("Arial", 9, FontStyle.Regular)
        reviewLabel.ForeColor = Color.White
        reviewLabel.Location = New Point(15, 75)
        reviewLabel.Size = New Size(270, 100)
        reviewLabel.AutoSize = False

        ' Date labels
        Dim travelDateLabel As New Label()
        travelDateLabel.Text = $"Traveled: {If(TravelDate = DateTime.MinValue, "N/A", TravelDate.ToString("MMM dd, yyyy"))}"
        travelDateLabel.Font = New Font("Arial", 8, FontStyle.Regular)
        travelDateLabel.ForeColor = Color.FromArgb(236, 240, 241)
        travelDateLabel.Location = New Point(15, 185)
        travelDateLabel.AutoSize = True

        Dim reviewDateLabel As New Label()
        reviewDateLabel.Text = $"Reviewed: {ReviewDate:MMM dd, yyyy}"
        reviewDateLabel.Font = New Font("Arial", 8, FontStyle.Regular)
        reviewDateLabel.ForeColor = Color.FromArgb(236, 240, 241)
        reviewDateLabel.Location = New Point(15, 200)
        reviewDateLabel.AutoSize = True

        Panel.Controls.AddRange({destinationLabel, ratingLabel, reviewLabel, travelDateLabel, reviewDateLabel})

        ' Add click events to all controls
        AddHandler Panel.Click, AddressOf Card_Click
        For Each ctrl As Control In Panel.Controls
            AddHandler ctrl.Click, AddressOf Card_Click
        Next
    End Sub

    Public Function GetCardColor() As Color
        ' Color based on rating
        If Rating >= 4.5 Then
            Return Color.FromArgb(39, 174, 96) ' Green for excellent
        ElseIf Rating >= 4.0 Then
            Return Color.FromArgb(52, 152, 219) ' Blue for very good
        ElseIf Rating >= 3.0 Then
            Return Color.FromArgb(230, 126, 34) ' Orange for good
        ElseIf Rating >= 2.0 Then
            Return Color.FromArgb(241, 196, 15) ' Yellow for fair
        Else
            Return Color.FromArgb(231, 76, 60) ' Red for poor
        End If
    End Function

    Private Sub Card_Click(sender As Object, e As EventArgs)
        ' Show full review details
        Dim message As String = $"Destination: {Destination}" & vbCrLf & vbCrLf &
                               $"Rating: {Rating:F1}/5.0 ⭐" & vbCrLf & vbCrLf &
                               $"Review: {ReviewText}" & vbCrLf & vbCrLf &
                               $"Travel Date: {If(TravelDate = DateTime.MinValue, "N/A", TravelDate.ToString("MMMM dd, yyyy"))}" & vbCrLf &
                               $"Review Date: {ReviewDate:MMMM dd, yyyy}"

        MessageBox.Show(message, "Review Details", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class

Public Class ReviewCardWithReviewer
    Inherits ReviewCard

    Public Property ReviewerName As String

    Public Sub New(reviewText As String, destination As String, rating As Double, reviewDate As DateTime, travelDate As DateTime, reviewerName As String)
        MyBase.New(reviewText, destination, rating, reviewDate, travelDate)
        Me.ReviewerName = reviewerName
        ' Recreate panel with reviewer name
        CreatePanelWithReviewer()
    End Sub

    Private Sub CreatePanelWithReviewer()
        Panel = New Panel()
        Panel.Margin = New Padding(10)
        Panel.Size = New Size(300, 240)  ' Slightly taller to accommodate reviewer name
        Panel.BackColor = MyBase.GetCardColor()  ' Use MyBase to access parent's public method
        Panel.Cursor = Cursors.Hand

        ' Destination label
        Dim destinationLabel As New Label()
        destinationLabel.Text = Destination
        destinationLabel.Font = New Font("Arial", 12, FontStyle.Bold)
        destinationLabel.ForeColor = Color.White
        destinationLabel.Location = New Point(15, 15)
        destinationLabel.Size = New Size(270, 25)
        destinationLabel.AutoEllipsis = True

        ' Reviewer name
        Dim reviewerLabel As New Label()
        reviewerLabel.Text = $"by {ReviewerName}"
        reviewerLabel.Font = New Font("Arial", 9, FontStyle.Italic)
        reviewerLabel.ForeColor = Color.FromArgb(220, 220, 220)
        reviewerLabel.Location = New Point(15, 40)
        reviewerLabel.AutoSize = True

        ' Rating display
        Dim ratingLabel As New Label()
        Dim stars As String = New String("⭐"c, CInt(Math.Round(Rating)))
        ratingLabel.Text = $"{Rating:F1}/5.0 {stars}"
        ratingLabel.Font = New Font("Arial", 10, FontStyle.Bold)
        ratingLabel.ForeColor = Color.FromArgb(241, 196, 15)
        ratingLabel.Location = New Point(15, 60)
        ratingLabel.AutoSize = True

        ' Review text
        Dim reviewLabel As New Label()
        reviewLabel.Text = If(ReviewText.Length > 120, ReviewText.Substring(0, 117) + "...", ReviewText)
        reviewLabel.Font = New Font("Arial", 9, FontStyle.Regular)
        reviewLabel.ForeColor = Color.White
        reviewLabel.Location = New Point(15, 85)
        reviewLabel.Size = New Size(270, 100)
        reviewLabel.AutoSize = False

        ' Date labels
        Dim travelDateLabel As New Label()
        travelDateLabel.Text = $"Traveled: {If(TravelDate = DateTime.MinValue, "N/A", TravelDate.ToString("MMM dd, yyyy"))}"
        travelDateLabel.Font = New Font("Arial", 8, FontStyle.Regular)
        travelDateLabel.ForeColor = Color.FromArgb(236, 240, 241)
        travelDateLabel.Location = New Point(15, 195)
        travelDateLabel.AutoSize = True

        Dim reviewDateLabel As New Label()
        reviewDateLabel.Text = $"Reviewed: {ReviewDate:MMM dd, yyyy}"
        reviewDateLabel.Font = New Font("Arial", 8, FontStyle.Regular)
        reviewDateLabel.ForeColor = Color.FromArgb(236, 240, 241)
        reviewDateLabel.Location = New Point(15, 210)
        reviewDateLabel.AutoSize = True

        Panel.Controls.AddRange({destinationLabel, reviewerLabel, ratingLabel, reviewLabel, travelDateLabel, reviewDateLabel})

        ' Add click events to all controls
        AddHandler Panel.Click, AddressOf CardWithReviewer_Click
        For Each ctrl As Control In Panel.Controls
            AddHandler ctrl.Click, AddressOf CardWithReviewer_Click
        Next
    End Sub

    Private Sub CardWithReviewer_Click(sender As Object, e As EventArgs)
        ' Show full review details with reviewer name
        Dim message As String = $"Destination: {Destination}" & vbCrLf & vbCrLf &
                               $"Reviewed by: {ReviewerName}" & vbCrLf & vbCrLf &
                               $"Rating: {Rating:F1}/5.0 ⭐" & vbCrLf & vbCrLf &
                               $"Review: {ReviewText}" & vbCrLf & vbCrLf &
                               $"Travel Date: {If(TravelDate = DateTime.MinValue, "N/A", TravelDate.ToString("MMMM dd, yyyy"))}" & vbCrLf &
                               $"Review Date: {ReviewDate:MMMM dd, yyyy}"

        MessageBox.Show(message, "Review Details", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
End Class