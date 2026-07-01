Imports System.Drawing
Imports System.Windows.Forms
Imports MySql.Data.MySqlClient

Public Class ReviewDialog
    Inherits Form
    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Dim conn As MySqlConnection = New MySqlConnection("Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;")
    Public sql As String
    Public cmd As MySqlCommand

    Private starLabels As New List(Of Label)
    Private selectedRating As Integer = 0
    Private txtComment As TextBox
    Private btnSubmit As Button
    Private btnCancel As Button
    Private user As MainForm.UserInfo
    Private lblCharacterCount As Label
    Private bookingID As Integer = 0

    ' New controls for booking selection
    Private rbGeneral As RadioButton
    Private rbSpecificBooking As RadioButton
    Private cmbBookings As ComboBox
    Private lblBookingInfo As Label
    Private pnlBookingSelection As Panel

    Public Sub New(user As MainForm.UserInfo, Optional bookingID As Integer = 0)
        Console.WriteLine($"User parameter is null: {user Is Nothing}")

        If user IsNot Nothing Then
            Console.WriteLine($"User.UserID: {user.UserID}")
            Console.WriteLine($"User.FirstName: {user.FirstName}")
            Console.WriteLine($"User.LastName: {user.LastName}")
            Console.WriteLine($"User.Username: {user.Username}")
            Console.WriteLine($"User.Email: {user.Email}")
        End If

        ' Validate user parameter with detailed error handling
        If user Is Nothing Then
            Console.WriteLine("ERROR: User parameter is null")
            MessageBox.Show("No user session found. Please log in to submit a review.", "Login Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            ' Don't create the form if user is null - close immediately
            Me.Close()
            Return
        End If

        ' Check if user ID is valid - using UserID property
        If user.UserID <= 0 Then
            Console.WriteLine($"ERROR: Invalid user ID: {user.UserID}")
            MessageBox.Show("Invalid user session detected. Please log out and log in again.", "Session Invalid", MessageBoxButtons.OK, MessageBoxIcon.Error)
            ' Don't create the form if user ID is invalid
            Me.Close()
            Return
        End If

        ' If we get here, user is valid
        Me.user = user
        Me.bookingID = bookingID

        Console.WriteLine($"ReviewDialog initialized successfully - UserID: {Me.user.UserID}, BookingID: {bookingID}")

        InitializeComponent()
        SetupUI()
        LoadUserBookings()

        ' If a specific bookingID is provided, pre-select it
        If bookingID > 0 Then
            rbSpecificBooking.Checked = True
            SelectBookingInComboBox(bookingID)
        End If
    End Sub

    ' Add property for external access
    Public Property BookingIDD As Integer
        Get
            Return bookingID
        End Get
        Set(value As Integer)
            bookingID = value
        End Set
    End Property

    Private Sub InitializeComponent()
        Me.Text = "Submit Your Review"
        Me.Size = New Size(600, 720) ' Increased size significantly
        Me.StartPosition = FormStartPosition.CenterParent
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.BackColor = Color.White
        Me.DoubleBuffered = True

    End Sub

    Private Sub SetupUI()
        ' Title
        Dim lblTitle As New Label()
        lblTitle.Text = "Submit Your Review"
        lblTitle.Font = New Font("Segoe UI", 18, FontStyle.Bold)
        lblTitle.ForeColor = Color.FromArgb(50, 50, 50)
        lblTitle.Location = New Point(30, 20)
        lblTitle.AutoSize = True

        ' Booking Selection Panel - Made much larger
        pnlBookingSelection = New Panel()
        pnlBookingSelection.Location = New Point(20, 60)
        pnlBookingSelection.Size = New Size(550, 160) ' Much larger panel
        pnlBookingSelection.BackColor = Color.FromArgb(248, 249, 250)
        pnlBookingSelection.BorderStyle = BorderStyle.FixedSingle

        ' Review type selection
        Dim lblReviewType As New Label()
        lblReviewType.Text = "Review Type:"
        lblReviewType.Font = New Font("Segoe UI", 11, FontStyle.Bold)
        lblReviewType.Location = New Point(15, 10)
        lblReviewType.Size = New Size(120, 20)
        pnlBookingSelection.Controls.Add(lblReviewType)

        ' General review option - Made wider
        rbGeneral = New RadioButton()
        rbGeneral.Text = "General review about Lakbay PH"
        rbGeneral.Font = New Font("Segoe UI", 10)
        rbGeneral.Location = New Point(15, 35)
        rbGeneral.Size = New Size(250, 25) ' Increased size
        rbGeneral.Checked = True
        rbGeneral.TabStop = True
        rbGeneral.UseVisualStyleBackColor = True
        pnlBookingSelection.Controls.Add(rbGeneral)

        ' Specific booking option - Positioned to the right with more space
        rbSpecificBooking = New RadioButton()
        rbSpecificBooking.Text = "Review a specific booking"
        rbSpecificBooking.Font = New Font("Segoe UI", 10)
        rbSpecificBooking.Location = New Point(280, 35) ' Better positioning
        rbSpecificBooking.Size = New Size(200, 25) ' Much larger size
        rbSpecificBooking.TabStop = True
        rbSpecificBooking.UseVisualStyleBackColor = True
        pnlBookingSelection.Controls.Add(rbSpecificBooking)

        ' Add event handlers AFTER adding to panel
        AddHandler rbGeneral.CheckedChanged, AddressOf RadioButton_CheckedChanged
        AddHandler rbSpecificBooking.CheckedChanged, AddressOf RadioButton_CheckedChanged

        ' Booking selection dropdown
        Dim lblSelectBooking As New Label()
        lblSelectBooking.Text = "Select Booking:"
        lblSelectBooking.Font = New Font("Segoe UI", 9)
        lblSelectBooking.Location = New Point(15, 70)
        lblSelectBooking.Size = New Size(100, 15)
        pnlBookingSelection.Controls.Add(lblSelectBooking)

        cmbBookings = New ComboBox()
        cmbBookings.Location = New Point(15, 90)
        cmbBookings.Size = New Size(450, 25) ' Much wider dropdown
        cmbBookings.Font = New Font("Segoe UI", 9)
        cmbBookings.DropDownStyle = ComboBoxStyle.DropDownList
        cmbBookings.Enabled = False
        AddHandler cmbBookings.SelectedIndexChanged, AddressOf CmbBookings_SelectedIndexChanged
        pnlBookingSelection.Controls.Add(cmbBookings)

        ' Booking info - More space
        lblBookingInfo = New Label()
        lblBookingInfo.Location = New Point(15, 120)
        lblBookingInfo.Size = New Size(450, 30) ' Increased height and width
        lblBookingInfo.Font = New Font("Segoe UI", 8)
        lblBookingInfo.ForeColor = Color.FromArgb(100, 100, 100)
        lblBookingInfo.TextAlign = ContentAlignment.MiddleLeft
        pnlBookingSelection.Controls.Add(lblBookingInfo)

        ' Rating section - Adjusted Y position for larger panel
        Dim lblRating As New Label()
        lblRating.Text = "Rating:"
        lblRating.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblRating.Location = New Point(30, 240)
        lblRating.Size = New Size(100, 25)

        ' Create star rating
        For i As Integer = 1 To 5
            Dim star As New Label()
            star.Text = "★"
            star.Font = New Font("Segoe UI", 24)
            star.ForeColor = Color.Gray
            star.Size = New Size(30, 35)
            star.Location = New Point(30 + (i - 1) * 35, 270)
            star.Cursor = Cursors.Hand
            star.Tag = i

            AddHandler star.Click, AddressOf Star_Click
            AddHandler star.MouseEnter, AddressOf Star_MouseEnter
            AddHandler star.MouseLeave, AddressOf Star_MouseLeave

            starLabels.Add(star)
            Me.Controls.Add(star)
        Next

        Dim lblSelectedRating As New Label()
        lblSelectedRating.Name = "lblSelectedRating"
        lblSelectedRating.Text = "Please select a rating"
        lblSelectedRating.Font = New Font("Segoe UI", 10, FontStyle.Italic)
        lblSelectedRating.ForeColor = Color.Gray
        lblSelectedRating.Location = New Point(200, 280)
        lblSelectedRating.Size = New Size(200, 20)

        ' Comment section - Adjusted Y position
        Dim lblComment As New Label()
        lblComment.Text = "Comments (Optional):"
        lblComment.Font = New Font("Segoe UI", 12, FontStyle.Bold)
        lblComment.Location = New Point(30, 330)
        lblComment.Size = New Size(200, 25)

        txtComment = New TextBox()
        txtComment.Multiline = True
        txtComment.ScrollBars = ScrollBars.Vertical
        txtComment.Location = New Point(30, 360)
        txtComment.Size = New Size(530, 140) ' Increased width and height
        txtComment.Font = New Font("Segoe UI", 10)
        txtComment.MaxLength = 1000
        AddHandler txtComment.TextChanged, AddressOf txtComment_TextChanged
        AddHandler txtComment.KeyDown, AddressOf txtComment_KeyDown

        ' Character counter - Adjusted position
        lblCharacterCount = New Label()
        lblCharacterCount.Text = "0/1000 characters"
        lblCharacterCount.Font = New Font("Segoe UI", 8, FontStyle.Italic)
        lblCharacterCount.ForeColor = Color.Gray
        lblCharacterCount.Location = New Point(460, 505)
        lblCharacterCount.Size = New Size(100, 15)
        lblCharacterCount.TextAlign = ContentAlignment.TopRight

        ' Buttons - Adjusted Y position for larger form
        btnSubmit = New Button()
        btnSubmit.Text = "Submit Review"
        btnSubmit.Size = New Size(120, 35)
        btnSubmit.Location = New Point(350, 530)
        btnSubmit.BackColor = Color.FromArgb(39, 174, 96)
        btnSubmit.ForeColor = Color.White
        btnSubmit.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnSubmit.FlatStyle = FlatStyle.Flat
        btnSubmit.FlatAppearance.BorderSize = 0
        btnSubmit.Cursor = Cursors.Hand
        AddHandler btnSubmit.Click, AddressOf BtnSubmit_Click

        btnCancel = New Button()
        btnCancel.Text = "Cancel"
        btnCancel.Size = New Size(80, 35)
        btnCancel.Location = New Point(480, 530)
        btnCancel.BackColor = Color.FromArgb(231, 76, 60)
        btnCancel.ForeColor = Color.White
        btnCancel.Font = New Font("Segoe UI", 10, FontStyle.Bold)
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderSize = 0
        btnCancel.Cursor = Cursors.Hand
        btnCancel.DialogResult = DialogResult.Cancel
        AddHandler btnCancel.Click, AddressOf BtnCancel_Click

        Me.Controls.AddRange({lblTitle, pnlBookingSelection, lblRating, lblSelectedRating, lblComment, txtComment, lblCharacterCount, btnSubmit, btnCancel})
    End Sub

    Private Sub LoadUserBookings()
        Try
            ' Validate user object before proceeding - using UserID property
            If user Is Nothing OrElse user.UserID <= 0 Then
                Console.WriteLine("User object is null or invalid in LoadUserBookings")
                cmbBookings.Items.Add("User not available")
                rbSpecificBooking.Enabled = False
                rbSpecificBooking.Text = "Review a specific booking (User not available)"
                Return
            End If

            Console.WriteLine($"Loading bookings for user ID: {user.UserID}")

            cmbBookings.Items.Clear()
            Using connection As New MySqlConnection(connectionString)
                Try
                    connection.Open()
                    Console.WriteLine("Database connection opened successfully")

                    ' Modified query to handle potential null values better
                    Dim query As String = "SELECT BookingID, CONCAT('Booking #', BookingID, ' (', DATE_FORMAT(COALESCE(BookingDate, NOW()), '%Y-%m-%d'), ')') AS BookingDisplay, COALESCE(BookingDate, NOW()) as BookingDate FROM Bookings WHERE UserID = @UserID ORDER BY BookingDate DESC"

                    Using command As New MySqlCommand(query, connection)
                        command.Parameters.AddWithValue("@UserID", user.UserID)

                        Using reader As MySqlDataReader = command.ExecuteReader()
                            Dim bookingCount As Integer = 0
                            While reader.Read()
                                Try
                                    Dim bookingItem As New BookingItem With {
                                        .BookingID = reader.GetInt32("BookingID"),
                                        .DisplayText = reader.GetString("BookingDisplay"),
                                        .Destination = "N/A",
                                        .BookingDate = reader.GetDateTime("BookingDate")
                                    }
                                    cmbBookings.Items.Add(bookingItem)
                                    bookingCount += 1

                                    Console.WriteLine($"Loaded booking: ID={bookingItem.BookingID}, Display={bookingItem.DisplayText}")
                                Catch ex As Exception
                                    Console.WriteLine($"Error processing booking record: {ex.Message}")
                                    Continue While
                                End Try
                            End While

                            Console.WriteLine($"Total bookings loaded: {bookingCount}")
                        End Using
                    End Using
                Catch dbEx As MySqlException
                    Console.WriteLine($"MySQL Error: {dbEx.Message}")
                    Throw New Exception($"Database error: {dbEx.Message}", dbEx)
                End Try
            End Using

            If cmbBookings.Items.Count = 0 Then
                cmbBookings.Items.Add("No bookings found")
                rbSpecificBooking.Enabled = False
                rbSpecificBooking.Text = "Review a specific booking (No bookings available)"
                Console.WriteLine("No bookings found for user")
            Else
                rbSpecificBooking.Enabled = True
                rbSpecificBooking.Text = "Review a specific booking"
            End If

        Catch ex As Exception
            Console.WriteLine($"Error in LoadUserBookings: {ex.Message}")
            MessageBox.Show($"Error loading bookings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbBookings.Items.Clear()
            cmbBookings.Items.Add("Error loading bookings")
            rbSpecificBooking.Enabled = False
            rbSpecificBooking.Text = "Review a specific booking (Error loading)"
        End Try
    End Sub

    Private Sub SelectBookingInComboBox(targetBookingID As Integer)
        Try
            For i As Integer = 0 To cmbBookings.Items.Count - 1
                Dim item As BookingItem = TryCast(cmbBookings.Items(i), BookingItem)
                If item IsNot Nothing AndAlso item.BookingID = targetBookingID Then
                    cmbBookings.SelectedIndex = i
                    Exit For
                End If
            Next
        Catch ex As Exception
            Console.WriteLine($"Error in SelectBookingInComboBox: {ex.Message}")
        End Try
    End Sub

    Private Sub RadioButton_CheckedChanged(sender As Object, e As EventArgs)
        Try
            Console.WriteLine($"RadioButton_CheckedChanged called - Sender: {sender.GetType().Name}")

            Dim senderRadio As RadioButton = TryCast(sender, RadioButton)
            If senderRadio Is Nothing Then
                Console.WriteLine("Sender is not a RadioButton")
                Return
            End If

            ' Only process if the radio button is being checked (not unchecked)
            If Not senderRadio.Checked Then
                Console.WriteLine($"RadioButton {senderRadio.Text} is being unchecked, ignoring")
                Return
            End If

            Console.WriteLine($"RadioButton {senderRadio.Text} is being checked")

            If senderRadio Is rbSpecificBooking AndAlso rbSpecificBooking.Checked Then
                Console.WriteLine("Specific booking selected")
                cmbBookings.Enabled = True
                If cmbBookings.Items.Count > 0 AndAlso Not cmbBookings.Items(0).ToString().Contains("No bookings") AndAlso Not cmbBookings.Items(0).ToString().Contains("Error") Then
                    If cmbBookings.SelectedIndex = -1 Then
                        cmbBookings.SelectedIndex = 0
                    End If
                    ' Ensure booking ID is set when switching to specific booking
                    Dim selectedItem As BookingItem = TryCast(cmbBookings.SelectedItem, BookingItem)
                    If selectedItem IsNot Nothing Then
                        bookingID = selectedItem.BookingID
                        lblBookingInfo.Text = $"Date: {selectedItem.BookingDate.ToString("MMM dd, yyyy")}"
                    End If
                End If
            ElseIf senderRadio Is rbGeneral AndAlso rbGeneral.Checked Then
                Console.WriteLine("General review selected")
                cmbBookings.Enabled = False
                cmbBookings.SelectedIndex = -1
                bookingID = 0
                lblBookingInfo.Text = ""
            End If

            Console.WriteLine($"Radio changed - General: {rbGeneral.Checked}, Specific: {rbSpecificBooking.Checked}, BookingID: {bookingID}")
        Catch ex As Exception
            Console.WriteLine($"Error in RadioButton_CheckedChanged: {ex.Message}")
            Console.WriteLine($"Stack trace: {ex.StackTrace}")
        End Try
    End Sub

    Private Sub CmbBookings_SelectedIndexChanged(sender As Object, e As EventArgs)
        Try
            If cmbBookings.SelectedIndex >= 0 Then
                Dim selectedItem As BookingItem = TryCast(cmbBookings.SelectedItem, BookingItem)
                If selectedItem IsNot Nothing Then
                    bookingID = selectedItem.BookingID
                    lblBookingInfo.Text = $"Date: {selectedItem.BookingDate.ToString("MMM dd, yyyy")}"
                    Console.WriteLine($"Selected Booking ID: {bookingID}")
                Else
                    bookingID = 0
                    lblBookingInfo.Text = ""
                End If
            Else
                bookingID = 0
                lblBookingInfo.Text = ""
            End If
        Catch ex As Exception
            Console.WriteLine($"Error in CmbBookings_SelectedIndexChanged: {ex.Message}")
        End Try
    End Sub

    Private Sub Star_Click(sender As Object, e As EventArgs)
        Try
            Dim clickedStar As Label = CType(sender, Label)
            selectedRating = CInt(clickedStar.Tag)
            UpdateStars()

            Dim lblSelectedRating As Label = CType(Me.Controls("lblSelectedRating"), Label)
            lblSelectedRating.Text = $"Rating: {selectedRating} star{If(selectedRating > 1, "s", "")}"
            lblSelectedRating.ForeColor = Color.FromArgb(39, 174, 96)
        Catch ex As Exception
            Console.WriteLine($"Error in Star_Click: {ex.Message}")
        End Try
    End Sub

    Private Sub Star_MouseEnter(sender As Object, e As EventArgs)
        Try
            Dim hoveredStar As Label = CType(sender, Label)
            Dim hoverRating As Integer = CInt(hoveredStar.Tag)

            For i As Integer = 0 To starLabels.Count - 1
                If i < hoverRating Then
                    starLabels(i).ForeColor = Color.Gold
                Else
                    starLabels(i).ForeColor = If(i < selectedRating, Color.Gold, Color.Gray)
                End If
            Next
        Catch ex As Exception
            Console.WriteLine($"Error in Star_MouseEnter: {ex.Message}")
        End Try
    End Sub

    Private Sub Star_MouseLeave(sender As Object, e As EventArgs)
        UpdateStars()
    End Sub

    Private Sub UpdateStars()
        Try
            For i As Integer = 0 To starLabels.Count - 1
                starLabels(i).ForeColor = If(i < selectedRating, Color.Gold, Color.Gray)
            Next
        Catch ex As Exception
            Console.WriteLine($"Error in UpdateStars: {ex.Message}")
        End Try
    End Sub

    Private Sub BtnSubmit_Click(sender As Object, e As EventArgs)
        Try
            Console.WriteLine($"User object is null: {user Is Nothing}")

            If user IsNot Nothing Then
                Console.WriteLine($"User ID: {user.UserID}")
                Console.WriteLine($"User FirstName: {user.FirstName}")
                Console.WriteLine($"User LastName: {user.LastName}")
                Console.WriteLine($"User Username: {user.Username}")
            End If

            ' Primary validation - check if user exists
            If user Is Nothing Then
                Console.WriteLine("ERROR: User object is null during submit")
                MessageBox.Show("Your session has expired. Please close this dialog and log in again.", "Session Expired", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
                Return
            End If

            ' Secondary validation - check if user ID is valid - using UserID property
            If user.UserID <= 0 Then
                Console.WriteLine($"ERROR: Invalid user ID during submit: {user.UserID}")
                MessageBox.Show($"Invalid user ID detected ({user.UserID}). Please log out and log in again.", "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
                Return
            End If

            Console.WriteLine("User validation passed")

            If selectedRating = 0 Then
                MessageBox.Show("Please select a rating before submitting your review.", "Rating Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If rbSpecificBooking.Checked Then
                If cmbBookings.SelectedIndex = -1 Then
                    MessageBox.Show("Please select a booking to review.", "Booking Required", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                ' Additional validation to ensure booking ID is properly set
                Dim selectedItem As BookingItem = TryCast(cmbBookings.SelectedItem, BookingItem)
                If selectedItem Is Nothing OrElse selectedItem.BookingID = 0 Then
                    MessageBox.Show("Invalid booking selection. Please select a valid booking.", "Invalid Booking", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If
                bookingID = selectedItem.BookingID
            Else
                bookingID = 0 ' Ensure it's 0 for general reviews
            End If

            If String.IsNullOrWhiteSpace(txtComment.Text) Then
                Dim result As DialogResult = MessageBox.Show("Would you like to submit your review without any comments?", "No Comments", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.No Then
                    txtComment.Focus()
                    Return
                End If
            End If

            Console.WriteLine("All validations passed, attempting to save review")
            SaveReview()

            Dim reviewType As String = If(rbGeneral.Checked, "general review", "booking review")
            MessageBox.Show($"Thank you for your {reviewType}! Your feedback has been submitted successfully.", "Review Submitted", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Me.DialogResult = DialogResult.OK
            Me.Close()

        Catch ex As Exception
            Console.WriteLine($"ERROR in BtnSubmit_Click: {ex.Message}")
            Console.WriteLine($"Stack trace: {ex.StackTrace}")
            MessageBox.Show($"An error occurred while submitting your review: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs)
        Try
            Dim result As DialogResult = DialogResult.Yes

            If selectedRating > 0 OrElse Not String.IsNullOrWhiteSpace(txtComment.Text) Then
                result = MessageBox.Show("Are you sure you want to cancel? Your review will be lost.", "Confirm Cancel", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            End If

            If result = DialogResult.Yes Then
                Me.DialogResult = DialogResult.Cancel
                Me.Close()
            End If
        Catch ex As Exception
            Console.WriteLine($"Error in BtnCancel_Click: {ex.Message}")
        End Try
    End Sub

    Private Sub SaveReview()
        Try
            ' Double-check user validity before saving - using UserID property
            If user Is Nothing OrElse user.UserID <= 0 Then
                Throw New Exception("Invalid user ID. Cannot save review.")
            End If

            Using connection As New MySqlConnection(connectionString)
                connection.Open()

                Dim packageID As Integer = 0

                ' Only get PackageID from booking if it's a booking-specific review
                If Not rbGeneral.Checked AndAlso bookingID > 0 Then
                    Dim getPackageQuery As String = "SELECT PackageID FROM Bookings WHERE BookingID = @BookingID"
                    Using getPackageCmd As New MySqlCommand(getPackageQuery, connection)
                        getPackageCmd.Parameters.AddWithValue("@BookingID", bookingID)
                        Dim result = getPackageCmd.ExecuteScalar()
                        If result IsNot Nothing Then
                            packageID = Convert.ToInt32(result)
                            Console.WriteLine($"Retrieved PackageID: {packageID} for BookingID: {bookingID}")
                        End If
                    End Using
                End If

                ' For general reviews, PackageID will remain 0 (or you could set it to NULL)
                If rbGeneral.Checked Then
                    Console.WriteLine("General review - PackageID will be set to NULL")
                End If

                Dim query As String = "INSERT INTO Reviews (UserID, BookingID, PackageID, Rating, Comment, ReviewDate) VALUES (@UserID, @BookingID, @PackageID, @Rating, @Comment, NOW())"
                Using command As New MySqlCommand(query, connection)
                    command.Parameters.AddWithValue("@UserID", user.UserID)

                    Console.WriteLine($"Saving review - General: {rbGeneral.Checked}, BookingID: {bookingID}, UserID: {user.UserID}, PackageID: {packageID}")

                    ' Handle BookingID properly - use NULL for general reviews
                    If rbGeneral.Checked OrElse bookingID = 0 Then
                        command.Parameters.AddWithValue("@BookingID", DBNull.Value)
                        Console.WriteLine("Saving as general review (BookingID = NULL)")
                    Else
                        command.Parameters.AddWithValue("@BookingID", bookingID)
                        Console.WriteLine($"Saving as booking-specific review (BookingID = {bookingID})")
                    End If

                    ' Handle PackageID - use NULL for general reviews, actual PackageID for booking-specific reviews
                    If rbGeneral.Checked OrElse packageID = 0 Then
                        command.Parameters.AddWithValue("@PackageID", DBNull.Value)
                        Console.WriteLine("PackageID set to NULL")
                    Else
                        command.Parameters.AddWithValue("@PackageID", packageID)
                        Console.WriteLine($"PackageID set to {packageID}")
                    End If

                    command.Parameters.AddWithValue("@Rating", selectedRating)

                    If String.IsNullOrWhiteSpace(txtComment.Text) Then
                        command.Parameters.AddWithValue("@Comment", DBNull.Value)
                    Else
                        command.Parameters.AddWithValue("@Comment", txtComment.Text.Trim())
                    End If

                    command.ExecuteNonQuery()
                    Console.WriteLine("Review saved successfully")
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine($"Error in SaveReview: {ex.Message}")
            Throw
        End Try
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        Try
            If Me.DialogResult = DialogResult.None AndAlso (selectedRating > 0 OrElse Not String.IsNullOrWhiteSpace(txtComment.Text)) Then
                Dim result As DialogResult = MessageBox.Show("Are you sure you want to close without submitting your review?", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If result = DialogResult.No Then
                    e.Cancel = True
                    Return
                End If
            End If

            MyBase.OnFormClosing(e)
        Catch ex As Exception
            Console.WriteLine($"Error in OnFormClosing: {ex.Message}")
        End Try
    End Sub

    Private Sub txtComment_KeyDown(sender As Object, e As KeyEventArgs)
        Try
            If e.Control AndAlso e.KeyCode = Keys.Enter Then
                BtnSubmit_Click(sender, e)
            End If
        Catch ex As Exception
            Console.WriteLine($"Error in txtComment_KeyDown: {ex.Message}")
        End Try
    End Sub

    Private Sub txtComment_TextChanged(sender As Object, e As EventArgs)
        Try
            Dim currentLength As Integer = txtComment.Text.Length
            lblCharacterCount.Text = $"{currentLength}/1000 characters"

            If currentLength > 900 Then
                lblCharacterCount.ForeColor = Color.Red
            ElseIf currentLength > 750 Then
                lblCharacterCount.ForeColor = Color.Orange
            Else
                lblCharacterCount.ForeColor = Color.Gray
            End If
        Catch ex As Exception
            Console.WriteLine($"Error in txtComment_TextChanged: {ex.Message}")
        End Try
    End Sub

    Public Shared Function GetReviewStatistics() As DataTable
        Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
        Dim dataTable As New DataTable()
        Using connection As New MySqlConnection(connectionString)
            connection.Open()
            Dim query As String = "SELECT * FROM ReviewStatistics"
            Using adapter As New MySqlDataAdapter(query, connection)
                adapter.Fill(dataTable)
            End Using
        End Using
        Return dataTable
    End Function

    ' Helper class for booking items
    Public Class BookingItem
        Public Property BookingID As Integer
        Public Property DisplayText As String
        Public Property Destination As String
        Public Property BookingDate As DateTime

        Public Overrides Function ToString() As String
            Return DisplayText
        End Function
    End Class

End Class