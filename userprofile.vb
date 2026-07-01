Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices
Imports MySql.Data.MySqlClient
Imports MySqlConnection = MySql.Data.MySqlClient.MySqlConnection

Public Class UserProfileForm
    Inherits Form

    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Dim conn As MySqlConnection = New MySqlConnection("Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;")
    Public sql As String
    Public dbcomm As MySql.Data.MySqlClient.MySqlCommand
    Private _currentUserId As Integer
    Private _currentUserEmail As String
    Private user As MainForm.UserInfo


    ' Form controls
    Private WithEvents lblTitle As Label
    Private WithEvents btnLogout As Button

    ' Basic Info Section
    Private WithEvents lblBasicInfo As Label
    Private WithEvents picProfile As PictureBox
    Private WithEvents lblFirstName As Label
    Private WithEvents txtFirstName As TextBox
    Private WithEvents lblLastName As Label
    Private WithEvents txtLastName As TextBox
    Private WithEvents lblGender As Label
    Private WithEvents cmbGender As ComboBox
    Private WithEvents lblBirthday As Label
    Private WithEvents dtpBirthDate As DateTimePicker

    ' Other Info Section
    Private WithEvents lblOtherInfo As Label
    Private WithEvents lblDateJoined As Label
    Private WithEvents txtJoinDate As TextBox
    Private WithEvents btnChangePassword As Button
    Private WithEvents btnUpdateProfile As Button

    ' Contact Info Section
    Private WithEvents lblContactInfo As Label
    Private WithEvents lblEmail As Label
    Private WithEvents txtEmail As TextBox
    Private WithEvents lblPhone As Label
    Private WithEvents txtPhone As TextBox
    Private WithEvents lblAddress As Label
    Private WithEvents txtAddress As TextBox
    Private WithEvents lblUsername As Label
    Private WithEvents txtUsername As TextBox

    ' Current Bookings Section
    Private WithEvents lstCurrentBookings As ListView
    Private WithEvents btnViewBooking As Button
    Private WithEvents btnCancelBooking As Button
    Private WithEvents btnRefreshBookings As Button

    ' Travel History Section
    Private WithEvents lblTravelHistory As Label
    Private WithEvents lstTravelHistory As ListView
    Private WithEvents btnReviewTrip As Button
    Private WithEvents btnExportHistory As Button
    Private WithEvents btnFilterHistory As Button
    Private WithEvents dtpHistoryFrom As DateTimePicker
    Private WithEvents dtpHistoryTo As DateTimePicker

    Public Sub New(userId As Integer)
        _currentUserId = userId
        InitializeComponent()
        LoadUserData()
        LoadBookings()
        LoadTravelHistory()
    End Sub

    Public Sub New(userEmail As String)
        _currentUserEmail = userEmail
        InitializeComponent()
        LoadUserDataByEmail()
        LoadBookings()
        LoadTravelHistory()
    End Sub

    Private Sub InitializeComponent()
        Me.Text = "My Profile"
        Me.Size = New Size(1220, 820)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.BackColor = Color.FromArgb(240, 248, 255)
        Me.Font = New Font("Segoe UI", 10)
        Me.DoubleBuffered = True

        ' Title
        lblTitle = New Label()
        lblTitle.Text = "My Profile"
        lblTitle.Font = New Font("Segoe UI", 18, FontStyle.Bold)
        lblTitle.ForeColor = Color.FromArgb(41, 98, 255)
        lblTitle.Location = New Point(20, 20)
        lblTitle.Size = New Size(400, 40)
        Me.Controls.Add(lblTitle)

        ' Logout Button
        btnLogout = New Button()
        btnLogout.Text = "LOG OUT"
        btnLogout.Location = New Point(330, 20)
        btnLogout.Size = New Size(120, 35)
        btnLogout.BackColor = Color.White
        btnLogout.ForeColor = Color.FromArgb(41, 98, 255)
        btnLogout.FlatStyle = FlatStyle.Flat
        btnLogout.FlatAppearance.BorderColor = Color.FromArgb(41, 98, 255)
        btnLogout.FlatAppearance.BorderSize = 1
        btnLogout.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        AddHandler btnLogout.Click, AddressOf btnLogout_Click
        Me.Controls.Add(btnLogout)
        btnLogout.BringToFront()


        ' Basic Info Section
        lblBasicInfo = New Label()
        lblBasicInfo.Text = "BASIC INFO"
        lblBasicInfo.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblBasicInfo.Location = New Point(20, 80)
        lblBasicInfo.Size = New Size(200, 30)
        Me.Controls.Add(lblBasicInfo)

        ' Profile Picture
        picProfile = New PictureBox()
        picProfile.Location = New Point(20, 140)
        picProfile.Size = New Size(100, 100)
        picProfile.BackColor = Color.LightGray
        picProfile.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(picProfile)

        ' First Name
        lblFirstName = New Label()
        lblFirstName.Text = "First Name:"
        lblFirstName.Location = New Point(140, 120)
        lblFirstName.Size = New Size(100, 25)
        Me.Controls.Add(lblFirstName)

        txtFirstName = New TextBox()
        txtFirstName.Location = New Point(140, 150)
        txtFirstName.Size = New Size(150, 30)
        txtFirstName.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtFirstName)

        ' Last Name
        lblLastName = New Label()
        lblLastName.Text = "Last Name:"
        lblLastName.Location = New Point(300, 120)
        lblLastName.Size = New Size(100, 25)
        Me.Controls.Add(lblLastName)

        txtLastName = New TextBox()
        txtLastName.Location = New Point(300, 150)
        txtLastName.Size = New Size(150, 30)
        txtLastName.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtLastName)

        ' Gender
        lblGender = New Label()
        lblGender.Text = "Gender:"
        lblGender.Location = New Point(140, 190)
        lblGender.Size = New Size(100, 25)
        Me.Controls.Add(lblGender)

        cmbGender = New ComboBox()
        cmbGender.Location = New Point(140, 220)
        cmbGender.Size = New Size(150, 30)
        cmbGender.DropDownStyle = ComboBoxStyle.DropDownList
        cmbGender.Items.AddRange({"Male", "Female", "Other"})
        Me.Controls.Add(cmbGender)

        ' Birthday
        lblBirthday = New Label()
        lblBirthday.Text = "Birth Date:"
        lblBirthday.Location = New Point(300, 190)
        lblBirthday.Size = New Size(100, 25)
        Me.Controls.Add(lblBirthday)

        dtpBirthDate = New DateTimePicker()
        dtpBirthDate.Location = New Point(300, 220)
        dtpBirthDate.Size = New Size(150, 30)
        dtpBirthDate.Format = DateTimePickerFormat.Short
        dtpBirthDate.Value = DateTime.Today.AddYears(-18)
        Me.Controls.Add(dtpBirthDate)

        ' Other Info Section
        lblOtherInfo = New Label()
        lblOtherInfo.Text = "OTHER INFO"
        lblOtherInfo.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblOtherInfo.Location = New Point(20, 270)
        lblOtherInfo.Size = New Size(200, 30)
        Me.Controls.Add(lblOtherInfo)

        ' Username
        lblUsername = New Label()
        lblUsername.Text = "Username:"
        lblUsername.Location = New Point(20, 310)
        lblUsername.Size = New Size(100, 25)
        Me.Controls.Add(lblUsername)

        txtUsername = New TextBox()
        txtUsername.Location = New Point(21, 340)
        txtUsername.Size = New Size(200, 30)
        txtUsername.ReadOnly = True
        txtUsername.BackColor = Color.White
        txtUsername.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtUsername)

        ' Date Joined
        lblDateJoined = New Label()
        lblDateJoined.Text = "Date Joined:"
        lblDateJoined.Location = New Point(245, 310)
        lblDateJoined.Size = New Size(100, 25)
        Me.Controls.Add(lblDateJoined)

        txtJoinDate = New TextBox()
        txtJoinDate.Location = New Point(246, 340)
        txtJoinDate.Size = New Size(200, 30)
        txtJoinDate.ReadOnly = True
        txtJoinDate.BackColor = Color.White
        txtJoinDate.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtJoinDate)

        ' Change Password
        btnChangePassword = New Button()
        btnChangePassword.Text = "Change Password"
        btnChangePassword.Location = New Point(150, 400)
        btnChangePassword.Size = New Size(150, 30)
        btnChangePassword.FlatStyle = FlatStyle.Flat
        btnChangePassword.FlatAppearance.BorderColor = Color.FromArgb(41, 98, 255)
        btnChangePassword.ForeColor = Color.FromArgb(41, 98, 255)
        AddHandler btnChangePassword.Click, AddressOf btnChangePassword_Click
        Me.Controls.Add(btnChangePassword)

        ' Contact Info Section
        lblContactInfo = New Label()
        lblContactInfo.Text = "CONTACT INFO"
        lblContactInfo.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblContactInfo.Location = New Point(20, 450)
        lblContactInfo.Size = New Size(200, 30)
        Me.Controls.Add(lblContactInfo)

        ' Email
        lblEmail = New Label()
        lblEmail.Text = "Email:"
        lblEmail.Location = New Point(20, 490)
        lblEmail.Size = New Size(100, 25)
        Me.Controls.Add(lblEmail)

        txtEmail = New TextBox()
        txtEmail.Location = New Point(21, 520)
        txtEmail.Size = New Size(200, 30)
        txtEmail.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtEmail)

        ' Phone
        lblPhone = New Label()
        lblPhone.Text = "Phone Number:"
        lblPhone.Location = New Point(245, 490)
        lblPhone.Size = New Size(120, 25)
        Me.Controls.Add(lblPhone)

        txtPhone = New TextBox()
        txtPhone.Location = New Point(246, 520)
        txtPhone.Size = New Size(200, 30)
        txtPhone.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtPhone)

        ' Address
        lblAddress = New Label()
        lblAddress.Text = "Address:"
        lblAddress.Location = New Point(20, 560)
        lblAddress.Size = New Size(100, 25)
        Me.Controls.Add(lblAddress)

        txtAddress = New TextBox()
        txtAddress.Location = New Point(20, 590)
        txtAddress.Size = New Size(200, 30)
        txtAddress.BorderStyle = BorderStyle.FixedSingle
        Me.Controls.Add(txtAddress)

        ' Update Profile Button
        btnUpdateProfile = New Button()
        btnUpdateProfile.Text = "Update Profile"
        btnUpdateProfile.Location = New Point(300, 690)
        btnUpdateProfile.Size = New Size(150, 30)
        btnUpdateProfile.BackColor = Color.FromArgb(41, 98, 255)
        btnUpdateProfile.ForeColor = Color.White
        btnUpdateProfile.FlatStyle = FlatStyle.Flat
        btnUpdateProfile.FlatAppearance.BorderSize = 0
        AddHandler btnUpdateProfile.Click, AddressOf btnUpdateProfile_Click
        Me.Controls.Add(btnUpdateProfile)

        ' ====== CURRENT BOOKINGS SECTION ======
        Dim cbbackPanel As New Panel()
        cbbackPanel.BackColor = Color.FromArgb(200, 6, 41, 55)
        cbbackPanel.Location = New Point(500, 20)
        cbbackPanel.Size = New Size(690, 370)
        Me.Controls.Add(cbbackPanel)

        Dim lblCurrentBookings As New Label()
        lblCurrentBookings.Text = "CURRENT BOOKINGS"
        lblCurrentBookings.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblCurrentBookings.ForeColor = Color.White
        lblCurrentBookings.Location = New Point(20, 10)
        lblCurrentBookings.Size = New Size(200, 30)
        lblCurrentBookings.BackColor = Color.Transparent
        cbbackPanel.Controls.Add(lblCurrentBookings)

        ' Current Bookings ListView - ADDED TO PANEL
        lstCurrentBookings = New ListView()
        lstCurrentBookings.Location = New Point(20, 50)
        lstCurrentBookings.Size = New Size(650, 250)
        lstCurrentBookings.View = View.Details
        lstCurrentBookings.FullRowSelect = True
        lstCurrentBookings.GridLines = True
        lstCurrentBookings.BackColor = Color.White
        lstCurrentBookings.Columns.Add("ID", 50)
        lstCurrentBookings.Columns.Add("Destination", 150)
        lstCurrentBookings.Columns.Add("Travel Date", 100)
        lstCurrentBookings.Columns.Add("Status", 80)
        lstCurrentBookings.Columns.Add("Amount", 100)
        lstCurrentBookings.Columns.Add("Booking Date", 100)
        lstCurrentBookings.Columns.Add("Type", 80)
        cbbackPanel.Controls.Add(lstCurrentBookings)

        ' Create View Details button
        btnViewBooking = New Button()
        btnViewBooking.Text = "View Details"
        btnViewBooking.Location = New Point(20, 310)
        btnViewBooking.Size = New Size(120, 35)
        btnViewBooking.BackColor = Color.FromArgb(41, 98, 255) ' Fixed color values
        btnViewBooking.ForeColor = Color.White
        btnViewBooking.FlatStyle = FlatStyle.Flat
        AddHandler btnViewBooking.Click, AddressOf btnViewBooking_Click ' Fixed event handler
        cbbackPanel.Controls.Add(btnViewBooking)

        btnCancelBooking = New Button()
        btnCancelBooking.Text = "Cancel Booking"
        btnCancelBooking.Location = New Point(150, 310)
        btnCancelBooking.Size = New Size(120, 35)
        btnCancelBooking.BackColor = Color.FromArgb(244, 67, 54)
        btnCancelBooking.ForeColor = Color.White
        btnCancelBooking.FlatStyle = FlatStyle.Flat
        AddHandler btnCancelBooking.Click, AddressOf btnCancelBooking_Click
        cbbackPanel.Controls.Add(btnCancelBooking)

        btnRefreshBookings = New Button()
        btnRefreshBookings.Text = "Refresh"
        btnRefreshBookings.Location = New Point(280, 310)
        btnRefreshBookings.Size = New Size(100, 35)
        btnRefreshBookings.BackColor = Color.FromArgb(76, 175, 80)
        btnRefreshBookings.ForeColor = Color.White
        btnRefreshBookings.FlatStyle = FlatStyle.Flat
        AddHandler btnRefreshBookings.Click, AddressOf btnRefreshBookings_Click
        cbbackPanel.Controls.Add(btnRefreshBookings)

        ' ====== TRAVEL HISTORY SECTION ======
        Dim thbackPanel As New Panel()
        thbackPanel.BackColor = Color.FromArgb(200, 6, 41, 55)
        thbackPanel.Location = New Point(500, 400)
        thbackPanel.Size = New Size(690, 370)
        Me.Controls.Add(thbackPanel)

        lblTravelHistory = New Label()
        lblTravelHistory.Text = "TRAVEL HISTORY"
        lblTravelHistory.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblTravelHistory.ForeColor = Color.White
        lblTravelHistory.BackColor = Color.Transparent
        lblTravelHistory.Location = New Point(20, 10)
        lblTravelHistory.Size = New Size(200, 30)
        thbackPanel.Controls.Add(lblTravelHistory)

        Dim lblFromDate As New Label()
        lblFromDate.Text = "From:"
        lblFromDate.ForeColor = Color.White
        lblFromDate.BackColor = Color.Transparent
        lblFromDate.Location = New Point(250, 13)
        lblFromDate.Size = New Size(50, 20)
        thbackPanel.Controls.Add(lblFromDate)

        dtpHistoryFrom = New DateTimePicker()
        dtpHistoryFrom.Location = New Point(300, 10)
        dtpHistoryFrom.Size = New Size(120, 25)
        dtpHistoryFrom.Format = DateTimePickerFormat.Short
        dtpHistoryFrom.Value = DateTime.Today.AddMonths(-6)
        thbackPanel.Controls.Add(dtpHistoryFrom)

        Dim lblToDate As New Label()
        lblToDate.Text = "To:"
        lblToDate.ForeColor = Color.White
        lblToDate.BackColor = Color.Transparent
        lblToDate.Location = New Point(430, 13)
        lblToDate.Size = New Size(30, 20)
        thbackPanel.Controls.Add(lblToDate)

        dtpHistoryTo = New DateTimePicker()
        dtpHistoryTo.Location = New Point(460, 10)
        dtpHistoryTo.Size = New Size(120, 25)
        dtpHistoryTo.Format = DateTimePickerFormat.Short
        dtpHistoryTo.Value = DateTime.Today
        thbackPanel.Controls.Add(dtpHistoryTo)

        btnFilterHistory = New Button()
        btnFilterHistory.Text = "Filter"
        btnFilterHistory.Location = New Point(590, 10)
        btnFilterHistory.Size = New Size(80, 30)
        btnFilterHistory.BackColor = Color.FromArgb(156, 39, 176)
        btnFilterHistory.ForeColor = Color.White
        btnFilterHistory.FlatStyle = FlatStyle.Flat
        AddHandler btnFilterHistory.Click, AddressOf btnFilterHistory_Click
        thbackPanel.Controls.Add(btnFilterHistory)

        lstTravelHistory = New ListView()
        lstTravelHistory.Location = New Point(20, 50)
        lstTravelHistory.Size = New Size(650, 250)
        lstTravelHistory.View = View.Details
        lstTravelHistory.FullRowSelect = True
        lstTravelHistory.GridLines = True
        lstTravelHistory.BackColor = Color.White
        lstTravelHistory.Columns.Add("ID", 50)
        lstTravelHistory.Columns.Add("Destination", 120)
        lstTravelHistory.Columns.Add("Date", 80)
        lstTravelHistory.Columns.Add("Duration", 70)
        lstTravelHistory.Columns.Add("Amount", 80)
        lstTravelHistory.Columns.Add("Rating", 80)
        lstTravelHistory.Columns.Add("Status", 80)
        AddHandler lstTravelHistory.SelectedIndexChanged, AddressOf lstTravelHistory_SelectedIndexChanged
        thbackPanel.Controls.Add(lstTravelHistory)

        btnReviewTrip = New Button()
        btnReviewTrip.Text = "Review Trip"
        btnReviewTrip.Location = New Point(20, 310)
        btnReviewTrip.Size = New Size(120, 35)
        btnReviewTrip.BackColor = Color.FromArgb(255, 193, 7)
        btnReviewTrip.ForeColor = Color.White
        btnReviewTrip.FlatStyle = FlatStyle.Flat
        btnReviewTrip.Enabled = False
        AddHandler btnReviewTrip.Click, AddressOf btnReviewTrip_Click
        thbackPanel.Controls.Add(btnReviewTrip)

        btnExportHistory = New Button()
        btnExportHistory.Text = "Export Receipt"
        btnExportHistory.Location = New Point(150, 310)
        btnExportHistory.Size = New Size(120, 35)
        btnExportHistory.BackColor = Color.FromArgb(96, 125, 139)
        btnExportHistory.ForeColor = Color.White
        btnExportHistory.FlatStyle = FlatStyle.Flat
        AddHandler btnExportHistory.Click, AddressOf btnExportHistory_Click
        thbackPanel.Controls.Add(btnExportHistory)
    End Sub

    ' Fixed BookingExistsAndBelongsToUser method - now checks for any existing booking for the user
    Private Function BookingExistsAndBelongsToUser(bookingId As String, userId As Integer) As Boolean
        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                Dim query As String = "SELECT COUNT(*) FROM Bookings WHERE BookingID = @bookingId AND UserID = @userId"
                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@bookingId", Convert.ToInt32(bookingId))
                    command.Parameters.AddWithValue("@userId", userId)

                    Dim count As Integer = Convert.ToInt32(command.ExecuteScalar())
                    Return count > 0
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error verifying booking: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        End Try
    End Function

    Private Function GetBookingAddOnsFromDatabase(bookingId As String) As List(Of Dictionary(Of String, String))
        Dim addOnsList As New List(Of Dictionary(Of String, String))

        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                Dim query As String = "
                SELECT 
                    bao.Quantity,
                    bao.UnitPrice,
                    (bao.Quantity * bao.UnitPrice) as TotalPrice,
                    pao.AddOnName,
                    pao.Unit
                FROM BookingAddOns bao
                INNER JOIN PackageAddOns pao ON bao.AddOnID = pao.AddOnID
                WHERE bao.BookingID = @bookingId
                ORDER BY pao.AddOnName"

                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@bookingId", Convert.ToInt32(bookingId))

                    Using reader As MySqlDataReader = command.ExecuteReader()
                        While reader.Read()
                            Dim addOnData As New Dictionary(Of String, String)
                            addOnData("Quantity") = If(IsDBNull(reader("Quantity")), "0", reader("Quantity").ToString())
                            addOnData("UnitPrice") = If(IsDBNull(reader("UnitPrice")), "0.00", reader("UnitPrice").ToString())
                            addOnData("TotalPrice") = If(IsDBNull(reader("TotalPrice")), "0.00", reader("TotalPrice").ToString())
                            addOnData("AddOnName") = If(IsDBNull(reader("AddOnName")), "Unknown Add-On", reader("AddOnName").ToString())
                            addOnData("Unit") = If(IsDBNull(reader("Unit")), "", reader("Unit").ToString())

                            addOnsList.Add(addOnData)
                        End While
                    End Using
                End Using
            End Using

            Return addOnsList

        Catch ex As Exception
            MessageBox.Show($"Error fetching add-ons: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return addOnsList
        End Try
    End Function
    Private Function GetBookingDataFromDatabase(bookingId As String) As Dictionary(Of String, String)
        Dim bookingData As New Dictionary(Of String, String)

        Try
            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                Dim query As String = "SELECT " &
                "b.BookingID, b.BookingReference, b.NumberOfPeople, b.Status, " &
                "u.FirstName, u.LastName, u.Email, u.Phone, " &
                "tp.PackageID, tp.Title AS PackageTitle, tp.Price AS PackagePrice, " &
                "pl.Location AS PackageLocation, tp.PackageType, " &
                "bp.PaymentMethod, bp.PaymentStatus, " &
                "DATE_FORMAT(b.BookingDate, '%Y-%m-%d') AS BookingDate, " &
                "DATE_FORMAT(b.TravelDate, '%Y-%m-%d') AS TravelStartDate, " &
                "DATE_FORMAT(b.EndDate, '%Y-%m-%d') AS TravelEndDate, " &
                "ba.BaseAmount, ba.AddOnTotal, ba.SubTotal, ba.DiscountAmount, ba.FinalAmount " &
                "FROM Bookings b " &
                "JOIN Users u ON b.UserID = u.UserID " &
                "JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
                "JOIN PackageLocations pl ON tp.PackageID = pl.PackageID " &
                "LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID " &
                "LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
                "WHERE b.BookingID = @BookingID"

                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@BookingId", Convert.ToInt32(bookingId))

                    Using reader As MySqlDataReader = command.ExecuteReader()
                        If reader.Read() Then
                            ' User Information
                            bookingData("FirstName") = If(IsDBNull(reader("FirstName")), "", reader("FirstName").ToString())
                            bookingData("LastName") = If(IsDBNull(reader("LastName")), "", reader("LastName").ToString())
                            bookingData("Email") = If(IsDBNull(reader("Email")), "", reader("Email").ToString())
                            bookingData("Phone") = If(IsDBNull(reader("Phone")), "", reader("Phone").ToString())

                            ' Booking Information
                            bookingData("BookingID") = If(IsDBNull(reader("BookingID")), "", reader("BookingID").ToString())
                            bookingData("BookingReference") = If(IsDBNull(reader("BookingReference")), "", reader("BookingReference").ToString())
                            bookingData("BookingDate") = If(IsDBNull(reader("BookingDate")), "", reader("BookingDate").ToString())
                            bookingData("TravelStartDate") = If(IsDBNull(reader("TravelStartDate")), "", reader("TravelStartDate").ToString())
                            bookingData("TravelEndDate") = If(IsDBNull(reader("TravelEndDate")), "", reader("TravelEndDate").ToString())
                            bookingData("NumPeople") = If(IsDBNull(reader("NumberOfPeople")), "", reader("NumberOfPeople").ToString())
                            bookingData("Status") = If(IsDBNull(reader("Status")), "", reader("Status").ToString())

                            ' Payment Information
                            bookingData("PaymentMethod") = If(IsDBNull(reader("PaymentMethod")), "", reader("PaymentMethod").ToString())
                            bookingData("PaymentStatus") = If(IsDBNull(reader("PaymentStatus")), "", reader("PaymentStatus").ToString())
                            bookingData("PaymentAmount") = If(IsDBNull(reader("FinalAmount")), "0.00", reader("FinalAmount").ToString())

                            ' Package Information
                            bookingData("PackageID") = If(IsDBNull(reader("PackageID")), "", reader("PackageID").ToString())
                            bookingData("PackageTitle") = If(IsDBNull(reader("PackageTitle")), "", reader("PackageTitle").ToString())
                            bookingData("PackageLocation") = If(IsDBNull(reader("PackageLocation")), "", reader("PackageLocation").ToString())
                            bookingData("PackageType") = If(IsDBNull(reader("PackageType")), "", reader("PackageType").ToString())
                            bookingData("PackagePrice") = If(IsDBNull(reader("PackagePrice")), "0.00", reader("PackagePrice").ToString())

                            ' Calculated amounts
                            bookingData("BaseAmount") = If(IsDBNull(reader("BaseAmount")), "0.00", reader("BaseAmount").ToString())
                            bookingData("AddOnTotal") = If(IsDBNull(reader("AddOnTotal")), "0.00", reader("AddOnTotal").ToString())
                            bookingData("SubTotal") = If(IsDBNull(reader("SubTotal")), "0.00", reader("SubTotal").ToString())
                            bookingData("DiscountAmount") = If(IsDBNull(reader("DiscountAmount")), "0.00", reader("DiscountAmount").ToString())
                            bookingData("FinalAmount") = If(IsDBNull(reader("FinalAmount")), "0.00", reader("FinalAmount").ToString())

                            Return bookingData
                        Else
                            Return Nothing
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Database error: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return Nothing
        End Try
    End Function

    ' Helper functions for safe data retrieval
    Private Function SafeGetString(reader As MySqlDataReader, columnName As String) As String
        Try
            If Not reader.IsDBNull(reader.GetOrdinal(columnName)) Then
                Return reader(columnName).ToString()
            End If
        Catch ex As Exception
            ' Column doesn't exist or other error
        End Try
        Return String.Empty
    End Function

    Private Function SafeGetDecimal(reader As MySqlDataReader, columnName As String) As Decimal
        Try
            If Not reader.IsDBNull(reader.GetOrdinal(columnName)) Then
                Return Convert.ToDecimal(reader(columnName))
            End If
        Catch ex As Exception
            ' Column doesn't exist or other error
        End Try
        Return 0D
    End Function

    Private Function SafeGetInteger(reader As MySqlDataReader, columnName As String) As Integer
        Try
            Dim ordinal As Integer = reader.GetOrdinal(columnName)
            If Not reader.IsDBNull(ordinal) Then
                Return Convert.ToInt32(reader(ordinal))
            End If
        Catch ex As Exception
            ' Column doesn't exist or other error
        End Try
        Return 0
    End Function

    Private Sub LoadUserData()
        Try
            ' Debug output to verify the user ID
            Debug.WriteLine($"Attempting to load data for UserID: {_currentUserId}")

            Using conn As New MySqlConnection(connectionString)
                ' Verify connection state
                If conn.State <> ConnectionState.Open Then
                    conn.Open()
                End If

                ' Enhanced query to get complete user data including statistics
                Dim query As String = "SELECT u.*, " &
                            "DATE_FORMAT(u.CreatedAt, '%Y-%m-%d') AS RegistrationDate, " &
                            "(SELECT COUNT(*) FROM Bookings WHERE UserID = u.UserID) AS TotalBookings, " &
                            "(SELECT COUNT(*) FROM Bookings WHERE UserID = u.UserID AND Status = 'Completed') AS CompletedTrips, " &
                            "(SELECT COUNT(*) FROM Bookings WHERE UserID = u.UserID AND Status = 'Pending') AS PendingBookings, " &
                            "(SELECT COUNT(*) FROM Bookings WHERE UserID = u.UserID AND Status = 'Cancelled') AS CancelledBookings " &
                            "FROM Users u " &
                            "WHERE u.UserID = @UserID"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@UserID", _currentUserId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        If reader.HasRows Then
                            reader.Read()

                            ' Basic Info
                            txtFirstName.Text = SafeGetString(reader, "FirstName")
                            txtLastName.Text = SafeGetString(reader, "LastName")
                            txtEmail.Text = SafeGetString(reader, "Email")
                            txtPhone.Text = SafeGetString(reader, "Phone")
                            txtAddress.Text = SafeGetString(reader, "Address")
                            txtUsername.Text = SafeGetString(reader, "Username")

                            ' Gender - handle enum values
                            Dim gender As String = SafeGetString(reader, "Gender")
                            If Not String.IsNullOrEmpty(gender) Then
                                If cmbGender.Items.Contains(gender) Then
                                    cmbGender.SelectedItem = gender
                                Else
                                    cmbGender.SelectedIndex = -1
                                End If
                            End If

                            ' Birth Date - handle NULL values
                            If Not reader.IsDBNull(reader.GetOrdinal("BirthDate")) Then
                                dtpBirthDate.Value = reader.GetDateTime("BirthDate")
                            Else
                                ' Default to 18 years ago if no birthdate set
                                dtpBirthDate.Value = DateTime.Today.AddYears(-18)
                            End If

                            ' Registration Date
                            txtJoinDate.Text = SafeGetString(reader, "RegistrationDate")

                            ' Add statistics to the form (you'll need to add these controls)
                            ' lblTotalBookings.Text = $"Total Bookings: {SafeGetInteger(reader, "TotalBookings")}"
                            ' lblCompletedTrips.Text = $"Completed Trips: {SafeGetInteger(reader, "CompletedTrips")}"
                            ' lblPendingBookings.Text = $"Pending Bookings: {SafeGetInteger(reader, "PendingBookings")}"
                            ' lblCancelledBookings.Text = $"Cancelled Bookings: {SafeGetInteger(reader, "CancelledBookings")}"

                            Debug.WriteLine("Successfully loaded complete user data")
                        Else
                            MessageBox.Show($"No user found with ID: {_currentUserId}",
                                   "Data Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading user data: {ex.Message}" & vbCrLf & vbCrLf &
                   $"Stack Trace: {ex.StackTrace}",
                   "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub LoadUserDataByEmail()
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT u.*, DATE(u.CreatedAt) as RegistrationDate " &
              "FROM Users u " &
              "WHERE u.Email = @Email"

            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@Email", _currentUserEmail)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.HasRows Then
                        reader.Read()

                        ' Store the UserID for later use
                        _currentUserId = SafeGetInteger(reader, "UserID")

                        ' Populate form fields (same as LoadUserData)
                        txtFirstName.Text = SafeGetString(reader, "FirstName")
                        txtLastName.Text = SafeGetString(reader, "LastName")
                        txtEmail.Text = SafeGetString(reader, "Email")
                        txtPhone.Text = SafeGetString(reader, "Phone")
                        txtAddress.Text = SafeGetString(reader, "Address")
                        txtUsername.Text = SafeGetString(reader, "Username")

                        Dim gender As String = SafeGetString(reader, "Gender")
                        If cmbGender.Items.Contains(gender) Then
                            cmbGender.SelectedItem = gender
                        End If

                        ' Set birth date - modified to be more defensive
                        Try
                            If Not reader.IsDBNull(reader.GetOrdinal("BirthDate")) Then
                                dtpBirthDate.Value = reader.GetDateTime("BirthDate")
                            Else
                                dtpBirthDate.Value = DateTime.Today.AddYears(-18) ' Default value
                            End If
                        Catch ex As Exception
                            dtpBirthDate.Value = DateTime.Today.AddYears(-18) ' Default value if error
                        End Try

                        txtJoinDate.Text = SafeGetString(reader, "RegistrationDate")
                    Else
                        MessageBox.Show("User data not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading user data: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Sub

    Private Sub LoadBookings()
        Try
            lstCurrentBookings.Items.Clear()

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim query As String = "SELECT b.BookingID, tp.Title as Destination, " &
                                "DATE_FORMAT(b.TravelDate, '%Y-%m-%d') as TravelDate, " &
                                "b.Status, IFNULL(ba.FinalAmount, 0) as FinalAmount, " &
                                "DATE_FORMAT(b.BookingDate, '%Y-%m-%d') as BookingDate, " &
                                "tp.PackageType, IFNULL(bp.PaymentStatus, 'Pending') as PaymentStatus " &
                                "FROM Bookings b " &
                                "INNER JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
                                "LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
                                "LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID " &
                                "WHERE b.UserID = @UserID AND b.Status IN ('Confirmed', 'Pending', 'Paid') " &
                                "ORDER BY b.TravelDate DESC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@UserID", _currentUserId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim item As New ListViewItem(reader("BookingID").ToString())
                            item.SubItems.Add(reader("Destination").ToString())
                            item.SubItems.Add(reader("TravelDate").ToString())

                            ' Combine status with payment status
                            Dim status As String = reader("Status").ToString()
                            Dim paymentStatus As String = reader("PaymentStatus").ToString()
                            item.SubItems.Add($"{status} ({paymentStatus})")

                            item.SubItems.Add("₱" & Convert.ToDecimal(reader("FinalAmount")).ToString("N2"))
                            item.SubItems.Add(reader("BookingDate").ToString())
                            item.SubItems.Add(reader("PackageType").ToString())

                            lstCurrentBookings.Items.Add(item)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading bookings: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    ' Event Handlers
    Private Sub btnUpdateProfile_Click(sender As Object, e As EventArgs)
        Try
            If ValidateProfileData() Then
                If conn.State = ConnectionState.Closed Then
                    conn.Open()
                End If

                Dim query As String = "UPDATE Users SET " &
                      "FirstName = @FirstName, " &
                      "LastName = @LastName, " &
                      "Email = @Email, " &
                      "Phone = @Phone, " &
                      "Address = @Address, " &
                      "Gender = @Gender, " &
                      "BirthDate = @BirthDate " &
                      "WHERE UserID = @UserID"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@FirstName", txtFirstName.Text.Trim())
                    cmd.Parameters.AddWithValue("@LastName", txtLastName.Text.Trim())
                    cmd.Parameters.AddWithValue("@Email", txtEmail.Text.Trim())
                    cmd.Parameters.AddWithValue("@Phone", txtPhone.Text.Trim())
                    cmd.Parameters.AddWithValue("@Address", txtAddress.Text.Trim())
                    cmd.Parameters.AddWithValue("@Gender", cmbGender.SelectedItem?.ToString())
                    cmd.Parameters.AddWithValue("@BirthDate", dtpBirthDate.Value.Date)
                    cmd.Parameters.AddWithValue("@UserID", _currentUserId)

                    Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                    If rowsAffected > 0 Then
                        MessageBox.Show("Profile updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        _currentUserEmail = txtEmail.Text.Trim() ' Update stored email
                    Else
                        MessageBox.Show("No changes were made to your profile.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    End If
                End Using
            End If
        Catch ex As Exception
            MessageBox.Show($"Error updating profile: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Sub

    Private Function ValidateProfileData() As Boolean
        If String.IsNullOrWhiteSpace(txtFirstName.Text) Then
            MessageBox.Show("First name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtFirstName.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtLastName.Text) Then
            MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtLastName.Focus()
            Return False
        End If

        If String.IsNullOrWhiteSpace(txtEmail.Text) OrElse Not IsValidEmail(txtEmail.Text) Then
            MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtEmail.Focus()
            Return False
        End If

        If cmbGender.SelectedIndex = -1 Then
            MessageBox.Show("Please select a gender.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbGender.Focus()
            Return False
        End If

        Return True
    End Function

    Private Function IsValidEmail(email As String) As Boolean
        Try
            Dim addr As New System.Net.Mail.MailAddress(email)
            Return addr.Address = email
        Catch
            Return False
        End Try
    End Function

    Private Sub ViewBookingDetails(bookingId As Integer)
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT " &
        "b.BookingID, b.BookingReference, " &
        "u.FirstName, u.LastName, u.Email, u.Phone, " &
        "b.BookingDate, b.TravelDate, b.EndDate, b.NumberOfPeople, b.Status, " &
        "bp.PaymentMethod, bp.PaymentStatus, bp.PaymentDate, bp.TransactionReference, " &
        "tp.PackageID, tp.Title AS PackageTitle, tp.Price AS PackagePrice, " &
        "pl.Location AS PackageLocation, tp.PackageType, " &
        "ba.BaseAmount, ba.AddOnTotal, ba.SubTotal, ba.DiscountAmount, ba.FinalAmount, " &
        "pt.PromoTypeName, pt.DiscountPercentage, " &
        "GROUP_CONCAT(DISTINCT CONCAT(pa.AddOnName, ' (', bao.Quantity, 'x ₱', bao.UnitPrice, ')') SEPARATOR '; ') as AddOns " &
        "FROM Bookings b " &
        "JOIN Users u ON b.UserID = u.UserID " &
        "JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
        "JOIN PackageLocations pl ON tp.PackageID = pl.PackageID " &
        "LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID " &
        "LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
        "LEFT JOIN PromoDiscounts pd ON b.PromoID = pd.PromoID " &
        "LEFT JOIN PromoTypes pt ON pd.PromoTypeID = pt.PromoTypeID " &
        "LEFT JOIN BookingAddOns bao ON b.BookingID = bao.BookingID " &
        "LEFT JOIN PackageAddOns pa ON bao.AddOnID = pa.AddOnID " &
        "WHERE b.BookingID = @BookingID " &
        "GROUP BY b.BookingID"

            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@BookingID", bookingId)

                Using reader As MySqlDataReader = cmd.ExecuteReader()
                    If reader.Read() Then
                        ShowBookingDetailsForm(reader)
                    Else
                        MessageBox.Show("Booking not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End If
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error retrieving booking details: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Sub
    Private Sub ShowBookingDetailsForm(reader As MySql.Data.MySqlClient.MySqlDataReader)
        ' Create main form
        Dim detailsForm As New Form()
        detailsForm.Text = $"Booking Details - #{reader("BookingID")}"
        detailsForm.Size = New Size(700, 800)
        detailsForm.StartPosition = FormStartPosition.CenterParent
        detailsForm.FormBorderStyle = FormBorderStyle.FixedDialog
        detailsForm.MaximizeBox = False
        detailsForm.BackColor = Color.FromArgb(240, 248, 255)
        detailsForm.Font = New Font("Segoe UI", 9)

        ' Header Panel
        Dim headerPanel As New Panel()
        headerPanel.Location = New Point(0, 0)
        headerPanel.Size = New Size(700, 80)
        headerPanel.BackColor = Color.FromArgb(70, 130, 180)
        detailsForm.Controls.Add(headerPanel)

        ' Header Title
        Dim headerTitle As New Label()
        headerTitle.Text = "BOOKING DETAILS"
        headerTitle.Location = New Point(20, 15)
        headerTitle.Size = New Size(300, 30)
        headerTitle.Font = New Font("Segoe UI", 16, FontStyle.Bold)
        headerTitle.ForeColor = Color.White
        headerPanel.Controls.Add(headerTitle)

        ' Booking Reference
        Dim bookingRef As New Label()
        bookingRef.Text = $"Booking #: {If(IsDBNull(reader("BookingReference")), "N/A", reader("BookingReference").ToString())}"
        bookingRef.Location = New Point(20, 45)
        bookingRef.Size = New Size(200, 20)
        bookingRef.Font = New Font("Segoe UI", 10)
        bookingRef.ForeColor = Color.White
        headerPanel.Controls.Add(bookingRef)

        ' Status Badge
        Dim statusBadge As New Label()
        Dim status As String = If(IsDBNull(reader("Status")), "Unknown", reader("Status").ToString())
        statusBadge.Text = status.ToUpper()
        statusBadge.Location = New Point(550, 25)
        statusBadge.Size = New Size(120, 30)
        statusBadge.TextAlign = ContentAlignment.MiddleCenter
        statusBadge.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        statusBadge.ForeColor = Color.White

        ' Set status color
        Select Case status.ToLower()
            Case "confirmed"
                statusBadge.BackColor = Color.Green
            Case "pending"
                statusBadge.BackColor = Color.Orange
            Case "cancelled"
                statusBadge.BackColor = Color.Red
            Case "completed"
                statusBadge.BackColor = Color.Blue
            Case Else
                statusBadge.BackColor = Color.Gray
        End Select

        headerPanel.Controls.Add(statusBadge)

        ' Create scrollable panel for content
        Dim scrollPanel As New Panel()
        scrollPanel.Location = New Point(13, 100)
        scrollPanel.Size = New Size(674, 580)
        scrollPanel.BackColor = Color.White
        scrollPanel.BorderStyle = BorderStyle.FixedSingle
        scrollPanel.AutoScroll = True
        detailsForm.Controls.Add(scrollPanel)

        ' Main Content Panel (inside scroll panel)
        Dim contentPanel As New Panel()
        contentPanel.Location = New Point(0, 0)
        contentPanel.Size = New Size(640, 1200) ' Increased height to accommodate all content
        contentPanel.BackColor = Color.White
        scrollPanel.Controls.Add(contentPanel)

        Dim yPos As Integer = 20

        ' Customer Information Section (Combined and cleaned up)
        ' Section Header
        CreateSectionHeader(contentPanel, "CUSTOMER INFORMATION", yPos)
        yPos += 40

        ' --- Customer Name ---
        Dim firstName As String = If(IsDBNull(reader("FirstName")), "", reader("FirstName").ToString())
        Dim lastName As String = If(IsDBNull(reader("LastName")), "", reader("LastName").ToString())
        Dim customerName As String = $"{firstName} {lastName}".Trim()
        If String.IsNullOrEmpty(customerName) Then customerName = "N/A"

        CreateDetailRow(contentPanel, "Customer Name:", customerName, yPos)
        yPos += 30

        ' --- Email ---
        Dim email As String = "N/A"
        If reader.GetSchemaTable().Rows.Cast(Of DataRow)().Any(Function(r) r("ColumnName").ToString() = "Email") Then
            email = If(IsDBNull(reader("Email")), "N/A", reader("Email").ToString())
        End If

        CreateDetailRow(contentPanel, "Email:", email, yPos)
        yPos += 30

        ' --- Phone ---
        Dim phone As String = "N/A"
        If reader.GetSchemaTable().Rows.Cast(Of DataRow)().Any(Function(r) r("ColumnName").ToString() = "Phone") Then
            phone = If(IsDBNull(reader("Phone")), "N/A", reader("Phone").ToString())
        End If

        CreateDetailRow(contentPanel, "Phone:", phone, yPos)
        yPos += 40


        ' Booking Information Section
        CreateSectionHeader(contentPanel, "BOOKING INFORMATION", yPos)
        yPos += 40

        CreateDetailRow(contentPanel, "Booking Reference:", If(IsDBNull(reader("BookingReference")), "N/A", reader("BookingReference").ToString()), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "Booking Date:", If(IsDBNull(reader("BookingDate")), "N/A", Convert.ToDateTime(reader("BookingDate")).ToString("MMM dd, yyyy")), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "Travel Date:", If(IsDBNull(reader("TravelDate")), "N/A", Convert.ToDateTime(reader("TravelDate")).ToString("MMM dd, yyyy")), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "End Date:", If(IsDBNull(reader("EndDate")), "N/A", Convert.ToDateTime(reader("EndDate")).ToString("MMM dd, yyyy")), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "Number of People:", If(IsDBNull(reader("NumberOfPeople")), "N/A", reader("NumberOfPeople").ToString()), yPos)
        yPos += 30

        ' Calculate and show trip duration
        If Not IsDBNull(reader("TravelDate")) AndAlso Not IsDBNull(reader("EndDate")) Then
            Try
                Dim startDate As DateTime = Convert.ToDateTime(reader("TravelDate"))
                Dim endDate As DateTime = Convert.ToDateTime(reader("EndDate"))
                Dim duration As Integer = (endDate - startDate).Days + 1
                CreateDetailRow(contentPanel, "Duration:", $"{duration} day{If(duration > 1, "s", "")}", yPos)
            Catch ex As Exception
                CreateDetailRow(contentPanel, "Duration:", "N/A", yPos)
            End Try
        Else
            CreateDetailRow(contentPanel, "Duration:", "N/A", yPos)
        End If
        yPos += 30

        CreateDetailRow(contentPanel, "Status:", status, yPos)
        yPos += 40

        ' Package Information Section
        CreateSectionHeader(contentPanel, "PACKAGE INFORMATION", yPos)
        yPos += 40

        CreateDetailRow(contentPanel, "Package Type:", If(IsDBNull(reader("PackageType")), "N/A", reader("PackageType").ToString()), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "Package Title:", If(IsDBNull(reader("PackageTitle")), "N/A", reader("PackageTitle").ToString()), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "Package Location:", If(IsDBNull(reader("PackageLocation")), "N/A", reader("PackageLocation").ToString()), yPos)
        yPos += 30

        ' Add-ons with improved formatting
        Dim addOnsText As String = If(IsDBNull(reader("AddOns")), "None", reader("AddOns").ToString())
        If String.IsNullOrEmpty(addOnsText) Then addOnsText = "None"
        CreateDetailRow(contentPanel, "Add-ons:", addOnsText, yPos)
        yPos += 40

        ' Payment Information Section
        CreateSectionHeader(contentPanel, "PAYMENT INFORMATION", yPos)
        yPos += 40

        CreateDetailRow(contentPanel, "Payment Method:", If(IsDBNull(reader("PaymentMethod")), "Pay on Trip", reader("PaymentMethod").ToString()), yPos)
        yPos += 30

        CreateDetailRow(contentPanel, "Payment Status:", If(IsDBNull(reader("PaymentStatus")), "Pending", reader("PaymentStatus").ToString()), yPos)
        yPos += 30

        ' Payment Date if available
        If Not IsDBNull(reader("PaymentDate")) Then
            CreateDetailRow(contentPanel, "Payment Date:", Convert.ToDateTime(reader("PaymentDate")).ToString("MMM dd, yyyy hh:mm tt"), yPos)
            yPos += 30
        End If

        ' Transaction Reference if available
        If Not IsDBNull(reader("TransactionReference")) AndAlso Not String.IsNullOrEmpty(reader("TransactionReference").ToString()) Then
            CreateDetailRow(contentPanel, "Transaction Ref:", reader("TransactionReference").ToString(), yPos)
            yPos += 30
        End If

        ' Final Amount with highlighting
        Dim amountLabel As New Label()
        amountLabel.Text = "Final Amount:"
        amountLabel.Location = New Point(20, yPos)
        amountLabel.Size = New Size(150, 25)
        amountLabel.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        amountLabel.ForeColor = Color.FromArgb(70, 130, 180)
        contentPanel.Controls.Add(amountLabel)

        Dim amountValue As New Label()
        amountValue.Text = $"₱{If(IsDBNull(reader("FinalAmount")), "0.00", Convert.ToDecimal(reader("FinalAmount")).ToString("N2"))}"
        amountValue.Location = New Point(180, yPos)
        amountValue.Size = New Size(200, 25)
        amountValue.Font = New Font("Segoe UI", 14, FontStyle.Bold)
        amountValue.ForeColor = Color.Green
        contentPanel.Controls.Add(amountValue)
        yPos += 50

        ' Update content panel height
        contentPanel.Height = yPos + 50

        ' Close Button
        Dim closeBtn As New Button()
        closeBtn.Text = "Close"
        closeBtn.Location = New Point(580, 720)
        closeBtn.Size = New Size(100, 35)
        closeBtn.BackColor = Color.FromArgb(70, 130, 180)
        closeBtn.ForeColor = Color.White
        closeBtn.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        closeBtn.FlatStyle = FlatStyle.Flat
        closeBtn.FlatAppearance.BorderSize = 0
        AddHandler closeBtn.Click, Sub() detailsForm.Close()
        detailsForm.Controls.Add(closeBtn)

        detailsForm.ShowDialog()
    End Sub

    ' Heper method to check if reader has a specific column
    Private Sub CreateSectionHeader(parent As Panel, title As String, yPos As Integer)
        Dim headerLabel As New Label()
        headerLabel.Text = title
        headerLabel.Location = New Point(20, yPos)
        headerLabel.Size = New Size(600, 25)
        headerLabel.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        headerLabel.ForeColor = Color.FromArgb(70, 130, 180)
        headerLabel.BorderStyle = BorderStyle.None
        parent.Controls.Add(headerLabel)

        ' Add separator line
        Dim separator As New Panel()
        separator.Location = New Point(20, yPos + 25)
        separator.Size = New Size(600, 2)
        separator.BackColor = Color.FromArgb(200, 200, 200)
        parent.Controls.Add(separator)
    End Sub

    Private Sub CreateDetailRow(parent As Panel, label As String, value As String, yPos As Integer)
        Dim lblLabel As New Label()
        lblLabel.Text = label
        lblLabel.Location = New Point(40, yPos)
        lblLabel.Size = New Size(150, 20)
        lblLabel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        lblLabel.ForeColor = Color.FromArgb(60, 60, 60)
        parent.Controls.Add(lblLabel)

        Dim lblValue As New Label()
        lblValue.Text = value
        lblValue.Location = New Point(200, yPos)
        lblValue.Size = New Size(400, 20)
        lblValue.Font = New Font("Segoe UI", 10)
        lblValue.ForeColor = Color.Black
        parent.Controls.Add(lblValue)
    End Sub

    Private Sub btnCancelBooking_Click(sender As Object, e As EventArgs)
        If lstCurrentBookings.SelectedItems.Count > 0 Then
            Dim bookingId As String = lstCurrentBookings.SelectedItems(0).Text
            Dim bookingStatus As String = lstCurrentBookings.SelectedItems(0).SubItems(3).Text

            If bookingStatus = "Cancelled" Then
                MessageBox.Show("This booking is already cancelled.", "Cannot Cancel", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim result As DialogResult = MessageBox.Show(
                $"Are you sure you want to cancel booking #{bookingId}?",
                "Confirm Cancellation",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question)

            If result = DialogResult.Yes Then
                CancelBooking(bookingId)
            End If
        Else
            MessageBox.Show("Please select a booking to cancel.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub CancelBooking(bookingId As String)
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "UPDATE Bookings SET Status = 'Cancelled', " &
          "Notes = CONCAT(IFNULL(Notes, ''), ' - Cancelled by user on ', NOW()) " &
          "WHERE BookingID = @BookingID AND UserID = @UserID"

            Using cmd As New MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@BookingID", Convert.ToInt32(bookingId))
                cmd.Parameters.AddWithValue("@UserID", _currentUserId)

                Dim rowsAffected As Integer = cmd.ExecuteNonQuery()

                If rowsAffected > 0 Then
                    ' Update payment status
                    Dim updatePaymentQuery As String = "UPDATE BookingPayments SET PaymentStatus = 'Cancelled' " &
                  "WHERE BookingID = @BookingID"

                    Using paymentCmd As New MySqlCommand(updatePaymentQuery, conn)
                        paymentCmd.Parameters.AddWithValue("@BookingID", Convert.ToInt32(bookingId))
                        paymentCmd.ExecuteNonQuery()
                    End Using

                    MessageBox.Show("Booking cancelled successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    LoadBookings()
                    LoadTravelHistory()
                Else
                    MessageBox.Show("Failed to cancel booking.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error cancelling booking: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Sub
    Private Sub btnRefreshBookings_Click(sender As Object, e As EventArgs)
        LoadBookings()
        MessageBox.Show("Bookings refreshed successfully.", "Refresh Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Private Sub btnFilterHistory_Click(sender As Object, e As EventArgs) Handles btnFilterHistory.Click
        If dtpHistoryFrom.Value > dtpHistoryTo.Value Then
            MessageBox.Show("'From' date cannot be later than 'To' date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If

        FilterTravelHistory(dtpHistoryFrom.Value.Date, dtpHistoryTo.Value.Date)
    End Sub

    Private Sub FilterTravelHistory(fromDate As Date, toDate As Date)
        Try
            If fromDate > toDate Then
                MessageBox.Show("From date cannot be later than To date.", "Invalid Date Range", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            UpdateCompletedTrips()

            lstTravelHistory.Items.Clear()
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT bh.BookingID, " &
    "bh.ActionDescription as Destination, " &
    "DATE(bh.ActionDate) as TravelDate, " &
    "'N/A' as EndDate, " &
    "'N/A' as Duration, " &
    "0 as FinalAmount, " &
    "'N/A' as Rating, " &
    "bh.NewStatus as Status, " &
    "'N/A' as PaymentStatus " &
    "FROM BookingHistory bh " &
    "WHERE bh.UserID = @UserID " &
    "AND DATE(bh.ActionDate) BETWEEN @FromDate AND @ToDate " &
    "AND bh.ActionType IN ('Cancelled', 'Completed') " &
    "UNION ALL " &
    "SELECT b.BookingID, " &
    "tp.Title as Destination, " &
    "DATE(b.TravelDate) as TravelDate, " &
    "DATE(b.EndDate) as EndDate, " &
    "DATEDIFF(b.EndDate, b.TravelDate) + 1 as Duration, " &
    "CASE WHEN b.Status = 'Cancelled' THEN 0 ELSE ba.FinalAmount END as FinalAmount, " &
    "COALESCE(r.Rating, 0) as Rating, " &
    "b.Status, " &
    "bp.PaymentStatus " &
    "FROM Bookings b " &
    "LEFT JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
    "LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
    "LEFT JOIN BookingPayments bp ON b.BookingID = bp.BookingID " &
    "LEFT JOIN Reviews r ON b.BookingID = r.BookingID " &
    "WHERE b.UserID = @UserID " &
    "AND DATE(b.TravelDate) BETWEEN @FromDate AND @ToDate " &
    "AND b.Status IN ('Completed', 'Cancelled') " &
    "ORDER BY TravelDate DESC"

            Using dbcomm As New MySql.Data.MySqlClient.MySqlCommand(query, conn)
                dbcomm.Parameters.AddWithValue("@UserID", _currentUserId)
                dbcomm.Parameters.AddWithValue("@FromDate", fromDate.ToString("yyyy-MM-dd"))
                dbcomm.Parameters.AddWithValue("@ToDate", toDate.ToString("yyyy-MM-dd"))

                Using reader As MySql.Data.MySqlClient.MySqlDataReader = dbcomm.ExecuteReader()
                    While reader.Read()
                        Dim item As New ListViewItem(reader("BookingID").ToString())
                        item.SubItems.Add(If(IsDBNull(reader("Destination")), "", reader("Destination").ToString()))

                        Dim travelDateStr As String = ""
                        If Not IsDBNull(reader("TravelDate")) Then
                            Dim travelDate As DateTime = Convert.ToDateTime(reader("TravelDate"))
                            travelDateStr = travelDate.ToString("yyyy-MM-dd")
                        End If
                        item.SubItems.Add(travelDateStr)

                        Dim duration As String = "N/A"
                        If Not IsDBNull(reader("Duration")) AndAlso reader("Duration").ToString() <> "N/A" Then
                            Dim days As Integer = Convert.ToInt32(reader("Duration"))
                            duration = If(days <= 0, "1 day", $"{days} day{If(days > 1, "s", "")}")
                        End If
                        item.SubItems.Add(duration)

                        Dim amount As String = "₱0.00"
                        If Not IsDBNull(reader("FinalAmount")) AndAlso reader("FinalAmount").ToString() <> "N/A" Then
                            amount = "₱" + Convert.ToDecimal(reader("FinalAmount")).ToString("N2")
                        End If
                        item.SubItems.Add(amount)

                        Dim rating As String = "Not Rated"
                        If Not IsDBNull(reader("Rating")) AndAlso reader("Rating").ToString() <> "N/A" Then
                            Dim ratingValue As Decimal = Convert.ToDecimal(reader("Rating"))
                            If ratingValue > 0 Then
                                rating = $"{ratingValue:F1}/5.0"
                            Else
                                rating = "Pending Rating"
                            End If
                        End If
                        item.SubItems.Add(rating)

                        Dim status As String = If(IsDBNull(reader("Status")), "Unknown", reader("Status").ToString())
                        item.SubItems.Add(status)

                        Select Case status.ToLower()
                            Case "completed"
                                item.BackColor = Color.LightGreen
                            Case "cancelled"
                                item.BackColor = Color.LightCoral
                            Case Else
                                item.BackColor = Color.LightBlue
                        End Select

                        lstTravelHistory.Items.Add(item)
                    End While
                End Using
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error loading travel history: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Sub

    Private Sub lstTravelHistory_SelectedIndexChanged(sender As Object, e As EventArgs)
        If lstTravelHistory.SelectedItems.Count > 0 Then
            Dim status As String = lstTravelHistory.SelectedItems(0).SubItems(6).Text
            Dim rating As String = lstTravelHistory.SelectedItems(0).SubItems(5).Text

            ' Enable review button only for completed trips that haven't been rated
            btnReviewTrip.Enabled = (status = "Completed" AndAlso rating = "Not Rated")
        Else
            btnReviewTrip.Enabled = False
        End If
    End Sub

    Private Sub UpdateCompletedTrips()
        Dim transaction As MySql.Data.MySqlClient.MySqlTransaction = Nothing

        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            transaction = conn.BeginTransaction()

            ' Get bookings that need to be completed
            Dim getBookingsQuery As String = "SELECT BookingID, BookingReference, Status FROM Bookings " &
                                   "WHERE UserID = @UserID " &
                                   "AND Status IN ('Confirmed', 'Pending') " &
                                   "AND DATE(EndDate) < CURDATE()"

            Dim bookingsToComplete As New List(Of Tuple(Of Integer, String, String))
            Using getBookingsCmd As New MySql.Data.MySqlClient.MySqlCommand(getBookingsQuery, conn, transaction)
                getBookingsCmd.Parameters.AddWithValue("@UserID", _currentUserId)
                Using reader As MySql.Data.MySqlClient.MySqlDataReader = getBookingsCmd.ExecuteReader()
                    While reader.Read()
                        bookingsToComplete.Add(New Tuple(Of Integer, String, String)(
                   Convert.ToInt32(reader("BookingID")),
                   reader("BookingReference").ToString(),
                   reader("Status").ToString()
               ))
                    End While
                End Using
            End Using

            If bookingsToComplete.Count = 0 Then
                Console.WriteLine("No trips found that need to be marked as completed.")
                Return
            End If

            ' Update trips that are past their end date to completed
            Dim updateQuery As String = "UPDATE Bookings SET Status = 'Completed', UpdatedAt = NOW() " &
                  "WHERE UserID = @UserID " &
                  "AND Status IN ('Confirmed', 'Pending') " &
                  "AND DATE(EndDate) < CURDATE()"

            Dim updatedRows As Integer = 0
            Using updateCmd As New MySql.Data.MySqlClient.MySqlCommand(updateQuery, conn, transaction)
                updateCmd.Parameters.AddWithValue("@UserID", _currentUserId)
                updatedRows = updateCmd.ExecuteNonQuery()
            End Using

            ' Update payment status for completed bookings
            Dim updatePaymentQuery As String = "UPDATE BookingPayments bp " &
                         "INNER JOIN Bookings b ON bp.BookingID = b.BookingID " &
                         "SET bp.PaymentStatus = 'Completed', bp.UpdatedAt = NOW() " &
                         "WHERE b.UserID = @UserID AND b.Status = 'Completed'"

            Using updatePaymentCmd As New MySql.Data.MySqlClient.MySqlCommand(updatePaymentQuery, conn, transaction)
                updatePaymentCmd.Parameters.AddWithValue("@UserID", _currentUserId)
                updatePaymentCmd.ExecuteNonQuery()
            End Using

            ' Add entries to BookingHistory for audit trail for each completed booking
            For Each booking In bookingsToComplete
                Dim insertHistoryQuery As String = "INSERT INTO BookingHistory " &
                             "(BookingID, UserID, ActionType, ActionDescription, PreviousStatus, NewStatus, ModifiedBy) " &
                             "VALUES (@BookingID, @UserID, 'Completed', 'Trip automatically marked as completed', @PreviousStatus, 'Completed', @UserID)"

                Using insertHistoryCmd As New MySql.Data.MySqlClient.MySqlCommand(insertHistoryQuery, conn, transaction)
                    insertHistoryCmd.Parameters.AddWithValue("@BookingID", booking.Item1)
                    insertHistoryCmd.Parameters.AddWithValue("@UserID", _currentUserId)
                    insertHistoryCmd.Parameters.AddWithValue("@PreviousStatus", booking.Item3)
                    insertHistoryCmd.ExecuteNonQuery()
                End Using
            Next

            transaction.Commit()

            Console.WriteLine($"Successfully processed {updatedRows} completed trips at {DateTime.Now}")

            ' After updating completed trips, refresh the travel history to show new completed trips
            ' This ensures newly completed trips appear with the "Review Trip" button available
            LoadTravelHistory()

        Catch ex As Exception
            Try
                If transaction IsNot Nothing Then
                    transaction.Rollback()
                    Console.WriteLine($"Transaction rolled back for completed trips update: {ex.Message}")
                End If
            Catch rollbackEx As Exception
                Console.WriteLine($"Error during rollback: {rollbackEx.Message}")
            End Try

            MessageBox.Show($"Error updating trip statuses: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error)

        Finally
            Try
                If transaction IsNot Nothing Then
                    transaction.Dispose()
                End If
            Catch disposeEx As Exception
                Console.WriteLine($"Error disposing transaction: {disposeEx.Message}")
            End Try

            Try
                If conn.State = ConnectionState.Open Then
                    conn.Close()
                End If
            Catch closeEx As Exception
                Console.WriteLine($"Error closing connection: {closeEx.Message}")
            End Try
        End Try
    End Sub

    Private Sub btnChangePassword_Click(sender As Object, e As EventArgs)
        Try
            Using changePasswordDialog As New ChangePasswordDialog(_currentUserId)
                Dim result As DialogResult = changePasswordDialog.ShowDialog(Me)

                If result = DialogResult.OK Then
                    ' Password was successfully changed
                    ' You can add any additional logic here if needed
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error opening change password dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnViewBooking_Click(sender As Object, e As EventArgs) Handles btnViewBooking.Click
        Try
            If lstCurrentBookings.SelectedItems.Count = 0 Then
                MessageBox.Show("Please select a booking to view details.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Dim selectedItem As ListViewItem = lstCurrentBookings.SelectedItems(0)
            Dim bookingId As String = selectedItem.SubItems(0).Text

            If String.IsNullOrEmpty(bookingId) Then
                MessageBox.Show("Invalid booking selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            ViewBookingDetails(Convert.ToInt32(bookingId))
        Catch ex As Exception
            MessageBox.Show($"Error viewing booking details: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub btnReviewTrip_Click(sender As Object, e As EventArgs) Handles btnReviewTrip.Click
        Try
            ' Check if any item is selected
            If lstTravelHistory.SelectedItems.Count = 0 Then
                MessageBox.Show("Please select a completed trip to review.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            ' Get the selected trip
            Dim selectedItem As ListViewItem = lstTravelHistory.SelectedItems(0)
            Dim bookingId As Integer = Convert.ToInt32(selectedItem.SubItems(0).Text) ' Trip ID column
            Dim status As String = selectedItem.SubItems(6).Text.ToLower() ' Status column
            Dim destination As String = selectedItem.SubItems(1).Text ' Destination column

            ' Verify the trip is completed
            If status <> "completed" Then
                MessageBox.Show("You can only review completed trips.", "Trip Not Completed", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            ' Check if user has already reviewed this booking
            If HasUserReviewedBooking(bookingId) Then
                Dim result As DialogResult = MessageBox.Show($"You have already reviewed this trip to {destination}. Would you like to update your review?",
                                                           "Review Already Exists", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.No Then
                    Return
                End If
            End If

            ' Create user info object for the review dialog
            Dim userInfo As New MainForm.UserInfo With {
                .UserID = _currentUserId,
                .FirstName = txtFirstName.Text,
                .LastName = txtLastName.Text,
                .Username = txtUsername.Text,
                .Email = txtEmail.Text
            }

            ' Open the review dialog with the specific booking ID
            Using reviewDialog As New ReviewDialog(userInfo, bookingId)
                Dim result As DialogResult = reviewDialog.ShowDialog(Me)

                If result = DialogResult.OK Then
                    ' Refresh the travel history to show updated rating if applicable
                    LoadTravelHistory()
                    MessageBox.Show($"Thank you for reviewing your trip to {destination}!", "Review Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using

        Catch ex As Exception
            MessageBox.Show($"Error opening review dialog: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Console.WriteLine($"Error in btnReviewTrip_Click: {ex.Message}")
        End Try
    End Sub
    Private Sub LoadTravelHistory()
        Try
            lstTravelHistory.Items.Clear()

            Using conn As New MySqlConnection(connectionString)
                conn.Open()

                Dim query As String = "SELECT b.BookingID, tp.Title as Destination, " &
                                "DATE_FORMAT(b.TravelDate, '%Y-%m-%d') as TravelDate, " &
                                "DATEDIFF(b.EndDate, b.TravelDate) + 1 as Duration, " &
                                "IFNULL(ba.FinalAmount, 0) as FinalAmount, " &
                                "IFNULL(r.Rating, 0) as Rating, b.Status " &
                                "FROM Bookings b " &
                                "INNER JOIN TourPackages tp ON b.PackageID = tp.PackageID " &
                                "LEFT JOIN BookingAmounts ba ON b.BookingID = ba.BookingID " &
                                "LEFT JOIN Reviews r ON b.BookingID = r.BookingID " &
                                "WHERE b.UserID = @UserID AND b.Status IN ('Completed', 'Cancelled') " &
                                "ORDER BY b.TravelDate DESC"

                Using cmd As New MySqlCommand(query, conn)
                    cmd.Parameters.AddWithValue("@UserID", _currentUserId)

                    Using reader As MySqlDataReader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim item As New ListViewItem(reader("BookingID").ToString())
                            item.SubItems.Add(reader("Destination").ToString())
                            item.SubItems.Add(reader("TravelDate").ToString())

                            Dim days As Integer = Convert.ToInt32(reader("Duration"))
                            item.SubItems.Add($"{days} day{If(days > 1, "s", "")}")

                            item.SubItems.Add("₱" & Convert.ToDecimal(reader("FinalAmount")).ToString("N2"))

                            Dim rating As Decimal = Convert.ToDecimal(reader("Rating"))
                            item.SubItems.Add(If(rating > 0, $"{rating:F1}/5.0", "Not Rated"))

                            Dim status As String = reader("Status").ToString()
                            item.SubItems.Add(status)

                            ' Color coding based on status
                            Select Case status.ToLower()
                                Case "completed" : item.BackColor = Color.LightGreen
                                Case "cancelled" : item.BackColor = Color.LightCoral
                                Case Else : item.BackColor = Color.LightBlue
                            End Select

                            lstTravelHistory.Items.Add(item)
                        End While
                    End Using
                End Using
            End Using
        Catch ex As Exception
            MessageBox.Show($"Error loading travel history: {ex.Message}", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    ' Helper method to check if user has already reviewed a booking
    Private Function HasUserReviewedBooking(bookingId As Integer) As Boolean
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT COUNT(*) FROM Reviews WHERE UserID = @UserID AND BookingID = @BookingID"
            Using cmd As New MySql.Data.MySqlClient.MySqlCommand(query, conn)
                cmd.Parameters.AddWithValue("@UserID", _currentUserId)
                cmd.Parameters.AddWithValue("@BookingID", bookingId)

                Dim count As Integer = Convert.ToInt32(cmd.ExecuteScalar())
                Return count > 0
            End Using

        Catch ex As Exception
            Console.WriteLine($"Error checking existing review: {ex.Message}")
            Return False
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Sub btnExportHistory_Click(sender As Object, e As EventArgs) Handles btnExportHistory.Click
        Try
            If lstTravelHistory.SelectedItems.Count = 0 Then
                MessageBox.Show("Please select a booking to export.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Dim saveDialog As New SaveFileDialog()
            saveDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*"
            saveDialog.DefaultExt = "txt"
            saveDialog.FileName = $"BookingReceipt_{txtFirstName.Text}_{txtLastName.Text}_{DateTime.Now:yyyyMMdd}.txt"

            If saveDialog.ShowDialog() = DialogResult.OK Then
                ExportSelectedBookingReceipt(saveDialog.FileName)
            End If
        Catch ex As Exception
            MessageBox.Show($"Error exporting receipt: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub ExportSelectedBookingReceipt(fileName As String)
        Try
            Dim selectedItem As ListViewItem = lstTravelHistory.SelectedItems(0)
            Dim bookingId As String = selectedItem.Text
            Dim status As String = selectedItem.SubItems(6).Text

            ' Get booking data with calculated amounts
            Dim bookingData As Dictionary(Of String, String) = GetBookingDataFromDatabase(bookingId)
            Dim addOnsData As List(Of Dictionary(Of String, String)) = GetBookingAddOnsFromDatabase(bookingId)

            If bookingData Is Nothing Then
                MessageBox.Show("Could not retrieve booking data from database.", "Data Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            Using writer As New System.IO.StreamWriter(fileName)
                writer.WriteLine("========== LAKBAYPH TRAVEL AND TOURS ==========")
                writer.WriteLine($"Export Date: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
                writer.WriteLine("===============================================")
                writer.WriteLine()

                ' User Information
                writer.WriteLine("USER INFORMATION:")
                writer.WriteLine($"Customer Name      : {bookingData("FirstName")} {bookingData("LastName")}")
                writer.WriteLine($"Email              : {bookingData("Email")}")
                writer.WriteLine($"Phone              : {bookingData("Phone")}")
                writer.WriteLine()

                ' Booking Information
                writer.WriteLine("BOOKING INFORMATION:")
                writer.WriteLine($"Booking ID         : {bookingData("BookingID")}")
                writer.WriteLine($"Booking Reference  : {bookingData("BookingReference")}")
                writer.WriteLine($"Booking Date       : {bookingData("BookingDate")}")
                writer.WriteLine($"Travel Start Date  : {bookingData("TravelStartDate")}")
                writer.WriteLine($"Travel End Date    : {bookingData("TravelEndDate")}")
                writer.WriteLine($"No. of People      : {bookingData("NumPeople")}")
                writer.WriteLine($"Status             : {bookingData("Status")}")
                writer.WriteLine()

                ' Calculate package amount
                Dim numPeople As Integer = Convert.ToInt32(bookingData("NumPeople"))
                Dim basePackagePrice As Decimal = Convert.ToDecimal(bookingData("PackagePrice"))
                Dim totalPackagePrice As Decimal = basePackagePrice * numPeople

                ' Add-ons information
                writer.WriteLine("ADD-ONS INFORMATION:")
                Dim totalAddOns As Decimal = 0
                If addOnsData IsNot Nothing AndAlso addOnsData.Count > 0 Then
                    For Each addOn In addOnsData
                        Dim unitText As String = If(String.IsNullOrEmpty(addOn("Unit")), "", $" {addOn("Unit")}")
                        writer.WriteLine($"- {addOn("AddOnName")} (Qty: {addOn("Quantity")}{unitText}) - ₱{addOn("UnitPrice")} each = ₱{addOn("TotalPrice")}")
                        totalAddOns += Convert.ToDecimal(addOn("TotalPrice"))
                    Next
                    writer.WriteLine($"Total Add-Ons Amount: ₱{totalAddOns:N2}")
                Else
                    writer.WriteLine("N/A - No add-ons selected")
                End If
                writer.WriteLine()

                ' Package information
                writer.WriteLine("PACKAGE INFORMATION:")
                writer.WriteLine($"Package ID         : {bookingData("PackageID")}")
                writer.WriteLine($"Package Title      : {bookingData("PackageTitle")}")
                writer.WriteLine($"Package Location   : {bookingData("PackageLocation")}")
                writer.WriteLine($"Package Type       : {bookingData("PackageType")}")
                writer.WriteLine($"Base Price Per Pax : ₱{basePackagePrice:N2}")
                writer.WriteLine($"Total Package Price: ₱{totalPackagePrice:N2} ({numPeople} × ₱{basePackagePrice:N2})")
                writer.WriteLine()

                ' Payment information
                writer.WriteLine("PAYMENT INFORMATION:")
                writer.WriteLine($"Payment Method     : {bookingData("PaymentMethod")}")
                writer.WriteLine($"Payment Status     : {bookingData("PaymentStatus")}")

                ' Calculate final amount (package + addons)
                Dim finalAmount As Decimal = totalPackagePrice + totalAddOns
                writer.WriteLine($"Payment Amount     : ₱{finalAmount:N2}")
                writer.WriteLine()

                writer.WriteLine("-------------------------------------------------")
                writer.WriteLine("Thank you for booking with LakbayPH!")
                writer.WriteLine($"Generated on: {DateTime.Now:MMMM dd, yyyy hh:mm tt}")
            End Using

            MessageBox.Show($"Receipt exported successfully to {fileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As Exception
            MessageBox.Show($"Error writing receipt: {ex.Message}", "File Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub btnLogout_Click(sender As Object, e As EventArgs)
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

        If result = DialogResult.Yes Then
            Try
                user = Nothing
                Dim mainForm As New MainForm()
                mainForm.Show()
                Me.Close()
                If Application.OpenForms("TravelHomepageForm") IsNot Nothing Then
                    Application.OpenForms("TravelHomepageForm").Close()
                End If
            Catch ex As Exception
                MessageBox.Show("Error during logout: " & ex.Message, "Logout Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        If conn.State = ConnectionState.Open Then
            conn.Close()
        End If
        MyBase.OnFormClosed(e)
    End Sub

End Class