Imports System.Data
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class BookingDetailsForms
    Inherits Form

    Private connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Private bookingReference As String

    ' UI Controls
    Private headerPanel As Panel
    Private companyLabel As Label
    Private logoLabel As Label
    Private detailsPanel As Panel
    Private backButton As Button
    Private bookingRefLabel As Label
    Private statusLabel As Label

    ' Detail Labels
    Private customerNameLabel As Label
    Private numberOfPeopleLabel As Label
    Private bookingDateLabel As Label
    Private travelDateLabel As Label
    Private endDateLabel As Label
    Private destinationLabel As Label
    Private packageLabel As Label
    Private addOnsLabel As Label
    Private paymentMethodLabel As Label
    Private paymentStatusLabel As Label
    Private finalAmountLabel As Label
    Private ratingLabel As Label

    Public Sub New(bookingRef As String)
        Me.bookingReference = bookingRef
        InitializeComponent()
        LoadBookingDetails()
    End Sub

    Private Sub InitializeComponent()
        ' Form Properties
        Me.Text = "Booking Details - LakbayPH Travel and Tours"
        Me.Size = New Size(900, 700)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.BackColor = Color.FromArgb(245, 247, 250)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.Font = New Font("Segoe UI", 9.0F, FontStyle.Regular)
        Me.DoubleBuffered = True


        ' Header Panel
        headerPanel = New Panel()
        headerPanel.BackColor = Color.FromArgb(41, 128, 185)
        headerPanel.Size = New Size(900, 100)
        headerPanel.Location = New Point(0, 0)
        headerPanel.Dock = DockStyle.Top

        ' Company Logo/Icon
        logoLabel = New Label()
        logoLabel.Text = "✈️"
        logoLabel.Font = New Font("Segoe UI Emoji", 24.0F, FontStyle.Bold)
        logoLabel.ForeColor = Color.White
        logoLabel.Location = New Point(30, 25)
        logoLabel.Size = New Size(60, 50)
        logoLabel.TextAlign = ContentAlignment.MiddleCenter

        ' Company Name
        companyLabel = New Label()
        companyLabel.Text = "LakbayPH Travel and Tours"
        companyLabel.Font = New Font("Segoe UI", 20.0F, FontStyle.Bold)
        companyLabel.ForeColor = Color.White
        companyLabel.Location = New Point(100, 15)
        companyLabel.Size = New Size(400, 35)

        ' Subtitle
        Dim subtitleLabel As New Label()
        subtitleLabel.Text = "Your Journey, Our Passion"
        subtitleLabel.Font = New Font("Segoe UI", 10.0F, FontStyle.Italic)
        subtitleLabel.ForeColor = Color.FromArgb(220, 235, 250)
        subtitleLabel.Location = New Point(100, 50)
        subtitleLabel.Size = New Size(200, 20)

        ' Back Button
        backButton = New Button()
        backButton.Text = "← Back"
        backButton.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        backButton.BackColor = Color.FromArgb(52, 152, 219)
        backButton.ForeColor = Color.White
        backButton.FlatStyle = FlatStyle.Flat
        backButton.FlatAppearance.BorderSize = 0
        backButton.Size = New Size(100, 35)
        backButton.Location = New Point(750, 32)
        backButton.Cursor = Cursors.Hand
        AddHandler backButton.Click, AddressOf BackButton_Click

        ' Add to header panel
        headerPanel.Controls.Add(logoLabel)
        headerPanel.Controls.Add(companyLabel)
        headerPanel.Controls.Add(subtitleLabel)
        headerPanel.Controls.Add(backButton)

        ' Main Details Panel
        detailsPanel = New Panel()
        detailsPanel.BackColor = Color.White
        detailsPanel.Location = New Point(30, 130)
        detailsPanel.Size = New Size(840, 520)
        detailsPanel.BorderStyle = BorderStyle.None

        ' Add shadow effect
        Dim shadowPanel As New Panel()
        shadowPanel.BackColor = Color.FromArgb(200, 200, 200)
        shadowPanel.Location = New Point(33, 133)
        shadowPanel.Size = New Size(840, 520)

        ' Booking Reference Header
        bookingRefLabel = New Label()
        bookingRefLabel.Text = $"Booking Reference: {bookingReference}"
        bookingRefLabel.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
        bookingRefLabel.ForeColor = Color.FromArgb(41, 128, 185)
        bookingRefLabel.Location = New Point(30, 20)
        bookingRefLabel.Size = New Size(400, 30)

        ' Status Label
        statusLabel = New Label()
        statusLabel.Text = "Status: Loading..."
        statusLabel.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        statusLabel.Location = New Point(550, 25)
        statusLabel.Size = New Size(200, 25)

        ' Initialize detail labels
        InitializeDetailLabels()

        ' Add controls to form
        Me.Controls.Add(shadowPanel)
        Me.Controls.Add(headerPanel)
        Me.Controls.Add(detailsPanel)
        detailsPanel.Controls.Add(bookingRefLabel)
        detailsPanel.Controls.Add(statusLabel)
    End Sub

    Private Sub InitializeDetailLabels()
        Dim startY As Integer = 70
        Dim labelHeight As Integer = 35
        Dim leftColumnX As Integer = 30
        Dim rightColumnX As Integer = 450

        ' Customer Information Section
        CreateSectionHeader("Customer Information", leftColumnX, startY)
        customerNameLabel = CreateDetailLabel("Customer Name:", "", leftColumnX, startY + 30)
        numberOfPeopleLabel = CreateDetailLabel("Number of People:", "", leftColumnX, startY + 65)

        ' Booking Information Section
        CreateSectionHeader("Booking Information", rightColumnX, startY)
        bookingDateLabel = CreateDetailLabel("Booking Date:", "", rightColumnX, startY + 30)
        travelDateLabel = CreateDetailLabel("Travel Date:", "", rightColumnX, startY + 65)
        endDateLabel = CreateDetailLabel("End Date:", "", rightColumnX, startY + 100)

        ' Trip Details Section
        CreateSectionHeader("Trip Details", leftColumnX, startY + 150)
        destinationLabel = CreateDetailLabel("Destination:", "", leftColumnX, startY + 180)
        packageLabel = CreateDetailLabel("Package:", "", leftColumnX, startY + 215)
        addOnsLabel = CreateDetailLabel("Add-ons:", "", leftColumnX, startY + 250)
        ratingLabel = CreateDetailLabel("Rating:", "", leftColumnX, startY + 285)

        ' Payment Information Section
        CreateSectionHeader("Payment Information", rightColumnX, startY + 150)
        paymentMethodLabel = CreateDetailLabel("Payment Method:", "", rightColumnX, startY + 180)
        paymentStatusLabel = CreateDetailLabel("Payment Status:", "", rightColumnX, startY + 215)
        finalAmountLabel = CreateDetailLabel("Final Amount:", "", rightColumnX, startY + 250)
    End Sub

    Private Sub CreateSectionHeader(text As String, x As Integer, y As Integer)
        Dim header As New Label()
        header.Text = text
        header.Font = New Font("Segoe UI", 12.0F, FontStyle.Bold)
        header.ForeColor = Color.FromArgb(52, 73, 94)
        header.Location = New Point(x, y)
        header.Size = New Size(300, 25)
        header.BackColor = Color.FromArgb(236, 240, 241)
        header.Padding = New Padding(10, 3, 0, 0)
        detailsPanel.Controls.Add(header)
    End Sub

    Private Function CreateDetailLabel(labelText As String, valueText As String, x As Integer, y As Integer) As Label
        ' Label for field name
        Dim fieldLabel As New Label()
        fieldLabel.Text = labelText
        fieldLabel.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
        fieldLabel.ForeColor = Color.FromArgb(52, 73, 94)
        fieldLabel.Location = New Point(x + 10, y)
        fieldLabel.Size = New Size(120, 20)
        detailsPanel.Controls.Add(fieldLabel)

        ' Label for field value
        Dim valueLabel As New Label()
        valueLabel.Text = valueText
        valueLabel.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
        valueLabel.ForeColor = Color.FromArgb(44, 62, 80)
        valueLabel.Location = New Point(x + 140, y)
        valueLabel.Size = New Size(250, 20)
        detailsPanel.Controls.Add(valueLabel)

        Return valueLabel
    End Function

    Private Sub LoadBookingDetails()
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                ' Main booking query with package information
                Dim query As String = "
                    SELECT b.*, tp.Title, tp.PackageType as PackageDescription
                    FROM Bookings b
                    LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID
                    WHERE b.BookingReference = @bookingRef"

                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@bookingRef", bookingReference)

                    Using reader As MySqlDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            ' Update all labels with data
                            customerNameLabel.Text = If(IsDBNull(reader("CustomerName")), "N/A", reader("CustomerName").ToString())
                            numberOfPeopleLabel.Text = reader("NumberOfPeople").ToString()
                            bookingDateLabel.Text = Convert.ToDateTime(reader("BookingDate")).ToString("MMMM dd, yyyy")
                            travelDateLabel.Text = Convert.ToDateTime(reader("TravelDate")).ToString("MMMM dd, yyyy")
                            endDateLabel.Text = Convert.ToDateTime(reader("EndDate")).ToString("MMMM dd, yyyy")
                            destinationLabel.Text = reader("Destination").ToString()
                            packageLabel.Text = If(IsDBNull(reader("Title")), "N/A", reader("Title").ToString())
                            paymentMethodLabel.Text = reader("PaymentMethod").ToString()
                            finalAmountLabel.Text = "₱" & Convert.ToDecimal(reader("FinalAmount")).ToString("N2")

                            ' Set rating with stars
                            Dim rating As Decimal = If(IsDBNull(reader("Rating")), 0, Convert.ToDecimal(reader("Rating")))
                            ratingLabel.Text = If(rating > 0, GenerateStars(rating) & $" ({rating}/5)", "Not yet rated")

                            ' Set status with color coding
                            Dim status As String = reader("Status").ToString()
                            statusLabel.Text = "Status: " & status
                            SetStatusColor(status)

                            ' Set payment status with color
                            Dim paymentStatus As String = reader("PaymentStatus").ToString()
                            paymentStatusLabel.Text = paymentStatus
                            SetPaymentStatusColor(paymentStatus)

                            ' Get booking ID for add-ons query
                            Dim bookingId As Integer = Convert.ToInt32(reader("BookingID"))
                            reader.Close()
                            LoadAddOns(connection, bookingId)
                        Else
                            MessageBox.Show("Booking not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading booking details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub LoadAddOns(connection As MySqlConnection, bookingId As Integer)
        Try
            Dim addOnsQuery As String = "
                SELECT ba.Quantity, ba.UnitPrice, ba.TotalPrice, pa.AddOnName
                FROM BookingAddOns ba
                LEFT JOIN PackageAddOns pa ON ba.AddOnID = pa.AddOnID
                WHERE ba.BookingID = @bookingId"

            Using addOnsCommand As New MySqlCommand(addOnsQuery, connection)
                addOnsCommand.Parameters.AddWithValue("@bookingId", BookingId)

                Using addOnsReader As MySqlDataReader = addOnsCommand.ExecuteReader()
                    Dim addOnsList As New List(Of String)()

                    While addOnsReader.Read()
                        Dim addOnText = $"{addOnsReader("AddOnName")} (x{addOnsReader("Quantity")}) - ₱{Convert.ToDecimal(addOnsReader("TotalPrice")):N2}"
                        addOnsList.Add(addOnText)
                    End While

                    addOnsLabel.Text = If(addOnsList.Count > 0, String.Join(", ", addOnsList), "None")
                End Using
            End Using
        Catch ex As Exception
            addOnsLabel.Text = "Error loading add-ons"
        End Try
    End Sub

    Private Function GenerateStars(rating As Decimal) As String
        Dim stars As String = ""
        Dim fullStars As Integer = Math.Floor(rating)
        Dim hasHalfStar As Boolean = (rating - fullStars) >= 0.5

        For i As Integer = 1 To fullStars
            stars += "★"
        Next

        If hasHalfStar Then
            stars += "☆"
        End If

        For i As Integer = fullStars + If(hasHalfStar, 1, 0) To 4
            stars += "☆"
        Next

        Return stars
    End Function

    Private Sub SetStatusColor(status As String)
        Select Case status.ToLower()
            Case "confirmed"
                statusLabel.ForeColor = Color.FromArgb(39, 174, 96)
            Case "pending"
                statusLabel.ForeColor = Color.FromArgb(241, 196, 15)
            Case "cancelled"
                statusLabel.ForeColor = Color.FromArgb(231, 76, 60)
            Case "completed"
                statusLabel.ForeColor = Color.FromArgb(46, 204, 113)
            Case Else
                statusLabel.ForeColor = Color.FromArgb(52, 73, 94)
        End Select
    End Sub

    Private Sub SetPaymentStatusColor(paymentStatus As String)
        Select Case paymentStatus.ToLower()
            Case "paid"
                paymentStatusLabel.ForeColor = Color.FromArgb(39, 174, 96)
            Case "pending"
                paymentStatusLabel.ForeColor = Color.FromArgb(241, 196, 15)
            Case "refunded"
                paymentStatusLabel.ForeColor = Color.FromArgb(155, 89, 182)
            Case Else
                paymentStatusLabel.ForeColor = Color.FromArgb(52, 73, 94)
        End Select
    End Sub

    Private Sub BackButton_Click(sender As Object, e As EventArgs)
        Me.Close()
    End Sub

    ' Override form paint for shadow effect
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)

        ' Add subtle gradient background
        Dim rect As New Rectangle(0, 100, Me.Width, Me.Height - 100)
        Using brush As New LinearGradientBrush(rect, Color.FromArgb(245, 247, 250), Color.FromArgb(235, 238, 243), LinearGradientMode.Vertical)
            e.Graphics.FillRectangle(brush, rect)
        End Using
    End Sub
End Class

' Example usage - Call this form from your main application
' Dim bookingDetailsForm As New BookingDetailsForm("BK2024001")
' bookingDetailsForm.ShowDialog()