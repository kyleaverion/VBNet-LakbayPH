Imports System.Drawing
Imports System.Windows.Forms
Imports MySqlConnector
Imports System.Security.Cryptography
Imports System.Text
Imports System.IO

Public Class SignUpForm
    Inherits Form

    Dim connectionString As String = "Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;"
    Dim conn As MySqlConnection = New MySqlConnection("Server=localhost;Database=lakbayph_web;Uid=root;Pwd=;")
    Public sql As String
    Public dbcomm As MySqlCommand

    ' Top Navigation Controls
    Private logo As PictureBox
    Private lblLogo As Label
    Private pnlTopNav As Panel

    Private txtFirstName As TextBox
    Private txtLastName As TextBox
    Private txtEmail As TextBox
    Private txtUsername As TextBox
    Private cmbGender As ComboBox
    Private dtpBirthDate As DateTimePicker
    Private txtPhoneNumber As TextBox
    Private txtAddress As TextBox
    Private txtPassword As TextBox
    Private txtConfirmPassword As TextBox
    Private btnSignUp As Button
    Private btnCancel As Button
    Private btnClear As Button

    Private Const PLACEHOLDER_FIRSTNAME As String = "First Name"
    Private Const PLACEHOLDER_LASTNAME As String = "Last Name"
    Private Const PLACEHOLDER_EMAIL As String = "Email"
    Private Const PLACEHOLDER_USERNAME As String = "Username"
    Private Const PLACEHOLDER_PHONE As String = "Phone Number"
    Private Const PLACEHOLDER_ADDRESS As String = "Address"
    Private Const PLACEHOLDER_PASSWORD As String = "Password"
    Private Const PLACEHOLDER_CONFIRMPASSWORD As String = "Confirm Password"

    Public Sub New()
        InitializeComponent()
        SetupForm()
        CreateControls()
        SetupEventHandlers()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'SignUpForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(282, 253)
        Me.Name = "SignUpForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "LakbayPH - Sign Up"
        Me.WindowState = System.Windows.Forms.FormWindowState.Maximized
        Me.ResumeLayout(False)
        Me.DoubleBuffered = True

    End Sub

    Private Sub Form_Resized(sender As Object, e As EventArgs) Handles MyBase.Resize
        Me.Controls.Clear()
        CreateControls()
        SetupEventHandlers()
    End Sub

    Private Sub SetupForm()
        ' Set form background with image
        Try
            ' Try output directory first, then source directory
            Dim bgImagePath As String = Path.Combine(Application.StartupPath, "loginbg.png")
            If Not File.Exists(bgImagePath) Then
                bgImagePath = Path.Combine(Application.StartupPath, "..\..\loginbg.png")
            End If

            If File.Exists(bgImagePath) Then
                Me.BackgroundImage = Image.FromFile(bgImagePath)
                Me.BackgroundImageLayout = ImageLayout.Stretch
            Else
                Me.BackColor = Color.FromArgb(45, 85, 95)
            End If
        Catch
            Me.BackColor = Color.FromArgb(45, 85, 95)
        End Try
        Me.FormBorderStyle = FormBorderStyle.Sizable
        Me.MinimumSize = New Size(1000, 600)
    End Sub

    Private Sub CreateTopNavigation()
        ' Top navigation panel with background
        pnlTopNav = New Panel()
        pnlTopNav.Size = New Size(Me.Width, 70)
        pnlTopNav.Location = New Point(0, 0)
        pnlTopNav.BackColor = Color.White
        pnlTopNav.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlTopNav)

        ' Logo
        logo = New PictureBox()
        logo.Size = New Size(40, 40)
        logo.Location = New Point(680, 15)
        Try
            ' Try output directory first, then source directory
            Dim logoPath As String = Path.Combine(Application.StartupPath, "logo.png")
            If Not File.Exists(logoPath) Then
                logoPath = Path.Combine(Application.StartupPath, "..\..\logo.png")
            End If

            If File.Exists(logoPath) Then
                logo.Image = Image.FromFile(logoPath)
                logo.SizeMode = PictureBoxSizeMode.StretchImage
            Else
                logo.BackColor = Color.White
            End If
        Catch
            logo.BackColor = Color.White
        End Try
        pnlTopNav.Controls.Add(logo)

        ' Logo text
        lblLogo = New Label()
        lblLogo.Text = "LakbayPH" & vbCrLf & "Travel + Tours"
        lblLogo.Font = New Font("Arial", 11, FontStyle.Bold)
        lblLogo.ForeColor = Color.FromArgb(6, 41, 55) ' #062937
        lblLogo.Location = New Point(730, 15)
        lblLogo.Size = New Size(120, 40)
        lblLogo.TextAlign = ContentAlignment.MiddleCenter
        pnlTopNav.Controls.Add(lblLogo)
    End Sub

    Private Sub CreateControls()
        CreateTopNavigation()

        ' Calculate center position based on form width
        Dim formWidth As Integer = Me.ClientSize.Width
        Dim formHeight As Integer = Me.ClientSize.Height

        ' Back panel
        Dim pnlBack = New Panel()
        pnlBack.BackColor = Color.FromArgb(82, 255, 255, 255) ' White with 32% transparency
        pnlBack.Location = New Point((formWidth - 900) \ 2, (formHeight - 450) \ 2)
        pnlBack.Size = New Size(900, 450)
        pnlBack.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        Me.Controls.Add(pnlBack)

        ' Create main panel
        Dim pnlMain As New Panel()
        pnlMain.Size = New Size(800, 400)
        pnlMain.Location = New Point(50, 25)
        pnlMain.BackColor = Color.FromArgb(25, 77, 98)
        pnlBack.Controls.Add(pnlMain)

        ' Title with airplane icon
        Dim lblTitle As New Label()
        lblTitle.Text = "Sign Up ✈"
        lblTitle.Font = New Font("Arial", 28, FontStyle.Bold)
        lblTitle.ForeColor = Color.White
        lblTitle.Location = New Point(35, 25)
        lblTitle.Size = New Size(250, 50)
        lblTitle.BackColor = Color.Transparent
        pnlMain.Controls.Add(lblTitle)

        ' Clear button (top right)
        Dim btnClearTop As New Button()
        btnClearTop.Text = "CLEAR"
        btnClearTop.Font = New Font("Arial", 10, FontStyle.Bold)
        btnClearTop.ForeColor = Color.White
        btnClearTop.BackColor = Color.FromArgb(100, 150, 200)
        btnClearTop.FlatStyle = FlatStyle.Flat
        btnClearTop.FlatAppearance.BorderSize = 0
        btnClearTop.Location = New Point(695, 50)
        btnClearTop.Size = New Size(70, 30)
        pnlMain.Controls.Add(btnClearTop)

        ' Field positioning
        Dim leftColumnX As Integer = 35
        Dim rightColumnX As Integer = 415
        Dim currentY As Integer = 90
        Dim fieldHeight As Integer = 30
        Dim fieldSpacing As Integer = 50

        ' LEFT COLUMN - 5 FIELDS
        ' First Name
        txtFirstName = New TextBox()
        txtFirstName.Font = New Font("Arial", 14)
        txtFirstName.Location = New Point(leftColumnX, currentY)
        txtFirstName.Size = New Size(350, fieldHeight)
        txtFirstName.BackColor = Color.White
        txtFirstName.ForeColor = Color.FromArgb(6, 41, 55)
        txtFirstName.BorderStyle = BorderStyle.None
        txtFirstName.Text = PLACEHOLDER_FIRSTNAME
        txtFirstName.Multiline = True
        txtFirstName.Padding = New Padding(15, 12, 15, 12)
        pnlMain.Controls.Add(txtFirstName)

        ' Last Name
        txtLastName = New TextBox()
        txtLastName.Font = New Font("Arial", 14)
        txtLastName.Location = New Point(leftColumnX, currentY + fieldSpacing)
        txtLastName.Size = New Size(350, fieldHeight)
        txtLastName.BackColor = Color.White
        txtLastName.ForeColor = Color.FromArgb(6, 41, 55)
        txtLastName.BorderStyle = BorderStyle.None
        txtLastName.Text = PLACEHOLDER_LASTNAME
        txtLastName.Multiline = True
        txtLastName.Padding = New Padding(15, 12, 15, 12)
        pnlMain.Controls.Add(txtLastName)

        ' Address
        txtAddress = New TextBox()
        txtAddress.Font = New Font("Arial", 14)
        txtAddress.Location = New Point(leftColumnX, currentY + fieldSpacing * 2)
        txtAddress.Size = New Size(350, fieldHeight)
        txtAddress.BackColor = Color.White
        txtAddress.ForeColor = Color.FromArgb(6, 41, 55)
        txtAddress.BorderStyle = BorderStyle.None
        txtAddress.Text = PLACEHOLDER_ADDRESS
        txtAddress.Multiline = True
        txtAddress.Padding = New Padding(15, 12, 15, 12)
        pnlMain.Controls.Add(txtAddress)

        ' Birth Date
        dtpBirthDate = New DateTimePicker()
        dtpBirthDate.Font = New Font("Arial", 12)
        dtpBirthDate.Location = New Point(leftColumnX, currentY + fieldSpacing * 3)
        dtpBirthDate.Size = New Size(350, 30)
        dtpBirthDate.Format = DateTimePickerFormat.Short
        dtpBirthDate.MaxDate = DateTime.Today.AddYears(-13)
        dtpBirthDate.MinDate = DateTime.Today.AddYears(-120)
        dtpBirthDate.Value = DateTime.Today.AddYears(-18)
        dtpBirthDate.BackColor = Color.FromArgb(51, 92, 103)
        dtpBirthDate.ForeColor = Color.FromArgb(6, 41, 55)
        pnlMain.Controls.Add(dtpBirthDate)

        ' Gender
        cmbGender = New ComboBox()
        cmbGender.Font = New Font("Arial", 12)
        cmbGender.Location = New Point(leftColumnX, currentY + fieldSpacing * 4)
        cmbGender.Size = New Size(350, 30)
        cmbGender.BackColor = Color.White
        cmbGender.ForeColor = Color.FromArgb(6, 41, 55)
        cmbGender.DropDownStyle = ComboBoxStyle.DropDownList
        cmbGender.Items.AddRange(New String() {"Select Gender", "Male", "Female", "Other", "Prefer not to say"})
        cmbGender.SelectedIndex = 0
        pnlMain.Controls.Add(cmbGender)

        ' Email
        Dim emailIcon As New Label()
        emailIcon.Text = "@"
        emailIcon.Font = New Font("Arial", 14, FontStyle.Bold)
        emailIcon.ForeColor = Color.FromArgb(6, 41, 55)
        emailIcon.Location = New Point(rightColumnX, currentY)
        emailIcon.Size = New Size(45, fieldHeight)
        emailIcon.BackColor = Color.White
        emailIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlMain.Controls.Add(emailIcon)

        txtEmail = New TextBox()
        txtEmail.Font = New Font("Arial", 14)
        txtEmail.Location = New Point(rightColumnX + 45, currentY)
        txtEmail.Size = New Size(305, fieldHeight)
        txtEmail.BackColor = Color.White
        txtEmail.ForeColor = Color.FromArgb(6, 41, 55)
        txtEmail.BorderStyle = BorderStyle.None
        txtEmail.Text = PLACEHOLDER_EMAIL
        txtEmail.Multiline = True
        txtEmail.Padding = New Padding(15, 12, 15, 12)
        pnlMain.Controls.Add(txtEmail)

        ' Phone Number
        Dim phoneIcon As New Label()
        phoneIcon.Text = "📞"
        phoneIcon.Font = New Font("Arial", 14)
        phoneIcon.ForeColor = Color.FromArgb(6, 41, 55)
        phoneIcon.Location = New Point(rightColumnX, currentY + fieldSpacing)
        phoneIcon.Size = New Size(45, fieldHeight)
        phoneIcon.BackColor = Color.White
        phoneIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlMain.Controls.Add(phoneIcon)

        txtPhoneNumber = New TextBox()
        txtPhoneNumber.Font = New Font("Arial", 14)
        txtPhoneNumber.Location = New Point(rightColumnX + 45, currentY + fieldSpacing)
        txtPhoneNumber.Size = New Size(305, fieldHeight)
        txtPhoneNumber.BackColor = Color.White
        txtPhoneNumber.ForeColor = Color.FromArgb(6, 41, 55)
        txtPhoneNumber.BorderStyle = BorderStyle.None
        txtPhoneNumber.Text = PLACEHOLDER_PHONE
        txtPhoneNumber.Multiline = True
        txtPhoneNumber.Padding = New Padding(15, 12, 15, 12)
        pnlMain.Controls.Add(txtPhoneNumber)

        ' Username
        Dim userIcon As New Label()
        userIcon.Text = "👤"
        userIcon.Font = New Font("Arial", 14)
        userIcon.ForeColor = Color.FromArgb(6, 41, 55)
        userIcon.Location = New Point(rightColumnX, currentY + fieldSpacing * 2)
        userIcon.Size = New Size(45, fieldHeight)
        userIcon.BackColor = Color.White
        userIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlMain.Controls.Add(userIcon)

        txtUsername = New TextBox()
        txtUsername.Font = New Font("Arial", 14)
        txtUsername.Location = New Point(rightColumnX + 45, currentY + fieldSpacing * 2)
        txtUsername.Size = New Size(305, fieldHeight)
        txtUsername.BackColor = Color.White
        txtUsername.ForeColor = Color.FromArgb(6, 41, 55)
        txtUsername.BorderStyle = BorderStyle.None
        txtUsername.Text = PLACEHOLDER_USERNAME
        txtUsername.Multiline = True
        txtUsername.Padding = New Padding(15, 12, 15, 12)
        pnlMain.Controls.Add(txtUsername)

        ' Password
        Dim passIcon As New Label()
        passIcon.Text = "🔒"
        passIcon.Font = New Font("Arial", 14)
        passIcon.ForeColor = Color.FromArgb(6, 41, 55)
        passIcon.Location = New Point(rightColumnX, currentY + fieldSpacing * 3)
        passIcon.Size = New Size(45, fieldHeight)
        passIcon.BackColor = Color.White
        passIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlMain.Controls.Add(passIcon)

        txtPassword = New TextBox()
        txtPassword.Font = New Font("Arial", 14)
        txtPassword.Location = New Point(rightColumnX + 45, currentY + fieldSpacing * 3)
        txtPassword.Size = New Size(305, fieldHeight)
        txtPassword.BackColor = Color.White
        txtPassword.ForeColor = Color.FromArgb(6, 41, 55)
        txtPassword.BorderStyle = BorderStyle.None
        txtPassword.Text = PLACEHOLDER_PASSWORD
        txtPassword.Multiline = True
        txtPassword.Padding = New Padding(15, 12, 15, 12)
        txtPassword.UseSystemPasswordChar = False ' Start with placeholder visible
        pnlMain.Controls.Add(txtPassword)

        ' Confirm Password
        Dim confirmPassIcon As New Label()
        confirmPassIcon.Text = "🔒"
        confirmPassIcon.Font = New Font("Arial", 14)
        confirmPassIcon.ForeColor = Color.FromArgb(6, 41, 55)
        confirmPassIcon.Location = New Point(rightColumnX, currentY + fieldSpacing * 4)
        confirmPassIcon.Size = New Size(45, fieldHeight)
        confirmPassIcon.BackColor = Color.White
        confirmPassIcon.TextAlign = ContentAlignment.MiddleCenter
        pnlMain.Controls.Add(confirmPassIcon)

        txtConfirmPassword = New TextBox()
        txtConfirmPassword.Font = New Font("Arial", 14)
        txtConfirmPassword.Location = New Point(rightColumnX + 45, currentY + fieldSpacing * 4)
        txtConfirmPassword.Size = New Size(305, fieldHeight)
        txtConfirmPassword.BackColor = Color.White
        txtConfirmPassword.ForeColor = Color.FromArgb(6, 41, 55)
        txtConfirmPassword.BorderStyle = BorderStyle.None
        txtConfirmPassword.Text = PLACEHOLDER_CONFIRMPASSWORD
        txtConfirmPassword.Multiline = True
        txtConfirmPassword.Padding = New Padding(15, 12, 15, 12)
        txtConfirmPassword.UseSystemPasswordChar = False ' Start with placeholder visible
        pnlMain.Controls.Add(txtConfirmPassword)

        ' Sign Up Button
        btnSignUp = New Button()
        btnSignUp.Text = "SIGN UP"
        btnSignUp.Font = New Font("Arial", 14, FontStyle.Bold)
        btnSignUp.ForeColor = Color.FromArgb(25, 77, 98)
        btnSignUp.BackColor = Color.White
        btnSignUp.FlatStyle = FlatStyle.Flat
        btnSignUp.FlatAppearance.BorderSize = 0
        btnSignUp.Location = New Point(285, 350)
        btnSignUp.Size = New Size(100, 30)
        pnlMain.Controls.Add(btnSignUp)

        ' Cancel Button
        btnCancel = New Button()
        btnCancel.Text = "CANCEL"
        btnCancel.Font = New Font("Arial", 14, FontStyle.Bold)
        btnCancel.ForeColor = Color.White
        btnCancel.BackColor = Color.FromArgb(120, 120, 120)
        btnCancel.FlatStyle = FlatStyle.Flat
        btnCancel.FlatAppearance.BorderSize = 0
        btnCancel.Location = New Point(415, 350)
        btnCancel.Size = New Size(100, 30)
        pnlMain.Controls.Add(btnCancel)

        ' Copy clear button functionality to top button
        AddHandler btnClearTop.Click, AddressOf BtnClear_Click
    End Sub

    Private Sub SetupEventHandlers()
        ' Remove existing handlers to prevent duplicates
        RemoveAllEventHandlers()

        ' Button event handlers
        If btnSignUp IsNot Nothing Then
            AddHandler btnSignUp.Click, AddressOf BtnSignUp_Click
        End If

        If btnCancel IsNot Nothing Then
            AddHandler btnCancel.Click, AddressOf BtnCancel_Click
        End If

        If btnClear IsNot Nothing Then
            AddHandler btnClear.Click, AddressOf BtnClear_Click
        End If

        ' Setup placeholder event handlers for all textboxes
        SetupTextBoxPlaceholders()

        ' Additional validation handlers
        If txtUsername IsNot Nothing Then
            AddHandler txtUsername.TextChanged, AddressOf TxtUsername_TextChanged
        End If

        If txtEmail IsNot Nothing Then
            AddHandler txtEmail.Leave, AddressOf TxtEmail_Leave
        End If

        If txtPhoneNumber IsNot Nothing Then
            AddHandler txtPhoneNumber.KeyPress, AddressOf TxtPhoneNumber_KeyPress
        End If

        If txtPassword IsNot Nothing Then
            AddHandler txtPassword.TextChanged, AddressOf TxtPassword_TextChanged
        End If

        If txtConfirmPassword IsNot Nothing Then
            AddHandler txtConfirmPassword.TextChanged, AddressOf TxtConfirmPassword_TextChanged
        End If
    End Sub

    Private Sub RemoveAllEventHandlers()
        ' Remove existing event handlers to prevent duplicates
        Try
            If txtFirstName IsNot Nothing Then
                RemoveHandler txtFirstName.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtFirstName.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtLastName IsNot Nothing Then
                RemoveHandler txtLastName.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtLastName.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtEmail IsNot Nothing Then
                RemoveHandler txtEmail.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtEmail.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtUsername IsNot Nothing Then
                RemoveHandler txtUsername.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtUsername.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtPhoneNumber IsNot Nothing Then
                RemoveHandler txtPhoneNumber.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtPhoneNumber.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtAddress IsNot Nothing Then
                RemoveHandler txtAddress.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtAddress.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtPassword IsNot Nothing Then
                RemoveHandler txtPassword.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtPassword.LostFocus, AddressOf TextBox_LostFocus
            End If
            If txtConfirmPassword IsNot Nothing Then
                RemoveHandler txtConfirmPassword.GotFocus, AddressOf TextBox_GotFocus
                RemoveHandler txtConfirmPassword.LostFocus, AddressOf TextBox_LostFocus
            End If
        Catch
            ' Ignore errors when removing handlers
        End Try
    End Sub

    Private Sub SetupTextBoxPlaceholders()
        ' Setup placeholder event handlers for all textboxes
        If txtFirstName IsNot Nothing Then
            AddHandler txtFirstName.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtFirstName.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtLastName IsNot Nothing Then
            AddHandler txtLastName.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtLastName.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtEmail IsNot Nothing Then
            AddHandler txtEmail.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtEmail.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtUsername IsNot Nothing Then
            AddHandler txtUsername.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtUsername.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtPhoneNumber IsNot Nothing Then
            AddHandler txtPhoneNumber.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtPhoneNumber.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtAddress IsNot Nothing Then
            AddHandler txtAddress.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtAddress.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtPassword IsNot Nothing Then
            AddHandler txtPassword.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtPassword.LostFocus, AddressOf TextBox_LostFocus
        End If
        If txtConfirmPassword IsNot Nothing Then
            AddHandler txtConfirmPassword.GotFocus, AddressOf TextBox_GotFocus
            AddHandler txtConfirmPassword.LostFocus, AddressOf TextBox_LostFocus
        End If
    End Sub

    ' Unified TextBox Focus Handler - This handles placeholder removal when clicked
    Private Sub TextBox_GotFocus(sender As Object, e As EventArgs)
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        ' Check if textbox contains placeholder text and remove it
        If IsPlaceholderText(textBox) Then
            textBox.Text = ""
            textBox.ForeColor = Color.FromArgb(6, 41, 55) ' Change to normal text color

            ' Enable password character for password fields
            If textBox Is txtPassword OrElse textBox Is txtConfirmPassword Then
                textBox.UseSystemPasswordChar = True
            End If
        End If
    End Sub

    ' Unified TextBox LostFocus Handler - This restores placeholder when empty
    Private Sub TextBox_LostFocus(sender As Object, e As EventArgs)
        Dim textBox As TextBox = DirectCast(sender, TextBox)

        ' If textbox is empty, restore placeholder
        If String.IsNullOrWhiteSpace(textBox.Text) Then
            textBox.ForeColor = Color.FromArgb(6, 41, 55) ' Change to placeholder color

            ' Restore appropriate placeholder text
            If textBox Is txtPassword Then
                textBox.UseSystemPasswordChar = False
                textBox.Text = PLACEHOLDER_PASSWORD
            ElseIf textBox Is txtConfirmPassword Then
                textBox.UseSystemPasswordChar = False
                textBox.Text = PLACEHOLDER_CONFIRMPASSWORD
            ElseIf textBox Is txtFirstName Then
                textBox.Text = PLACEHOLDER_FIRSTNAME
            ElseIf textBox Is txtLastName Then
                textBox.Text = PLACEHOLDER_LASTNAME
            ElseIf textBox Is txtEmail Then
                textBox.Text = PLACEHOLDER_EMAIL
            ElseIf textBox Is txtUsername Then
                textBox.Text = PLACEHOLDER_USERNAME
            ElseIf textBox Is txtPhoneNumber Then
                textBox.Text = PLACEHOLDER_PHONE
            ElseIf textBox Is txtAddress Then
                textBox.Text = PLACEHOLDER_ADDRESS
            End If
        End If
    End Sub

    ' Helper function to check if textbox contains placeholder text
    Private Function IsPlaceholderText(textBox As TextBox) As Boolean
        If textBox Is Nothing Then Return False

        ' Check if text color is FromArgb(6, 41, 55) (placeholder color) and text matches placeholder
        If textBox.ForeColor = Color.FromArgb(6, 41, 55) Then
            If textBox Is txtFirstName Then
                Return textBox.Text = PLACEHOLDER_FIRSTNAME
            ElseIf textBox Is txtLastName Then
                Return textBox.Text = PLACEHOLDER_LASTNAME
            ElseIf textBox Is txtEmail Then
                Return textBox.Text = PLACEHOLDER_EMAIL
            ElseIf textBox Is txtUsername Then
                Return textBox.Text = PLACEHOLDER_USERNAME
            ElseIf textBox Is txtPhoneNumber Then
                Return textBox.Text = PLACEHOLDER_PHONE
            ElseIf textBox Is txtAddress Then
                Return textBox.Text = PLACEHOLDER_ADDRESS
            ElseIf textBox Is txtPassword Then
                Return textBox.Text = PLACEHOLDER_PASSWORD
            ElseIf textBox Is txtConfirmPassword Then
                Return textBox.Text = PLACEHOLDER_CONFIRMPASSWORD
            End If
        End If
        Return False
    End Function

    ' Helper function to get actual text value (excluding placeholder)
    Private Function GetActualTextValue(textBox As TextBox) As String
        If IsPlaceholderText(textBox) Then
            Return ""
        Else
            Return textBox.Text
        End If
    End Function

    ' Helper function for validation - gets text value excluding placeholder
    Private Function GetTextValue(txt As TextBox, placeholder As String) As String
        ' Only check if the text matches the placeholder exactly
        If txt.Text = placeholder Then
            Return ""
        End If
        Return txt.Text
    End Function
    Private Sub BtnSignUp_Click(sender As Object, e As EventArgs)
        If ValidateInputs() Then
            Try
                If CheckUserExists(GetTextValue(txtUsername, PLACEHOLDER_USERNAME), GetTextValue(txtEmail, PLACEHOLDER_EMAIL)) Then
                    MessageBox.Show("Username or email already exists. Please choose different values.",
                                  "Registration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                    Return
                End If

                If CreateUserAccount() Then
                    Dim message As String = "Account created successfully!" & vbCrLf & vbCrLf &
                                           "Welcome to LakbayPH, " & GetTextValue(txtFirstName, PLACEHOLDER_FIRSTNAME) & "!" & vbCrLf &
                                           "Username: " & GetTextValue(txtUsername, PLACEHOLDER_USERNAME) & vbCrLf &
                                           "Email: " & GetTextValue(txtEmail, PLACEHOLDER_EMAIL)

                    MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Me.Close()
                    Dim login As New MainForm()
                    login.Show()
                End If
            Catch ex As Exception
                MessageBox.Show("Error creating account: " & ex.Message, "Database Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End If
    End Sub

    Private Function CreateUserAccount() As Boolean
        Try
            ' Check if user exists first
            If CheckUserExists(GetTextValue(txtUsername, PLACEHOLDER_USERNAME), GetTextValue(txtEmail, PLACEHOLDER_EMAIL)) Then
                MessageBox.Show("Username or email already exists. Please choose different credentials.",
              "User Exists", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return False
            End If

            ' Open connection
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim hashedPassword As String = HashPassword(GetTextValue(txtPassword, PLACEHOLDER_PASSWORD))

            ' Updated query to match database schema
            Dim query As String = "INSERT INTO Users (Username, Email, Password, FirstName, LastName, Phone, Address, BirthDate, Gender) VALUES (@Username, @Email, @Password, @FirstName, @LastName, @Phone, @Address, @BirthDate, @Gender)"

            Using dbcomm As New MySqlCommand(query, conn)
                ' Add parameters matching database schema
                dbcomm.Parameters.AddWithValue("@Username", GetTextValue(txtUsername, PLACEHOLDER_USERNAME).Trim())
                dbcomm.Parameters.AddWithValue("@Email", GetTextValue(txtEmail, PLACEHOLDER_EMAIL).Trim())
                dbcomm.Parameters.AddWithValue("@Password", hashedPassword)
                dbcomm.Parameters.AddWithValue("@FirstName", GetTextValue(txtFirstName, PLACEHOLDER_FIRSTNAME).Trim())
                dbcomm.Parameters.AddWithValue("@LastName", GetTextValue(txtLastName, PLACEHOLDER_LASTNAME).Trim())
                dbcomm.Parameters.AddWithValue("@Phone", If(String.IsNullOrWhiteSpace(GetTextValue(txtPhoneNumber, PLACEHOLDER_PHONE)), DBNull.Value, GetTextValue(txtPhoneNumber, PLACEHOLDER_PHONE).Trim()))
                dbcomm.Parameters.AddWithValue("@Address", If(String.IsNullOrWhiteSpace(GetTextValue(txtAddress, PLACEHOLDER_ADDRESS)), DBNull.Value, GetTextValue(txtAddress, PLACEHOLDER_ADDRESS).Trim()))
                dbcomm.Parameters.AddWithValue("@BirthDate", dtpBirthDate.Value.Date)
                dbcomm.Parameters.AddWithValue("@Gender", cmbGender.SelectedItem.ToString())

                Dim rowsAffected As Integer = dbcomm.ExecuteNonQuery()

                If rowsAffected > 0 Then
                    Return True
                Else
                    MessageBox.Show("Failed to create user account. No rows affected.", "Error",
                  MessageBoxButtons.OK, MessageBoxIcon.Error)
                    Return False
                End If
            End Using

        Catch ex As MySqlException
            Select Case ex.Number
                Case 1062
                    MessageBox.Show("Username or email already exists. Please choose different credentials.",
                  "Duplicate Entry", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Case 2003
                    MessageBox.Show("Cannot connect to database server. Please check your connection.",
                  "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Case Else
                    MessageBox.Show($"Database error: {ex.Message}", "Database Error",
                  MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Select
            Return False

        Catch ex As Exception
            MessageBox.Show($"Unexpected error: {ex.Message}", "Error",
          MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return False
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Function CheckUserExists(username As String, email As String) As Boolean
        Try
            If conn.State = ConnectionState.Closed Then
                conn.Open()
            End If

            Dim query As String = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email"

            Using dbcomm As New MySqlCommand(query, conn)
                dbcomm.Parameters.AddWithValue("@Username", username.Trim())
                dbcomm.Parameters.AddWithValue("@Email", email.Trim())

                Dim count As Integer = Convert.ToInt32(dbcomm.ExecuteScalar())
                Return count > 0
            End Using

        Catch ex As Exception
            MessageBox.Show("Error checking user existence: " & ex.Message, "Database Error",
                  MessageBoxButtons.OK, MessageBoxIcon.Error)
            Return True
        Finally
            If conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        End Try
    End Function

    Private Function HashPassword(password As String) As String
        Try
            ' Input validation
            If String.IsNullOrEmpty(password) Then
                Throw New ArgumentException("Password cannot be null or empty")
            End If

            Using sha256Hash As SHA256 = SHA256.Create()
                ' ComputeHash - returns byte array
                Dim bytes As Byte() = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password))

                ' Convert byte array to a string using StringBuilder for better performance
                Dim builder As New StringBuilder(bytes.Length * 2)
                For Each b As Byte In bytes
                    builder.Append(b.ToString("x2"))
                Next

                Return builder.ToString()
            End Using

        Catch ex As Exception
            ' Log the error for debugging purposes
            MessageBox.Show($"Error hashing password: {ex.Message}", "Hash Error",
                      MessageBoxButtons.OK, MessageBoxIcon.Error)

            Throw New InvalidOperationException("Password hashing failed", ex)
        End Try
    End Function

    Private Sub BtnCancel_Click(sender As Object, e As EventArgs)
        Dim result As DialogResult = MessageBox.Show("Are you sure you want to cancel? All entered data will be lost.",
                                                    "Cancel Registration",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question)
        If result = DialogResult.Yes Then
            Me.Close()
        End If
    End Sub

    Private Sub BtnClear_Click(sender As Object, e As EventArgs)
        ClearAllFields()
    End Sub

    Private Sub TxtUsername_TextChanged(sender As Object, e As EventArgs)
        ' Remove spaces and convert to lowercase for username
        If txtUsername IsNot Nothing AndAlso Not IsPlaceholderText(txtUsername) Then
            Dim cursorPosition As Integer = txtUsername.SelectionStart
            Dim newText As String = txtUsername.Text.Replace(" ", "").ToLower()
            If txtUsername.Text <> newText Then
                txtUsername.Text = newText
                txtUsername.SelectionStart = Math.Min(cursorPosition, txtUsername.Text.Length)
            End If
        End If
    End Sub

    Private Sub TxtEmail_Leave(sender As Object, e As EventArgs)
        ' Email validation
        If txtEmail IsNot Nothing AndAlso Not IsPlaceholderText(txtEmail) Then
            If Not String.IsNullOrWhiteSpace(txtEmail.Text) Then
                If Not IsValidEmail(txtEmail.Text) Then
                    txtEmail.BackColor = Color.LightPink
                Else
                    txtEmail.BackColor = Color.White
                End If
            End If
        End If
    End Sub

    Private Sub TxtPhoneNumber_KeyPress(sender As Object, e As KeyPressEventArgs)
        ' Allow only numbers, backspace, delete, and common phone number characters
        If Not (Char.IsDigit(e.KeyChar) OrElse e.KeyChar = vbBack OrElse e.KeyChar = Chr(127) OrElse
                e.KeyChar = "+"c OrElse e.KeyChar = "-"c OrElse e.KeyChar = "("c OrElse e.KeyChar = ")"c OrElse e.KeyChar = " "c) Then
            e.Handled = True
        End If
    End Sub

    Private Sub TxtPassword_TextChanged(sender As Object, e As EventArgs)
        ' Password strength indicator
        If txtPassword IsNot Nothing AndAlso Not IsPlaceholderText(txtPassword) Then
            If txtPassword.Text.Length >= 8 Then
                txtPassword.BackColor = Color.LightGreen
            ElseIf txtPassword.Text.Length >= 6 Then
                txtPassword.BackColor = Color.LightYellow
            ElseIf txtPassword.Text.Length > 0 Then
                txtPassword.BackColor = Color.LightPink
            Else
                txtPassword.BackColor = Color.White
            End If
        Else
            txtPassword.BackColor = Color.White
        End If
    End Sub

    Private Sub TxtConfirmPassword_TextChanged(sender As Object, e As EventArgs)
        ' Check if passwords match
        If txtConfirmPassword IsNot Nothing AndAlso txtPassword IsNot Nothing AndAlso
           Not IsPlaceholderText(txtConfirmPassword) AndAlso Not IsPlaceholderText(txtPassword) Then
            If txtConfirmPassword.Text.Length > 0 Then
                If GetActualTextValue(txtPassword) = GetActualTextValue(txtConfirmPassword) Then
                    txtConfirmPassword.BackColor = Color.LightGreen
                Else
                    txtConfirmPassword.BackColor = Color.LightPink
                End If
            Else
                txtConfirmPassword.BackColor = Color.White
            End If
        Else
            txtConfirmPassword.BackColor = Color.White
        End If
    End Sub

    Private Function ValidateInputs() As Boolean
        ' Check if controls exist first
        If txtFirstName Is Nothing OrElse txtLastName Is Nothing OrElse
           txtEmail Is Nothing OrElse txtUsername Is Nothing OrElse
           cmbGender Is Nothing OrElse dtpBirthDate Is Nothing OrElse
           txtPassword Is Nothing OrElse txtConfirmPassword Is Nothing Then
            Return False
        End If

        ' Reset all background colors
        txtFirstName.BackColor = Color.White
        txtLastName.BackColor = Color.White
        txtEmail.BackColor = Color.White
        txtUsername.BackColor = Color.White
        txtPassword.BackColor = Color.White
        txtConfirmPassword.BackColor = Color.White

        ' Validate First Name
        Dim firstName As String = GetTextValue(txtFirstName, PLACEHOLDER_FIRSTNAME)
        If String.IsNullOrWhiteSpace(firstName) Then
            MessageBox.Show("Please enter your first name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtFirstName.BackColor = Color.LightPink
            txtFirstName.Focus()
            Return False
        End If

        ' Validate Last Name
        Dim lastName As String = GetTextValue(txtLastName, PLACEHOLDER_LASTNAME)
        If String.IsNullOrWhiteSpace(lastName) Then
            MessageBox.Show("Please enter your last name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtLastName.BackColor = Color.LightPink
            txtLastName.Focus()
            Return False
        End If

        ' Validate Email
        Dim email As String = GetTextValue(txtEmail, PLACEHOLDER_EMAIL)
        If String.IsNullOrWhiteSpace(email) Then
            MessageBox.Show("Please enter your email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtEmail.BackColor = Color.LightPink
            txtEmail.Focus()
            Return False
        End If

        If Not IsValidEmail(email) Then
            MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtEmail.BackColor = Color.LightPink
            txtEmail.Focus()
            Return False
        End If

        ' Validate Username
        Dim username As String = GetTextValue(txtUsername, PLACEHOLDER_USERNAME)
        If String.IsNullOrWhiteSpace(username) Then
            MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtUsername.BackColor = Color.LightPink
            txtUsername.Focus()
            Return False
        End If

        If username.Length < 3 Then
            MessageBox.Show("Username must be at least 3 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtUsername.BackColor = Color.LightPink
            txtUsername.Focus()
            Return False
        End If

        ' Validate Birth Date
        Dim age As Integer = CalculateAge(dtpBirthDate.Value)
        If age < 13 Then
            MessageBox.Show("You must be at least 13 years old to register.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            dtpBirthDate.Focus()
            Return False
        End If

        If age > 120 Then
            MessageBox.Show("Please enter a valid birth date.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            dtpBirthDate.Focus()
            Return False
        End If

        ' Validate Gender
        If cmbGender.SelectedIndex = 0 Then
            MessageBox.Show("Please select your gender.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            cmbGender.Focus()
            Return False
        End If

        ' Validate Password
        Dim password As String = GetTextValue(txtPassword, PLACEHOLDER_PASSWORD)
        If String.IsNullOrWhiteSpace(password) Then
            MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPassword.BackColor = Color.LightPink
            txtPassword.Focus()
            Return False
        End If

        If password.Length < 6 Then
            MessageBox.Show("Password must be at least 6 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtPassword.BackColor = Color.LightPink
            txtPassword.Focus()
            Return False
        End If

        ' Validate Confirm Password
        Dim confirmPassword As String = GetTextValue(txtConfirmPassword, PLACEHOLDER_CONFIRMPASSWORD)
        If String.IsNullOrWhiteSpace(confirmPassword) Then
            MessageBox.Show("Please confirm your password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtConfirmPassword.BackColor = Color.LightPink
            txtConfirmPassword.Focus()
            Return False
        End If

        If password <> confirmPassword Then
            MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtConfirmPassword.BackColor = Color.LightPink
            txtConfirmPassword.Focus()
            Return False
        End If

        ' Validate Phone Number if entered
        Dim phoneNumber As String = GetTextValue(txtPhoneNumber, PLACEHOLDER_PHONE)
        If Not String.IsNullOrWhiteSpace(phoneNumber) Then
            If phoneNumber.Length < 10 Then
                MessageBox.Show("Please enter a valid phone number (at least 10 digits).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtPhoneNumber.BackColor = Color.LightPink
                txtPhoneNumber.Focus()
                Return False
            End If
        End If

        Return True
    End Function

    Private Function IsValidEmail(email As String) As Boolean
        Try
            Dim addr As New System.Net.Mail.MailAddress(email)
            Return addr.Address = email.Trim().ToLower()
        Catch
            Return False
        End Try
    End Function

    Private Function CalculateAge(birthDate As DateTime) As Integer
        Dim today As DateTime = DateTime.Today
        Dim age As Integer = today.Year - birthDate.Year
        If birthDate.Date > today.AddYears(-age) Then
            age -= 1
        End If
        Return age
    End Function

    Private Sub ClearAllFields()
        ' Clear all fields and restore placeholders
        txtFirstName.Text = PLACEHOLDER_FIRSTNAME
        txtFirstName.ForeColor = Color.FromArgb(6, 41, 55)
        txtLastName.Text = PLACEHOLDER_LASTNAME
        txtLastName.ForeColor = Color.FromArgb(6, 41, 55)
        txtEmail.Text = PLACEHOLDER_EMAIL
        txtEmail.ForeColor = Color.FromArgb(6, 41, 55)
        txtUsername.Text = PLACEHOLDER_USERNAME
        txtUsername.ForeColor = Color.FromArgb(6, 41, 55)
        txtPhoneNumber.Text = PLACEHOLDER_PHONE
        txtPhoneNumber.ForeColor = Color.FromArgb(6, 41, 55)
        txtAddress.Text = PLACEHOLDER_ADDRESS
        txtAddress.ForeColor = Color.FromArgb(6, 41, 55)
        txtPassword.Text = PLACEHOLDER_PASSWORD
        txtPassword.ForeColor = Color.FromArgb(6, 41, 55)
        txtPassword.UseSystemPasswordChar = False
        txtConfirmPassword.Text = PLACEHOLDER_CONFIRMPASSWORD
        txtConfirmPassword.ForeColor = Color.FromArgb(6, 41, 55)
        txtConfirmPassword.UseSystemPasswordChar = False
        cmbGender.SelectedIndex = 0
        dtpBirthDate.Value = DateTime.Today.AddYears(-18)

        ' Reset background colors
        txtFirstName.BackColor = Color.White
        txtLastName.BackColor = Color.White
        txtEmail.BackColor = Color.White
        txtUsername.BackColor = Color.White
        txtPhoneNumber.BackColor = Color.White
        txtAddress.BackColor = Color.White
        txtPassword.BackColor = Color.White
        txtConfirmPassword.BackColor = Color.White

        txtFirstName.Focus()
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        ' Ensure database connection is closed when form is closed
        Try
            If conn IsNot Nothing AndAlso conn.State = ConnectionState.Open Then
                conn.Close()
            End If
        Catch ex As Exception
            ' Silently handle any connection cleanup errors
        End Try
        MyBase.OnFormClosed(e)
    End Sub

    Private Sub SignUpForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Form load event - can be used for initialization if needed
    End Sub

End Class